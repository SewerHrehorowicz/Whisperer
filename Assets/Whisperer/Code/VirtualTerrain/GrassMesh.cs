using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisperer.MeshHelpers;

namespace Whisperer.VirtualTerrain
{
    [System.Serializable]
    public class GrassMesh
    {
        [SerializeField] private Vector3 _start = Vector3.zero;
        [SerializeField] private Vector3 _end = new Vector3(2, 1, 0);
        [SerializeField] private Mesh _mesh;

        public Mesh Mesh => _mesh;

        private void GenerateMesh(VTerrain terrain)
        {
            var line = Drawing.CreateLine(_start, _end, 1);

            for (int i = 0; i < line.Length; i++)
            {
                var pt = line[i];
                line[i].y = terrain.GetHeightAt(pt);
            }

            var mesh = Drawing.CreateLineMesh(line, 1, 1);
            _mesh = mesh;
        }
    }
}