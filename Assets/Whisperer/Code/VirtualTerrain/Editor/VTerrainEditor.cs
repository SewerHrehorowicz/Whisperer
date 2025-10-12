using System;
using UnityEditor;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    [CustomEditor(typeof(VTerrain))]
    public class VTerrainEditor : Editor
    {
        private VTerrain vTerrain => (VTerrain)target;

        private void CreateHandles<T>(T[] items, Action callback = null) where T : IVTerrainItem
        {
            for (int i = 0; i < items.Length; i++)
            {
                Vector3 worldPos = vTerrain.transform.TransformPoint(items[i].Position);
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

                if (newWorldPos != worldPos)
                {
                    Vector3 newLocalPos = vTerrain.transform.InverseTransformPoint(newWorldPos);
                    items[i].SetPosition(newLocalPos, vTerrain.TerrainSize); // @todo no method, just set property
                    if (callback != null)
                        callback();
                }
            }
        }

        private void OnSceneGUI()
        {
            if (vTerrain.ControlPoints == null) return;

            if (vTerrain.Mode == VTerrain.Modes.Shape)
                CreateHandles(vTerrain.ControlPoints, () => vTerrain.BlendControlPoints());
            if (vTerrain.Mode == VTerrain.Modes.Grass)
                CreateHandles(vTerrain.GrassPatches);
        }
    }
}
