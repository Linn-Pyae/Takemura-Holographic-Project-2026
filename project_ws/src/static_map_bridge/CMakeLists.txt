cmake_minimum_required(VERSION 3.10)
project(static_map_bridge)

if(NOT CMAKE_CXX_STANDARD)
  set(CMAKE_CXX_STANDARD 17)
endif()
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_package(ament_cmake REQUIRED)
find_package(rclcpp REQUIRED)
find_package(visualization_msgs REQUIRED)

add_library(static_map_packet_protocol
  src/packet_protocol.cpp
)
target_include_directories(static_map_packet_protocol
  PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
)

add_executable(static_map_bridge_node
  src/static_map_bridge_node.cpp
)
target_link_libraries(static_map_bridge_node static_map_packet_protocol)
ament_target_dependencies(static_map_bridge_node
  rclcpp
  visualization_msgs
)

install(
  TARGETS static_map_packet_protocol static_map_bridge_node
  ARCHIVE DESTINATION lib
  LIBRARY DESTINATION lib
  RUNTIME DESTINATION lib/${PROJECT_NAME}
)
install(DIRECTORY include/ DESTINATION include)

if(BUILD_TESTING)
  enable_testing()
  add_executable(test_packet_protocol test/test_packet_protocol.cpp)
  target_link_libraries(test_packet_protocol static_map_packet_protocol)
  add_test(NAME packet_protocol COMMAND test_packet_protocol)
endif()

ament_export_include_directories(include)
ament_export_libraries(static_map_packet_protocol)
ament_package()
