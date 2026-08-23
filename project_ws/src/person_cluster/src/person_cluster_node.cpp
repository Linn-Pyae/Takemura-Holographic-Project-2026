#include <algorithm>
#include <cmath>
#include <cstdint>
#include <deque>
#include <fstream>
#include <memory>
#include <unordered_map>
#include <unordered_set>
#include <vector>

#include "geometry_msgs/msg/pose.hpp"
#include "geometry_msgs/msg/pose_array.hpp"
#include "rclcpp/rclcpp.hpp"
#include "sensor_msgs/msg/point_cloud2.hpp"

#include <Eigen/Eigenvalues>
#include <pcl/common/centroid.h>
#include <pcl/filters/passthrough.h>
#include <pcl/filters/voxel_grid.h>
#include <pcl/point_cloud.h>
#include <pcl/point_types.h>
#include <pcl/search/kdtree.h>
#include <pcl/segmentation/extract_clusters.h>
#include <pcl_conversions/pcl_conversions.h>

using std::placeholders::_1;

namespace {

struct VoxelKey {
  int32_t x{0};
  int32_t y{0};
  int32_t z{0};

  bool operator==(const VoxelKey &other) const {
    return x == other.x && y == other.y && z == other.z;
  }
};

struct VoxelKeyHash {
  std::size_t operator()(const VoxelKey &key) const noexcept {
    std::size_t h = static_cast<std::size_t>(key.x) * 73856093u;
    h ^= static_cast<std::size_t>(key.y) * 19349663u;
    h ^= static_cast<std::size_t>(key.z) * 83492791u;
    return h;
  }
};

struct ClusterShape {
  float cx{0.0f};
  float cy{0.0f};
  float cz{0.0f};
  float width{0.0f};       // horizontal footprint diagonal [m]
  float height{0.0f};      // vertical extent [m]
  float range{0.0f};       // horizontal distance from sensor [m]
  float verticality{0.0f}; // |principal axis . z|, 1.0 = upright
  std::size_t points{0};

  // Kidono-style discriminative features (all computed from XYZ only).
  float eig_width_ratio{1.0f}; // lambda2 / lambda1: horizontal spread vs height
  float eig_depth_ratio{1.0f}; // lambda3 / lambda2: flatness in horizontal plane
  float slice_occupancy{1.0f}; // fraction of vertical slices with >= min points
  float slice_peak{1.0f};      // largest slice point fraction (concentration)
};

/// A blob that has been seen for a while, used to tell walkers from
/// static objects that flicker in and out of the background model.
struct Candidate {
  float x{0.0f};
  float y{0.0f};
  float z{0.0f};
  std::deque<std::pair<float, float>> history;
  int unseen{0};
  long last_move_frame{-1000000};
};

constexpr float kFloorBinSize = 0.05f;
constexpr float kFloorRangeMin = -5.0f;
constexpr float kFloorRangeMax = 5.0f;
constexpr std::size_t kFloorBins =
    static_cast<std::size_t>((kFloorRangeMax - kFloorRangeMin) / kFloorBinSize);

} // namespace

/**
 * Detect walking people in a Velodyne cloud.
 *
 * 1) Estimate the floor height from the z histogram during warmup, so the
 *    height band is relative to the floor and not to the sensor mount.
 * 2) Keep a floor-relative height band, limit range, voxel downsample.
 * 3) Background model: per-voxel exponential moving average of occupancy,
 *    updated every frame. Walls, furniture and objects that only flicker
 *    still converge to a high occupancy and get subtracted; a person walking
 *    through a voxel barely moves its average.
 * 4) Euclidean clustering of what remains.
 * 5) Human-shape gate: vertically elongated, narrow blob whose point count
 *    is consistent with its distance from the sensor.
 * 6) Motion memory: a blob is only published once it has actually travelled,
 *    with a hold time so a person who pauses is not dropped instantly.
 */
class PersonClusterNode : public rclcpp::Node {
public:
  PersonClusterNode() : Node("person_cluster_node") {
    auto_floor_ = declare_parameter<bool>("auto_floor", true);
    floor_z_ = declare_parameter<double>("floor_z", -0.85);
    floor_search_span_ = declare_parameter<double>("floor_search_span", 0.60);
    z_offset_min_ = declare_parameter<double>("z_offset_min", 0.30);
    z_offset_max_ = declare_parameter<double>("z_offset_max", 2.00);
    max_range_ = declare_parameter<double>("max_range", 15.0);
    leaf_size_ = declare_parameter<double>("leaf_size", 0.07);

    bg_voxel_size_ = declare_parameter<double>("bg_voxel_size", 0.30);
    floor_frames_ = declare_parameter<int>("floor_frames", 15);
    warmup_frames_ = declare_parameter<int>("warmup_frames", 20);
    bg_alpha_ = declare_parameter<double>("bg_alpha", 0.05);
    bg_warm_alpha_ = declare_parameter<double>("bg_warm_alpha", 0.30);
    static_threshold_ = declare_parameter<double>("static_threshold", 0.35);

    cluster_tolerance_ = declare_parameter<double>("cluster_tolerance", 0.45);
    min_cluster_size_ = declare_parameter<int>("min_cluster_size", 4);
    max_cluster_size_ = declare_parameter<int>("max_cluster_size", 1200);

    min_height_m_ = declare_parameter<double>("min_height_m", 0.45);
    max_height_m_ = declare_parameter<double>("max_height_m", 2.10);
    min_width_m_ = declare_parameter<double>("min_width_m", 0.08);
    max_width_m_ = declare_parameter<double>("max_width_m", 1.00);
    min_aspect_ratio_ = declare_parameter<double>("min_aspect_ratio", 0.90);
    min_verticality_ = declare_parameter<double>("min_verticality", 0.45);
    verticality_min_points_ =
        declare_parameter<int>("verticality_min_points", 12);
    points_at_one_meter_ =
        declare_parameter<double>("points_at_one_meter", 45.0);
    min_points_floor_ = declare_parameter<int>("min_points_floor", 5);

    // Kidono-style shape gates. A value of 0.0 disables the corresponding
    // gate, so the node keeps its pre-feature behaviour until tuned.
    max_aspect_ratio_ = declare_parameter<double>("max_aspect_ratio", 0.0);
    max_eig_width_ratio_ =
        declare_parameter<double>("max_eig_width_ratio", 0.0);
    min_eig_depth_ratio_ =
        declare_parameter<double>("min_eig_depth_ratio", 0.0);
    n_slices_ = declare_parameter<int>("n_slices", 10);
    slice_min_points_ = declare_parameter<int>("slice_min_points", 1);
    min_slice_occupancy_ =
        declare_parameter<double>("min_slice_occupancy", 0.0);
    feature_dump_path_ =
        declare_parameter<std::string>("feature_dump_path", "");

    assoc_radius_ = declare_parameter<double>("assoc_radius", 0.70);
    history_frames_ = declare_parameter<int>("history_frames", 10);
    move_distance_m_ = declare_parameter<double>("move_distance_m", 0.35);
    hold_frames_ = declare_parameter<int>("hold_frames", 15);
    drop_frames_ = declare_parameter<int>("drop_frames", 5);

    require_motion_ = declare_parameter<bool>("require_motion", true);
    debug_stats_ = declare_parameter<bool>("debug_stats", true);
    process_period_ = declare_parameter<double>("process_period", 0.12);

    if (!feature_dump_path_.empty()) {
      dump_.open(feature_dump_path_, std::ios::out | std::ios::app);
      if (dump_.is_open()) {
        dump_ << "frame,human,cx,cy,cz,points,range,height,width,aspect,"
                 "verticality,eig_width_ratio,eig_depth_ratio,"
                 "slice_occupancy,slice_peak\n";
        RCLCPP_INFO(get_logger(), "dumping cluster features to %s",
                    feature_dump_path_.c_str());
      } else {
        RCLCPP_ERROR(get_logger(), "cannot open feature dump %s",
                     feature_dump_path_.c_str());
      }
    }

    auto qos = rclcpp::SensorDataQoS().keep_last(1);
    sub_ = create_subscription<sensor_msgs::msg::PointCloud2>(
        "/velodyne_points_bag", qos,
        std::bind(&PersonClusterNode::cloud_cb, this, _1));
    pub_ = create_publisher<geometry_msgs::msg::PoseArray>(
        "/person_detections", rclcpp::QoS(1).reliable());

    floor_hist_.assign(kFloorBins, 0);

    RCLCPP_INFO(
        get_logger(),
        "person clusterer ready (floor_frames=%d, warmup=%d, bg_alpha=%.3f, "
        "static>=%.2f, motion=%s, gate h=[%.2f,%.2f] w<=%.2f)",
        floor_frames_, warmup_frames_, bg_alpha_, static_threshold_,
        require_motion_ ? "on" : "off", min_height_m_, max_height_m_,
        max_width_m_);
  }

private:
  VoxelKey to_key(float x, float y, float z) const {
    const float inv = static_cast<float>(1.0 / bg_voxel_size_);
    return VoxelKey{
        static_cast<int32_t>(std::floor(x * inv)),
        static_cast<int32_t>(std::floor(y * inv)),
        static_cast<int32_t>(std::floor(z * inv)),
    };
  }

  void accumulate_floor_hist(const pcl::PointCloud<pcl::PointXYZ> &cloud) {
    for (const auto &pt : cloud.points) {
      if (!std::isfinite(pt.z) || pt.z < kFloorRangeMin ||
          pt.z >= kFloorRangeMax) {
        continue;
      }
      const auto bin =
          static_cast<std::size_t>((pt.z - kFloorRangeMin) / kFloorBinSize);
      if (bin < floor_hist_.size()) {
        ++floor_hist_[bin];
      }
      floor_seen_min_ = std::min(floor_seen_min_, pt.z);
      floor_seen_max_ = std::max(floor_seen_max_, pt.z);
    }
  }

  /// The floor is the densest slab just above the lowest returns.
  ///
  /// Searching the whole lower half picks the walls instead of the floor when
  /// the sensor is mounted high: most returns then sit near sensor height, and
  /// a floor guessed there puts the height band above people's heads.
  void estimate_floor() {
    if (!auto_floor_ || floor_seen_max_ <= floor_seen_min_) {
      return;
    }

    long total = 0;
    for (const long count : floor_hist_) {
      total += count;
    }
    if (total <= 0) {
      return;
    }

    // Ignore the first few stray returns below the floor.
    const long noise_budget = std::max(1L, total / 200);
    std::size_t lowest_bin = 0;
    long seen = 0;
    for (std::size_t i = 0; i < floor_hist_.size(); ++i) {
      seen += floor_hist_[i];
      if (seen > noise_budget) {
        lowest_bin = i;
        break;
      }
    }

    const auto span_bins =
        static_cast<std::size_t>(floor_search_span_ / kFloorBinSize);
    const std::size_t last_bin =
        std::min(floor_hist_.size() - 1, lowest_bin + span_bins);

    std::size_t best_bin = lowest_bin;
    long best_count = -1;
    for (std::size_t i = lowest_bin; i <= last_bin; ++i) {
      if (floor_hist_[i] > best_count) {
        best_count = floor_hist_[i];
        best_bin = i;
      }
    }
    if (best_count > 0) {
      floor_z_ = kFloorRangeMin +
                 (static_cast<float>(best_bin) + 0.5f) * kFloorBinSize;
    }
    RCLCPP_INFO(get_logger(),
                "floor estimated at z=%.2f (lowest return %.2f) -> height band "
                "%.2f..%.2f",
                floor_z_, floor_seen_min_, floor_z_ + z_offset_min_,
                floor_z_ + z_offset_max_);
  }

  pcl::PointCloud<pcl::PointXYZ>::Ptr
  preprocess(const pcl::PointCloud<pcl::PointXYZ>::Ptr &cloud) const {
    pcl::PassThrough<pcl::PointXYZ> pass;
    pass.setInputCloud(cloud);
    pass.setFilterFieldName("z");
    pass.setFilterLimits(static_cast<float>(floor_z_ + z_offset_min_),
                         static_cast<float>(floor_z_ + z_offset_max_));
    pcl::PointCloud<pcl::PointXYZ>::Ptr band(
        new pcl::PointCloud<pcl::PointXYZ>);
    pass.filter(*band);

    pcl::PointCloud<pcl::PointXYZ>::Ptr near(
        new pcl::PointCloud<pcl::PointXYZ>);
    near->reserve(band->size());
    const float max_r2 = static_cast<float>(max_range_ * max_range_);
    for (const auto &pt : band->points) {
      if (pt.x * pt.x + pt.y * pt.y <= max_r2) {
        near->push_back(pt);
      }
    }
    if (near->empty()) {
      return near;
    }

    pcl::VoxelGrid<pcl::PointXYZ> vg;
    vg.setInputCloud(near);
    const float leaf = static_cast<float>(leaf_size_);
    vg.setLeafSize(leaf, leaf, leaf);
    pcl::PointCloud<pcl::PointXYZ>::Ptr down(
        new pcl::PointCloud<pcl::PointXYZ>);
    vg.filter(*down);
    return down;
  }

  /// Occupied voxels decay towards 1.0, unobserved ones towards 0.0.
  void
  update_background(const std::unordered_set<VoxelKey, VoxelKeyHash> &occupied,
                    double alpha) {
    for (const auto &key : occupied) {
      auto &score = background_[key];
      score = score * (1.0 - alpha) + alpha;
    }
    for (auto it = background_.begin(); it != background_.end();) {
      if (occupied.find(it->first) == occupied.end()) {
        it->second *= (1.0 - alpha);
        if (it->second < 0.01) {
          it = background_.erase(it);
          continue;
        }
      }
      ++it;
    }
  }

  bool is_static(const VoxelKey &key) const {
    const auto it = background_.find(key);
    return it != background_.end() && it->second >= static_threshold_;
  }

  ClusterShape measure(const pcl::PointCloud<pcl::PointXYZ> &cloud,
                       const pcl::PointIndices &indices) const {
    ClusterShape shape;
    shape.points = indices.indices.size();

    const auto &first = cloud[indices.indices.front()];
    float min_x = first.x, max_x = first.x;
    float min_y = first.y, max_y = first.y;
    float min_z = first.z, max_z = first.z;
    double sx = 0.0, sy = 0.0, sz = 0.0;

    for (const int idx : indices.indices) {
      const auto &pt = cloud[idx];
      min_x = std::min(min_x, pt.x);
      max_x = std::max(max_x, pt.x);
      min_y = std::min(min_y, pt.y);
      max_y = std::max(max_y, pt.y);
      min_z = std::min(min_z, pt.z);
      max_z = std::max(max_z, pt.z);
      sx += pt.x;
      sy += pt.y;
      sz += pt.z;
    }

    const double n = static_cast<double>(shape.points);
    shape.cx = static_cast<float>(sx / n);
    shape.cy = static_cast<float>(sy / n);
    shape.cz = static_cast<float>(sz / n);

    const float dx = max_x - min_x;
    const float dy = max_y - min_y;
    shape.width = std::sqrt(dx * dx + dy * dy);
    shape.height = max_z - min_z;
    shape.range = std::sqrt(shape.cx * shape.cx + shape.cy * shape.cy);

    if (shape.points >= static_cast<std::size_t>(verticality_min_points_)) {
      Eigen::Vector4f centroid;
      Eigen::Matrix3f covariance;
      pcl::computeMeanAndCovarianceMatrix(cloud, indices.indices, covariance,
                                          centroid);
      Eigen::SelfAdjointEigenSolver<Eigen::Matrix3f> solver(covariance);
      // Eigenvalues come out ascending, so the last vector is the long axis.
      shape.verticality = std::abs(solver.eigenvectors().col(2).z());

      const float lam1 = solver.eigenvalues()[2]; // largest (height axis)
      const float lam2 = solver.eigenvalues()[1];
      const float lam3 = solver.eigenvalues()[0]; // smallest
      if (lam1 > 0.0f) {
        shape.eig_width_ratio = lam2 / lam1;
      }
      if (lam2 > 0.0f) {
        shape.eig_depth_ratio = lam3 / lam2;
      }
    } else {
      shape.verticality = 1.0f; // too sparse to judge orientation
    }

    // Vertical slice profile: how the points spread over the height band.
    if (shape.height > 0.0f && n_slices_ > 0) {
      std::vector<int> counts(static_cast<std::size_t>(n_slices_), 0);
      const float inv = static_cast<float>(n_slices_) / shape.height;
      for (const int idx : indices.indices) {
        const auto &pt = cloud[idx];
        int bin = static_cast<int>((pt.z - min_z) * inv);
        bin = std::max(0, std::min(n_slices_ - 1, bin));
        ++counts[static_cast<std::size_t>(bin)];
      }
      int occupied = 0;
      int peak = 0;
      for (const int c : counts) {
        if (c >= slice_min_points_) {
          ++occupied;
        }
        peak = std::max(peak, c);
      }
      shape.slice_occupancy =
          static_cast<float>(occupied) / static_cast<float>(n_slices_);
      if (shape.points > 0) {
        shape.slice_peak =
            static_cast<float>(peak) / static_cast<float>(shape.points);
      }
    }

    return shape;
  }

  bool looks_human(const ClusterShape &shape) const {
    const double r = std::max(1.0, static_cast<double>(shape.range));
    const int needed =
        std::max(min_points_floor_,
                 static_cast<int>(std::lround(points_at_one_meter_ / r)));
    if (static_cast<int>(shape.points) < needed) {
      return false;
    }
    if (shape.height < min_height_m_ || shape.height > max_height_m_) {
      return false;
    }
    if (shape.width < min_width_m_ || shape.width > max_width_m_) {
      return false;
    }
    const double aspect =
        static_cast<double>(shape.height) /
        std::max(static_cast<double>(shape.width), 0.05);
    if (aspect < min_aspect_ratio_) {
      return false;
    }
    if (max_aspect_ratio_ > 0.0 && aspect > max_aspect_ratio_) {
      return false;
    }
    if (max_eig_width_ratio_ > 0.0 &&
        shape.eig_width_ratio > max_eig_width_ratio_) {
      return false;
    }
    if (min_eig_depth_ratio_ > 0.0 &&
        shape.eig_depth_ratio < min_eig_depth_ratio_) {
      return false;
    }
    if (min_slice_occupancy_ > 0.0 &&
        shape.slice_occupancy < min_slice_occupancy_) {
      return false;
    }
    return shape.verticality >= min_verticality_;
  }

  /// Append one labelled feature row to the CSV (for offline tuning / Phase 2
  /// labelling). `human` is the outcome of the current gate, so the dump stays
  /// useful even before a real label set exists.
  void dump_feature(const ClusterShape &shape, bool human) {
    if (!dump_.is_open()) {
      return;
    }
    dump_ << frame_index_ << ',' << (human ? 1 : 0) << ',' << shape.cx << ','
          << shape.cy << ',' << shape.cz << ',' << shape.points << ','
          << shape.range << ',' << shape.height << ',' << shape.width << ','
          << (static_cast<double>(shape.height) /
              std::max(static_cast<double>(shape.width), 0.05))
          << ',' << shape.verticality << ',' << shape.eig_width_ratio << ','
          << shape.eig_depth_ratio << ',' << shape.slice_occupancy << ','
          << shape.slice_peak << '\n';
  }

  /// Match shaped blobs to persistent candidates and report the ones that
  /// have travelled far enough recently to be a walking person.
  std::vector<Candidate *>
  track_motion(const std::vector<ClusterShape> &shapes) {
    std::vector<bool> taken(candidates_.size(), false);

    for (const auto &shape : shapes) {
      int best = -1;
      double best_d = assoc_radius_;
      for (std::size_t i = 0; i < candidates_.size(); ++i) {
        if (taken[i]) {
          continue;
        }
        const double d = std::hypot(shape.cx - candidates_[i].x,
                                    shape.cy - candidates_[i].y);
        if (d < best_d) {
          best_d = d;
          best = static_cast<int>(i);
        }
      }

      if (best < 0) {
        Candidate c;
        c.x = shape.cx;
        c.y = shape.cy;
        c.z = shape.cz;
        c.history.emplace_back(shape.cx, shape.cy);
        candidates_.push_back(std::move(c));
        taken.push_back(true);
      } else {
        Candidate &c = candidates_[static_cast<std::size_t>(best)];
        c.x = shape.cx;
        c.y = shape.cy;
        c.z = shape.cz;
        c.history.emplace_back(shape.cx, shape.cy);
        while (static_cast<int>(c.history.size()) > history_frames_) {
          c.history.pop_front();
        }
        c.unseen = 0;
        taken[static_cast<std::size_t>(best)] = true;
      }
    }

    for (std::size_t i = 0; i < candidates_.size(); ++i) {
      if (!taken[i]) {
        ++candidates_[i].unseen;
      }
    }
    candidates_.erase(std::remove_if(candidates_.begin(), candidates_.end(),
                                     [this](const Candidate &c) {
                                       return c.unseen > drop_frames_;
                                     }),
                      candidates_.end());

    std::vector<Candidate *> published;
    for (auto &c : candidates_) {
      if (c.unseen > 0) {
        continue;
      }
      double span = 0.0;
      if (c.history.size() > 1) {
        const auto &origin = c.history.front();
        for (const auto &p : c.history) {
          span = std::max(
              span, static_cast<double>(std::hypot(p.first - origin.first,
                                                   p.second - origin.second)));
        }
      }
      if (span >= move_distance_m_) {
        c.last_move_frame = frame_index_;
      }
      if (frame_index_ - c.last_move_frame <= hold_frames_) {
        published.push_back(&c);
      }
    }
    return published;
  }

  void cloud_cb(const sensor_msgs::msg::PointCloud2::SharedPtr msg) {
    const rclcpp::Time t = this->now();
    if (last_cb_time_.nanoseconds() > 0 &&
        (t - last_cb_time_).seconds() < process_period_) {
      return;
    }
    last_cb_time_ = t;
    ++frame_index_;

    pcl::PointCloud<pcl::PointXYZ>::Ptr cloud(
        new pcl::PointCloud<pcl::PointXYZ>);
    pcl::fromROSMsg(*msg, *cloud);

    geometry_msgs::msg::PoseArray out;
    out.header = msg->header;

    // Phase 1: learn where the floor is before anything else, otherwise the
    // height band and the background model are built around the wrong plane.
    if (!floor_done_) {
      accumulate_floor_hist(*cloud);
      pub_->publish(out);
      if (++floor_seen_ >= floor_frames_) {
        estimate_floor();
        floor_done_ = true;
      }
      RCLCPP_INFO_THROTTLE(get_logger(), *get_clock(), 1000,
                           "locating floor %d/%d", floor_seen_, floor_frames_);
      return;
    }

    pcl::PointCloud<pcl::PointXYZ>::Ptr down = preprocess(cloud);
    if (down->empty()) {
      pub_->publish(out);
      return;
    }

    std::unordered_set<VoxelKey, VoxelKeyHash> occupied;
    occupied.reserve(down->size());
    for (const auto &pt : down->points) {
      occupied.insert(to_key(pt.x, pt.y, pt.z));
    }
    // Phase 2: seed the background model quickly, then settle to a slow rate.
    const bool warming = warmup_seen_ < warmup_frames_;
    update_background(occupied, warming ? bg_warm_alpha_ : bg_alpha_);

    if (warming) {
      ++warmup_seen_;
      pub_->publish(out);
      RCLCPP_INFO_THROTTLE(
          get_logger(), *get_clock(), 1000,
          "warmup %d/%d - learning the static scene (no detections yet)",
          warmup_seen_, warmup_frames_);
      return;
    }

    pcl::PointCloud<pcl::PointXYZ>::Ptr candidates(
        new pcl::PointCloud<pcl::PointXYZ>);
    candidates->reserve(down->size() / 4);
    for (const auto &pt : down->points) {
      if (!is_static(to_key(pt.x, pt.y, pt.z))) {
        candidates->push_back(pt);
      }
    }

    std::vector<ClusterShape> shapes;
    std::size_t n_clusters = 0;
    if (candidates->size() >= static_cast<std::size_t>(min_cluster_size_)) {
      pcl::search::KdTree<pcl::PointXYZ>::Ptr tree(
          new pcl::search::KdTree<pcl::PointXYZ>);
      tree->setInputCloud(candidates);

      std::vector<pcl::PointIndices> cluster_indices;
      pcl::EuclideanClusterExtraction<pcl::PointXYZ> ec;
      ec.setClusterTolerance(cluster_tolerance_);
      ec.setMinClusterSize(min_cluster_size_);
      ec.setMaxClusterSize(max_cluster_size_);
      ec.setSearchMethod(tree);
      ec.setInputCloud(candidates);
      ec.extract(cluster_indices);
      n_clusters = cluster_indices.size();

      for (const auto &cluster : cluster_indices) {
        const ClusterShape shape = measure(*candidates, cluster);
        const bool human = looks_human(shape);
        dump_feature(shape, human);
        if (human) {
          shapes.push_back(shape);
        } else {
          const double r = std::max(1.0, static_cast<double>(shape.range));
          const int needed =
              std::max(min_points_floor_,
                       static_cast<int>(std::lround(points_at_one_meter_ / r)));
          RCLCPP_INFO_THROTTLE(
              get_logger(), *get_clock(), 2000,
              "reject pts=%zu need=%d r=%.1f h=%.2f w=%.2f vert=%.2f "
              "eigW=%.3f eigD=%.3f occ=%.2f peak=%.2f",
              shape.points, needed, shape.range, shape.height, shape.width,
              shape.verticality, shape.eig_width_ratio, shape.eig_depth_ratio,
              shape.slice_occupancy, shape.slice_peak);
        }
      }
    }

    if (require_motion_) {
      for (const Candidate *c : track_motion(shapes)) {
        geometry_msgs::msg::Pose pose;
        pose.position.x = c->x;
        pose.position.y = c->y;
        pose.position.z = c->z;
        pose.orientation.w = 1.0;
        out.poses.push_back(pose);
      }
    } else {
      for (const auto &shape : shapes) {
        geometry_msgs::msg::Pose pose;
        pose.position.x = shape.cx;
        pose.position.y = shape.cy;
        pose.position.z = shape.cz;
        pose.orientation.w = 1.0;
        out.poses.push_back(pose);
      }
    }

    pub_->publish(out);

    if (debug_stats_) {
      RCLCPP_INFO_THROTTLE(get_logger(), *get_clock(), 1000,
                           "cloud=%zu candidates=%zu clusters=%zu "
                           "human_shaped=%zu published=%zu",
                           down->size(), candidates->size(), n_clusters,
                           shapes.size(), out.poses.size());
    }
  }

  bool auto_floor_;
  double floor_z_;
  double floor_search_span_;
  double z_offset_min_;
  double z_offset_max_;
  double max_range_;
  double leaf_size_;
  double bg_voxel_size_;
  int floor_frames_;
  int warmup_frames_;
  double bg_alpha_;
  double bg_warm_alpha_;
  double static_threshold_;
  double cluster_tolerance_;
  int min_cluster_size_;
  int max_cluster_size_;
  double min_height_m_;
  double max_height_m_;
  double min_width_m_;
  double max_width_m_;
  double min_aspect_ratio_;
  double min_verticality_;
  int verticality_min_points_;
  double points_at_one_meter_;
  int min_points_floor_;
  double max_aspect_ratio_;
  double max_eig_width_ratio_;
  double min_eig_depth_ratio_;
  int n_slices_;
  int slice_min_points_;
  double min_slice_occupancy_;
  std::string feature_dump_path_;
  double assoc_radius_;
  int history_frames_;
  double move_distance_m_;
  int hold_frames_;
  int drop_frames_;
  bool require_motion_;
  bool debug_stats_;
  double process_period_;
  rclcpp::Time last_cb_time_{0, 0, RCL_ROS_TIME};

  long frame_index_ = 0;
  int floor_seen_ = 0;
  bool floor_done_ = false;
  int warmup_seen_ = 0;
  std::vector<long> floor_hist_;
  float floor_seen_min_ = kFloorRangeMax;
  float floor_seen_max_ = kFloorRangeMin;

  std::unordered_map<VoxelKey, double, VoxelKeyHash> background_;
  std::vector<Candidate> candidates_;
  std::ofstream dump_;

  rclcpp::Subscription<sensor_msgs::msg::PointCloud2>::SharedPtr sub_;
  rclcpp::Publisher<geometry_msgs::msg::PoseArray>::SharedPtr pub_;
};

int main(int argc, char **argv) {
  rclcpp::init(argc, argv);
  rclcpp::spin(std::make_shared<PersonClusterNode>());
  rclcpp::shutdown();
  return 0;
}