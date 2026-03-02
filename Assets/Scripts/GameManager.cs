using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Event.Talk;
using Game.Map;
using Game.Object;
using Game.Object.Character;
using Game.Room;
using Game.School;
using Game.Time;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class GameManager : Singleton<GameManager>
    {
        private Character _player;
        [SerializeField] private CharacterData initPlayer;
        [SerializeField] private StringFloatDict vars = new();

        public readonly CancellationTokenSource Global = new();

        [SerializeField] public Camera mainCamera;

        public Character Player => _player;

        public static Transform TEMP => GameObject.Find("TEMP")?.transform ?? new GameObject("TEMP").transform;

        public Action OnGameStart = () => { };
        public Action<Character> OnSetPlayer = (ch) => { UnityEngine.Debug.Log($"player changed {ch.charName}"); };

        protected void Awake()
        {
            Global.RegisterRaiseCancelOnDestroy(gameObject);
            Init();
        }

        public void Init()
        {
            MapController.Instance.Init();
            RoomManager.Instance.Init();
            ObjectManager.Instance.Init();
            NavManager.Instance.Init();

            ScheduleManager.Instance.Init();
            
            SchoolManager.Instance.Init();
            
            var player = ObjectManager.Instance.Find(initPlayer?.ID ?? "") as Character;
            SetPlayer(player);

            UIManager.Instance.Init();
            
            TalkWindow.Instance.Init();

            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(true);
            }
        }

        public void SetPlayer(Character newPlayer)
        {
            
            if (_player != null)
                _player.ControllerType = ControllerType.AI;
            _player = newPlayer;
            
            if (_player == null) return;
            
            _player.ControllerType = ControllerType.Player;
            
            OnSetPlayer(_player);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TimeManager.Instance.TogglePause();
            }
        }

        private string GetValueName(string id, string name) => $"{id}|{name}";
        public float GetVar(string id, string name)
        {
            var key = GetValueName(id, name);
            if (vars.TryGetValue(key, out var value)) return value;

            vars.TryAdd(key, 0);
            return 0;
        }
        public void SetVar(string id, string name, float value)
        {
            var key = GetValueName(id, name);
            vars[key] = value;
        }
        
        public float GetVar(string name)
        {
            if (vars.TryGetValue(name, out var value)) return value;

            vars.TryAdd(name, 0);
            return 0;
        }
        public void SetVar(string name, float value)
        {
            vars[name] = value;
        }

        public int CountValue(string id)
        {
            int cnt = 0;
            foreach (var kv in vars)
            {
                if (kv.Key.Contains($"{id}|")) cnt++;
            }

            return cnt;
        }
        public void ClearValue(string id)
        {
            if (CountValue(id) == 0) return;
            
            StringFloatDict newVars = new();

            foreach (var kv in vars)
            {
                if (!kv.Key.Contains($"{id}|")) newVars[kv.Key] = kv.Value;
            }

            vars = newVars;
        }

        private static bool _isQuitting = false;
        public static bool IsQuitting => _isQuitting;

        protected void OnApplicationQuit()
        {
            _isQuitting = true;
        }

#if UNITY_EDITOR
        public void InitOnEditorMode(bool clearRoom)
        {
            MapController.Instance.Init(clearRoom);
            ObjectManager.Instance.Init();
            
            if (clearRoom)
            {
                ObjectManager.Instance.Clear();
            }

            NavManager.Instance.SetWalkableCell();
        }
#endif
    }
}