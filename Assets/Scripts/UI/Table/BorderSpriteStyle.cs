using System;
using UnityEngine;

namespace Game.UI
{
    [Flags]
    public enum BorderDirection
    {
        Left = 1,
        Right = 1 << 1,
        Top = 1 << 2,
        Bottom = 1 << 3,
        
        All = Left | Right | Top | Bottom,
        None = 0,
        
        Horizontal = Top | Bottom,
        Vertical = Left | Right,
    }
    
    [CreateAssetMenu(menuName = "UI/Border Sprite")]
    public class BorderSpriteStyle : ScriptableObject
    {
        [SerializeField] private Sprite all;
        [SerializeField] private Sprite none;
        [SerializeField] private Sprite l;
        [SerializeField] private Sprite r;
        [SerializeField] private Sprite t;
        [SerializeField] private Sprite b;
        [SerializeField] private Sprite lr;
        [SerializeField] private Sprite lt;
        [SerializeField] private Sprite lb;
        [SerializeField] private Sprite rt;
        [SerializeField] private Sprite rb;
        [SerializeField] private Sprite tb;
        [SerializeField] private Sprite lrt;
        [SerializeField] private Sprite lrb;
        [SerializeField] private Sprite ltb;
        [SerializeField] private Sprite rtb;

        public Sprite Default => all;
        
        public Sprite Select(BorderDirection direction)
        {
            var L = (direction & BorderDirection.Left) != 0;
            var R = (direction & BorderDirection.Right) != 0;
            var T = (direction & BorderDirection.Top) != 0;
            var B = (direction & BorderDirection.Bottom) != 0;
            
            if (L && R && T && B) return all;
            if (L && R && T && !B) return lrt;
            if (L && R && !T && B) return lrb;
            if (L && R && !T && !B) return lr;
            if (L && !R && T && B) return ltb;
            if (L && !R && T && !B) return lt;
            if (L && !R && !T && B) return lb;
            if (L && !R && !T && !B) return l;
            if (!L && R && T && B) return rtb;
            if (!L && R && T && !B) return rt;
            if (!L && R && !T && B) return rb;
            if (!L && R && !T && !B) return r;
            if (!L && !R && T && B) return tb;
            if (!L && !R && T && !B) return t;
            if (!L && !R && !T && B) return b;
            
            return none;
        }
    }
}