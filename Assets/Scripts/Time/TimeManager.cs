using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Time
{
    public class TimeManager : Singleton<TimeManager>
    {
        [SerializeField] private float secPerTick = 0.5f;
        [SerializeField] private float time;
        [SerializeField] private int ticks; //1 tick = 1 sec in game
        [SerializeField] private float scale = 1f;
        [SerializeField] private float maxTimeScale = 100f;
        [SerializeField] private float minTimeScale = 0.16f;
        [SerializeField] public bool isPaused;
        [SerializeField] public int deltaScale = 1;

        public float TickForSecs(float sec)
        {
            return sec / secPerTick;
        }

        public float SecForTicks(int ticks)
        {
            return ticks * secPerTick;
        }
        
        public int Ticks => ticks;

        private void Start()
        {
            deltaScale = 1;
        }

        private void Update()
        {
            time += UnityEngine.Time.deltaTime;

            while (time >= secPerTick)
            {
                ticks += deltaScale;

                time -= secPerTick;
            }
        }

        public string ToTimeString()
        {
            return "1-1-1 (SUN) 00:00 AM";
        }

        public void Faster()
        {
            if (isPaused) SetPause(false);
            
            scale *= 1.5f;
            scale = Mathf.Clamp(scale, minTimeScale, maxTimeScale);
            UnityEngine.Time.timeScale = scale;
        }

        public void Slower()
        {
            if (isPaused) SetPause(false);
            
            scale /= 1.5f;
            scale = Mathf.Clamp(scale, minTimeScale, maxTimeScale);
            UnityEngine.Time.timeScale = scale;
        }

        public void TogglePause()
        {
            SetPause(!isPaused);
        }

        public void SetPause(bool val)
        {
            var old = isPaused;
            if (old == val) return;
            
            isPaused = val;
            if (isPaused)
            {
                DOTween.TogglePauseAll();
                UnityEngine.Time.timeScale = 0;
            }
            else
            {
                DOTween.TogglePauseAll();
                UnityEngine.Time.timeScale = scale;
            }
        }

        public void Jump(int dest)
        {
            if (ticks >= dest) return;

            ticks = dest;
        }

        public void Add(int delta)
        {
            Jump(ticks + delta);
        }

        public void NextDay()
        {
            Add(1440);
        }
        
        public static async UniTask WaitForTicks(CancellationToken token, int ticks)
        {
            if (token.IsCancellationRequested) return;

            ticks += TimeManager.Instance.ticks;
            
            await UniTask.WaitWhile(() => Instance.ticks < ticks, cancellationToken: token);
        }

        public void SetTimeScale(float val)
        {
            if (val == 0)
            {
                SetPause(true);
                return;
            }
            else
            {
                if (isPaused) SetPause(false);
            }
            
            scale = Mathf.Clamp(val, minTimeScale, maxTimeScale);
            UnityEngine.Time.timeScale = scale;
        }
        public void SetTimeScale0_1()
        {
            deltaScale = 1;
            SetTimeScale(0.1f);
        }
        public void SetTimeScale1()
        {
            deltaScale = 1;
            SetTimeScale(1f);
        }
        public void SetTimeScale10()
        {
            deltaScale = 1;
            SetTimeScale(10f);
        }
        public void SetTimeScale50()
        {
            deltaScale = 1;
            SetTimeScale(50f);
        }

        public void SetTimeScaleMaximum()
        {
            deltaScale = 30;
            SetTimeScale(100f);
        }

        public void Init()
        {
            
        }
    }
}