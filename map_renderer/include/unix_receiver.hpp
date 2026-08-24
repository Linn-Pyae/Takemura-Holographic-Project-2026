#pragma once

#include "person_packet.hpp"

#include <cstddef>
#include <cstdint>
#include <optional>
#include <string>
#include <vector>

namespace mapipc {

enum class ReceiveStatus {
    packet,
    would_block,
    invalid_packet,
    socket_error,
};

struct ReceiveResult {
    ReceiveStatus status = ReceiveStatus::would_block;
    std::optional<PersonUpdate> update;
    std::string error;
};

// A non-blocking Unix domain datagram receiver intended to be drained once
// per render frame. Linux/POSIX only.
class UnixDatagramReceiver {
public:
    explicit UnixDatagramReceiver(std::string socket_path);
    ~UnixDatagramReceiver();

    UnixDatagramReceiver(const UnixDatagramReceiver&) = delete;
    UnixDatagramReceiver& operator=(const UnixDatagramReceiver&) = delete;

    UnixDatagramReceiver(UnixDatagramReceiver&& other) noexcept;
    UnixDatagramReceiver& operator=(UnixDatagramReceiver&& other) noexcept;

    bool open(std::string* error = nullptr);
    void close() noexcept;
    bool isOpen() const noexcept;
    const std::string& socketPath() const noexcept;

    // Call repeatedly until would_block to consume everything currently
    // queued. The caller should retain only the greatest sequence per person.
    ReceiveResult receive();

    // Drain one raw datagram (static-map packets are up to 8 KiB).
    ReceiveStatus receiveRaw(std::vector<std::uint8_t> *bytes,
                             std::string *error = nullptr,
                             std::size_t max_bytes = 8192);

private:
    std::string socket_path_;
    int socket_fd_ = -1;
    bool owns_socket_path_ = false;
};

}  // namespace mapipc
