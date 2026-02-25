using System;
using System.Collections.Generic;
using Game.UI.Table; 
using UnityEngine; 
using Calendar = Game.Time.Calendar;

namespace Game.Debug
{
    public interface ITable
    {
        string[] ToTable();
    }
    
    public struct SelectLogData : ITable
    {
        public ulong Tick;
        public string Who;
        public (string, float)[] Actions;
        
        public string[] ToTable()
        {
            string[] ret = new string[Actions.Length * 3 + 3];

            ret[0] = Calendar.CalendarString(Tick);
            ret[1] = Who;

            for (int i = 0; i < Actions.Length; i++)
            {
                var (action, score) = Actions[i];
                
                ret[3 * (i + 1) + 1] = action;
                ret[3 * (i + 1) + 2] = score.ToString("F2");
            }
            
            return ret;
        }
    }

    public class DebugSystem : Singleton<DebugSystem>
    {
        [SerializeField] private RectTransform debugPanel;
        [SerializeField] private TableData logData;
        [SerializeField] private Table table;

        [SerializeField] private bool debugMode;
        
        public List<ITable> Datas = new();
        
        #if UNITY_EDITOR
        private void Awake()
        {
            logData = ScriptableObject.CreateInstance<TableData>();
            table.Init(logData);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                debugMode = !debugMode;
            
                if (debugMode) Activate();
                else Deactivate();
            }
        }

        private void LateUpdate()
        {
            var res = new List<string>();

            if (!debugMode)
            {
                Datas.Clear();
                return;
            }

            if (Datas.Count == 0) return;
            
            logData.row = 0;
            logData.col = 0;
            logData.Init();
            
            foreach (var data in Datas)
            {
                res.AddRange(data.ToTable());
            }

            logData.row = res.Count / 3;
            logData.col = 3;
            logData.Init();

            for (int i = 0; i < res.Count; i++)
            {
                var (r, c) = (i / 3, i% 3);

                logData[r, c] = res[i];
            }
            
            Datas.Clear();
            logData.Update();
        }

        public void Activate()
        {
            debugPanel.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            debugPanel.gameObject.SetActive(false);
        }
#endif
    }
}