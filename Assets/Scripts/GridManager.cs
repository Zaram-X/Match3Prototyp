using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Settings Asset")]
    public Match3Settings settings;
    private GameObject[,] gridArray;

    void Start()
    {
        if (settings == null)
        {
            Debug.LogError("Settings asset not assigned in GridManager.");
            return;
        }

        gridArray = new GameObject[settings.gridWidth, settings.gridHeight];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < settings.gridWidth; x++)
        {
            for (int y = 0; y < settings.gridHeight; y++)
            {
                int prefabIndex = -1;
                if (settings.layout != null && settings.layout.Length == settings.gridWidth *
                settings.gridHeight)
                {
                    prefabIndex = settings.GetLayoutValue(x, y);
                }

                GameObject tilePrefab;
                if (prefabIndex >= 0 && prefabIndex < settings.tilePrefabs.Length)
                    tilePrefab = settings.tilePrefabs[prefabIndex]; // fixed prefab
                else
                    tilePrefab = settings.tilePrefabs[Random.Range(0, settings.tilePrefabs.Length)];

                Vector3 position = new Vector3(x * settings.tileSize, 0, y * settings.tileSize);
                Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);

                GameObject spawnedTile = Instantiate(tilePrefab, position, rotation, transform);
                gridArray[x, y] = spawnedTile;
            }
        }

        // Center the grid in the scene
        transform.position = new Vector3(
            -(settings.gridWidth * settings.tileSize) / 2f + settings.tileSize / 2f,
            0,
            -(settings.gridHeight * settings.tileSize) / 2f + settings.tileSize / 2f
        );
    }

    // Draw grid gizmos in Scene view for debugging
    void OnDrawGizmos()
    {
        if (settings == null)
            return;

        if (settings.gridWidth <= 0 || settings.gridHeight <= 0 || settings.tileSize <= 0f)
            return;

        Gizmos.color = Color.white;
        for (int x = 0; x < settings.gridWidth; x++)
        {
            for (int y = 0; y < settings.gridHeight; y++)
            {
                Vector3 pos = new Vector3(x * settings.tileSize, 0, y * settings.tileSize);
                Gizmos.DrawWireCube(pos, new Vector3(settings.tileSize, 0.1f, settings.tileSize));
            }
        }
    }
}
