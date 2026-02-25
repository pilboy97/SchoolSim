using System.Collections.Generic;
using Game.Task;
using UnityEngine;

namespace Game.Time
{
    [CreateAssetMenu(menuName = "Event/Schedule/Repeat Schedule")]
    public class RepeatSchedule : Schedule
    {
        public MonthFlag monthCond;
        public int weekCond;
        public int dateCond;
        public DayFlag dayCond;

        public override Condition DefaultTimeCond
        {
            get {
                var dCond = new DayFlagCond()
                {
                    flag = this.dayCond
                };
                var tCond = new DailyTimeRangeCond()
                {
                    begin = (ulong)start,
                    end = (ulong)end,
                };
                var cond = new AndCond()
                {
                    conds = new List<Condition>()
                    {
                        dCond,
                        tCond
                    }
                };
                return cond;
            }
        }
    }
}