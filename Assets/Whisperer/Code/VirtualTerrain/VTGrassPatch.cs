using System;
using System.Collections.Generic;
using UnityEngine;
using Whisperer.MeshHelpers;

namespace Whisperer.VirtualTerrain
{
    [ExecuteAlways]
    [Serializable]
    public class VTGrassPatch : MonoBehaviour, IPoolUser
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] [Range(1, 20)] private float _radius = 10f;
        [SerializeField] [Range(1, 5)] private float _maxHeight = 1f;
        [SerializeField] private AnimationCurve _falloffCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private VTerrain _vTerrain;
        [SerializeField] [Range(0.2f, 1)] private float _density = 1;
        [SerializeField] [Range(0.2f, 2)] private float _lineDensity = 1;
        [SerializeField] private float _minWidth = 0.5f;

        private List<(Vector3, Vector3)> _circleSections = new();
        private Vector3 _prevPos;

        public ObjectPool Pool => GameConfig.Instance.GrassPool;
        public List<GameObject> Borrowed { get; private set; }

        private void DrawCircleMeshes(VTerrain terrain)
        {
            Borrowed = Pool.BorrowItems(_circleSections.Count, this);
            for (int i = 0; i < _circleSections.Count; i++)
            {
                (Vector3, Vector3) section = _circleSections[i];
                Vector3[] line = Drawing.CreateLine(section.Item1, section.Item2, _lineDensity);

                Debug.Log($"Line length for section {i} of {name}: {line.Length}");
                for (int j = 0; j < line.Length; j++)
                {
                    var pt = line[j];
                    line[j].y = terrain.GetHeightAt(pt);
                }

                float height = _falloffCurve.Evaluate((float)i / _circleSections.Count) * _maxHeight;

                var mesh = Drawing.CreateLineMesh(line, height, 1, _falloffCurve);
                Borrowed[i].GetComponent<MeshFilter>().mesh = mesh;
            }
        }

        private void GenerateCircleSections(VTerrain terrain = null)
        {
            terrain ??= _vTerrain;
            float density = 1 / _density; // GameConfig.Instance.VTSettings.GrassLinesDensity;
            Vector3 position = transform.position;
            float startZ = position.z - _radius;
            float startOffset = -density - startZ % density;
            startZ += startOffset;
            float remainingWidth = _radius + _radius - startOffset;
            int linesNum = Mathf.FloorToInt(remainingWidth / density);
            bool hasTerrain = terrain != null;

            _circleSections.Clear();

            for (int i = 0; i < linesNum; i++)
            {
                float densityOffset = (i + 1) * density;
                float height = _radius - startOffset - densityOffset;
                float width = Mathf.Sqrt(_radius * _radius - height * height);
                float z = startZ + densityOffset;

                Vector3 start = new Vector3(position.x - width, 0, z);
                Vector3 end = new Vector3(position.x + width, 0, z);

                if (hasTerrain)
                {
                    if (!terrain.LineWithinBounds(start, end))
                        continue;
                    start = terrain.ClampToTerrain(start);
                    end = terrain.ClampToTerrain(end);
                    if (end.x - start.x < _minWidth)
                        continue;
                }

                _circleSections.Add((start, end));
            }

            if (hasTerrain)
                DrawCircleMeshes(terrain);
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
            /*if (_circleSections == null)
                GenerateCircleSections();

            Gizmos.color = Color.blue;
            for (int i = 0; i < _circleSections?.Count; i++)
            {
                var pt = _circleSections[i];
                Gizmos.DrawLine(pt.Item1, pt.Item2);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position.AddZ(-_radius), transform.position.AddZ(_radius));*/
        }

        private void OnDestroy() => ReleaseAll();

        public void ReleaseAll() => Pool.ReleaseAll(this);
    }
}

