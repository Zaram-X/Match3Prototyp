using UnityEngine;
using UnityEditor;

public class Match3EditorWindow : EditorWindow
{
    private int gridWidth = 8;
    private int gridHeight = 8;
    private float tileSize = 1f;
    private GameObject[] tilePrefab;

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

