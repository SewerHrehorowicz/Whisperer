using UnityEditor;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    [CustomEditor(typeof(VTerrain))]
    public class VTerrainEditor : Editor
    {
        private void OnSceneGUI()
        {
            VTerrain terrain = (VTerrain)target;

            if (terrain.ControlPoints == null) return;

            for (int i = 0; i < terrain.ControlPoints.Length; i++)
            {
                Vector3 worldPos = terrain.transform.TransformPoint(terrain.ControlPoints[i].Position);
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

                if (newWorldPos != worldPos)
                {
                    Vector3 newLocalPos = terrain.transform.InverseTransformPoint(newWorldPos);
                    terrain.ControlPoints[i].SetPosition(newLocalPos, terrain.TerrainSize);
                    terrain.BlendControlPoints();
                }
            }
        }
    }
}
