using UnityEngine;

namespace Whisperer.Pooling
{
    [CreateAssetMenu(menuName = "Whisperer/" + nameof(GenericPoolSettings))]
    [System.Serializable]
    public class GenericPoolSettings : ScriptableObject
    {
        [SerializeField] private int _maxItems = 200;
        [SerializeField] private int _initialCount = 50;
        [SerializeField] private GameObject _objectPrefab; // jak podstawiæ jakikolwiek komponent tutaj bez tworzenia podklas?;
        [SerializeField] private int _maxReleasedIndices = 50;

        public int MaxItems => _maxItems;
        public int InitialCount => _initialCount;
        public int MaxReleasedIndices => _maxReleasedIndices;
        public GameObject ObjectPrefab => _objectPrefab;
    }
}