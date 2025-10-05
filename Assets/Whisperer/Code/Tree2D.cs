using UnityEngine;

namespace Whisperer
{
    public class Tree2D : MonoBehaviour
    {
        [SerializeField] [Range(1, 20)] private float _trunkLength = 10f;
        [SerializeField] private AnimationCurve _trunkNoiseAmount = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private Vector2 _trunkNoiseStrength = new Vector2(1f, 0.2f);
        [SerializeField] private Vector2 _trunkNoiseScale = new Vector2(2f, 2f);
        [SerializeField] private float _density = 2f;
        [SerializeField] [Range(0, 32000)] private int _seed;

        [SerializeField] private Vector3[] _points;

        private int GetVertsCountFor(float length) => (int)(length * _density);

        private void Generate()
        {
            Random.InitState(_seed);
            int trunkVerts = GetVertsCountFor(_trunkLength);
            Vector3 pos = transform.position;

            _points = new Vector3[trunkVerts];

            for (int i = 0; i < trunkVerts; i++)
            {
                float normalizedDistance = (float)i / trunkVerts;
                //float noiseX = Random.Range(-_trunkNoiseStrength.x, _trunkNoiseStrength.x);
                float xCoord = i / (float)trunkVerts * _trunkNoiseScale.x;
                float yCoord = _seed; 

                float perlinValue = Mathf.PerlinNoise(xCoord, yCoord); 

                float noiseX = (perlinValue - 0.5f) * _trunkNoiseStrength.x;
                float noiseY = Random.Range(-_trunkNoiseStrength.y, _trunkNoiseStrength.y);
                float noiseFactor = _trunkNoiseAmount.Evaluate(normalizedDistance);
                Vector2 noiseVector = new Vector2(noiseX * noiseFactor, noiseY * noiseFactor);

                float y = Mathf.Lerp(pos.y, pos.y + _trunkLength, normalizedDistance);
                _points[i] = new Vector3(pos.x + noiseVector.x, y + noiseVector.y, pos.z);
            }
        }

        private void OnValidate() => Generate();

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                if (i == 0) continue;
                Gizmos.DrawLine(_points[i - 1], _points[i]);
            }
        }
    }
}