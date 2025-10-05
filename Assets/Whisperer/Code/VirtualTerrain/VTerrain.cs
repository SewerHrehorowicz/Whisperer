using System.Linq;
using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    public class VTerrain : MonoBehaviour
    {
        private static float GizmoSize = 0.1f;

        [SerializeField] private int _pointsPerUnit = 5;
        [SerializeField] private XZVector _terrainSize = new XZVector(10, 10);
        [SerializeField] private VTControlPoint[] _controlPoints;

        private float[,] _grid;

        private XZVector GridSize
            => _grid == null
            ? new XZVector(0, 0)
            : new XZVector(_grid.GetLength(0), _grid.GetLength(1));

        public VTControlPoint[] ControlPoints => _controlPoints;
        public XZVector TerrainSize => _terrainSize;

        private Vector3 PointToWorld(float x, float y, float z)
        {
            float worldX = x / (GridSize.x - 1) * _terrainSize.x;
            float worldZ = z / (GridSize.z - 1) * _terrainSize.z;

            return new Vector3(worldX, y, worldZ);
        }

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
            for (int x = 0; x < GridSize.x; x++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    float height = 0f;
                    Vector3 terrainPoint = PointToWorld(x, height, z);

                    foreach (var controlPoint in _controlPoints.OrderBy(p => p.Position.y))
                    {
                        float distance = Vector3.Distance(terrainPoint, controlPoint.Position.ZeroY());
                        if (distance < controlPoint.Radius)
                        {
                            float blendDistance = Mathf.Min(distance, 3);
                            float normalizedDistance = 1f - (distance / controlPoint.Radius);
                            float falloff = controlPoint.FalloffCurve.Evaluate(normalizedDistance);
                            float newHeight = controlPoint.Position.y * falloff;
                            height = Mathf.Lerp(newHeight, height, 1 - normalizedDistance);
                        }
                    }
                    _grid[x, z] = height;
                }
            }
        }

        private void CreateTerrainGrid(int pointsPerUnit = 1)
        {
            int width = Mathf.CeilToInt(_terrainSize.x * pointsPerUnit);
            int depth = Mathf.CeilToInt(_terrainSize.z * pointsPerUnit);

            _grid = new float[width, depth];

            BlendControlPoints();
        }

        public float GetHeightAt(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            // returns original position if out of terrain bounds
            if (localPos.x < 0 || localPos.x > _terrainSize.x || localPos.z < 0 || localPos.z > _terrainSize.z)
                return worldPosition.y;

            float closestX = (localPos.x / _terrainSize.x) * (GridSize.x - 1);
            float closestZ = (localPos.z / _terrainSize.z) * (GridSize.z - 1);

            int x0 = Mathf.FloorToInt(closestX);
            int x1 = Mathf.Min(x0 + 1, GridSize.x - 1);
            int z0 = Mathf.FloorToInt(closestZ);
            int z1 = Mathf.Min(z0 + 1, GridSize.z - 1);

            float deltaX = closestX - x0;
            float deltaZ = closestZ - z0;

            float heightBL = _grid[x0, z0];
            float heightBR = _grid[x1, z0];
            float heightTL = _grid[x0, z1];
            float heightTR = _grid[x1, z1];

            // bilinear interpolation
            float heightBottom = Mathf.Lerp(heightBL, heightBR, deltaX);
            float heightTop = Mathf.Lerp(heightTL, heightTR, deltaX);
            float interpolatedHeight = Mathf.Lerp(heightBottom, heightTop, deltaZ);

            return interpolatedHeight;
        }

        private void OnValidate() => CreateTerrainGrid(_pointsPerUnit);

        private void OnDrawGizmos()
        {
            if (_grid == null)
                return;

            Gizmos.color = Color.white;
            for (int x = 0; x < GridSize.x; x++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    Vector3 terrainPoint = PointToWorld(x, _grid[x, z], z);
                    terrainPoint = transform.TransformPoint(terrainPoint);
                    Gizmos.DrawCube(terrainPoint, GizmoSize.ToVector3());
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_grid == null)
                return;

            Gizmos.color = Color.green;
            foreach (var pt in _controlPoints)
            {
                Gizmos.DrawSphere(transform.TransformPoint(pt.Position), 0.3f);
            }
        }
    }
}
