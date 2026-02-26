using System;
using System.Collections.Generic;
using Unity.Mathematics;

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
        public static int SelectWithMultiplier(List<float> arr)
        {
            if (arr == null || arr.Count == 0) return -1;

            // 1. 점수 총합 계산 (float 정밀도 유지)
            float totalSum = 0f;
            foreach (var p in arr)
            {
                totalSum += math.max(p, 0);
            }

            if (totalSum <= 0) return UnityEngine.Random.Range(0, arr.Count);

            // 2. 룰렛 휠 선택
            float rand = UnityEngine.Random.Range(0f, totalSum);
            float acc = 0f;

            for (int i = 0; i < arr.Count; i++)
            {
                acc += math.max(arr[i], 0);
                if (acc >= rand) return i;
            }

            return arr.Count - 1;
        }
    }
}