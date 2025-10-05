using UnityEngine;

namespace Whisperer.VirtualTerrain
{
    [System.Serializable]
    public class VTControlPoint
    {
        [SerializeField] private Vector3 _position;
        [Range(1, 20f)] [SerializeField] private float _radius;
        [Range(0, 1f)] [SerializeField] private float _noiseStrength = 1;
        [SerializeField] private AnimationCurve _falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private float _noiseScale;
        private Texture2D _noiseTex;

        public Vector3 Position => _position;
        public float Radius => _radius;
        public AnimationCurve FalloffCurve => _falloffCurve;
        public float NoiseStrength => 1;

        public void SetPosition(Vector3 newPos, XZVector terrainSize)
        {
            _position.x = Mathf.Clamp(newPos.x, 0, terrainSize.x);
            _position.z = Mathf.Clamp(newPos.z, 0, terrainSize.z);
            _position.y = newPos.y;
        }
    }
}