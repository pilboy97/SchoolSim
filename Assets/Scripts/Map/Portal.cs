using System;
using System.Linq;
using Game.Object;
using Game.Task;
using UnityEngine;
using Action = Game.Task.Action;

namespace Game.Map
{ 
    [Serializable]
    public class Portal : ScriptableObject, IInteractable
    {
        [SerializeField] public Portal dest;
        [SerializeField] public Vector3Int position;

        [SerializeField] private string id;
        
        public string ID => id;
        public void Init()
        {
            id ??= IHasID.GenerateID();
        }

        public string Name => "";
        public Vector3[] Positions => new []{ MapController.Instance.CellToWorld(position) };
        public Vector3Int[] CPositions => new[] { position };
        public Vector3 CenterPosition => Positions[0];
        public Vector3Int Dest => dest.position;
        
        public Action[] Actions => new[]
        {
            new Action
            {
                effect = new JumpEffect()
                {
                    position = Dest
                }
            }
        };

        public int ZIndex { get=>position.z; set => position.z = value; }
    }
}