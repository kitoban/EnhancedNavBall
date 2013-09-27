using UnityEngine;

namespace EnhancedNavBall
{
    enum LogLevel
    {
        None = 0,
        Minimal = 1,
        Diagnostic = 2
    }

    static class Utilities
    {
        static LogLevel loggingLevel = LogLevel.None;

        public static void DebugLog(LogLevel logLevel, string log)
        {
            if (logLevel <= loggingLevel)
            {
                Debug.Log(log);
            }
        }

        public static GameObject CreateSimplePlane(
            string name,
            float vectorSize)
        {
            Mesh mesh = new Mesh();

            GameObject obj = new GameObject(name);
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();

            const float uvize = 1f;

            Vector3 p0 = new Vector3(-vectorSize, 0, vectorSize);
            Vector3 p1 = new Vector3(vectorSize, 0, vectorSize);
            Vector3 p2 = new Vector3(-vectorSize, 0, -vectorSize);
            Vector3 p3 = new Vector3(vectorSize, 0, -vectorSize);

            mesh.vertices = new[]
            {
                p0, p1, p2,
                p1, p3, p2
            };

            mesh.triangles = new[]
            {
                0, 1, 2,
                3, 4, 5
            };

            Vector2 uv1 = new Vector2(0, 0);
            Vector2 uv2 = new Vector2(uvize, uvize);
            Vector2 uv3 = new Vector2(0, uvize);
            Vector2 uv4 = new Vector2(uvize, 0);

            mesh.uv = new[]{
                uv1, uv4, uv3,
                uv4, uv2, uv3
            };
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            
            meshFilter.mesh = mesh;

            DebugLog(LogLevel.Minimal, name + " mesh created");

            return obj;
        }
    }
}
