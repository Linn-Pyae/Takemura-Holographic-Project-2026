#include "unix_receiver.hpp"

#include <cerrno>
#include <cstring>
#include <fcntl.h>
#include <sys/socket.h>
#include <sys/stat.h>
#include <sys/un.h>
#include <unistd.h>

#include <array>
#include <cstddef>
#include <utility>

namespace mapipc {
namespace {

void setError(std::string* output, const std::string& message) {
    if (output != nullptr) {
        *output = message;
    }
}

std::string systemError(const std::string& operation) {
    return operation + ": " + std::strerror(errno);
}

bool prepareSocketPath(const std::string& path, std::string* error) {
    struct stat info {};
    if (::lstat(path.c_str(), &info) < 0) {
        if (errno == ENOENT) {
            return true;
        }
        setError(error, systemError("lstat"));
        return false;
    }

    if (!S_ISSOCK(info.st_mode)) {
        setError(error, "socket path exists and is not a socket: " + path);
        return false;
    }

    // Do not unlink a socket owned by another live receiver. Connecting a
    // datagram socket succeeds while a receiver is bound and fails with
    // ECONNREFUSED for the filesystem node left behind after a crash.
    const int probe_fd = ::socket(AF_UNIX, SOCK_DGRAM, 0);
    if (probe_fd < 0) {
        setError(error, systemError("socket probe"));
        return false;
    }

    sockaddr_un destination{};
    destination.sun_family = AF_UNIX;
    std::memcpy(destination.sun_path, path.c_str(), path.size() + 1);
    const auto destination_size = static_cast<socklen_t>(
        offsetof(sockaddr_un, sun_path) + path.size() + 1);

    if (::connect(probe_fd, reinterpret_cast<sockaddr*>(&destination),
                  destination_size) == 0) {
        ::close(probe_fd);
        setError(error, "socket path is already in use: " + path);
        return false;
    }

    const int connect_error = errno;
    ::close(probe_fd);
    if (connect_error != ECONNREFUSED) {
        errno = connect_error;
        setError(error, systemError("socket path probe"));
        return false;
    }

    if (::unlink(path.c_str()) < 0) {
        setError(error, systemError("unlink"));
        return false;
    }
    return true;
}

}  // namespace

UnixDatagramReceiver::UnixDatagramReceiver(std::string socket_path)
    : socket_path_(std::move(socket_path)) {}

UnixDatagramReceiver::~UnixDatagramReceiver() {
    close();
}

UnixDatagramReceiver::UnixDatagramReceiver(
    UnixDatagramReceiver&& other) noexcept
    : socket_path_(std::move(other.socket_path_)),
      socket_fd_(other.socket_fd_),
      owns_socket_path_(other.owns_socket_path_) {
    other.socket_fd_ = -1;
    other.owns_socket_path_ = false;
}

UnixDatagramReceiver& UnixDatagramReceiver::operator=(
    UnixDatagramReceiver&& other) noexcept {
    if (this != &other) {
        close();
        socket_path_ = std::move(other.socket_path_);
        socket_fd_ = other.socket_fd_;
        owns_socket_path_ = other.owns_socket_path_;
        other.socket_fd_ = -1;
        other.owns_socket_path_ = false;
    }
    return *this;
}

bool UnixDatagramReceiver::open(std::string* error) {
    close();

    if (socket_path_.empty()) {
        setError(error, "socket path must not be empty");
        return false;
    }

    sockaddr_un local_address{};
    if (socket_path_.size() >= sizeof(local_address.sun_path)) {
        setError(error, "socket path is too long: " + socket_path_);
        return false;
    }

    if (!prepareSocketPath(socket_path_, error)) {
        return false;
    }

    socket_fd_ = ::socket(AF_UNIX, SOCK_DGRAM, 0);
    if (socket_fd_ < 0) {
        setError(error, systemError("socket"));
        return false;
    }

    const int flags = ::fcntl(socket_fd_, F_GETFL, 0);
    if (flags < 0 || ::fcntl(socket_fd_, F_SETFL, flags | O_NONBLOCK) < 0) {
        setError(error, systemError("fcntl"));
        close();
        return false;
    }

    local_address.sun_family = AF_UNIX;
    std::memcpy(local_address.sun_path, socket_path_.c_str(),
                socket_path_.size() + 1);

    const auto address_size = static_cast<socklen_t>(
        offsetof(sockaddr_un, sun_path) + socket_path_.size() + 1);
    if (::bind(socket_fd_, reinterpret_cast<sockaddr*>(&local_address),
               address_size) < 0) {
        setError(error, systemError("bind"));
        close();
        return false;
    }

    owns_socket_path_ = true;
    if (error != nullptr) {
        error->clear();
    }
    return true;
}

void UnixDatagramReceiver::close() noexcept {
    if (socket_fd_ >= 0) {
        ::close(socket_fd_);
        socket_fd_ = -1;
    }

    if (owns_socket_path_) {
        struct stat info {};
        if (::lstat(socket_path_.c_str(), &info) == 0 &&
            S_ISSOCK(info.st_mode)) {
            ::unlink(socket_path_.c_str());
        }
        owns_socket_path_ = false;
    }
}

bool UnixDatagramReceiver::isOpen() const noexcept {
    return socket_fd_ >= 0;
}

const std::string& UnixDatagramReceiver::socketPath() const noexcept {
    return socket_path_;
}

ReceiveResult UnixDatagramReceiver::receive() {
    ReceiveResult result;
    if (!isOpen()) {
        result.status = ReceiveStatus::socket_error;
        result.error = "Unix datagram receiver is not open";
        return result;
    }

    // One extra byte distinguishes the valid fixed-size packet from any
    // oversized datagram.
    std::array<std::uint8_t, kPersonPacketSize + 1> buffer{};
    const ssize_t received =
        ::recv(socket_fd_, buffer.data(), buffer.size(), 0);

    if (received < 0) {
        if (errno == EAGAIN || errno == EWOULDBLOCK) {
            result.status = ReceiveStatus::would_block;
            return result;
        }
        result.status = ReceiveStatus::socket_error;
        result.error = systemError("recv");
        return result;
    }

    std::string parse_error;
    result.update = parsePersonPacket(
        buffer.data(), static_cast<std::size_t>(received), &parse_error);
    if (!result.update.has_value()) {
        result.status = ReceiveStatus::invalid_packet;
        result.error = std::move(parse_error);
        return result;
    }

    result.status = ReceiveStatus::packet;
    return result;
}

}  // namespace mapipc
