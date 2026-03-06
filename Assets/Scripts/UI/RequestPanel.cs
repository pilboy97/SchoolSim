using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class RequestPanel : UIBehaviour
    {
        [SerializeField] private Timer timer;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private float timeLimit;
        [SerializeField] private bool isYes = false;
        [SerializeField] private bool waitForAnswer = false;

        public void Init(string desc, float timeLimit = 3f)
        {
            descText.text = desc;
            timer.curTime = 0;
            this.timeLimit = timeLimit;
            isYes = false;
            waitForAnswer = true;
        }

        public void OnDone()
        {
            gameObject.SetActive(false);
            timer.onDone -= OnDone;
        }

        public async UniTask<bool> WaitForAnswer(CancellationToken token)
        {
            isYes = false;
            waitForAnswer = true;
            
            timer.unscaled = true;
            timer.curTime = 0;
            timer.goalTime = timeLimit;
            timer.onDone += OnDone;
            timer.Init();
            
            if (token.IsCancellationRequested) return false;
            await UniTask.WaitUntil(()=>timer.IsDone || !waitForAnswer, cancellationToken:token);

            timer.curTime = 0;
            OnDone();
            timer.Init();
            
            return isYes;
        }
        
        public void Yes()
        {
            isYes = true;
            waitForAnswer = false;
        }

        public void No()
        {
            isYes = false;
            waitForAnswer = false;
        }
    }
}