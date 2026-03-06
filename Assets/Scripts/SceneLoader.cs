using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private int titleScene = 0;
        [SerializeField] private int playScene = 1;
        [SerializeField] private int editScene = 2;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(int idx)
        {
            SceneManager.LoadScene(idx);
            TooltipBox.Instance.gameObject.SetActive(false);
        }

        public void LoadTitle()
        {
            LoadScene(titleScene);
        }

        public void LoadPlay()
        {
            LoadScene(playScene);
        }

        public void LoadEdit()
        {
            LoadScene(editScene);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}