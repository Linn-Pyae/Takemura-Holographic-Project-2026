#include "unix_receiver.hpp"

#include <chrono>
#include <cstdint>
#include <iostream>
#include <string>
#include <thread>
#include <unordered_map>

int main(int argc, char** argv) {
    const std::string socket_path =
        argc > 1 ? argv[1] : "/tmp/takemura-renderer.sock";

    mapipc::UnixDatagramReceiver receiver(socket_path);
    std::string error;
    if (!receiver.open(&error)) {
        std::cerr << "Could not start Unix socket receiver: " << error << '\n';
        return 1;
    }

    std::unordered_map<std::int32_t, mapipc::PersonUpdate> people;
    std::cout << "Listening on " << receiver.socketPath() << "...\n";

    // This loop stands in for the future render loop. Each iteration drains
    // the socket and keeps only the greatest sequence seen for each person.
    for (;;) {
        for (;;) {
            const mapipc::ReceiveResult result = receiver.receive();
            if (result.status == mapipc::ReceiveStatus::would_block) {
                break;
            }
            if (result.status == mapipc::ReceiveStatus::invalid_packet) {
                std::cerr << "Ignored packet: " << result.error << '\n';
                continue;
            }
            if (result.status == mapipc::ReceiveStatus::socket_error) {
                std::cerr << "Unix socket error: " << result.error << '\n';
                return 1;
            }

            const mapipc::PersonUpdate& incoming = *result.update;
            const auto current = people.find(incoming.id);
            if (current != people.end() &&
                incoming.sequence <= current->second.sequence) {
                continue;
            }

            people[incoming.id] = incoming;
            std::cout << "id=" << incoming.id
                      << " x=" << incoming.x
                      << " y=" << incoming.y
                      << " sequence=" << incoming.sequence
                      << " timestamp=" << incoming.timestamp << '\n';
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(16));
    }
}
