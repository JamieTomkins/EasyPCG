using System;

namespace PCG_2D.Generation
{
    public class SimplexNoise
    {
        // Gradient vectors for 2D simplex noise
        private static readonly int[][] grad3 =
        {
            new[] {1,1}, new[] {-1,1}, new[] {1,-1}, new[] {-1,-1},
            new[] {1,0}, new[] {-1,0}, new[] {1,0}, new[] {-1,0},
            new[] {0,1}, new[] {0,-1}, new[] {0,1}, new[] {0,-1}
        };

        // Permutation table
        private int[] perm;

        // Constructor to initialize the permutation table with a given seed
        public SimplexNoise(int seed)
        {
            perm = new int[512];
            int[] p = new int[256];

            for (int i = 0; i < 256; i++)
                p[i] = i;

            Random rng = new Random(seed);

            // Fisher-Yates shuffle
            for (int i = 255; i > 0; i--)
            {
                int swap = rng.Next(i + 1);
                int temp = p[i];
                p[i] = p[swap];
                p[swap] = temp;
            }

            // Duplicate the permutation table
            for (int i = 0; i < 512; i++)
                perm[i] = p[i & 255];
        }

        // Dot product function for 2D gradients
        private static float Dot(int[] g, float x, float y)
        {
            return g[0] * x + g[1] * y;
        }

        // 2D simplex noise function
        public float Noise(float xin, float yin)
        {
            // Skewing and unskewing factors for 2D simplex noise
            float F2 = 0.5f * (MathF.Sqrt(3f) - 1f);
            float G2 = (3f - MathF.Sqrt(3f)) / 6f;

            // Skew the input space to determine which simplex cell we're in
            float s = (xin + yin) * F2;
            int i = (int)MathF.Floor(xin + s);
            int j = (int)MathF.Floor(yin + s);

            // Unskew the cell origin back to (x,y) space
            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;

            // The x,y distances from the cell origin
            float x0 = xin - X0;
            float y0 = yin - Y0;

            // Determine which simplex we are in
            int i1, j1;

            // Offsets for the second (middle) corner of simplex in (i,j) coords
            if (x0 > y0)
            {
                i1 = 1; j1 = 0;
            }
            else
            {
                i1 = 0; j1 = 1;
            }

            // Offsets for the middle corner in (x,y) unskewed coords
            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1f + 2f * G2;
            float y2 = y0 - 1f + 2f * G2;

            // Calculate the hashed gradient indices of the three simplex corners
            int ii = i & 255;
            int jj = j & 255;

            // Calculate the contribution from the three corners
            float n0 = 0, n1 = 0, n2 = 0;

            // Calculate the contribution from the first corner
            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 >= 0)
            {
                t0 *= t0;
                int gi0 = perm[ii + perm[jj]] % 12;
                n0 = t0 * t0 * Dot(grad3[gi0], x0, y0);
            }

            // Calculate the contribution from the second corner
            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 >= 0)
            {
                t1 *= t1;
                int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
                n1 = t1 * t1 * Dot(grad3[gi1], x1, y1);
            }

            // Calculate the contribution from the third corner
            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 >= 0)
            {
                t2 *= t2;
                int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
                n2 = t2 * t2 * Dot(grad3[gi2], x2, y2);
            }

            // Add contributions from each corner to get the final noise value.
            return 70f * (n0 + n1 + n2) * 0.5f + 0.5f;
        }

        //  Generate2D noise map 
        public float[,] GenerateMap(int width, int height, float scale)
        {
            float[,] map = new float[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y] = Noise(x * scale, y * scale);
                }
            }

            return map;
        }
    }
}