using UnityEngine;

[ExecuteAlways]
public class SwapToTerrain : MonoBehaviour
{
    [SerializeField] private PseudoTerrain _terrain;

    void Update()
    {
        if (!_terrain) return;
        float y = _terrain.GetTerrainHeight(transform.position);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        transform.position = _terrain.ClampToTerrain(transform.position);
    }
}
