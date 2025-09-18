using UnityEngine;

[CreateAssetMenu(fileName = "Match3Settings", menuName = "Scriptable Objects/Match3Settings")]
public class Match3Settings : ScriptableObject
{
    [Header("Grid Settings")]
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float tileSize = 1.0f;

    [Header("Tile")]
    public GameObject[] tilePrefabs;

    [Header("Preset Layout (use -1 for random)")]
    public int[] layout; // prefab index per cell

    public void InitLayout()
    {
        layout = new int[gridWidth * gridHeight];
        for (int i = 0; i < layout.Length; i++)
        {
            layout[i] = -1; // -1 indicates random tile
        }
    }

    public int GetLayoutValue(int x, int y)
    {
        return layout[x + y * gridWidth];
    }

    public void SetLayoutValue(int x, int y, int value)
    {
        layout[x + y * gridWidth] = value;
    }


}
