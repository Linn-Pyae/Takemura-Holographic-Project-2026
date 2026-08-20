cmake_minimum_required(VERSION 3.10)
project(static_map_projector)

if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
  add_compile_options(-Wall -Wextra -Wpedantic)
endif()

find_package(ament_cmake REQUIRED)
find_package(nav_msgs REQUIRED)
find_package(rclcpp REQUIRED)
find_package(sensor_msgs REQUIRED)
find_package(std_msgs REQUIRED)

add_executable(static_map_projector_node
  src/static_map_projector_node.cpp
)
target_compile_features(static_map_projector_node PUBLIC cxx_std_17)
ament_target_dependencies(static_map_projector_node
  nav_msgs
  rclcpp
  sensor_msgs
  std_msgs
)

install(TARGETS static_map_projector_node
  DESTINATION lib/${PROJECT_NAME}
)

ament_package()
