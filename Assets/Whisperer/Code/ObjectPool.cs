using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private int _capacity = 200;
        [SerializeField] private int _initialCount = 50;
        [SerializeField] private GameObject _objectPrefab;
        [SerializeField] private MeshFilter _meshFilter;

        private int _lastIndex;
        private int[] _freeIndices;
        private int _lastFreeIndex;
        private Dictionary<IPoolUser, List<GameObject>> _users = new();

        private void CreateBatch()
        {
            int space = _capacity - transform.childCount;
            int batchSize = Mathf.Min(space, _initialCount);

            for (int i = 0; i < batchSize; i++)
            {
                var go = Instantiate(_objectPrefab, transform);
                go.transform.position = transform.position;
                go.SetActive(false);
            }
        }

        private GameObject GetFromIndex(int index)
        {
            var go = transform.GetChild(index).gameObject;
            go.SetActive(true);
            return go;
        }

        private List<GameObject> GetItems(int count)
        {
            List<GameObject> items = new();
            for (int i = 0; i < count; i++)
            {
                items.Add(GetItem());
            }
            return items;
        }

        public List<GameObject> Borrow(int count, IPoolUser user)
        {
            if (!_users.TryGetValue(user, out var items))
            {
                items = GetItems(count);
                _users.Add(user, items);
                return items;
            }

            if (items.Count < count)
            {
                items.AddRange(GetItems(count - items.Count));
            }
            
            else if (items.Count > count)
            {
                int excess = items.Count - count;
                for (int i = 0; i < excess; i++)
                {
                    var go = items[items.Count - 1];
                    items.RemoveAt(items.Count - 1);
                    ReturnToPool(go);
                }
            }

            return items;
        }

        public GameObject GetItem()
        {
            if (_lastFreeIndex == 0 && _lastIndex >= transform.childCount - 1)
                CreateBatch();

            if (_lastFreeIndex > 0)
            {
                int index = _freeIndices[--_lastFreeIndex];
                return GetFromIndex(index);
            }

            if (_lastIndex >= _capacity - 1)
            {
                Debug.LogWarning($"{name}: pool capacity overflow!");
                return null;
            }

            return GetFromIndex(_lastIndex++);
        }

        public void ReturnToPool(GameObject go)
        {
            if (go.transform.parent != transform)
            {
                Debug.LogWarning($"Trying to return {go.name} to {name} pool, but it doesn't belong there");
                return;
            }

            if (_lastFreeIndex >= _capacity)
            {
                Debug.LogWarning($"{name}: free index buffer overflow!");
                return;
            }

            go.SetActive(false);
            _freeIndices[_lastFreeIndex++] = go.transform.GetSiblingIndex();
        }

        private void Awake() => _freeIndices = new int[_capacity];

        private void OnValidate()
        {
            if (_freeIndices.Length < _capacity)
            {
                var newArray = new int[_capacity];
                _freeIndices.CopyTo(newArray, 0);
                _freeIndices = newArray;
            }

            transform.position = Vector3.zero;
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _lastIndex = 0;
            _lastFreeIndex = 0;
            System.Array.Clear(_freeIndices, 0, _freeIndices.Length);
            _users.Clear();
        }
    }
}