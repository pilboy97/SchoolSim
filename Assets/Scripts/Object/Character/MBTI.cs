using System;

namespace Game.Object.Character
{
    public enum MBTIComponent
    {
        None,
        E,
        I,
        S,
        N,
        T,
        F,
        J,
        P,
    }

    public enum MBTI
    {
        ESTJ = 0b0000,
        ESTP = 0b0001,
        ESFJ = 0b0010,
        ESFP = 0b0011,
        ENTJ = 0b0100,
        ENTP = 0b0101,
        ENFJ = 0b0110,
        ENFP = 0b0111,
        ISTJ = 0b1000,
        ISTP = 0b1001,
        ISFJ = 0b1010,
        ISFP = 0b1011,
        INTJ = 0b1100,
        INTP = 0b1101,
        INFJ = 0b1110,
        INFP = 0b1111,
    }

    public static class MBTIHelper
    {
        public static MBTIComponent[] ToComponents(this MBTI mbti)
        {
            var ret = new MBTIComponent[4];

            if (((int)mbti & 0b1000) != 0) ret[0] = MBTIComponent.I;
            else ret[0] = MBTIComponent.E;
            if (((int)mbti & 0b0100) != 0) ret[1] = MBTIComponent.N;
            else ret[1] = MBTIComponent.S;
            if (((int)mbti & 0b0010) != 0) ret[2] = MBTIComponent.F;
            else ret[2] = MBTIComponent.T;
            if (((int)mbti & 0b0001) != 0) ret[3] = MBTIComponent.P;
            else ret[3] = MBTIComponent.J;

            return ret;
        }

        public static MBTI FromComponents(this MBTIComponent[] components)
        {
            UnityEngine.Debug.Assert(components.Length != 4, "Wrong MBTI");

            int ret = 0;
            foreach (var comp in components)
            {
                switch (comp)
                {
                    case MBTIComponent.I:
                        ret |= 0b1000;
                        break;
                    case MBTIComponent.N:
                        ret |= 0b0100;
                        break;
                    case MBTIComponent.F:
                        ret |= 0b0010;
                        break;
                    case MBTIComponent.P:
                        ret |= 0b0001;
                        break;
                }
            }

            return (MBTI)ret;
        }

        public static MBTI RandomMBTI()
        {
            return Random.ChooseEnum<MBTI>();
        }

        public static MBTI GenerateMBTI(MBTIComponent[] cond)
        {
            var fix = new bool[4];
            int ret = 0;

            foreach (var comp in cond)
            {
                switch (comp)
                {
                    case MBTIComponent.E or MBTIComponent.I:
                        fix[3] = true;

                        ret |= (comp == MBTIComponent.I) ? 0b1000 : 0;
                        break;
                    case MBTIComponent.S or MBTIComponent.N:
                        fix[2] = true;

                        ret |= (comp == MBTIComponent.N) ? 0b0100 : 0;
                        break;
                    case MBTIComponent.T or MBTIComponent.F:
                        fix[1] = true;

                        ret |= (comp == MBTIComponent.F) ? 0b0010 : 0;
                        break;
                    case MBTIComponent.J or MBTIComponent.P:
                        fix[0] = true;

                        ret |= (comp == MBTIComponent.P) ? 0b0001 : 0;
                        break;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (!fix[i])
                {
                    int r = UnityEngine.Random.Range(0, 2);

                    ret |= (r != 0) ? (1 << i) : 0;
                }
            }

            return (MBTI)ret;
        }

        public static bool CheckComponent(this MBTI mbti, MBTIComponent comp)
        {
            int val = (int)mbti;
            switch (comp)
            {
                case MBTIComponent.I: return (val & 0b1000) != 0; // 8이면 True
                case MBTIComponent.E: return (val & 0b1000) == 0; // 0이면 True
                case MBTIComponent.N: return (val & 0b0100) != 0;
                case MBTIComponent.S: return (val & 0b0100) == 0;
                case MBTIComponent.F: return (val & 0b0010) != 0;
                case MBTIComponent.T: return (val & 0b0010) == 0;
                case MBTIComponent.P: return (val & 0b0001) != 0;
                case MBTIComponent.J: return (val & 0b0001) == 0;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToString(this MBTI mbti)
        {
            return Enum.GetName(typeof(MBTI), mbti);
        }
    }
}