using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float tileSize = 1.0f;

    [Header("Tiles")]
    [SerializeField] private GameObject[] tilePrefabs;

    private GameObject[,] gridArray;

    void Start()
    {
        gridArray = new GameObject[gridWidth, gridHeight];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Pick a random tile prefab
                GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

                // Position on XZ plane
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);

                // Instantiate and parent to this object
                Quaternion rotation = Quaternion.Euler(90f, 0f, 0f); // 90 on X-axis
                GameObject spawnedTile = Instantiate(tilePrefab, position, rotation);
                spawnedTile.transform.SetParent(transform);

                // Store tile in grid
                gridArray[x, y] = spawnedTile;
            }
        }

        // Center the grid in the scene
        transform.position = new Vector3(
            -gridWidth / 2f + tileSize / 2f,
            0,
            -gridHeight / 2f + tileSize / 2f
        );
    }

    // Draw grid gizmos in Scene view for debugging
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                Gizmos.DrawWireCube(pos, new Vector3(tileSize, 0.1f, tileSize));
            }
        }
    }
}
