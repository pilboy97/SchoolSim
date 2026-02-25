using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public static class TransformHelper
    {
        public static void PurgeChild(this Transform transform)
        {
            List<GameObject> objs = new(transform.childCount);
            foreach (Transform child in transform)
            {
                if(child.gameObject == transform.gameObject) continue;
                
                objs.Add(child.gameObject);
            }

            foreach (var obj in objs)
            {
                obj.SmartDestroy();
            }
        }
    }
}