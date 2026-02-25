using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Map;
using Game.Object;
using Game.Object.Character;
using Game.Room;
using Game.School;
using Game.Time;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Character player;
        [SerializeField] private CharacterData initPlayer;
        [SerializeField] private StringFloatDict vars = new();

        public readonly CancellationTokenSource Global = new();

        [SerializeField] public Camera mainCamera;

        public Action afterInit = () => { };

        public Character Player => player;

        public static Transform TEMP => GameObject.Find("TEMP")?.transform ?? new GameObject("TEMP").transform;

        public Action OnGameStart = () => { };
        public Action<Character> OnSetPlayer = (_) => { };

        private void Awake()
        {
            Global.RegisterRaiseCancelOnDestroy(gameObject);
        }

        private void Start()
        {
            MapController.Instance.Init();
            RoomManager.Instance.Init();
            ObjectManager.Instance.Init();
            NavManager.Instance.Init();
            
            SchoolManager.Instance.Init();
            
            player = ObjectManager.Instance.Find(initPlayer.ID) as Character;
            SetPlayer(player);

            afterInit();
        }

        public void SetPlayer(Character newPlayer)
        {
            player = newPlayer;
            player.ControllerType = ControllerType.AutoPilot;
            
            OnSetPlayer(player);
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