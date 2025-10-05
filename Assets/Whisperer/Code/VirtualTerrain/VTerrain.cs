using System.Linq;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    public class VTerrain : MonoBehaviour
    {
        [SerializeField] private int _pointsPerUnit = 5;
        [SerializeField] private XZVector _terrainSize = new XZVector(10, 10);
        [SerializeField] private VTControlPoint[] _controlPoints;

        private float[,] _terrainPoints;

        private XZVector _pointsSize;
        public VTControlPoint[] ControlPoints => _controlPoints;
        public XZVector TerrainSize => _terrainSize;

        public Vector3 ClampToTerrain(Vector3 point)
        {
            var x = transform.position.x;
            var z = transform.position.z;
            point.x = Mathf.Clamp(point.x, x, x + _terrainSize.x);
            point.z = Mathf.Clamp(point.z, z, z + _terrainSize.z);
            return point;
        }

        public void BlendControlPoints()
        {
            for (int x = 0; x < _pointsSize.x; x++)
            {
                for (int z = 0; z < _pointsSize.z; z++)
                {
                    float worldX = x / (float)(_pointsSize.x - 1) * _terrainSize.x;
                    float worldZ = z / (float)(_pointsSize.z - 1) * _terrainSize.z;

                    Vector3 worldPos = new Vector3(worldX, 0, worldZ);

                    float height = 0f;

                    foreach (var pt in _controlPoints.OrderBy(p => p.Position.y))
                    {
                        float distance = Vector3.Distance(worldPos, new Vector3(pt.Position.x, 0, pt.Position.z));
                        if (distance < pt.Radius)
                        {
                            float blendDistance = Mathf.Min(distance, 3);
                            float normalizedDistance = 1f - (distance / pt.Radius);
                            float falloff = pt.FalloffCurve.Evaluate(normalizedDistance);
                            float newHeight = pt.Position.y * falloff;
                            height = Mathf.Lerp(newHeight, height, 1 - normalizedDistance);
                        }
                    }
                    _terrainPoints[x, z] = height;
                }
            }
        }

        private void GenerateTerrainData(int pointsPerUnit = 5)
        {
            _pointsSize = new XZVector(
                Mathf.CeilToInt(_terrainSize.x * pointsPerUnit),
                Mathf.CeilToInt(_terrainSize.z * pointsPerUnit)
            );

            _terrainPoints = new float[_pointsSize.x, _pointsSize.z];

            BlendControlPoints();
        }

        public float GetTerrainHeight(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            // returns original position if out of terrain bounds
            if (localPos.x < 0 || localPos.x > _terrainSize.x || localPos.z < 0 || localPos.z > _terrainSize.z)
                return worldPosition.y;

            float closestX = (localPos.x / _terrainSize.x) * (_pointsSize.x - 1);
            float closestZ = (localPos.z / _terrainSize.z) * (_pointsSize.z - 1);

            int x0 = Mathf.FloorToInt(closestX);
            int x1 = Mathf.Min(x0 + 1, _pointsSize.x - 1);
            int z0 = Mathf.FloorToInt(closestZ);
            int z1 = Mathf.Min(z0 + 1, _pointsSize.z - 1);

            float deltaX = closestX - x0;
            float deltaZ = closestZ - z0;

            float heightBL = _terrainPoints[x0, z0];
            float heightBR = _terrainPoints[x1, z0];
            float heightTL = _terrainPoints[x0, z1];
            float heightTR = _terrainPoints[x1, z1];

            // bilinear interpolation
            float heightBottom = Mathf.Lerp(heightBL, heightBR, deltaX);
            float heightTop = Mathf.Lerp(heightTL, heightTR, deltaX);
            float interpolatedHeight = Mathf.Lerp(heightBottom, heightTop, deltaZ);

            return interpolatedHeight;
        }

        private void OnValidate() => GenerateTerrainData(_pointsPerUnit);

        private void OnDrawGizmos()
        {
            if (_terrainPoints == null)
                return;

            Gizmos.color = Color.white;
            for (int x = 0; x < _pointsSize.x; x++)
            {
                for (int z = 0; z < _pointsSize.z; z++)
                {
                    Vector3 worldPos = transform.TransformPoint(new Vector3(
                        (x / (float)(_pointsSize.x - 1)) * _terrainSize.x,
                        _terrainPoints[x, z],
                        (z / (float)(_pointsSize.z - 1)) * _terrainSize.z
                    ));
                    Gizmos.DrawCube(worldPos, new Vector3(0.05f, 0.05f, 0.05f));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_terrainPoints == null)
                return;

            Gizmos.color = Color.green;
            foreach (var pt in _controlPoints)
            {
                Gizmos.DrawSphere(transform.TransformPoint(pt.Position), 0.3f);
            }
        }
    }
}
