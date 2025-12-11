using Microsoft.Xna.Framework;

namespace PCG_2D.Generation
{
    public enum TileType
    {
        Water,
        Sand,
        Grass,
        Mountain
    }

    public static class TerrainGenerator
    {
        public static TileType[,] FromNoise(float[,] noise)
        {
            int width = noise.GetLength(0);
            int height = noise.GetLength(1);

            TileType[,] map = new TileType[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = noise[x, y];

                    if (v < 0.3f)
                        map[x, y] = TileType.Water;
                    else if (v < 0.5f)
                        map[x, y] = TileType.Sand;
                    else if (v < 0.75f)
                        map[x, y] = TileType.Grass;
                    else
                        map[x, y] = TileType.Mountain;
                }
            }

            return map;
        }
    }
}
