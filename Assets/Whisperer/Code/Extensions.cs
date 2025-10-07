using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{

    public static class Extensions
    {
        public static Vector3 ToVector3(this float f) => new Vector3(f, f, f);
        public static Vector2 ToVector2(this float f) => new Vector2(f, f);
        public static Vector3 ZeroY(this Vector3 v) => new Vector3(v.x, 0, v.z);

        public static Vector3 AddY(this Vector3 v, float y) => new Vector3(v.x, v.y + y, v.z);

        public static void Add<T>(this List<T> collection, params T[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                collection.Add(items[i]);
            }
        }

        public static bool VariableChanged<T>(ref T cache, T variable) 
        {
            if (!EqualityComparer<T>.Default.Equals(cache, variable))
            {
                cache = variable;
                return true;
            }
            return false;
        }
    }
}