using System;
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
        [SerializeField] private VTerrain _vTerrain;
        [SerializeField] [Range(0.2f, 1)] private float _density = 1;

        private (Vector3, Vector3)[] _points;
        private Vector3 _prevPos;

        private void GenerateCircleSections(VTerrain terrain = null)
        {
            terrain ??= _vTerrain;
            float density = 1 / _density; // GameConfig.Instance.VTSettings.GrassLinesDensity;
            Vector3 position = transform.position;
            float startZ = position.z - _radius;
            float startOffset = -density - startZ % density;
            startZ += startOffset;
            float remainingWidth = _radius + _radius - startOffset; // real width to divide;
            int linesNum = Mathf.FloorToInt(remainingWidth / density);

            _points = new (Vector3, Vector3)[linesNum];

            for (int i = 0; i < linesNum; i++)
            {
                float densityOffset = (i + 1) * density;
                float height = _radius - startOffset - densityOffset;
                float width = Mathf.Sqrt(_radius * _radius - height * height);
                float z = startZ + densityOffset;

                Vector3 start = new Vector3(position.x - width, 0, z);
                Vector3 end = new Vector3(position.x + width, 0, z);
                
                if (terrain != null)
                {
                    if (!terrain.AnyWithinBounds(start, end))
                        continue;
                    start = terrain.ClampToTerrain(start);
                    end = terrain.ClampToTerrain(end);
                }

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
            GenerateCircleSections();
        }

        private void OnValidate() => GenerateCircleSections();

        private void OnDrawGizmos()
        {
            if (_points == null)
                GenerateCircleSections();

            Gizmos.color = Color.blue;
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

