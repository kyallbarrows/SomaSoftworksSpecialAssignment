import numpy as np
# import matplotlib.pyplot as plt

def generate_clustered_points(
    num_points,
    x_range,
    y_range,
    cluster_centers,
    cluster_std_dev,
    cluster_ratio=0.75,
    random_seed=42
):
    """
    Generates a set of 2D random points with clustering around specified centers.

    Parameters:
    - num_points (int): The total number of points to generate.
    - x_range (tuple): (min_x, max_x) for the overall boundary of the points.
    - y_range (tuple): (min_y, max_y) for the overall boundary of the points.
    - cluster_centers (list of tuples): List of (x, y) coordinates for the cluster centers.
    - cluster_std_dev (float): The standard deviation for the Gaussian distribution around the centers.
    - cluster_ratio (float): The proportion of points that will belong to a cluster (0.0 to 1.0).
    - random_seed (int): Seed for the random number generator for reproducibility.

    Returns:
    - numpy.ndarray: A 2D array of the generated points (shape: num_points, 2).
    """
    np.random.seed(random_seed)
    
    all_points = []
    num_clusters = len(cluster_centers)
    
    # 1. Generate Clustered Points (The Majority)
    num_clustered_points = int(num_points * cluster_ratio)
    
    # Calculate how many points go into each cluster (approximately)
    points_per_cluster = num_clustered_points // num_clusters
    
    for i, center in enumerate(cluster_centers):
        # Determine the number of points for this cluster
        current_points = points_per_cluster
        # Add any remaining points to the last cluster to reach the total
        if i == num_clusters - 1:
            current_points += num_clustered_points % num_clusters
            
        center_x, center_y = center
        
        # Generate points using a Normal (Gaussian) distribution
        # The center of the distribution is (center_x, center_y)
        # The standard deviation controls the spread (tightness) of the cluster
        x_cluster = np.random.normal(loc=center_x, scale=cluster_std_dev, size=current_points)
        y_cluster = np.random.normal(loc=center_y, scale=cluster_std_dev, size=current_points)
        
        cluster_points = np.column_stack((x_cluster, y_cluster))
        all_points.append(cluster_points)

    # 2. Generate Random Noise Points (The Minority)
    num_noise_points = num_points - num_clustered_points
    
    min_x, max_x = x_range
    min_y, max_y = y_range
    
    # Generate points uniformly across the entire range
    x_noise = np.random.uniform(low=min_x, high=max_x, size=num_noise_points)
    y_noise = np.random.uniform(low=min_y, high=max_y, size=num_noise_points)
    
    noise_points = np.column_stack((x_noise, y_noise))
    all_points.append(noise_points)

    # Combine all points into a single NumPy array
    all_points_array = np.concatenate(all_points, axis=0)
    
    # Shuffle the points so the clustered and noise points are mixed
    np.random.shuffle(all_points_array)
    
    return all_points_array, num_clustered_points

# --- Configuration ---
TOTAL_POINTS = 500
X_BOUNDS = (0, 100)
Y_BOUNDS = (0, 100)
CLUSTER_CENTERS = [
    (20, 80),  # Cluster 1 center
    (50, 20),  # Cluster 2 center
    (80, 70)   # Cluster 3 center
]
CLUSTER_STD = 5.0      # Standard deviation (controls the 'tightness' of the cluster)
CLUSTER_PROPORTION = 0.85 # 85% of points will be in a cluster, 15% will be random noise

# --- Generate Data ---
data, num_clustered = generate_clustered_points(
    TOTAL_POINTS,
    X_BOUNDS,
    Y_BOUNDS,
    CLUSTER_CENTERS,
    CLUSTER_STD,
    CLUSTER_PROPORTION
)

# --- Plotting the Data ---
#plt.figure(figsize=(10, 8))
#plt.scatter(data[:, 0], data[:, 1], s=15, alpha=0.7)
#plt.scatter([c[0] for c in CLUSTER_CENTERS], 
##            [c[1] for c in CLUSTER_CENTERS], 
#            marker='X', s=200, color='red', label='Cluster Centers')

#plt.title(f'{TOTAL_POINTS} Random Points with Clustering')
#plt.xlabel('X Coordinate')
#plt.ylabel('Y Coordinate')
#plt.xlim(X_BOUNDS)
#plt.ylim(Y_BOUNDS)
#plt.grid(True, linestyle='--', alpha=0.6)
#plt.legend()
#plt.show()

print(f"Total points generated: {TOTAL_POINTS}")
print(f"Points in clusters: {num_clustered}")
print(f"Noise points: {TOTAL_POINTS - num_clustered}")
print(f"Shape of the final data array: {data.shape}")

for i, thisPoint in enumerate(data): 
    print(f"Here's a point: {data[i][0]},{data[i][1]}")