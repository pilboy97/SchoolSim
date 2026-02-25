using System.Collections.Generic;

namespace Game
{
    public interface IHasID
    {
        const int IDLen = 10;
        static char[] IDCharset;

        static IHasID()
        {
            var charSet = new List<char>();

            for (var i = 'a'; i < 'z'; i++) charSet.Add(i);

            for (var i = 'A'; i < 'Z'; i++) charSet.Add(i);

            for (var i = '0'; i < '9'; i++) charSet.Add(i);

            charSet.Add('_');
            charSet.Add('.');

            IDCharset = charSet.ToArray();
        }

        string ID { get; }

        static string GenerateID()
        {
            List<char> ret = new();
            for (int i = 0; i < IDLen; i++)
            {
                int id = UnityEngine.Random.Range(0, IDCharset.Length);
                
                ret.Add(IDCharset[id]);
            }

            return new string(ret.ToArray());
        }
    }
}