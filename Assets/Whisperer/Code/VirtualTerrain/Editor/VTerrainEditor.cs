using UnityEditor;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    [CustomEditor(typeof(VTerrain))]
    public class VTerrainEditor : Editor
    {
        private VTerrain vTerrain => (VTerrain)target;

        private void HandleShapeMode()
        {
            for (int i = 0; i < vTerrain.ControlPoints.Length; i++)
            {
                Vector3 worldPos = vTerrain.transform.TransformPoint(vTerrain.ControlPoints[i].Position);
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

                if (newWorldPos != worldPos)
                {
                    Vector3 newLocalPos = vTerrain.transform.InverseTransformPoint(newWorldPos);
                    vTerrain.ControlPoints[i].SetPosition(newLocalPos, vTerrain.TerrainSize);
                    vTerrain.BlendControlPoints();
                }
            }
        }

        private void OnSceneGUI()
        {
            if (vTerrain.ControlPoints == null) return;

            if (vTerrain.Mode == VTerrain.Modes.Shape)
                HandleShapeMode();
        }
    }
}
