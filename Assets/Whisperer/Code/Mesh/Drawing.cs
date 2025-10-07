using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer.MeshHelpers
{
    public static class Drawing
    {
        public static Vector3[] CreateLine(Vector3 start, Vector3 end, float density)
        {
            Vector3 dir = end - start;
            int points = Mathf.CeilToInt(dir.magnitude * density);
            Vector3[] line = new Vector3[points];

            for (int i = 0; i < points; i++)
            {
                float t = i / (float)(points - 1);
                line[i] = Vector3.Lerp(start, end, t);
            }

            return line;
        }

        /// <summary>
        /// Helper function that draws a shape
        /// </summary>
        /// <param name="thickness">constant value for now</param>
        /// <returns>new mesh</returns>
        public static Mesh CreateLineMesh(Vector3[] line, float thickness = 1, float upAmount = 0.5f, AnimationCurve profile = null)
        {
            Mesh shape = new Mesh();
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            for (int i = 0; i < line.Length; i++)
            {
                // points
                upAmount = Mathf.Clamp01(upAmount);
                var isLast = i == line.Length - 1;
                var currentPoint = isLast ? line[i-1] : line[i];
                var nextPoint = isLast ? line[i] : line[i + 1];
                var segmentThickness = thickness;

                if (profile != null)
                {
                    float t = (float)i / (line.Length - 1);
                    segmentThickness = profile.Evaluate(t) * thickness;
                }
                
                Vector3 direction = (nextPoint - currentPoint).normalized;
                Vector3 up = Vector3.Cross(Vector3.forward, direction).normalized * segmentThickness * upAmount;
                Vector3 down = Vector3.Cross(Vector3.forward, direction).normalized * segmentThickness * (1-upAmount);
                verts.Add(currentPoint + up, currentPoint - down);

                // triangles
                if (i > 0)
                {
                    var prev = i - 1;
                    var v0 = prev * 2;
                    var v1 = v0 + 1;
                    var v2 = v0 + 2;
                    var v3 = v0 + 3;

                    tris.Add(v0, v2, v1);
                    tris.Add(v2, v3, v1);
                }
            }

            shape.SetVertices(verts);
            shape.SetTriangles(tris, 0);
            return shape;
        }
    }
}

