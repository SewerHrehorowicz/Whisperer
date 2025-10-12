using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer.Pooling
{
    [ExecuteAlways]
    public class GenericPool<T> where T: Component
    {
        [SerializeField] private GenericPoolSettings _settings;
        private string _name;
        private Transform _transform;

        private string ContainerName => $"Pool_{_name}_Container";

        private Transform Transform
        {
            get
            {
                if (_transform == null)
                {
                    var go = new GameObject(ContainerName);
                    bool debug = GameConfig.Instance.DebugMode;
                    go.hideFlags = debug ? HideFlags.HideAndDontSave : HideFlags.DontSaveInEditor;
                    _transform = go.transform;
                }
                return _transform;
            }
        }
        //private int i;

        private T[] _items;
        private int _tip;
        private int[] _releasedIndices;
        private Dictionary<IGenericPoolUser<T>, List<T>> _users = new();

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
            int space = _settings.MaxItems - ItemsCount;
            int batchSize = Mathf.Min(space, _settings.InitialCount);

            T[] batch = new T[batchSize];

            for (int i = 0; i < batchSize; i++)
            {
                var go = GameObject.Instantiate(_settings.ObjectPrefab, Transform);
                go.transform.position = Transform.position;
                go.SetActive(false);
                go.name = $"{_settings.ObjectPrefab.name}_{i + ItemsCount}";
                T item = go.GetComponent<T>();
                batch[i] = item;
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

        private T GetItemFromIndex(int index)
        {
            var item = _items[index];
            item.gameObject.SetActive(true);
            return item;
        }

        private int GetIndexOfItem(T item)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] == item)
                    return i;
            }
            return -1;
        }

        private List<T> GetFromPool(int count)
        {
            List<T> items = new();
            for (int i = 0; i < count; i++)
            {
                items.Add(GetFromPool());
            }
            return items;
        }

        public T GetFromPool()
        {
            int freeIndex = GetReleasedIndex();
            if (freeIndex == -1 && _tip >= ItemsCount)
                CreateBatch();

            if (freeIndex > -1)
            {
                return GetItemFromIndex(freeIndex);
            }

            if (_tip >= _settings.MaxItems - 1)
            {
                Debug.LogWarning($"{_name}: pool capacity overflow!");
                return null;
            }

            return GetItemFromIndex(_tip++);
        }

        public void ReturnToPool(T item)
        {
            if (item.transform.parent != Transform)
            {
                Debug.LogWarning($"Trying to return {item.name} to {_name} pool, but it doesn't belong there");
                return;
            }

            item.gameObject.SetActive(false);
            int index = GetIndexOfItem(item);
            ReleaseIndex(index);
        }

        // if Transform is destroyed, it will throw error - trying to access GameObject that's been doestroyed
        public List<T> BorrowItems(int count, IGenericPoolUser<T> user)
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

        public void ReleaseAll(IGenericPoolUser<T> user)
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
            if (_items != null)
            {
                for (int i = _items.Length - 1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(_items[i]);
                }
            }

            _tip = 0;
            _releasedIndices = InitializeAray(-1, _settings.MaxReleasedIndices);
            _items = null;
            _users.Clear();
        }

        /// <summary>
        /// Don't call it directly, use <see cref="Create{T}(GenericPoolSettings, string)"/>.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        public GenericPool(GenericPoolSettings settings, string name)
        {
            _settings = settings;
            _name = name;
            Reset();
        }

        public static GenericPool<T> Create<T>(GenericPoolSettings settings, string name) where T: Component
        {
            bool noSettings = settings == null;
            bool noName = string.IsNullOrEmpty(name);
            if (noSettings || noName)
            {
                if (noName)
                    Debug.LogError("No name passed for pool");
                if (noSettings)
                {
                    string namePart = noName ? "" : " " + name;
                    Debug.LogError($"No settings passed for{namePart} pool creation");
                }

                return null;
            }
            return new GenericPool<T>(settings, name);
        }

        private void Awake() => Reset();

        private void Update()
        {
            if (Transform.position != Vector3.zero)
                Transform.position = Vector3.zero;
        }

        // clear old pools in case of recompiled scripts
        public void Recompiled()
        {
            var go = GameObject.Find(ContainerName);
            if (go)
                GameObject.DestroyImmediate(go);
        }
        #endregion
    }
}