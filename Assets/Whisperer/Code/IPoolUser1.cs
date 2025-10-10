using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public interface IPoolUser1
    {
        public ObjectPool Pool { get; }
        public string name { get; }
        public List<GameObject> Borrowed { get; }
    }
}