using System;
using System.Collections.Generic;
using UnityEngine;
using Whisperer.Management;

namespace Whisperer
{
    public class ObjectPool1 : MonoBehaviour
    {
        private struct PoolUserData
        {
            private IPoolUser _user;
            private int _id;
            private int _itemsCount;

            public IPoolUser User => _user;
            public int Id => _id;
            public int ItemsCount => _itemsCount;

            public PoolUserData(IPoolUser user, int id, int itemsCount)
            {
                _user = user;
                _itemsCount = itemsCount;
                _id = id;
            }
        }

        [SerializeField] private int _capacity = 3000;
        [SerializeField] private int _initialCount = 1000;
        [SerializeField] private int _batchSize = 500;
        [SerializeField] private int _maxItemsPerUser = 50;
        [SerializeField] private int _maxUsersItems = 1000;
        [SerializeField] private int _maxUsers = 50;
        [SerializeField] private int _maxFreeIndices = 50;
        [Range(0.2f, 1f)][SerializeField] private float _borrowersSpace = 0.5f;
        [SerializeField] private GameObject _objectPrefab;
        [SerializeField] private MeshFilter _meshFilter;

        [SerializeField] [HideInInspector] private int _lastIndex;
        [SerializeField] [HideInInspector] private int[] _freeIndices;
        [SerializeField] [HideInInspector] private int _lastFreeIndex;

        [SerializeField] private GameObject[] _items;
        [SerializeField] private GameObject[] _usersItems;

        [SerializeField] private PoolUserData[] _users; // no data needed?
        [SerializeField] private bool[] _freeBorrowerIndices;

        [SerializeField] [HideInInspector] private int _lastBorrowerIndex = 0;

        private void Initialize()
        {
            _usersItems = new GameObject[_maxUsersItems];
            _users = new PoolUserData[_maxUsers];
            _freeIndices = new int[_maxFreeIndices];
            for (int i = 0; i < _maxFreeIndices; i++)
            {
                _freeIndices[i] = -1;
            }
        }

        private void CreateBatch(bool initial = false)
        {
            int space = _capacity - _items.Length; //transform.childCount;
            int batchSize = Mathf.Min(space, initial ? _initialCount : _batchSize);
            if (batchSize == 0) return;

            GameObject[] batch = new GameObject[batchSize];

            for (int i = 0; i < batchSize; i++)
            {
                var go = Instantiate(_objectPrefab, transform);
                go.transform.position = transform.position;
                go.SetActive(false);
                batch[i] = go;
            }

            //_objects.rezise?
            int oldLength = _items.Length;
            GameObject[] newArray = new GameObject[oldLength + batchSize];
            Array.Copy(_items, newArray, oldLength);
            Array.Copy(batch, 0, newArray, oldLength, batchSize);
            _items = newArray;
        }

        private GameObject GetFromIndex(int index)
        {
            var go = _items[index];//transform.GetChild(index).gameObject;
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

        private PoolUserData GetUserData(IPoolUser user, int count, out int freeId, out int freeSpace)
        {
            freeId = -1;
            freeSpace = _usersItems.Length;
            for (int i = 0; i < _users.Length; i++)
            {
                var found = _users[i];
                if (found.User == user)
                    return found;
                if (found.ItemsCount > 0)
                    freeSpace -= found.ItemsCount;
                if (found.User == null && freeId == -1)
                    freeId = i;
            }
            
            return new PoolUserData();
        }

        public GameObject[] Borrow(IPoolUser user, int count)
        {
            int freeSpace, freeId;
            PoolUserData data = GetUserData(user, count, out freeId, out freeSpace);
            
            if (data.User == null)
            {
                if (freeSpace < count)
                {
                    Debug.LogWarning("Not enough items in pool");
                    return null;
                }

                data = new PoolUserData(user, freeId, count);
                _users[freeId] = data;
                // get count items from pool
                return new GameObject[count]; //mock
            }
            
            if (data.ItemsCount == count)
                return null; // doesn't have do do anything

            // we have to expand returned array
            return new GameObject[count]; //mock
        }

        public GameObject GetItem()
        {
            if (_lastFreeIndex == 0 && _lastIndex >= _items.Length - 1/*transform.childCount - 1*/)
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
            _freeIndices[_lastFreeIndex++] = Array.IndexOf(_items, go);//go.transform.GetSiblingIndex();
        }

        private void Awake()
        {
            // @todo fix initializing, releasing, serialization
            if (_freeIndices?.Length == 0)
                _freeIndices = new int[_capacity];
        }

        private void OnValidate()
        {
            transform.position = Vector3.zero;
        }

        public void Clear()
        {
            for (int i = _items.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(_items[i]);
            }
            Array.Clear(_items, 0, _items.Length);

            _lastIndex = 0;
            _lastFreeIndex = 0;
            Array.Clear(_freeIndices, 0, _freeIndices.Length);
            Array.Clear(_users, 0, _users.Length);
        }
    }
}