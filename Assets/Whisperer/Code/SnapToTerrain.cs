using UnityEngine;
using Whisperer.VirtualTerrain;
[ExecuteAlways]
public class SnapToTerrain : MonoBehaviour
{
    [SerializeField] private VTerrain _terrain;

    void Update()
    {
        if (!_terrain) return;
        
        Vector3 pos = transform.position;
        float y = _terrain.GetTerrainHeight(pos);
        pos = new Vector3(pos.x, y, pos.z);
        transform.position = _terrain.ClampToTerrain(pos);
    }
}
