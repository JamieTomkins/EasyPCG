using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PCG_2D.Engine.Rendering;

namespace PCG_2D
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D testTexture;
        private int mapWidth = 256;
        private int mapHeight = 256;

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

            Random rng = new Random();

            Color[] pixelData = new Color[mapWidth * mapHeight];

            for (int i = 0; i < pixelData.Length; i++)
            {
                byte value = (byte)rng.Next(0, 256);

                pixelData[i] = new Color(value, value, value); 
            }

            testTexture = TextureBuilder.FromColorArray(GraphicsDevice, pixelData, mapWidth, mapHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.R))
            {
                Random rng = new Random();

                Color[] pixelData = new Color[mapWidth * mapHeight];

                for (int i = 0; i < pixelData.Length; i++)
                {
                    byte value = (byte)rng.Next(0, 255);
                    pixelData[i] = new Color(value, value, value);
                }

                testTexture = TextureBuilder.FromColorArray(GraphicsDevice, pixelData, mapWidth, mapHeight);
            }

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(testTexture, new Vector2(0, 0), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
