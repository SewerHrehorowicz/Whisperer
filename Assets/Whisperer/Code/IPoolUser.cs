using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public interface IPoolUser {
        public ObjectPool Pool { get; }
        public List<GameObject> Borrowed { get; }
    }
}