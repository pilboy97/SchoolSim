using UnityEditor;
using UnityEngine;

namespace Game
{
    public static class GameObjectHelper
    {
        public static void SmartDestroy(this GameObject x)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                UnityEngine.Object.Destroy(x);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(x);
            }
#else
            UnityEngine.Object.Destroy(x);
#endif
        }
    }
}