.PHONY: all clean test

BUILD_DIR := build
DIST_DIR := dist
BINARY := $(DIST_DIR)/metastudio-cli

all:
	@test "$$(uname -s)" = Linux || { echo "error: Linux is required" >&2; exit 1; }
	cmake -S . -B $(BUILD_DIR) -DCMAKE_BUILD_TYPE=Release
	cmake --build $(BUILD_DIR) --parallel
	cmake -E make_directory $(DIST_DIR)
	cmake -E copy $(BUILD_DIR)/metastudio-cli $(BINARY)
	strip $(BINARY)

test:
	cmake -S . -B $(BUILD_DIR) -DCMAKE_BUILD_TYPE=Release
	cmake --build $(BUILD_DIR) --parallel
	ctest --test-dir $(BUILD_DIR) --output-on-failure

clean:
	cmake -E rm -rf $(BUILD_DIR) $(DIST_DIR)
