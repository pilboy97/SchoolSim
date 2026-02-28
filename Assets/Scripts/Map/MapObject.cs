using System;
using Game.Object;
using TMPro;
using UnityEngine;

namespace Game.Map
{
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(Map))]
    public class MapObject : MonoBehaviour, IInteractable, IHasID
    {
        [SerializeField] public int zIndex;
        [SerializeField] public ObjectData obj;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private string id;

        public string ID => id;
        
        private Vector3 _center;
        private Vector3Int[] _cPositions;
        private Vector3[] _positions;
        
        private Grid _grid;
        private Map _map;
        
        public Vector3Int[] CPositions => _cPositions;
        public Vector3[] Positions => _positions;
        public string Name => obj?.objectName ?? "";

        public Vector3 CenterPosition
        {
            get => _center;
            set
            {
                var old = CenterPosition;
                var delta = value - old;

                transform.position += delta;
            }
        }

        public Vector3 Orig
        {
            get => _grid.CellToWorld(Vector3Int.zero) + transform.position;
            set => transform.position = value;
        }
        public Task.Action[] Actions => obj?.actions ?? Array.Empty<Task.Action>();
        public int ZIndex { get => CPositions[0].z; set => SetZIndex(value); }

        private void SetZIndex(int idx)
        {
            zIndex = idx;
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            id = IHasID.GenerateID();
            
            _grid = GetComponent<Grid>();
            _map = GetComponent<Map>();

            _map.Init(false);
            var cells = _map.Cells();

            var sum = Vector3.zero;

            _positions = new Vector3[cells.Length];
            _cPositions = new Vector3Int[cells.Length];
            
            for(int i = 0;i < cells.Length;i++)
            {
                _positions[i] = MapController.Instance.CellToWorld(cells[i]) + transform.position;
                _cPositions[i] = MapController.Instance.WorldToCell(Positions[i]);

                sum += Positions[i];
            }

            _center = sum / cells.Length;
            label.text = obj?.objectName ?? "";
        }
    }
}