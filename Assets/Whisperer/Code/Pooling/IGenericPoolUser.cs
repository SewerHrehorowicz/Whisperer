using System.Collections.Generic;
using UnityEngine;

namespace Whisperer.Pooling
{
    public interface IGenericPoolUser<T> where T : Component {
        public GenericPool<T> Pool { get; }
        public string name { get; }
        public List<T> Borrowed { get; }

        /// <summary>
        /// Don't forget to release all meshes.
        /// Call it when your user is destroyed.
        /// </summary>
        public void ReleaseAll();
    }
}