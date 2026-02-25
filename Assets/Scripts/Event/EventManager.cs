using System.Collections.Generic;
// using System.Linq;
using UnityEngine;

namespace Game.Event
{
    public class EventManager :Singleton<EventManager>
    {
        [SerializeReference] public List<Event> events = new();

        public SimpleEvent CreateSimpleEvent(EventBase e)
        {
            var ret = new SimpleEvent(e);
            ret.Init();
            AddEvent(ret);

            return ret;
        }

        public void AddEvent(Event e)
        {
            e.Init();
            events.Add(e);
        }

        private void LateUpdate()
        {
            var neoList = new List<Event>(events.Count);   
            
            foreach (var e in events)
            {
                e.Update();
                
                if ((!e.zombie && e.members.Count == 0) || e.Status == EventStatus.Done)
                {
                    e.Finish();
                    
                    continue;
                }
                
                neoList.Add(e);
            }

            events = neoList;
        }

        public Event Find(string id)
        {
            return events.Find(e => e.ID == id);
        }
    }
}