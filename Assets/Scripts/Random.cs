using System;
using System.Collections.Generic;

namespace Game
{
    public static class Random
    {
        public static T Choose<T>(IList<T> list)
        {
            var idx = UnityEngine.Random.Range(0, list.Count);
            
            return list[idx];
        }
        public static T Choose<T>(T[] array)
        {
            var idx = UnityEngine.Random.Range(0, array.Length);
            
            return array[idx];
        }
            
        public static T ChooseEnum<T>()
        {
            var array = Enum.GetValues(typeof(T));
            int idx = UnityEngine.Random.Range(0, array.Length);

            return (T)array.GetValue(idx);
        }
    }
}