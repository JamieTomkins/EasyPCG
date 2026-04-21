# EasyPCG

## An All-in-One solution to 2D Procedural Content Generation

A MonoGame based 2D game terrain generator which utalises a 3-layered noise system to create a variety of different terrain types.

![image of an example map generated using EasyPCG](https://github.com/JamieTomkins/EasyPCG/blob/main/Images/ExampleMap.png)

USER GUIDE:

EXPRESS DOWNLOAD (recommended)
1. Go to releases page and download latest release (v1.0)
2. Extract ZIP (select extract all)
3. Double click ‘EasyPCG.exe’
4. If Windows shows a popup, click "More Info" and then "Run Anyway."

DEVELOPER DOWNLOAD
If you would like to modify the algorithms, or see how it works:
1. Clone Repo: `git clone https://github.com/JamieTomkins/EasyPCG.git`
2. Ensure .NET 8 SDK is installed
3. Open PCG_2D.csproj in Visual Studio or VS code
4. Build and run (F5)

---

Once downloaded and correctly loaded into your MonoGame solution using the sliders in the UI window you can adjust parameters such as ‘detail’, ‘warp’, ‘low threshold', and ‘high threshold’. 

Adjusting each will do as follows:

* **Detail:** Adjusts ‘roughness’ of terrain. Increasing this value adds high-frequency Perlin noise artifacts, simulating jagged edges, bumps and irregularities. Lowering this will create a smoother landscape
* **Warp:** Controls intensity of the coordinate distortion. Dictating how much Perlin noise ‘swirls’ the base Simplex layer. Higher values create organic, non-linear features like long rivers, while zero creates a uniform, mathematical gradient
* **Low threshold:** Determines the ‘floor’ for mountain generation. Sets the cut-off point for Worley cell values; any cell value falling below this threshold is treated as a high-altitude zone. Increasing the slider expands surface area of mountainous regions.
* **High threshold:** Defines boundaries for cliffs and plateaus. This parameter sets the upper limit for the ‘grasslands’. Values exceeding this threshold are multiplied by a height constant to create a sharp verticality. Adjusting this allows the user to control the prevalence of steep plateaus versus flat plains. 

Once the map looks how you desire you can press ‘randomize seed’ to change what the initial seed is, creating a new completely randomized look to the terrain to make it your own.

To save the map you have created you can press ‘Save New Map’ this will save it locally so you can create more maps without losing the current one. 

To load a saved map, below will be a list of saved maps, find and click the map you want to load. 

To export your map into a JSON file containing the exact parameters, seed and location of each tile, press ‘Export TileMap’.

To find your exported maps press ‘Open Folder’ this will open the folder where your JSON files are exported in your file explorer. 

---

### TROUBLESHOOTING

## Investigating procedural generation techniques for 2D game engines 

A MonoGame based 2D game engine which contains a sample game which will test different types of Proecedural Content Generation (PCG) Techniques

- **By Jamie Tomkins**
