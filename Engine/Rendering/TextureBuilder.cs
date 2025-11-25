using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PCG_2D.Engine.Rendering
{
    public static class TextureBuilder
    {
        public static Texture2D FromColorArray(GraphicsDevice graphicsDevice, Color[] pixels, int width, int height)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(pixels);
            return texture;
        }
    }
}