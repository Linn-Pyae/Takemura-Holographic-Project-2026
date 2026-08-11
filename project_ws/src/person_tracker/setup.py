from setuptools import find_packages, setup

package_name = "person_tracker"

setup(
    name=package_name,
    version="0.0.1",
    packages=find_packages(exclude=["test"]),
    data_files=[
        (
            "share/ament_index/resource_index/packages",
            ["resource/" + package_name],
        ),
        ("share/" + package_name, ["package.xml"]),
    ],
    install_requires=["setuptools"],
    zip_safe=True,
    maintainer="linnpyae",
    maintainer_email="mglinnpyae2014@gmail.com",
    description="Track person cluster centroids from PoseArray detections",
    license="Apache-2.0",
    extras_require={
        "test": ["pytest"],
    },
    entry_points={
        "console_scripts": [
            "track_node = person_tracker.track_node:main",
            "viz_node = person_tracker.viz_node:main",
        ],
    },
)
