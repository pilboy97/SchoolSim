namespace Game.Time
{
    public static class Calendar
    {
        private static readonly int[] _monthDays =
        {
            0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
        };

        public static int ToTick(int hour, int min, int sec)
        {
            return (int)hour * 3600 + (int)min * 60 + (int)sec;
        }
        
        public static (int, int, int) ToTime(int tick)
        {
            tick %= 24 * 3600;

            return ((int)tick / 3600, (int)(tick / 60) % 60, (int)tick % 60);
        }

        public static (int, int, int) ToTime()
        {
            return ToTime(TimeManager.Instance.Ticks);
        }

        public static (int, int, int, int, DayFlag) ToDate(int tick)
        {
            var days = tick / (24 * 60 * 60);
            var years = days / 365;

            var y = (int)years;
            int m;
            var d = (int)days % 365;
            
            for (m = 1; m <= 12; m++)
                if (d > _monthDays[m])
                    d -= _monthDays[m];
                else break;

            var day = (int)days % 7;
            var dayFlag = day switch
            {
                1 => DayFlag.Mon,
                2 => DayFlag.Tue,
                3 => DayFlag.Wed,
                4 => DayFlag.Thu,
                5 => DayFlag.Fri,
                6 => DayFlag.Sat,
                _ => DayFlag.Sun
            };

            return (y, m, (d / 7) + 1, d+1, dayFlag);
        }

        public static bool IsAm(int ticks)
        {
            var (h, _, _) = ToTime(ticks);
            return h < 12;
        }

        public static (int, int, int, int,DayFlag) ToDate()
        {
            return ToDate(TimeManager.Instance.Ticks);
        }

        public static string CalendarString(int ticks)
        {
            var (y, m, _, d, dayFlag) = ToDate(ticks);
            var (h, mm, s) = ToTime(ticks);
            
            var dayString = dayFlag switch
            {
                DayFlag.Mon => "Mon",
                DayFlag.Tue => "Tue",
                DayFlag.Wed => "Wed",
                DayFlag.Thu => "Thu",
                DayFlag.Fri => "Fri",
                DayFlag.Sat => "Sat",
                _ => "Sun"
            };
            var amString = IsAm(ticks) ? "AM" : "PM";

            return $"{y:00}-{m:00}-{d:00} ({dayString}) {h:00}:{mm:00}:{s:00} {amString}";
        }

        public static string CalendarString()
        {
            return CalendarString(TimeManager.Instance.Ticks);
        }
    }
}