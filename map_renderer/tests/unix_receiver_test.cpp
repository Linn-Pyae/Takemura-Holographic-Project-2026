#include "unix_receiver.hpp"

#include <cassert>
#include <cerrno>
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <string>
#include <sys/socket.h>
#include <sys/stat.h>
#include <sys/un.h>
#include <unistd.h>
#include <unordered_map>

namespace {

void sendUpdate(
    int sender_fd,
    const sockaddr_un& destination,
    socklen_t destination_size,
    const mapipc::PersonUpdate& update) {
    const auto bytes = mapipc::serializePersonPacket(update);
    const ssize_t sent = ::sendto(
        sender_fd, bytes.data(), bytes.size(), 0,
        reinterpret_cast<const sockaddr*>(&destination), destination_size);
    assert(sent == static_cast<ssize_t>(bytes.size()));
}

}  // namespace

int main() {
    const std::string socket_path =
        "/tmp/takemura-renderer-test-" + std::to_string(::getpid()) + ".sock";

    mapipc::UnixDatagramReceiver receiver(socket_path);
    std::string error;
    assert(receiver.open(&error));
    assert(error.empty());
    assert(receiver.receive().status == mapipc::ReceiveStatus::would_block);

    mapipc::UnixDatagramReceiver duplicate_receiver(socket_path);
    assert(!duplicate_receiver.open(&error));
    assert(!error.empty());
    assert(receiver.isOpen());

    const int sender_fd = ::socket(AF_UNIX, SOCK_DGRAM, 0);
    assert(sender_fd >= 0);

    sockaddr_un destination{};
    destination.sun_family = AF_UNIX;
    std::memcpy(destination.sun_path, socket_path.c_str(),
                socket_path.size() + 1);
    const auto destination_size = static_cast<socklen_t>(
        offsetof(sockaddr_un, sun_path) + socket_path.size() + 1);

    sendUpdate(sender_fd, destination, destination_size,
               {7, 1.0F, 2.0F, 10, 1000});
    sendUpdate(sender_fd, destination, destination_size,
               {7, 99.0F, 99.0F, 9, 1001});
    sendUpdate(sender_fd, destination, destination_size,
               {8, 3.0F, 4.0F, 1, 1002});

    std::unordered_map<std::int32_t, mapipc::PersonUpdate> people;
    for (;;) {
        const mapipc::ReceiveResult result = receiver.receive();
        if (result.status == mapipc::ReceiveStatus::would_block) {
            break;
        }
        assert(result.status == mapipc::ReceiveStatus::packet);

        const mapipc::PersonUpdate& incoming = *result.update;
        const auto current = people.find(incoming.id);
        if (current == people.end() ||
            incoming.sequence > current->second.sequence) {
            people[incoming.id] = incoming;
        }
    }

    assert(people.size() == 2);
    assert(people.at(7).sequence == 10);
    assert(people.at(7).x == 1.0F);
    assert(people.at(8).sequence == 1);

    ::close(sender_fd);
    receiver.close();

    struct stat info {};
    assert(::lstat(socket_path.c_str(), &info) < 0);
    assert(errno == ENOENT);
    return 0;
}
