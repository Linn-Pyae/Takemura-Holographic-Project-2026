cmake_minimum_required(VERSION 3.16)

project(MetaStudioCLI VERSION 0.1.0 LANGUAGES C)

if(NOT CMAKE_SYSTEM_NAME STREQUAL "Linux")
  message(FATAL_ERROR "metastudio-cli currently supports Linux only")
endif()

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS OFF)

add_executable(metastudio-cli
  src/main.c
  src/cli.c
  src/controller.c
  src/protocol.c
  src/serial.c
)
target_include_directories(metastudio-cli PRIVATE src)
target_compile_definitions(metastudio-cli PRIVATE
  METASTUDIO_CLI_VERSION="${PROJECT_VERSION}"
)

target_compile_options(metastudio-cli PRIVATE -Wall -Wextra -Wpedantic)
target_link_options(metastudio-cli PRIVATE -static)

include(CTest)
if(BUILD_TESTING)
  add_test(NAME cli-help COMMAND metastudio-cli --help)
  add_test(NAME cli-short-help COMMAND metastudio-cli -h)
  add_test(NAME cli-version COMMAND metastudio-cli --version)
  add_test(NAME cli-command-help COMMAND metastudio-cli set --help)
  add_test(NAME cli-scan-help COMMAND metastudio-cli scan-vdbox --help)
  add_test(NAME cli-brightness-help COMMAND metastudio-cli test-brightness --help)
endif()

include(GNUInstallDirs)
install(TARGETS metastudio-cli
  RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR})
install(FILES ${CMAKE_CURRENT_SOURCE_DIR}/README.md
  DESTINATION .)

set(CPACK_PACKAGE_NAME "metastudio-cli")
set(CPACK_PACKAGE_VENDOR "MetaStudio")
set(CPACK_PACKAGE_VERSION ${PROJECT_VERSION})
set(CPACK_PACKAGE_FILE_NAME
  "metastudio-cli-${PROJECT_VERSION}-linux-${CMAKE_SYSTEM_PROCESSOR}")
set(CPACK_GENERATOR "TGZ")
include(CPack)
