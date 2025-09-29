using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Settings Asset")]
    public Match3Settings settings;
    private GameObject[,] gridArray;

    private Tile firstSelected;
    private Tile secondSelected;

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

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                Debug.Log($"✅ Clicked on tile {tile.name} at ({tile.x}, {tile.y})");
                SelectTile(tile);
            }
        }
    }
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

                //Add tie component
                Tile tileComponent = spawnedTile.GetComponent<Tile>();
                if (tileComponent == null)
                {
                    tileComponent = spawnedTile.AddComponent<Tile>();
                }

                tileComponent.x = x;
                tileComponent.y = y;
                tileComponent.gridManager = this; // make sure reference is set
            }
        }

        // Center the grid in the scene
        transform.position = new Vector3(
            -(settings.gridWidth * settings.tileSize) / 2f + settings.tileSize / 2f,
            0,
            -(settings.gridHeight * settings.tileSize) / 2f + settings.tileSize / 2f
        );
    }

    //Handle tile clicks
    public void SelectTile(Tile tile)
    {
        if (firstSelected == null)
        {
            firstSelected = tile;
             HighlightTile(tile, true);
             Debug.Log("First tile selected: " + tile.name + $" ({tile.x}, {tile.y})");
        }
        else if (secondSelected == null)
        {
            secondSelected = tile;
            HighlightTile(tile, true);
            Debug.Log("Second tile selected: " + tile.name + $" ({tile.x}, {tile.y})");
            TrySwap();
        }
    }

   private void TrySwap()
{
    if (AreNeighbors(firstSelected, secondSelected))
    {
        Debug.Log("Tiles are neighbors → attempting swap.");

        // Swap temporarily
        SwapTiles(firstSelected, secondSelected);

        // Check if this creates a match
        if (CheckForMatches())
        {
            Debug.Log("✅ Swap created a match!");
            // Keep the swap (matches will be cleared automatically)
            HandleMatches();
        }
        else
        {
            Debug.Log("❌ No match → swapping back.");
            // Undo the swap
            SwapTiles(firstSelected, secondSelected);
        }
    }
    else
    {
        Debug.Log("Tiles are not neighbors → cannot swap.");
    }

    // Reset highlights
    HighlightTile(firstSelected, false);
    HighlightTile(secondSelected, false);

    firstSelected = null;
    secondSelected = null;
}


 private bool AreNeighbors(Tile a, Tile b)
    {
        return (Mathf.Abs(a.x - b.x) == 1 && a.y == b.y) ||
               (Mathf.Abs(a.y - b.y) == 1 && a.x == b.x);
    }

 private void SwapTiles(Tile a, Tile b)
    {
        // Swap world positions
        Vector3 tempPos = a.transform.position;
        a.transform.position = b.transform.position;
        b.transform.position = tempPos;

        // Swap grid coords
        int tempX = a.x;
        int tempY = a.y;
        a.x = b.x;
        a.y = b.y;
        b.x = tempX;
        b.y = tempY;

        // Update grid array
        gridArray[a.x, a.y] = a.gameObject;
        gridArray[b.x, b.y] = b.gameObject;
    }

// Returns true if any matches exist on the board
private bool CheckForMatches()
{
    for (int x = 0; x < settings.gridWidth; x++)
    {
        for (int y = 0; y < settings.gridHeight; y++)
        {
            if (HasMatchAt(x, y))
                return true;
        }
    }
    return false;
}

// Checks only around a given tile
private bool HasMatchAt(int x, int y)
{
    if (gridArray[x, y] == null) return false;

    Tile center = gridArray[x, y].GetComponent<Tile>();

    // Horizontal check
    int left = x - 1, right = x + 1, count = 1;
    while (left >= 0 && gridArray[left, y] != null &&
           gridArray[left, y].GetComponent<Tile>().tileType == center.tileType)
    {
        count++; left--;
    }
    while (right < settings.gridWidth && gridArray[right, y] != null &&
           gridArray[right, y].GetComponent<Tile>().tileType == center.tileType)
    {
        count++; right++;
    }
    if (count >= 3) return true;

    // Vertical check
    int down = y - 1, up = y + 1; count = 1;
    while (down >= 0 && gridArray[x, down] != null &&
           gridArray[x, down].GetComponent<Tile>().tileType == center.tileType)
    {
        count++; down--;
    }
    while (up < settings.gridHeight && gridArray[x, up] != null &&
           gridArray[x, up].GetComponent<Tile>().tileType == center.tileType)
    {
        count++; up++;
    }
    if (count >= 3) return true;

    return false;
}

private List<Tile> FindAllMatches()
{
    List<Tile> matches = new List<Tile>();

    // Horizontal check
    for (int y = 0; y < settings.gridHeight; y++)
    {
        int matchCount = 1;
        for (int x = 1; x < settings.gridWidth; x++)
        {
            Tile current = gridArray[x, y]?.GetComponent<Tile>();
            Tile previous = gridArray[x - 1, y]?.GetComponent<Tile>();

            if (current != null && previous != null && current.tileType == previous.tileType)
            {
                matchCount++;
                if (x == settings.gridWidth - 1 && matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                        matches.Add(gridArray[x - i, y].GetComponent<Tile>());
                }
            }
            else
            {
                if (matchCount >= 3)
                {
                    for (int i = 1; i <= matchCount; i++)
                        matches.Add(gridArray[x - i, y].GetComponent<Tile>());
                }
                matchCount = 1;
            }
        }
    }

    // Vertical check
    for (int x = 0; x < settings.gridWidth; x++)
    {
        int matchCount = 1;
        for (int y = 1; y < settings.gridHeight; y++)
        {
            Tile current = gridArray[x, y]?.GetComponent<Tile>();
            Tile previous = gridArray[x, y - 1]?.GetComponent<Tile>();

            if (current != null && previous != null && current.tileType == previous.tileType)
            {
                matchCount++;
                if (y == settings.gridHeight - 1 && matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                        matches.Add(gridArray[x, y - i].GetComponent<Tile>());
                }
            }
            else
            {
                if (matchCount >= 3)
                {
                    for (int i = 1; i <= matchCount; i++)
                        matches.Add(gridArray[x, y - i].GetComponent<Tile>());
                }
                matchCount = 1;
            }
        }
    }

    return matches;
}

private void HandleMatches()
{
    List<Tile> tilesToClear;
    do
    {
        tilesToClear = FindAllMatches();
        if (tilesToClear.Count > 0)
        {
            ClearTiles(tilesToClear);
            DropTiles();
        }
    } while (tilesToClear.Count > 0); // keep checking after refill
}



private void ClearTiles(List<Tile> tilesToClear)
{
    foreach (Tile tile in tilesToClear)
    {
        gridArray[tile.x, tile.y] = null; // free the grid cell
        Destroy(tile.gameObject);         // remove the tile
    }

}
private void DropTiles()
{
    for (int x = 0; x < settings.gridWidth; x++)
    {
        for (int y = 0; y < settings.gridHeight; y++)
        {
            if (gridArray[x, y] == null) // empty cell
            {
                // Look upwards for next filled tile
                for (int ny = y + 1; ny < settings.gridHeight; ny++)
                {
                    if (gridArray[x, ny] != null)
                    {
                        // Move tile down
                        GameObject fallingTile = gridArray[x, ny];
                        gridArray[x, y] = fallingTile;
                        gridArray[x, ny] = null;

                        Tile tileComponent = fallingTile.GetComponent<Tile>();
                        tileComponent.y = y;

                        fallingTile.transform.position = new Vector3(
                            x * settings.tileSize, 0, y * settings.tileSize
                        );
                        break;
                    }
                }
            }
        }
    }

    FillEmptyTiles();
}

private void FillEmptyTiles()
{
    for (int x = 0; x < settings.gridWidth; x++)
    {
        for (int y = 0; y < settings.gridHeight; y++)
        {
            if (gridArray[x, y] == null)
            {
                GameObject tilePrefab = settings.tilePrefabs[Random.Range(0, settings.tilePrefabs.Length)];
                Vector3 position = new Vector3(x * settings.tileSize, 0, y * settings.tileSize);
                Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);

                GameObject newTile = Instantiate(tilePrefab, position, rotation, transform);
                gridArray[x, y] = newTile;

                Tile tileComponent = newTile.GetComponent<Tile>();
                if (tileComponent == null)
                {
                    tileComponent = newTile.AddComponent<Tile>();
                }

                tileComponent.x = x;
                tileComponent.y = y;
                tileComponent.gridManager = this;
                tileComponent.originalColor = newTile.GetComponent<Renderer>().material.color;
            }
        }
    }
}





    private void HighlightTile(Tile tile, bool highlight)
{
        var renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (highlight)
            {
                renderer.material.color = Color.pink; // highlight color
            }
            else
            {
                renderer.material.color = tile.originalColor; // revert to original color
            }
        }
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
