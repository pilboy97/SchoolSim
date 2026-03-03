using System;
using System.IO;
using Game.Object.Character;
using UnityEngine;

namespace Game
{
    public class NameGenerator : Singleton<NameGenerator>
    {
        [SerializeField] private string[] fNames;
        [SerializeField] private string[] mNames;
        [SerializeField] private TextAsset nameFile;
        
        
        public void Init()
        {
            var lines = nameFile.text.Split('\n');

            mNames = lines[0].Split(",");
            fNames = lines[1].Split(",");
        }
        
        public string RandomName(Gender gender)
        {
            return gender switch
            {
                Gender.Male => Random.Choose(mNames),
                Gender.Female => Random.Choose(fNames),
                _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
            };
        }
    }
}