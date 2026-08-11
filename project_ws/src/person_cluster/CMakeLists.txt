cmake_minimum_required(VERSION 3.10)
project(person_cluster)

if(CMAKE_COMPILER_IS_GNUCXX OR CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  add_compile_options(-Wall -Wextra -Wpedantic)
endif()

# find dependencies
find_package(ament_cmake REQUIRED)
find_package(rclcpp REQUIRED)
find_package(sensor_msgs REQUIRED)
find_package(geometry_msgs REQUIRED)
find_package(pcl_conversions REQUIRED)
if(POLICY CMP0074)
  cmake_policy(SET CMP0074 NEW)
endif()
find_package(PCL REQUIRED COMPONENTS common filters segmentation search)

add_executable(person_cluster_node src/person_cluster_node.cpp)
ament_target_dependencies(person_cluster_node
  rclcpp sensor_msgs geometry_msgs pcl_conversions)
target_include_directories(person_cluster_node PRIVATE ${PCL_INCLUDE_DIRS})
target_link_libraries(person_cluster_node ${PCL_LIBRARIES})
target_compile_definitions(person_cluster_node PRIVATE ${PCL_DEFINITIONS})

install(TARGETS person_cluster_node
  DESTINATION lib/${PROJECT_NAME})

if(BUILD_TESTING)
  find_package(ament_lint_auto REQUIRED)
  # the following line skips the linter which checks for copyrights
  # comment the line when a copyright and license is added to all source files
  set(ament_cmake_copyright_FOUND TRUE)
  # the following line skips cpplint (only works in a git repo)
  # comment the line when this package is in a git repo and when
  # a copyright and license is added to all source files
  set(ament_cmake_cpplint_FOUND TRUE)
  ament_lint_auto_find_test_dependencies()
endif()

ament_package()
