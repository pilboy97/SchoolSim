using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Game.Object.Character;
using UnityEngine;

namespace Game
{
    public static class NameGenerator
    {
        private static string[] _fNames;
        private static string[] _mNames;
        private static readonly string path = Application.dataPath + "/Resources/CommonNames.csv";
        
        
        static NameGenerator()
        {
            var lines = File.ReadAllLines(path);

            _mNames = lines[0].Split(",");
            _fNames = lines[1].Split(",");
        }
        public static string RandomName(Gender gender)
        {
            return gender switch
            {
                Gender.Male => Random.Choose(_mNames),
                Gender.Female => Random.Choose(_fNames),
                _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
            };
        }
    }
}