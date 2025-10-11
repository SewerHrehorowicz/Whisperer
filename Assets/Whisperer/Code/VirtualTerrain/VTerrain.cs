using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Whisperer.VirtualTerrain
{
    public class VTerrain : MonoBehaviour
    {
        public enum Modes { Shape, Grass }

        [SerializeField] private int _pointsPerUnit = 5;
        [SerializeField] private XZVector _terrainSize = new XZVector(10, 10);
        [SerializeField] private VTControlPoint[] _controlPoints;
        [SerializeField] private Modes _mode;

        private float[,] _grid;

        private XZVector GridSize
            => _grid == null
            ? new XZVector(0, 0)
            : new XZVector(_grid.GetLength(0), _grid.GetLength(1));

        public VTControlPoint[] ControlPoints => _controlPoints;
        public XZVector TerrainSize => _terrainSize;

        public Modes Mode => _mode;

        private Vector3 PointToWorld(float x, float y, float z)
        {
            float worldX = x / (GridSize.x - 1) * _terrainSize.x;
            float worldZ = z / (GridSize.z - 1) * _terrainSize.z;

            return new Vector3(worldX, y, worldZ);
        }

        public bool IsWithinBounds(Vector3 position, float tolerance = 0.01f)
        {
            var t = tolerance;
            var pos = position;
            var tp = transform.position;
            bool xWithin = pos.x > tp.x - t && pos.x < tp.x + _terrainSize.x + t;
            bool zWithin = pos.z > tp.z - t && pos.z < tp.z + _terrainSize.z + t;
            return xWithin && zWithin;
        }

        public bool LineWithinBounds(Vector3 start, Vector3 end, float tolerance = 0.01f)
        {
            var t = tolerance;
            var tp = transform.position;
            bool XWithin(Vector3 vec) => vec.x > tp.x - t && vec.x < tp.x + _terrainSize.x + t;
            bool ZWithin(Vector3 vec) => vec.z > tp.z - t && vec.z < tp.z + _terrainSize.z + t;
            bool anyZWithin = ZWithin(start) || ZWithin(end);
            bool anyXWithin = XWithin(start) || XWithin(end);

            bool XContained = start.x < tp.x && end.x > tp.x + _terrainSize.x;

            return XContained && anyZWithin || anyXWithin && anyZWithin;
        }

        public bool AnyWithinBounds(params Vector3[] positions)
        {
            for(int i = 0; i < positions.Length; i++)
            {
                if (IsWithinBounds(positions[i]))
                    return true;
            }
            return false;
        }

        public Vector3 ClampToTerrain(Vector3 point)
        {
            var x = transform.position.x;
            var z = transform.position.z;
            point.x = Mathf.Clamp(point.x, x, x + _terrainSize.x);
            point.z = Mathf.Clamp(point.z, z, z + _terrainSize.z);
            return point;
        }

        private void CreateTerrainGrid(int pointsPerUnit = 1)
        {
            int width = Mathf.CeilToInt(_terrainSize.x * pointsPerUnit);
            int depth = Mathf.CeilToInt(_terrainSize.z * pointsPerUnit);

            _grid = new float[width, depth];

            BlendControlPoints();
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

        public float GetHeightAt(Vector3 worldPosition)
        {
            if (_grid == null) return 0;

            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            // returns original position if out of terrain bounds
            if (localPos.x < 0 || localPos.x > _terrainSize.x || localPos.z < 0 || localPos.z > _terrainSize.z)
                return worldPosition.y;

            float closestX = (localPos.x / _terrainSize.x) * (GridSize.x - 1);
            float closestZ = (localPos.z / _terrainSize.z) * (GridSize.z - 1);

            // keep points in grid
            int x0 = Mathf.Clamp(Mathf.FloorToInt(closestX), 0, GridSize.x - 1);
            int z0 = Mathf.Clamp(Mathf.FloorToInt(closestZ), 0, GridSize.z - 1);
            int x1 = Mathf.Min(x0 + 1, GridSize.x - 1);
            int z1 = Mathf.Min(z0 + 1, GridSize.z - 1);

            float deltaX = closestX - x0;
            float deltaZ = closestZ - z0;

            /*Debug.Log("grid: " + _grid.GetLength(0) + ", " + _grid.GetLength(1));
            Debug.Log($"x0 {x0}, x0 {z0}, x1 {x1}, z1 {z1}");
            */
            float heightBL = _grid[x0, z0]; // NullReferenceException?
            float heightBR = _grid[x1, z0];
            float heightTL = _grid[x0, z1];
            float heightTR = _grid[x1, z1];

            // bilinear interpolation
            float heightBottom = Mathf.Lerp(heightBL, heightBR, deltaX);
            float heightTop = Mathf.Lerp(heightTL, heightTR, deltaX);
            float interpolatedHeight = Mathf.Lerp(heightBottom, heightTop, deltaZ);

            return interpolatedHeight;
        }

        private void OnValidate() => CreateTerrainGrid(_pointsPerUnit/*GameConfig.Instance.VTSettings.PointsPerUnit*/);

        private void OnDrawGizmos()
        {
            if (_grid == null)
                return;

            Gizmos.color = Color.grey;

            for (int z = 0; z < GridSize.z; z++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    if (x == 0) continue;
                    Vector3 terrainPoint = PointToWorld(x, _grid[x, z], z);
                    terrainPoint = transform.TransformPoint(terrainPoint);
                    Vector3 prevPoint = PointToWorld(x - 1, _grid[x - 1, z], z);
                    prevPoint = transform.TransformPoint(prevPoint);
                    Gizmos.DrawLine(prevPoint, terrainPoint);
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
