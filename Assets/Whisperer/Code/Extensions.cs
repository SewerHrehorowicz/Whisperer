using UnityEngine;

namespace Whisperer
{

    public static class Extensions
    {
        public static Vector3 ToVector3(this float f) => new Vector3(f, f, f);
        public static Vector3 ZeroY(this Vector3 v) => new Vector3(v.x, 0, v.z);
    }
}