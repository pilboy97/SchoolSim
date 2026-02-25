using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI.Table
{
    public class Table : UIBehaviour
    {
        private int _row, _col;
        private RectTransform _rectTransform;
        
        [SerializeField] public BorderSpriteStyle style;
        [SerializeField] public TableData data;
        [SerializeField] private TableCell cellPrefab;
        [SerializeField] private GridLayoutGroup cellRoot;
        [SerializeField] public Vector2 cellSize = new Vector2(100, 100);
        
        [SerializeField] public List<TableCell> cells;

        private ObjectPool<TableCell> _objectPool;
        
        protected override void Awake()
        {
            base.Awake();
            
            Init(data);
        }

        public void Init(TableData data)
        {
            _objectPool = new ObjectPool<TableCell>(
                createFunc: () =>Instantiate(cellPrefab),
                actionOnGet: (o) => { o.gameObject.SetActive(true); o.transform.SetParent(cellRoot.transform); },
                actionOnRelease: (o)=>{ o.gameObject.SetActive(false); o.transform.SetParent(GameManager.TEMP); },
                actionOnDestroy: (o)=>Destroy(o.gameObject),
                collectionCheck: true, 
                defaultCapacity: 1000,
                maxSize: 5000);
            
            if (_rectTransform == null)
                _rectTransform = cellRoot.GetComponent<RectTransform>();
    
            this.data = data;

            if (data != null)
            {
                data.OnUpdateHandler += Draw;
                
                Draw();
            }
        }
        
        private void Draw()
        {
            _row = data.row;
            _col = data.col;
            
            var size = new Vector2(_col * cellSize.x, _row * cellSize.y);
            _rectTransform.sizeDelta = size;
            
            if (data.row <= 0 || data.col <= 0) return;

            if (cells.Count < data.Length)
                for (int i = cells.Count; i < data.Length; i++)
                    cells.Add(_objectPool.Get());
            else
                for (int i = cells.Count - 1; i >= data.Length; i--)
                {
                    _objectPool.Release(cells[i]);
                    cells.Remove(cells[i]);
                }

            cellRoot.cellSize = cellSize;
            cellRoot.spacing = Vector2.zero;
            cellRoot.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cellRoot.constraintCount = _col;

            for (int i = 0; i < data.Length; i++)
            {
                var (r, c) = (i / data.col, i % data.col);
                BorderDirection b = BorderDirection.Left | BorderDirection.Top;
                if (r == data.row - 1) b |= BorderDirection.Bottom;
                if (c == data.col - 1) b |= BorderDirection.Right;

                cells[i].Init(this, r, c, b);
            }
        }
    }
}