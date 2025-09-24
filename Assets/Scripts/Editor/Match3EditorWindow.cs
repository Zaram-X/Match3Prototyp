using UnityEngine;
using UnityEditor;

public class Match3EditorWindow : EditorWindow
{
    private bool showGridSettings = true;
    private bool showPrefabs = true;
    private bool showSaveLoad = true;
    private bool showLayout = true;


    private int gridWidth = 8;
    private int gridHeight = 8;
    private float tileSize = 1f;
    private GameObject[] tilePrefab;

    private Match3Settings settingsAsset;   // Reference to the ScriptableObject

    [MenuItem("Tools/Match 3 Grid Editor")]
    public static void ShowWindow()
    {
        GetWindow<Match3EditorWindow>("Match 3 Grid Editor");
    }

    private void OnGUI()
    {
        //GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        showGridSettings = EditorGUILayout.Foldout(showGridSettings, "Grid Settings", true);
        if (showGridSettings)
        {
            gridWidth = EditorGUILayout.IntField(
             new GUIContent("Width", "Number of columns in the grid"), gridWidth);

            gridHeight = EditorGUILayout.IntField(
                new GUIContent("Height", "Number of rows in the grid"), gridHeight);

            tileSize = EditorGUILayout.FloatField(
                new GUIContent("Tile Size", "Size of each tile in Unity units"), tileSize);

        }

        GUILayout.Space(10);

        //GUILayout.Label("Tile Prefabs", EditorStyles.boldLabel);
        showPrefabs = EditorGUILayout.Foldout(showPrefabs, "Tile Prefabs", true);
        if (showPrefabs)
        {
            int newSize = EditorGUILayout.IntField("Size", tilePrefab != null ? tilePrefab.Length : 0);

            if (tilePrefab == null || tilePrefab.Length != newSize)
            {
                tilePrefab = new GameObject[newSize];
            }

            for (int i = 0; i < newSize; i++)
            {
                tilePrefab[i] = (GameObject)EditorGUILayout.ObjectField(
                 new GUIContent("Tile Prefab " + (i + 1), "Prefab used for tiles in the grid"),
                    tilePrefab[i], typeof(GameObject), false);

            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("Generate Grid", "Spawns a temporary preview grid in the scene")))
        {
            GenerateGrid();
        }

        if (GUILayout.Button(new GUIContent("Clear Grid", "Removes the generated grid from the scene")))
        {
            ClearGrid();
        }

        GUILayout.Space(10);

        //GUILayout.Label("Save/Load", EditorStyles.boldLabel);
        showSaveLoad = EditorGUILayout.Foldout(showSaveLoad, "Save/Load Settings", true);
        if (showSaveLoad)
        {
            settingsAsset = (Match3Settings)EditorGUILayout.ObjectField(
             new GUIContent("Settings Asset", "ScriptableObject that stores grid data"),
                settingsAsset, typeof(Match3Settings), false);


            if (GUILayout.Button("Save Settings") && settingsAsset != null)
            {
                SaveSettings();
            }
            if (GUILayout.Button("Load Settings") && settingsAsset != null)
            {
                LoadSettings();
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Create New Settings Asset"))
        {
            Match3Settings newAsset = ScriptableObject.CreateInstance<Match3Settings>();
            string path = EditorUtility.SaveFilePanelInProject("Save Match3 Settings", "NewMatch3Settings", "asset", "Choose location to save settings");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newAsset, path);
                AssetDatabase.SaveAssets();
                settingsAsset = newAsset;
                Debug.Log("Created new settings asset at " + path);
            }
        }
        GUILayout.Space(10);

        // GUILayout.Label("Preset Layout", EditorStyles.boldLabel);
        showLayout = EditorGUILayout.Foldout(showLayout, "Preset Layout", true);
        if (showLayout)
        {
            if (settingsAsset != null)
            {
                if (settingsAsset.layout == null ||
                    settingsAsset.layout.Length != gridWidth * gridHeight)
                {
                    settingsAsset.InitLayout();
                }

                // Build prefab name list for dropdowns
                string[] prefabNames = new string[(tilePrefab != null ? tilePrefab.Length : 0) + 1];
                prefabNames[0] = "Empty";
                for (int i = 0; i < (tilePrefab != null ? tilePrefab.Length : 0); i++)
                {
                    prefabNames[i + 1] = tilePrefab[i] != null ? tilePrefab[i].name : "Missing Prefab " + i;
                }

                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    GUILayout.BeginHorizontal();
                    for (int x = 0; x < gridWidth; x++)
                    {
                        int index = x + y * gridWidth;
                        int currentValue = settingsAsset.layout[index];

                        // Convert stored value (-1 for empty, 0+ for prefab index)
                        int popupIndex = currentValue + 1;

                        // Show dropdown
                        popupIndex = EditorGUILayout.Popup(popupIndex, prefabNames, GUILayout.Width(70));

                        // Convert back to layout value
                        settingsAsset.layout[index] = popupIndex - 1;

                    }
                    GUILayout.EndHorizontal();
                }
            }
        }


    }

    private void SaveSettings()
    {
        if (settingsAsset != null)
        {
            if (EditorUtility.DisplayDialog("Save Settings",
            "Are you sure you want to overwrite " + settingsAsset.name + "?",
            "Yes", "No"))
            {
                settingsAsset.gridWidth = gridWidth;
                settingsAsset.gridHeight = gridHeight;
                settingsAsset.tileSize = tileSize;
                settingsAsset.tilePrefabs = tilePrefab;

                EditorUtility.SetDirty(settingsAsset);
                AssetDatabase.SaveAssets();
                Debug.Log("Settings saved to " + settingsAsset.name);
            }
        }
        else
        {
            Debug.LogWarning("No Match3Settings asset assigned.");
        }
    }

    private void LoadSettings()
    {
        if (settingsAsset != null)
        {
            gridWidth = settingsAsset.gridWidth;
            gridHeight = settingsAsset.gridHeight;
            tileSize = settingsAsset.tileSize;
            tilePrefab = settingsAsset.tilePrefabs;
            Debug.Log("Settings loaded from " + settingsAsset.name);

            GenerateGrid(); // regenerate grid with loaded settings
        }
        else
        {
            Debug.LogWarning("No Match3Settings asset assigned.");
        }
    }

    private void GenerateGrid()
{
    if (tilePrefab == null || tilePrefab.Length == 0)
    {
        Debug.LogError("Please assign at least one tile prefab.");
        return;
    }

    ClearGrid(); // clear old one first

    GameObject parent = new GameObject("Match3Grid");
    parent.transform.position = Vector3.zero; // keep centered at origin

    float offsetX = (gridWidth - 1) * tileSize / 2f;
    float offsetY = (gridHeight - 1) * tileSize / 2f;

    for (int y = 0; y < gridHeight; y++)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            int index = x + y * gridWidth;
            int layoutValue = (settingsAsset != null && settingsAsset.layout.Length > index)
                ? settingsAsset.layout[index]
                : -1; // default to random if no layout

            if (layoutValue == -2)
            {
                // Empty space → skip
                continue;
            }

            GameObject prefab = null;

            if (layoutValue == -1)
            {
                // Random tile
                prefab = tilePrefab[Random.Range(0, tilePrefab.Length)];
            }
            else if (layoutValue >= 0 && layoutValue < tilePrefab.Length)
            {
                // Specific prefab
                prefab = tilePrefab[layoutValue];
            }
            else
            {
                Debug.LogWarning($"Invalid layout value {layoutValue} at ({x},{y})");
                continue;
            }

            if (prefab != null)
            {
                Vector3 pos = new Vector3(x * tileSize - offsetX, 0, y * tileSize - offsetY);
                Quaternion rot = Quaternion.Euler(90, 0, 0);

                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                tile.transform.SetParent(parent.transform);
                tile.transform.SetPositionAndRotation(pos, rot);
            }
        }
    }

    Selection.activeGameObject = parent;
}




    private void ClearGrid()
    {
        GameObject oldGrid = GameObject.Find("Match3Grid");
        if (oldGrid != null)
        {
            DestroyImmediate(oldGrid);
        }
        else
        {
            Debug.LogWarning("No Match3Grid found to clear.");
        }
    }
}

