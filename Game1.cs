using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PCG_2D.Engine.Rendering;
using PCG_2D.Generation;
using ImGuiNET;
using Newtonsoft.Json;
using System.IO;

namespace PCG_2D
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ImGuiRenderer _imGuiRenderer;

        // Map Sizes for Testing
        private int testSize = 256;

        //directory and naming for saves/exports
        private string newSaveName = "MyMap";
        private string saveDirectory = "MapSaves";
        private string exportDirectory = "MapExports";

        //Global tile size for rendering
        private int tileSize = 8;

        private Dictionary<TileType, Texture2D> tileTextures;
        private TileType[,] tileMap;

        private KeyboardState previousKeyboard;

        private SpriteFont font;


        //Parameters for noise generation
        float detailStrength = 0.3f;
        float warpStrength = 10f;
        float lowThresh = 0.4f;
        float highThresh = 0.7f;
        int currentSeed = 12345;

        // Constructor
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        // Initialization of the game, including setting up ImGui and ensuring save/export directories exist
        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            if (!System.IO.Directory.Exists(saveDirectory))
                System.IO.Directory.CreateDirectory(saveDirectory);

            if (!System.IO.Directory.Exists(exportDirectory))
                System.IO.Directory.CreateDirectory(exportDirectory);

            tileMap = new TileType[testSize, testSize];

            base.Initialize();
        }

        // Core method to generate the map based on current parameters
        private void GenerateMap()
        {
            Stopwatch sw = Stopwatch.StartNew();

            float[,] noiseMap = HybridGenerator.Generate(
                testSize,
                testSize,
                currentSeed,
                detailStrength,
                warpStrength,
                lowThresh,
                highThresh
            );

            tileMap = TerrainGenerator.FromNoise(noiseMap);

            sw.Stop();
            Debug.WriteLine($"Scale: {tileMap.GetLength(0)}x{tileMap.GetLength(1)} | Time: {sw.ElapsedMilliseconds}ms");
        }

        protected override void LoadContent()
        {
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            tileTextures = new Dictionary<TileType, Texture2D>();
            
            font = Content.Load<SpriteFont>("Font");

            // Sample color textures for each tile type
            tileTextures[TileType.Water] = MakeTexture(Color.Blue);
            tileTextures[TileType.Sand] = MakeTexture(Color.Yellow);
            tileTextures[TileType.Grass] = MakeTexture(Color.Green);
            tileTextures[TileType.Mountain] = MakeTexture(Color.Gray);

            GenerateMap();

        }

        // Utility method to create a sample texture
        private Texture2D MakeTexture(Color color)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, 1, 1);
            tex.SetData(new[] { color });
            return tex;
        }
      

        protected override void Update(GameTime gameTime)
        {
            // Handle input for adjusting parameters
            detailStrength = Math.Clamp(detailStrength, 0f, 1f);
            warpStrength = Math.Clamp(warpStrength, 0f, 50f);
            lowThresh = Math.Clamp(lowThresh, 0f, 1f);
            highThresh = Math.Clamp(highThresh, 0f, 1f);
   
        }

        protected override void Draw(GameTime gameTime)
        {
            // Background color
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw terrain
            _spriteBatch.Begin();
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
            _spriteBatch.End();

            // Draw GUI
            _imGuiRenderer.BeforeLayout(gameTime);

            ImGui.SetWindowSize(new System.Numerics.Vector2(300, 400), ImGuiCond.FirstUseEver); 

            ImGui.Begin("Map Generator Controls");

            if (ImGui.Button("Randomize Seed"))
            {
                currentSeed = new Random().Next();
                GenerateMap();
            }

            // Slider UIs for adjusting parameters, with immediate map regeneration on change
            if (ImGui.SliderFloat("Detail", ref detailStrength, 0f, 1f)) GenerateMap();
            if (ImGui.SliderFloat("Warp", ref warpStrength, 0f, 50f)) GenerateMap();
            if (ImGui.SliderFloat("Low Threshold", ref lowThresh, 0f, 1f)) GenerateMap();
            if (ImGui.SliderFloat("High Threshold", ref highThresh, 0f, 1f)) GenerateMap();

            ImGui.Separator();

            // Save map
            ImGui.Text("Quick Save");
            ImGui.Text("Save Current Map");

            string nextName = GetNextAvailableName();
            ImGui.Text("Next File: " + nextName);

            if (ImGui.Button("Save New Map"))
            {
                SaveMap(nextName);
            }

            ImGui.Separator();

            // Load maps
            ImGui.Text("Existing Saves");
            bool childOpen = ImGui.BeginChild("SaveList", new System.Numerics.Vector2(0, 150), ImGuiChildFlags.Borders);
            if (childOpen)
            {
                string[] files = System.IO.Directory.GetFiles(saveDirectory, "*.json");
                foreach (string file in files)
                {
                    if (ImGui.Selectable(System.IO.Path.GetFileNameWithoutExtension(file)))
                    {
                        LoadMap(file);
                    }
                }
            }
            ImGui.EndChild();

            ImGui.Separator();

            // Export tilemap
            ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.8f, 0.2f, 1.0f), "EXTERNAL EXPORT");
            ImGui.Text("Exports raw tile layout for other games.");
            if (ImGui.Button("Export TileMap (.json)"))
            {
                ExportTileMap(nextName);
            }

            ImGui.SameLine();

            if (ImGui.Button("Open Folder"))
            {
                OpenExportFolder();
            }

            ImGui.End();

            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        // Data structure for saving map parameters
        private void SaveMap(string fileName)
        {
            var data = new MapSaveData 
            {
                Seed = currentSeed,
                Detail = detailStrength,
                Warp = warpStrength,
                LowThresh = lowThresh,
                HighThresh = highThresh
            };

            string fullPath = System.IO.Path.Combine(saveDirectory, fileName + ".json");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(fullPath, json);
        }

        // Method to load map parameters from a file and regenerate the map
        private void LoadMap(string fullPath)
        {
            // Ensure file check
            if (System.IO.File.Exists(fullPath))
            {
                // Read file and store it 
                string json = System.IO.File.ReadAllText(fullPath);

                // Convert string
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<MapSaveData>(json);

                // Apply values to Game1 variables
                currentSeed = data.Seed;
                detailStrength = data.Detail;
                warpStrength = data.Warp;
                lowThresh = data.LowThresh;
                highThresh = data.HighThresh;

                // Re-run the generator
                GenerateMap();
            }
        }

        private string GetNextAvailableName()
        {
            //example: MapSave_1, MapSave_2, etc.
            int count = 1;
            string fileName = "MapSave_" + count;

            // Keep incrementing 'count' until finding a filename that doesn't exist yet
            while (System.IO.File.Exists(System.IO.Path.Combine(saveDirectory, fileName + ".json")))
            {
                count++;
                fileName = "MapSave_" + count;
            }

            return fileName;
        }

        private void ExportTileMap(string fileName)
        {
            // Capture tile layout
            var exportData = new
            {
                MapName = fileName,
                Width = tileMap.GetLength(0),
                Height = tileMap.GetLength(1),
                TileLayout = tileMap
            };

            // Serialize to JSON and save to export directory
            string fullPath = System.IO.Path.Combine(exportDirectory, fileName + "_layout.json");
            string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            System.IO.File.WriteAllText(fullPath, json);
        }

        private void OpenExportFolder()
        {
            // Open folder in File Explorer
            if (System.IO.Directory.Exists(exportDirectory))
            {
                System.Diagnostics.Process.Start("explorer.exe", exportDirectory);
            }
        }

    }


}
