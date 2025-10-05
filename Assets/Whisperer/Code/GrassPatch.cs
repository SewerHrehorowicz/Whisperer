using UnityEngine;
using System.Collections.Generic;
using Whisperer.VirtualTerrain;
using Whisperer;

[System.Serializable]
public class BillboardData
{
    [field: SerializeField] public Vector3 Position { get; set; }
    [field: SerializeField] public Vector2 Size { get; set; }

    public BillboardData(Vector3 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }
}

[ExecuteAlways]
public class GrassPatch : MonoBehaviour
{
    [SerializeField] [Range(1, 20)] private float _radius = 10f;
    [SerializeField] [Range(1, 20)] private float _density = 5f;
    [SerializeField] [Range(0, 32000)] private int _seed;
    [SerializeField] private Vector2 _maxQuadSize = 2f.ToVector2();
    [SerializeField] private Vector2 _minQuadSize = 0.5f.ToVector2();
    [SerializeField] private AnimationCurve _falloffCurve;
    [SerializeField] private VTerrain _virtualTerrain;
    
    private Vector3 _lastPosition;

    private BillboardData[] _quads;

    private void GenerateQuads()
    {
        if (!_virtualTerrain) return;

        Random.InitState(_seed);
        List<BillboardData> quads = new List<BillboardData>();
        int quadsNum = Mathf.RoundToInt(_radius * _radius * _density);

        for (int i = 0; i < quadsNum; i++)
        {
            Vector2 randomPosFactor = Random.insideUnitCircle * _radius;
            Vector3 quadPos = transform.position + new Vector3(randomPosFactor.x, 0, randomPosFactor.y);

            if (!_virtualTerrain.IsWithinBounds(quadPos)) continue;

            float normalizedDistance = Vector2.Distance(transform.position, quadPos) / _radius;
            Vector2 size = Vector2.Lerp(_maxQuadSize, _minQuadSize, normalizedDistance);
            
            quadPos.y = _virtualTerrain.GetHeightAt(quadPos);
            BillboardData quadData = new BillboardData(quadPos, size);
            quads.Add(quadData);
        }

        _quads = quads.ToArray();
    }

    private void Update()
    {
        if (_lastPosition == transform.position) return;
        _lastPosition = transform.position;
        GenerateQuads();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i =0; i < _quads.Length; i++)
        {
            var quad = _quads[i];
            Gizmos.DrawCube(quad.Position, quad.Size);
        }
    }

    private void OnValidate() => GenerateQuads();
}
