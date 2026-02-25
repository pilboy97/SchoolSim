using System;
using UnityEngine;

namespace Game.Map
{
    [Serializable]
    public struct MapObjectInit
    {
        [SerializeField] public MapObject objectPrefab;
        [SerializeField] public Vector3Int position;

        public int ZIndex
        {
            get => position.z;
            set => position.z = value;
        }
    }
}