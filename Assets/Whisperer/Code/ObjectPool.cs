using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    [ExecuteAlways]
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private int _maxItems = 200;
        [SerializeField] private int _initialCount = 50;
        [SerializeField] private GameObject _objectPrefab;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private int _maxReleasedIndices = 50;

        [SerializeField] private GameObject[] _items;
        [SerializeField] private int _tip;
        [SerializeField] private int[] _releasedIndices;
        private Dictionary<IPoolUser, List<GameObject>> _users = new();

        private int ItemsCount => _items == null ? 0 : _items.Length;

        private T[] InitializeAray<T>(T defaultValue, int size)
        {
            T[] arr = new T[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = defaultValue;
            }
            return arr;
        }

        private T[] MergeArrays<T>(T[] arr1, T[] arr2)
        {
            int length = arr1.Length + arr2.Length;
            T[] res = new T[length];
            Array.Copy(arr1, res, arr1.Length);
            Array.Copy(arr2, 0, res, arr1.Length, arr2.Length);
            return res;
        }

        private void CreateBatch()
        {
            int space = _maxItems - ItemsCount;
            int batchSize = Mathf.Min(space, _initialCount);

            GameObject[] batch = new GameObject[batchSize];

            for (int i = 0; i < batchSize; i++)
            {
                var go = Instantiate(_objectPrefab, transform);
                go.transform.position = transform.position;
                go.SetActive(false);
                go.name = $"{_objectPrefab.name}_{i + ItemsCount}";
                batch[i] = go;
            }

            _items = _items == null ? batch : MergeArrays(_items, batch);
        }

        private int GetReleasedIndex()
        {
            for (int i = 0; i < _releasedIndices.Length; i++)
            {
                if (_releasedIndices[i] > -1)
                {
                    int free = _releasedIndices[i];
                    _releasedIndices[i] = -1;
                    return free;
                }
            }
            return -1;
        }

        private void ReleaseIndex(int index)
        {
            for (int i = 0; i < _releasedIndices.Length; i++)
            {
                if (_releasedIndices[i] == -1)
                {
                    _releasedIndices[i] = index;
                    return;
                }
            }
        }

        private GameObject GetItemFromIndex(int index)
        {
            var go = _items[index];
            go.SetActive(true);
            return go;
        }

        private int GetIndexOfItem(GameObject go)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] == go)
                    return i;
            }
            return -1;
        }

        private List<GameObject> GetFromPool(int count)
        {
            List<GameObject> items = new();
            for (int i = 0; i < count; i++)
            {
                items.Add(GetFromPool());
            }
            return items;
        }

        public GameObject GetFromPool()
        {
            int freeIndex = GetReleasedIndex();
            if (freeIndex == -1 && _tip >= ItemsCount)
                CreateBatch();

            if (freeIndex > -1)
            {
                return GetItemFromIndex(freeIndex);
            }

            if (_tip >= _maxItems - 1)
            {
                Debug.LogWarning($"{name}: pool capacity overflow!");
                return null;
            }

            return GetItemFromIndex(_tip++);
        }

        public void ReturnToPool(GameObject go)
        {
            if (go.transform.parent != transform)
            {
                Debug.LogWarning($"Trying to return {go.name} to {name} pool, but it doesn't belong there");
                return;
            }

            go.SetActive(false);
            int index = GetIndexOfItem(go);
            ReleaseIndex(index);
        }

        public List<GameObject> BorrowItems(int count, IPoolUser user)
        {
            if (!_users.TryGetValue(user, out var items))
            {
                items = GetFromPool(count);
                _users.Add(user, items);
                return items;
            }

            if (items.Count < count)
            {
                items.AddRange(GetFromPool(count - items.Count));
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

        public void ReleaseAll(IPoolUser user)
        {
            if (_users.TryGetValue(user, out var items))
            {
                foreach (var go in items)
                    ReturnToPool(go);
                _users.Remove(user);
            }
        }

        #region Events
        public void Reset()
        {
            for (int i = _items.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(_items[i]);
            }

            _tip = 0;
            _releasedIndices = InitializeAray(-1, _maxReleasedIndices);
            _items = null;
            _users.Clear();
        }

        private void Awake() => Reset();

        private void OnValidate() => transform.position = Vector3.zero;

        private void Update()
        {
            if (transform.position != Vector3.zero)
                transform.position = Vector3.zero;
        }
        #endregion
    }
}