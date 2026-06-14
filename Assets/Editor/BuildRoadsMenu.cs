using UnityEngine;
using UnityEditor;

public class BuildRoadsMenu
{
    [MenuItem("Tools/Build Dirt Roads")]
    public static void BuildRoads()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active terrain found in the scene.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        // 1. Setup Terrain Layers if needed
        if (terrainData.terrainLayers == null || terrainData.terrainLayers.Length < 3)
        {
            string grassPath = "Assets/Idyllic Fantasy Nature/Textures/Ground/Grass/Grass_Albedo.png";
            string cityGroundPath = "Assets/Naganeupseong/Resource/Textures/Ground/T_Ground01a_BC.png";
            string roadPath = "Assets/Namhansanseong/Textures/Landscape/Road.png";

            Texture2D grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>(grassPath);
            Texture2D cityGroundTex = AssetDatabase.LoadAssetAtPath<Texture2D>(cityGroundPath);
            Texture2D roadTex = AssetDatabase.LoadAssetAtPath<Texture2D>(roadPath);

            if (grassTex == null || roadTex == null || cityGroundTex == null)
            {
                Debug.LogError("Could not find required ground textures.");
                return;
            }

            TerrainLayer grassLayer = new TerrainLayer();
            grassLayer.diffuseTexture = grassTex;
            grassLayer.tileSize = new Vector2(15, 15);

            TerrainLayer cityGroundLayer = new TerrainLayer();
            cityGroundLayer.diffuseTexture = cityGroundTex;
            cityGroundLayer.tileSize = new Vector2(15, 15);

            TerrainLayer roadLayer = new TerrainLayer();
            roadLayer.diffuseTexture = roadTex;
            roadLayer.tileSize = new Vector2(10, 10);

            terrainData.terrainLayers = new TerrainLayer[] { grassLayer, cityGroundLayer, roadLayer };
        }

        // 2. Paint Alphamaps
        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

        // Define road parameters
        float roadWidth = 2.5f;
        float fadeWidth = 1.5f;
        
        // Terrain position and size
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Convert splatmap coordinate to world coordinate
                float worldX = terrainPos.x + ((float)x / mapWidth) * terrainSize.x;
                float worldZ = terrainPos.z + ((float)y / mapHeight) * terrainSize.z;

                float distToRoad = float.MaxValue;
                float radius = Mathf.Sqrt(worldX * worldX + worldZ * worldZ);

                float maxDistFromCenter = Mathf.Max(Mathf.Abs(worldX), Mathf.Abs(worldZ));
                float grassWeight = 1f;
                float cityWeight = 0f;
                float roadWeight = 0f;

                // Only paint roads and city ground within the square fortress
                if (maxDistFromCenter < 60f)
                {
                    cityWeight = 1f;
                    grassWeight = 0f;

                    // Blend softly at the boundary
                    if (maxDistFromCenter > 55f)
                    {
                        grassWeight = (maxDistFromCenter - 55f) / 5f;
                        cityWeight = 1f - grassWeight;
                    }

                    // A. Main vertical road (North-South)
                    distToRoad = Mathf.Min(distToRoad, Mathf.Abs(worldX));

                    // B. Main horizontal road (East-West)
                    distToRoad = Mathf.Min(distToRoad, Mathf.Abs(worldZ));

                    // C. Grid secondary roads
                    float[] gridLines = { -48f, -24f, 24f, 48f };
                    foreach (float line in gridLines)
                    {
                        // Vertical grid lines
                        distToRoad = Mathf.Min(distToRoad, Mathf.Abs(worldX - line));
                        // Horizontal grid lines
                        distToRoad = Mathf.Min(distToRoad, Mathf.Abs(worldZ - line));
                    }
                    
                    if (distToRoad < roadWidth)
                    {
                        roadWeight = 1f;
                    }
                    else if (distToRoad < roadWidth + fadeWidth)
                    {
                        roadWeight = 1f - ((distToRoad - roadWidth) / fadeWidth);
                    }

                    // Road weight overrides city weight, but leaves grass alone
                    cityWeight *= (1f - roadWeight);
                }

                // Apply to splatmap
                splatmapData[x, y, 0] = grassWeight; // Grass
                splatmapData[x, y, 1] = cityWeight;  // City Dirt
                splatmapData[x, y, 2] = roadWeight;  // Dirt Road
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);

        Debug.Log("Dirt Roads painted successfully onto the Terrain!");
    }
}
