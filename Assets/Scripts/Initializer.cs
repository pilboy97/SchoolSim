using System;
using Game.Debug;
using Game.Event;
using Game.Object;
using Game.Room;
using Game.School;
using Game.Time;
using UnityEngine;

namespace Game
{
    public class Initializer : MonoBehaviour
    {
        [SerializeField] private GameManager gameManger;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private ObjectManager objectManager;
        [SerializeField] private DebugSystem debugSystem;
        [SerializeField] private EventManager eventManager;
        [SerializeField] private NavManager navManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private SchoolManager schoolManager;
        [SerializeField] private ScheduleManager scheduleManager;
        [SerializeField] private Camera mainCamera;

        private void Awake()
        {
            var c = Instantiate(mainCamera);
            var g = Instantiate(gameManger);
            Instantiate(inputManager);
            Instantiate(timeManager);
            Instantiate(roomManager);
            Instantiate(objectManager);
            Instantiate(debugSystem);
            Instantiate(eventManager);
            Instantiate(navManager);
            Instantiate(uiManager);
            Instantiate(schoolManager);
            Instantiate(scheduleManager);

            g.mainCamera = c;
        }

        private void Start()
        {
            GameManager.Instance.Init();
            
            Destroy(gameObject);
        }
    }
}