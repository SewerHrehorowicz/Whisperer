using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    [ExecuteAlways]
    [System.Serializable]
    public class VTGrassPatch : MonoBehaviour
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] [Range(1, 20)] private float _radius = 10f;
        [SerializeField] private float _minHeight = 0.25f;
        [SerializeField] private float _maxHeight = 1f;
        [SerializeField] private AnimationCurve _falloffCurve;
        [SerializeField] private VTerrain _virtualTerrain;
        [SerializeField] [Range(0.2f, 1)] private float _density = 1;

        private (Vector3, Vector3)[] _points;
        private Vector3 _prevPos;

        private void GenerateLines(VTerrain terrain = null)
        {
            float density = 1 / _density;//GameConfig.Instance.VTSettings.GrassLinesDensity;
            Vector3 position = transform.position;
            float startZ = position.z - _radius;
            float startFraction = -density - startZ % density;
            startZ += startFraction;
            float remainingWidth = _radius + _radius - startFraction; // real width to divide;
            int linesNum = Mathf.FloorToInt(remainingWidth / density);

            _points = new (Vector3, Vector3)[linesNum];
            for (int i = 0; i < linesNum; i++)
            {
                float h = _radius - startFraction - (i + 1) * density;
                float width = Mathf.Sqrt(_radius * _radius - h * h);

                Vector3 start = new Vector3(position.x - width, 0, startZ + density * (i + 1));
                Vector3 end = new Vector3(position.x + width, 0, startZ + density * (i + 1));
                Debug.Log($"point {start}, {end}");
                _points[i] = (start, end);
            }
        }

        public void SetPosition(Vector3 newPos, XZVector terrainSize)
        {
            _position.x = Mathf.Clamp(newPos.x, 0, terrainSize.x);
            _position.z = Mathf.Clamp(newPos.z, 0, terrainSize.z);
            _position.y = newPos.y;
        }

        private void Update()
        {
            if (_prevPos == transform.position) return;
            _prevPos = transform.position;
            GenerateLines();
        }

        private void OnValidate()
        {
            GenerateLines();
        }

        private void OnDrawGizmos()
        {
            if (_points == null)
                GenerateLines();

            Gizmos.color = Color.blue;
            Debug.LogWarning($"points? {_points.Length}");
            for (int i = 0; i < _points?.Length; i++)
            {
                var pt = _points[i];
                Gizmos.DrawLine(pt.Item1, pt.Item2);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position.AddZ(-_radius), transform.position.AddZ(_radius));
        }
    }
}

