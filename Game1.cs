using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PCG_2D.Engine.Rendering;
using PCG_2D.Generation;

namespace PCG_2D
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D playerTexture;
        //private int mapWidth = 256;
        //private int mapHeight = 256;
        private int tileSize = 8;

        private Dictionary<TileType, Texture2D> tileTextures;
        private TileType[,] tileMap;

        private Vector2 playerPosition = new Vector2(100, 100);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            tileTextures = new Dictionary<TileType, Texture2D>();

            tileTextures[TileType.Water] = MakeTexture(Color.Blue);
            tileTextures[TileType.Sand] = MakeTexture(Color.Yellow);
            tileTextures[TileType.Grass] = MakeTexture(Color.Green);
            tileTextures[TileType.Mountain] = MakeTexture(Color.Gray);

            int seed = 12345;
            PerlinNoise perlin = new PerlinNoise(seed);

            float[,] noise = perlin.GenerateMap(128, 128, 0.05f);

            tileMap = TerrainGenerator.FromNoise(noise);

            playerTexture = MakeTexture(Color.Red);
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, 1, 1);
            tex.SetData(new[] { color });
            return tex;
        }


        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.W)) playerPosition.Y -= 2;
            if (ks.IsKeyDown(Keys.S)) playerPosition.Y += 2;
            if (ks.IsKeyDown(Keys.A)) playerPosition.X -= 2;
            if (ks.IsKeyDown(Keys.D)) playerPosition.X += 2;

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw terrain
            for (int y = 0; y < tileMap.GetLength(1); y++)
            {
                for (int x = 0; x < tileMap.GetLength(0); x++)
                {
                    Texture2D tex = tileTextures[tileMap[x, y]];
                    _spriteBatch.Draw(
                        tex,
                        new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize),
                        Color.White
                    );
                }
            }

            // Draw player (scaled up so it's visible)
            _spriteBatch.Draw(
                playerTexture,
                playerPosition,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                8f,        // SCALE UP PLAYER
                SpriteEffects.None,
                0f
            );

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
