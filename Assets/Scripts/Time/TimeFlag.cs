using System;

namespace Game.Time
{
    [Flags]
    public enum MonthFlag
    {
        None = 0,
        Jan = 1 << 0,
        Feb = 1 << 1,
        Mar = 1 << 2,
        Apr = 1 << 3,
        May = 1 << 4,
        Jun = 1 << 5,
        Jul = 1 << 6,
        Aug = 1 << 7,
        Sep = 1 << 8,
        Oct = 1 << 9,
        Nov = 1 << 10,
        Dec = 1 << 11,
        Spring = Mar | Apr | May,
        Summer = Jun | Jul | Aug,
        Autumn = Sep | Oct | Nov,
        Winter = Dec | Jan | Feb
    }

    [Flags]
    public enum DayFlag
    {
        None = 0,
        Mon = 1 << 0,
        Tue = 1 << 1,
        Wed = 1 << 2,
        Thu = 1 << 3,
        Fri = 1 << 4,
        Sat = 1 << 5,
        Sun = 1 << 6,
        Weekday = Mon | Tue | Wed | Thu | Fri,
        Weekend = Sat | Sun
    }

    public static class DayFlagHelper
    {
        public static bool Check (this DayFlag x, DayFlag y)
        {
            return (x & y) != 0;
        }
    }
}