using System;

namespace PCG_2D.Generation
{
    public class PerlinNoise
    {
        private int[] permutation;

        public PerlinNoise(int seed)
        {
            Random rng = new Random(seed);

            permutation = new int[512];
            int[] p = new int[256];

            // Fill p with 0–255
            for (int i = 0; i < 256; i++)
                p[i] = i;

            // Shuffle using Fisher-Yates
            for (int i = 255; i > 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                int temp = p[i];
                p[i] = p[swapIndex];
                p[swapIndex] = temp;
            }

            // Duplicate the permutation table
            for (int i = 0; i < 512; i++)
                permutation[i] = p[i & 255];
        }

        private static float Fade(float t)
            => t * t * t * (t * (t * 6 - 15) + 10);

        private static float Lerp(float a, float b, float t)
            => a + t * (b - a);

        private static float Grad(int hash, float x, float y)
        {
            int h = hash & 3;
            float u = (h < 2) ? x : y;
            float v = (h < 2) ? y : x;
            return ((h & 1) == 0 ? u : -u) +
                   ((h & 2) == 0 ? v : -v);
        }

        public float Noise(float x, float y)
        {
            int X = (int)MathF.Floor(x) & 255;
            int Y = (int)MathF.Floor(y) & 255;

            x -= MathF.Floor(x);
            y -= MathF.Floor(y);

            float u = Fade(x);
            float v = Fade(y);

            int aa = permutation[X + permutation[Y]];
            int ab = permutation[X + permutation[Y + 1]];
            int ba = permutation[X + 1 + permutation[Y]];
            int bb = permutation[X + 1 + permutation[Y + 1]];

            float lerp1 = Lerp(Grad(aa, x, y), Grad(ba, x - 1, y), u);
            float lerp2 = Lerp(Grad(ab, x, y - 1), Grad(bb, x - 1, y - 1), u);

            return (Lerp(lerp1, lerp2, v) + 1) * 0.5f;  // remap to 0–1
        }

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
