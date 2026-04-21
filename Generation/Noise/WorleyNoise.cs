using System;
using Microsoft.Xna.Framework;

namespace PCG_2D.Generation
{
    // A simple implementation of Worley noise (cellular noise) for 2D maps.
    public class WorleyNoise
    {
        // Feature points that define the noise pattern
        private Vector2[] featurePoints;
        private int pointCount;

        // Constructor to initialize the feature points based on a seed for reproducibility
        public WorleyNoise(int seed, int pointCount, int width, int height)
        {
            // Store the number of feature points and initialize the array
            this.pointCount = pointCount;
            featurePoints = new Vector2[pointCount];

            // Generate random feature points within the specified width and height using seed
            Random rng = new Random(seed);

            // Populate the feature points with random positions
            for (int i = 0; i < pointCount; i++)
            {
                // Each feature point is assigned a random position within the map bounds
                featurePoints[i] = new Vector2(
                    (float)rng.NextDouble() * width,
                    (float)rng.NextDouble() * height
                );
            }
        }

        // Generate a 2D map of Worley noise values based on the distances to the nearest feature points
        public float[,] GenerateMap(int width, int height)
        {
            // Create a 2D array to hold the noise values for each point in the map
            float[,] map = new float[width, height];

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            // First pass: calculate distances
            for (int y = 0; y < height; y++)
            {
                // For each point in the map, calculate the distance to the nearest feature point
                for (int x = 0; x < width; x++)
                {
                    float minDist = float.MaxValue;

                    // Iterate through all feature points to find the minimum distance to the current point (x, y)
                    foreach (var point in featurePoints)
                    {
                        float dx = x - point.X;
                        float dy = y - point.Y;

                        float dist = MathF.Sqrt(dx * dx + dy * dy);

                        if (dist < minDist)
                            minDist = dist;
                    }

                    // Store the minimum distance in the map
                    map[x, y] = minDist;

                    // Update the minimum and maximum distance values for normalization
                    if (minDist < minValue) minValue = minDist;
                    if (minDist > maxValue) maxValue = minDist;
                }
            }

            // Second pass: normalize properly
            for (int y = 0; y < height; y++)
            {
                // Normalize the distance values to the range [0, 1] based on the minimum and maximum distances found
                for (int x = 0; x < width; x++)
                {
                    map[x, y] = (map[x, y] - minValue) / (maxValue - minValue);
                }
            }

            return map;
        }
    }
}