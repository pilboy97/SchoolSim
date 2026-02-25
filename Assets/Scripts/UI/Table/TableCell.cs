using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class TableCell : BorderSpriteImage
    {
        [SerializeField] private Table.Table table;
        [SerializeField]private TextMeshProUGUI text;
        [SerializeField] private string content;
        
        [ShowInInspector] public int row, col;

        protected override void Awake()
        {
            base.Awake();

            text = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Init(Table.Table table, int r, int c, BorderDirection border)
        {
            gameObject.SetActive(true);
            this.table = table;
            
            style = this.table.style;
            this.border = border;
            
            row = r;
            col = c;
            
            content = this.table.data[row, col] ?? "";

            text.text = content;
            
            Draw();
        }
    }
}