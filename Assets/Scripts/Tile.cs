using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public GridManager gridManager;
    public string tileType;

    [HideInInspector] public Color originalColor; // store the original color
  

    void Awake()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalColor = renderer.material.color; // save starting color
        }

        tileType = gameObject.name.Replace("(Clone)", ""); // set type based on prefab name
    }
}
