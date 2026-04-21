using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PCG_2D
{
    // Integrating ImGui.NET with MonoGame
    public class ImGuiRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;
        private RasterizerState _rasterizerState;

        private ImGuiVertex[] _vertexData;
        private short[] _indexData;

        private Dictionary<IntPtr, Texture2D> _loadedTextures = new();
        private int _textureId = 1;

        private IntPtr _fontTextureId;

        private int lastWheelValue;

        // Constructor initializes ImGui context and sets up rendering resources
        public ImGuiRenderer(Game game)
        {
            _graphicsDevice = game.GraphicsDevice;

            ImGui.CreateContext();
            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            // Set up ImGui style
            _effect = new BasicEffect(_graphicsDevice)
            {
                World = Matrix.Identity,
                View = Matrix.Identity,
                TextureEnabled = true,
                VertexColorEnabled = true
            };

            // Set up rasterizer state for ImGui rendering
            _rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true,
                DepthClipEnable = false
            };
        }

        // Rebuilds the font atlas texture for ImGui and uploads it to the GPU
        public unsafe void RebuildFontAtlas()
        {
            var io = ImGui.GetIO();

            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out _);

            byte[] pixelData = new byte[width * height * 4];
            System.Runtime.InteropServices.Marshal.Copy((IntPtr)pixels, pixelData, 0, pixelData.Length);

            // RGBA to BGRA - Swap the red and blue channels
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte r = pixelData[i];
                byte g = pixelData[i + 1];
                byte b = pixelData[i + 2];
                byte a = pixelData[i + 3];

                pixelData[i] = b;
                pixelData[i + 1] = g;
                pixelData[i + 2] = r;
                pixelData[i + 3] = a;
            }

            // Create a MonoGame texture from the pixel data
            Texture2D texture = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData(pixelData);

            // Bind the texture to ImGui and store the ID
            IntPtr id = BindTexture(texture);

            _fontTextureId = id;
            io.Fonts.SetTexID(id);

            io.Fonts.ClearTexData();
        }

        // Binds a MonoGame Texture2D to ImGui and returns an IntPtr ID for use in ImGui draw calls
        public IntPtr BindTexture(Texture2D texture)
        {
            IntPtr id = (IntPtr)_textureId++;
            _loadedTextures[id] = texture;
            return id;
        }

        // Unbinds a texture from ImGui and disposes of the MonoGame Texture2D
        public void BeforeLayout(GameTime gameTime)
        {
            var io = ImGui.GetIO();

            io.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float w = _graphicsDevice.Viewport.Width;
            float h = _graphicsDevice.Viewport.Height;

            // Avoid rendering when minimized
            if (w <= 0 || h <= 0) return;

            // Update display size and mouse state for ImGui
            io.DisplaySize = new System.Numerics.Vector2(w, h);

            // Update mouse
            var mouse = Mouse.GetState();
            io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);
            io.MouseDown[0] = mouse.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouse.RightButton == ButtonState.Pressed;

            io.MouseWheel = (mouse.ScrollWheelValue - lastWheelValue) / 120f;
            lastWheelValue = mouse.ScrollWheelValue;

            ImGui.NewFrame();
        }

        // Renders ImGui draw data using MonoGame's graphics device
        public void AfterLayout()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

        // Renders the ImGui draw data by setting up the appropriate graphics state and issuing draw calls
        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            _effect.Projection = Matrix.CreateOrthographicOffCenter(
                drawData.DisplayPos.X,
                drawData.DisplayPos.X + drawData.DisplaySize.X,
                drawData.DisplayPos.Y + drawData.DisplaySize.Y,
                drawData.DisplayPos.Y,
                -1f,
                1f
            );

            // Set up graphics device state for ImGui rendering
            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // Iterate through ImGui draw lists and render them
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdLists[n];

                UpdateBuffers(cmdList);

                // Set vertex and index buffers for rendering
                for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                {
                    var drawCmd = cmdList.CmdBuffer[i];

                    Texture2D texture;

                    // Try to get the texture for the current draw command, falling back to the font texture if necessary
                    if (!_loadedTextures.TryGetValue(drawCmd.TextureId, out texture))
                    {
                        if (!_loadedTextures.TryGetValue(_fontTextureId, out texture))
                            continue;
                    }

                    // Calculate the scissor rectangle for the current draw command
                    var clip = drawCmd.ClipRect;
                    var displayPos = drawData.DisplayPos;
                    var scale = drawData.FramebufferScale;

                    int x = (int)((clip.X - displayPos.X) * scale.X);
                    int y = (int)((clip.Y - displayPos.Y) * scale.Y);
                    int w = (int)((clip.Z - clip.X) * scale.X);
                    int h = (int)((clip.W - clip.Y) * scale.Y);

                    // Skip rendering if the scissor rectangle is invalid
                    if (w <= 0 || h <= 0)
                        continue;

                    _graphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);

                    _effect.Texture = texture;

                    // Apply the effect and draw the indexed primitives for the current draw command
                    foreach (var pass in _effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        _graphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            _vertexData,
                            (int)drawCmd.VtxOffset,
                            cmdList.VtxBuffer.Size - (int)drawCmd.VtxOffset,
                            _indexData,
                            (int)drawCmd.IdxOffset,
                            (int)drawCmd.ElemCount / 3
                        );
                    }
                }
            }

            // Reset graphics device
            _graphicsDevice.SetVertexBuffer(null);
        }

        // Updates the vertex and index buffers based on the current ImGui draw list
        private unsafe void UpdateBuffers(ImDrawListPtr cmdList)
        {
            int vCount = cmdList.VtxBuffer.Size;
            int iCount = cmdList.IdxBuffer.Size;

            // Ensure vertex and index buffers are large enough to hold the current draw data
            if (_vertexData == null || _vertexData.Length < vCount)
                _vertexData = new ImGuiVertex[vCount];

            // Ensure index buffer is large enough to hold the current draw data
            if (_indexData == null || _indexData.Length < iCount)
                _indexData = new short[iCount];

            // Convert ImGui vertex data to our custom vertex format, extracting position, UV, and color information
            for (int i = 0; i < vCount; i++)
            {
                var vert = cmdList.VtxBuffer[i];
                uint c = vert.col;

                byte r = (byte)(c & 0xFF);
                byte g = (byte)((c >> 8) & 0xFF);
                byte b = (byte)((c >> 16) & 0xFF);
                byte a = (byte)((c >> 24) & 0xFF);

                _vertexData[i] = new ImGuiVertex
                {
                    Position = new Vector2(vert.pos.X, vert.pos.Y),
                    UV = new Vector2(vert.uv.X, vert.uv.Y),
                    Color = new Color(r, g, b, a)
                };
            }

            // Convert ImGui index data to short format for rendering
            for (int i = 0; i < iCount; i++)
            {
                _indexData[i] = (short)cmdList.IdxBuffer[i];
            }
        }

        // Custom vertex structure for ImGui rendering, containing position, UV coordinates, and color information
        struct ImGuiVertex : IVertexType
        {
            public Vector2 Position;
            public Vector2 UV;
            public Color Color;

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        }
    }
}