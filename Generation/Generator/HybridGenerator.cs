using System;
using System.Collections.Generic;
using PCG_2D.Generation;

public class HybridGenerator
{
    public static float[,] Generate(int width, int height, int seed, float pDetail, float pWarp, float low, float high)
    {
        // Parameters
        float scale = 0.05f;
        var perlin = new PerlinNoise(seed);
        var simplex = new SimplexNoise(seed + 1);
        var worley = new WorleyNoise(seed, 100, width, height);

        // Generate noise maps from each algorithm
        float[,] perlinMap = perlin.GenerateMap(width, height, scale);
        float[,] simplexMap = simplex.GenerateMap(width, height, scale);
        float[,] worleyMap = worley.GenerateMap(width, height);
        // Combine 
        float[,] finalMap = new float[width, height];

        // Blend the noise maps together
        for (int y = 0; y < height; y++)
        {
            // Apply warping to the coordinates based on Perlin noise
            for (int x = 0; x < width; x++)
            {
                float warpOffset = perlinMap[x, y] * pWarp;

                // Warp the coordinates for the simplex noise based on the Perlin noise
                int warpedX = (int)Math.Clamp(x + warpOffset, 0, width - 1);
                int warpedY = (int)Math.Clamp(y + warpOffset, 0, height - 1);

                // Get the base height from the simplex noise using the warped coordinates
                float basedHeight = simplexMap[warpedX, warpedY];
                float detail = perlinMap[x, y];
                float structure = worleyMap[x, y];

                // Combine the base height with the detail and structure to create the final height value
                float heightValue = basedHeight + detail * pDetail;

                // Adjust the height value based on the structure to create different terrain types
                if (structure < low)
                {
                    heightValue += 0.6f; // Mountains
                }
                else if (structure < high)
                {
                    heightValue *= 1.0f; // Maintain Plains
                }
                else
                {
                    heightValue *= 1.5f; // Flatten plains
                }

                // Clamp the final height value to ensure it stays within a reasonable range
                heightValue = Math.Clamp(heightValue, 0f, 1f);

                finalMap[x, y] = heightValue;
            }
        }
        return finalMap;

    }
}
