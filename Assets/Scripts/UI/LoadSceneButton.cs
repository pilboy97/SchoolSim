using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField] private int sceneIdx;
        [SerializeField] private Button btn;

        private void Awake()
        {
            btn.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene(sceneIdx);
            });
        }
    }
}