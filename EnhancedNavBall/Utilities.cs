using System;
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

        public static void DebugLogFormatted(LogLevel logLevel, string message, params object[] strParams)
        {
            if (logLevel <= loggingLevel)
            {
                message = String.Format(message, strParams);
                String strMessageLine = String.Format("{0},EnhancedNavball,{1}", DateTime.Now, message);
                Debug.Log(strMessageLine);
            }
        }

        public static void DebugLog(LogLevel logLevel, string log)
        {
            DebugLogFormatted(logLevel, log, new object[0]);
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

        public static string BuildOutput(
            Quaternion quart,
            string paramName)
        {
            return string.Format(
                "{0} Euler - x:{1} y:{2} z:{3}",
                paramName,
                FloatFormat(quart.eulerAngles.x),
                FloatFormat(quart.eulerAngles.y),
                FloatFormat(quart.eulerAngles.z));
        }

        public static string BuildOutput(
            float f,
            string paramName)
        {
            return string.Format(
                "{0} {1}",
                paramName,
                FloatFormat(f));
        }

        public static string BuildOutput(
            Vector3 vector3,
            string paramName)
        {
            return string.Format(
                "{0} Vector - x:{1} y:{2} z:{3}",
                paramName,
                FloatFormat(vector3.x),
                FloatFormat(vector3.y),
                FloatFormat(vector3.z));
        }

        public static string BuildOutput(
            Vector2 vector2,
            string paramName)
        {
            return string.Format(
                "{0} Vector - x:{1} y:{2}",
                paramName,
                FloatFormat(vector2.x),
                FloatFormat(vector2.y));
        }

        public static string FloatFormat(float f)
        {
            return f.ToString("0.0000");
        }
    }
}
