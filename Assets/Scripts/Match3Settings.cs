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
    
}
