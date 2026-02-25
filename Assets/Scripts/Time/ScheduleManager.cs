using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Game.Event;
using UnityEngine;
using Action = Game.Task.Action;

namespace Game.Time
{
    public class ScheduleManager : Singleton<ScheduleManager>
    {
        [Serializable]
        private class MonthFlagScheduleListDict : UnitySerializedDictionary<MonthFlag, List<Schedule>>
        {
            
        }

        [Serializable]
        private class IntScheduleListDict : UnitySerializedDictionary<int, List<Schedule>>
        {
            
        }

        [Serializable]
        private class DayFlagScheduleListDict : UnitySerializedDictionary<DayFlag, List<Schedule>>
        {
            
        }
        
        [SerializeReference] private MonthFlagScheduleListDict monthCache = new ();
        [SerializeReference] private IntScheduleListDict weekCache = new ();
        [SerializeReference] private IntScheduleListDict dateCache = new ();
        [SerializeReference] private DayFlagScheduleListDict dayCache = new ();
        
        [SerializeField] private ulong last = 0;
        [SerializeReference] private List<Event.Event> activated = new();

        private void Awake()
        {
            for (int i = 0; i < 12; i++)
            {
                monthCache.Add((MonthFlag)(1<<i), new List<Schedule>());
            }
            for (int i = 0; i < 7; i++)
            {
                dayCache.Add((DayFlag)(1<<i), new List<Schedule>());
            }

            for (int i = 0; i < 31; i++)
            {
                dateCache.Add(i, new List<Schedule>());
            }

            for (int i = 0; i < 5; i++)
            {
                weekCache.Add(i, new List<Schedule>());
            }
        }

        public void AddSchedule(Schedule schedule)
        {
            schedule.Init();
            
            if (schedule is RepeatSchedule r)
            {
                var mflag = r.monthCond;
                for (int i = 0; i < 12; i++)
                {
                    int f = 1 << i;
                    if (((int)mflag & f) != 0)
                    {
                        monthCache[(MonthFlag)f].Add(schedule);
                    }
                }

                var wFlag = r.weekCond;
                for (int i = 0; i < 12; i++)
                {
                    int f = 1 << i;
                    if ((wFlag & f) != 0)
                    {
                        weekCache[i].Add(schedule);
                    }
                }

                var dFlag = r.dateCond;
                for (int i = 0; i < 31; i++)
                {
                    int f = 1 << i;
                    if ((dFlag & f) != 0)
                    {
                        dateCache[i].Add(schedule);
                    }
                }

                var dayFlag = r.dayCond;
                for (int i = 0; i < 7; i++)
                {
                    int f = 1 << i;
                    if (((int)dayFlag & f) != 0)
                    {
                        dayCache[(DayFlag)f].Add(schedule);
                    }
                }

                return;
            }

            var (_, m,w,d,day) = Calendar.ToDate(schedule.start);
            
            monthCache[(MonthFlag)(1 << (m - 1))].Add(schedule);
            weekCache[w - 1].Add(schedule);
            dateCache[d - 1].Add(schedule);
            dayCache[day].Add(schedule);
        }

        public void RemoveSchedule(Schedule schedule)
        {
            if (schedule is RepeatSchedule r)
            {
                var mflag = r.monthCond;
                for (int i = 0; i < 12; i++)
                {
                    int f = 1 << i;
                    if (((int)mflag & f) != 0)
                    {
                        monthCache[(MonthFlag)f].Remove(schedule);
                    }
                }

                var wFlag = r.weekCond;
                for (int i = 0; i < 12; i++)
                {
                    int f = 1 << i;
                    if ((wFlag & f) != 0)
                    {
                        weekCache[i].Remove(schedule);
                    }
                }

                var dFlag = r.dateCond;
                for (int i = 0; i < 31; i++)
                {
                    int f = 1 << i;
                    if ((dFlag & f) != 0)
                    {
                        dateCache[i].Remove(schedule);
                    }
                }

                var dayFlag = r.dayCond;
                for (int i = 0; i < 7; i++)
                {
                    int f = 1 << i;
                    if (((int)dayFlag & f) != 0)
                    {
                        dayCache[(DayFlag)f].Remove(schedule);
                    }
                }

                return;
            }

            var (_, m,w,d,day) = Calendar.ToDate(schedule.start);
            
            monthCache[(MonthFlag)(1 << (m - 1))].Remove(schedule);
            weekCache[w - 1].Remove(schedule);
            dateCache[d - 1].Remove(schedule);
            dayCache[day].Remove(schedule);
        }

        public void Start()
        {
            LoadTodaySchedule(0);
            last = 0;
        }

        public void Update()
        {
            var t = TimeManager.Instance.Ticks;
            if (t - last <= 24 * 60 * 60) return;
            
            PurgeZombie();
            LoadTodaySchedule(t);
            last = t;
        }

        private void PurgeZombie()
        {
            foreach (var e in activated)
            {
                if (e.Status != EventStatus.Run) e.Finish(true);
            }
            
            activated.Clear();
        }
        
        private void LoadTodaySchedule(ulong t)
        {
            var (_, m, w, d, day) = Calendar.ToDate(t);

            var mList = monthCache[(MonthFlag)(1 << (m - 1))];
            var wList = weekCache[w - 1];
            var dList = dateCache[d - 1];
            var dayList = dayCache[day];

            var enumerable = mList.Concat(wList).Concat(dList).Concat(dayList);
            HashSet<Schedule> set = new();

            foreach (var schedule in enumerable)
            {
                schedule.zombie = true;
                set.Add(schedule);
            }

            foreach (var schedule in set)
            {
                var e = EventManager.Instance.CreateSimpleEvent(schedule);
                activated.Add(e);
            }
        }
    }
}