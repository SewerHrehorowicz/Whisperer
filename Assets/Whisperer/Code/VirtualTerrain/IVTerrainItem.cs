using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    public interface IVTerrainItem
    {
        public Vector3 Position { get; }
        public void SetPosition(Vector3 position, XZVector terrainSize);
    }
}