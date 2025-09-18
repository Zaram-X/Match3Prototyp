using UnityEngine;
using UnityEditor;

public class Match3EditorWindow : EditorWindow
{
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
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        gridWidth = EditorGUILayout.IntField("Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Height", gridHeight);
        tileSize = EditorGUILayout.FloatField("Tile Size", tileSize);

        GUILayout.Space(10);

        GUILayout.Label("Tile Prefabs", EditorStyles.boldLabel);
        int newSize = EditorGUILayout.IntField("Size", tilePrefab != null ? tilePrefab.Length : 0);

        if (tilePrefab == null || tilePrefab.Length != newSize)
        {
            tilePrefab = new GameObject[newSize];
        }

        for (int i = 0; i < newSize; i++)
        {
            tilePrefab[i] = (GameObject)EditorGUILayout.ObjectField("Prefab " + i, tilePrefab[i], typeof(GameObject), false);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Grid"))
        {
            GenerateGrid();
        }

        if (GUILayout.Button("Clear Grid"))
        {
            ClearGrid();
        }
        GUILayout.Space(10);

        GUILayout.Label("Save/Load", EditorStyles.boldLabel);
        settingsAsset = (Match3Settings)EditorGUILayout.ObjectField("Settings Asset", settingsAsset, typeof(Match3Settings), false);

        if (GUILayout.Button("Save Settings") && settingsAsset != null)
        {
            SaveSettings();
        }
        if (GUILayout.Button("Load Settings") && settingsAsset != null)
        {
            LoadSettings();
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

        GUILayout.Label("Preset Layout", EditorStyles.boldLabel);

        if (settingsAsset != null)
        {
            if (settingsAsset.layout == null ||
                settingsAsset.layout.Length != gridWidth * gridHeight)
            {
                settingsAsset.InitLayout();
            }

            for (int y = gridHeight - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < gridWidth; x++)
                {
                    int index = x + y * gridWidth;
                    settingsAsset.layout[index] = EditorGUILayout.IntField(settingsAsset.layout[index],
                    GUILayout.Width(25));
                }
                GUILayout.EndHorizontal();
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

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject prefab = tilePrefab[Random.Range(0, tilePrefab.Length)];
                Vector3 pos = new Vector3(x * tileSize - offsetX, 0, y * tileSize - offsetY);
                Quaternion rot = Quaternion.Euler(90, 0, 0);

                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                tile.transform.SetParent(parent.transform);
                tile.transform.SetPositionAndRotation(pos, rot);
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

