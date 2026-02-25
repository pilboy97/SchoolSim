using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct NavData
    {
        [SerializeField] public Vector3Int st;
        [SerializeField] public Vector3Int ed;
        [SerializeField] public Vector3Int value;
        [SerializeField] public int dist;
    }
}