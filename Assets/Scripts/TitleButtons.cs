using UnityEngine;

namespace Game
{
    public class TitleButtons : MonoBehaviour
    {
        public void LoadEdit()
        {
            SceneLoader.Instance.LoadEdit();
        }

        public void LoadPlay()
        {
            SceneLoader.Instance.LoadPlay();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}