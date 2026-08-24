cmake_minimum_required(VERSION 3.10)
project(static_map_line_extractor)

if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
  add_compile_options(-Wall -Wextra -Wpedantic)
endif()

find_package(ament_cmake REQUIRED)
find_package(geometry_msgs REQUIRED)
find_package(nav_msgs REQUIRED)
find_package(OpenCV REQUIRED COMPONENTS core imgproc)
find_package(rclcpp REQUIRED)
find_package(visualization_msgs REQUIRED)

add_library(static_map_line_extractor_core
  src/line_extractor.cpp
)
target_compile_features(static_map_line_extractor_core PUBLIC cxx_std_17)
target_include_directories(static_map_line_extractor_core
  PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
    ${OpenCV_INCLUDE_DIRS}
)
target_link_libraries(static_map_line_extractor_core PUBLIC ${OpenCV_LIBS})

add_executable(static_map_line_extractor_node
  src/static_map_line_extractor_node.cpp
)
target_compile_features(static_map_line_extractor_node PUBLIC cxx_std_17)
ament_target_dependencies(static_map_line_extractor_node
  geometry_msgs
  nav_msgs
  rclcpp
  visualization_msgs
)
target_link_libraries(static_map_line_extractor_node
  static_map_line_extractor_core
)

install(
  TARGETS static_map_line_extractor_core static_map_line_extractor_node
  ARCHIVE DESTINATION lib
  LIBRARY DESTINATION lib
  RUNTIME DESTINATION lib/${PROJECT_NAME}
)
install(DIRECTORY include/ DESTINATION include)

if(BUILD_TESTING)
  find_package(ament_cmake_gtest REQUIRED)
  ament_add_gtest(test_line_extractor test/test_line_extractor.cpp)
  target_link_libraries(test_line_extractor static_map_line_extractor_core)
endif()

ament_export_include_directories(include)
ament_export_libraries(static_map_line_extractor_core)
ament_package()
