cmake_minimum_required(VERSION 3.16)

project(takemura_renderer LANGUAGES C CXX)

find_package(raylib 6.0 QUIET)
if(NOT raylib_FOUND)
    include(FetchContent)
    set(BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
    set(BUILD_GAMES OFF CACHE BOOL "" FORCE)
    FetchContent_Declare(
        raylib
        URL https://github.com/raysan5/raylib/archive/refs/tags/6.0.tar.gz
        DOWNLOAD_EXTRACT_TIMESTAMP FALSE
    )
    FetchContent_MakeAvailable(raylib)
endif()

add_library(renderer_ipc
    src/person_packet.cpp
    src/unix_receiver.cpp
)

target_include_directories(renderer_ipc PUBLIC include)
target_compile_features(renderer_ipc PUBLIC cxx_std_17)
target_compile_options(renderer_ipc PRIVATE -Wall -Wextra -Wpedantic)

add_executable(renderer src/main.cpp)
target_compile_features(renderer PRIVATE cxx_std_17)
target_compile_options(renderer PRIVATE -Wall -Wextra -Wpedantic)
target_link_libraries(renderer PRIVATE renderer_ipc raylib)
add_custom_command(
    TARGET renderer POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
            "${CMAKE_CURRENT_SOURCE_DIR}/assets"
            "$<TARGET_FILE_DIR:renderer>/assets"
    COMMENT "Copying renderer assets"
)

add_executable(unix_receive_example examples/unix_receive_example.cpp)
target_link_libraries(unix_receive_example PRIVATE renderer_ipc)

include(CTest)
if(BUILD_TESTING)
    add_executable(person_packet_test tests/person_packet_test.cpp)
    target_link_libraries(person_packet_test PRIVATE renderer_ipc)
    add_test(NAME person_packet_test COMMAND person_packet_test)

    add_executable(unix_receiver_test tests/unix_receiver_test.cpp)
    target_link_libraries(unix_receiver_test PRIVATE renderer_ipc)
    add_test(NAME unix_receiver_test COMMAND unix_receiver_test)
endif()
