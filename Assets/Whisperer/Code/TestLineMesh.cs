using UnityEngine;
using Whisperer.VirtualTerrain;

namespace Whisperer
{
    // @todo class not mono, co can be submodule for terrain, tree, anything
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TestLineMesh : MonoBehaviour
    {
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _end;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] [Range(0.5f, 3)] private float _thickness = 1;
        [SerializeField] [Range(0.1f, 1)] private float _density = 1;
        [SerializeField] private VTerrain _terrain;
        [SerializeField] private AnimationCurve _profile;

        private Vector3 _lastPos;
        private Vector3 _lastStart;
        private Vector3 _lastEnd;

        private void Awake()
        {
            if (!_meshFilter)
                _meshFilter.GetComponent<MeshFilter>();
        }

        private bool ShouldUpdate()
        {
            if (Extensions.VariableChanged(ref _lastPos, transform.position)) return true;
            if (Extensions.VariableChanged(ref _lastStart, _start.position)) return true;
            if (Extensions.VariableChanged(ref _lastEnd, _end.position)) return true;
            return false;
        }

        private void OnValidate()
        {
            if (!_start || !_end || !_meshFilter || !_terrain) return;

            var line = Drawing.CreateLine(_start.localPosition, _end.localPosition, _density);

            for (int i = 0; i < line.Length; i++)
            {
                var pt = transform.position + line[i];
                line[i].y = _terrain.GetHeightAt(pt);
            }

            var mesh = Drawing.CreateLineMesh(line, _thickness, 1, _profile);
            _meshFilter.mesh = mesh;
        }

        private void Update()
        {
            if (ShouldUpdate())
                OnValidate();
        }
    }
}
