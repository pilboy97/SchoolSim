using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class Timer : UIBehaviour
    {
        public bool stopWhenPause = true;
        public bool unscaled = false;
        public float goalTime = 3f;
        public float curTime = 0f;
        [SerializeField] private bool isDone;

        [SerializeField] private bool start = false;

        public bool IsDone => isDone;

        [SerializeField] private ProgressBar bar;

        public Action onDone = () => { };

        public void Init()
        {
            curTime = 0;
            bar.min = 0;
            bar.max = goalTime;
            bar.value = curTime;
            isDone = false;
            
            start = true;
        }

        public void Update()
        {
            if (!start) return;
            if (stopWhenPause && UnityEngine.Time.timeScale == 0) return;
            
            curTime += (!unscaled)?UnityEngine.Time.deltaTime:UnityEngine.Time.unscaledDeltaTime;
            bar.value = curTime;
        }

        private void LateUpdate()
        {
            if (!start) return;
            
            if (curTime >= goalTime)
            {
                isDone = true;
                onDone();
                return;
            }
        }
    }
}