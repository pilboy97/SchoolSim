using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private int titleScene = 0;
        [SerializeField] private int playScene = 1;

        public void LoadScene(int idx)
        {
            SceneManager.LoadScene(idx);
        }

        public void LoadTitle()
        {
            LoadScene(titleScene);
        }

        public void LoadPlay()
        {
            LoadScene(playScene);
        }
    }
}