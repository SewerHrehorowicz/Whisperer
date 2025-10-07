using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    [System.Serializable]
    public class VTSettings
    {
        [SerializeField] private int _pointsPerUnit = 5;
        [SerializeField] private float _grassMeshDensity = 1;
        [SerializeField] private float _grassLinesDensity = 1f; // how many lines in depth per world unit

        public float GrassMeshDensity => _grassMeshDensity;
        public float GrassLinesDensity => _grassLinesDensity;
        public int PointsPerUnit => _pointsPerUnit;
    }
}

