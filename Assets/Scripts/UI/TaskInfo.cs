using Game.Task;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class TaskInfo : UIBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button cancelButton;
        [SerializeField] private string taskID;

        public void Init(ITask actionTask)
        {
            if (actionTask == null) return;
            taskID = actionTask.ID;

            text.text = actionTask.Desc;
            cancelButton.onClick.AddListener(OnClickCancelButton);
        }

        private void OnClickCancelButton()
        {
            GameManager.Instance.Player.TaskQueue.Cancel(taskID);
        }
    }
}