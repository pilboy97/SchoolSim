using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UI.Table
{
    [CreateAssetMenu(menuName = "UI/Table Data")]
    public class TableData : ScriptableObject
    {
        [SerializeField] public int row;
        [SerializeField] public int col;

        public int Length => row * col;

        [SerializeField] private string[] data;

        public Action OnUpdateHandler = () => { };

        public void Update()
        {
            OnUpdateHandler();
        }

        public string this[int r, int c]
        {
            get => data[r * col + c];
            set
            {
                data[r * col + c] = value;
                OnUpdateHandler();
            }
        }

        [Button("Init")]
        public void Init()
        {
            var old = data;
            data = new string[row * col];

            if (old == null) return;
            for (int i = 0; i < math.min(old.Length, data.Length); i++)
            {
                data[i] = old[i];
            }

            OnUpdateHandler();
        }
    }
}