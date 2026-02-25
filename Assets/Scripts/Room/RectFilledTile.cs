using System;
using Game.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Room
{
    [Serializable]
    public struct RectFilledTile
    {
        [SerializeField] public string targetMapID;
        [SerializeField] public RectInt rect;
        [SerializeField] public TileBase tile;

        public void FillRect()
        {
            if (tile == null) return;
            
            MapController.Instance.maps[targetMapID].SetTiles(tile, rect);
        }
    }
}