cmake_minimum_required(VERSION 3.10)
project(renderer_bridge)

if(CMAKE_COMPILER_IS_GNUCXX OR CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  add_compile_options(-Wall -Wextra -Wpedantic)
endif()

find_package(ament_cmake REQUIRED)
find_package(rclcpp REQUIRED)
find_package(std_msgs REQUIRED)

# Keep conda/RoboStack shared libs resolvable after install (macOS @rpath).
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)
if(DEFINED ENV{CONDA_PREFIX})
  list(APPEND CMAKE_INSTALL_RPATH "$ENV{CONDA_PREFIX}/lib")
endif()

add_executable(renderer_bridge_node
  src/renderer_bridge_node.cpp
  src/person_packet.cpp
)
target_include_directories(renderer_bridge_node PRIVATE include)
target_compile_features(renderer_bridge_node PUBLIC cxx_std_17)
ament_target_dependencies(renderer_bridge_node rclcpp std_msgs)

install(TARGETS renderer_bridge_node
  DESTINATION lib/${PROJECT_NAME})

if(BUILD_TESTING)
  find_package(ament_lint_auto REQUIRED)
  set(ament_cmake_copyright_FOUND TRUE)
  set(ament_cmake_cpplint_FOUND TRUE)
  ament_lint_auto_find_test_dependencies()
endif()

ament_package()
