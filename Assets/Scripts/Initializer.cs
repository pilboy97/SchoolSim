using System;
using Game.Debug;
using Game.Event;
using Game.Map;
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
        [SerializeField] private MapController mapController;
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
            
            Instantiate(inputManager).Init();
            Instantiate(timeManager).Init();
            Instantiate(debugSystem);
            Instantiate(eventManager);
            Instantiate(scheduleManager);
            
            Instantiate(mapController).Init();
            Instantiate(roomManager).Init();
            Instantiate(objectManager).Init();
            Instantiate(navManager).Init();
            Instantiate(schoolManager).Init();
            Instantiate(uiManager).Init();
            
            Instantiate(gameManger).Init(); 
            Instantiate(mainCamera);
        }

        private void Start()
        {
            Destroy(gameObject);
        }
    }
}