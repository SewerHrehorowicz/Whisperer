using System.Collections.Generic;
using UnityEngine;

namespace Whisperer.Pooling
{
    public interface IPoolUser {
        public ObjectPool Pool { get; }
        public string name { get; }
        public List<GameObject> Borrowed { get; }

        /// <summary>
        /// Don't forget to release all meshes
        /// </summary>
        public void ReleaseAll();
    }
}