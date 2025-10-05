using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PseudoTerrain))]
public class PseudoTerrainEditor : Editor
{
    private void OnSceneGUI()
    {
        PseudoTerrain terrain = (PseudoTerrain)target;

        if (terrain.ControlPoints == null) return;

        for (int i = 0; i < terrain.ControlPoints.Length; i++)
        {
            Vector3 worldPos = terrain.transform.TransformPoint(terrain.ControlPoints[i].Position);

            // Handle przesuwania
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