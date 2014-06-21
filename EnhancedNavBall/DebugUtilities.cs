using UnityEngine;

namespace EnhancedNavBall
{
    internal class DebugUtilities
    {
        public static void DebugCall()
        {

            GameObject gameObject = GameObject.Find("ScreenSafeUI");
            Debug.Log("############################################");
            RetrieveParentStructure("", gameObject);
            Debug.Log("############################################");
        }

        private static void RetrieveChildStructure(string empty,
            GameObject gameObject)
        {
            Debug.Log(empty + gameObject.name);

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var transform = gameObject.transform.GetChild(i);
                RetrieveChildStructure(empty + " ", transform.gameObject);
            }
        }

        private static void RetrieveParentStructure(string empty,
            GameObject gameObject)
        {
            Debug.Log(empty + gameObject.name);

            var parent = gameObject.transform.parent;

            if (parent != null)
                RetrieveParentStructure(" ", parent.gameObject);
        }
    }
}