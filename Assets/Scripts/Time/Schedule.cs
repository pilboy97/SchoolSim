using System;
using System.Collections.Generic;
using Game.Event;
using Game.Task;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Time
{
    [CreateAssetMenu(menuName = "Event/Schedule/Schedule")]
    public class Schedule : EventBase
    {
        public string desc;

        public int start;
        public int end;

        public virtual Condition DefaultTimeCond =>
            new TimeRangeCond()
            {
                begin = start,
                end = end,
            };
        
        [ShowInInspector] private string StartString => Calendar.CalendarString(start);
        [ShowInInspector] private string EndString => Calendar.CalendarString(end);
        
        public void Init()
        {
            runCond = new AndCond(){
                conds = {
                    runCond,
                    DefaultTimeCond
                }
            };
        }
    }
}