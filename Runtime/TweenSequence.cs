using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Nimlok.Tweens
{
    public class TweenSequence : MonoBehaviour
    {
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool startAllAtOnce;
        [Space]
        [SerializeField] private ABSAnimationComponent[] tweenToSequence;
        
        private Sequence tweenSequence;

        [Space]
        public UnityEvent OnSequenceComplete;

        public bool GetIsPlaying => tweenSequence.IsPlaying();

        private void OnDisable()
        {
            tweenSequence.Rewind();
        }

        private void Start()
        {
            if (playOnStart)
                PlaySequenceForward();
        }

        public void PlaySequenceForward(Action onComplete = null)
        {
            PlayTweenSequence(true, () => 
            { 
                OnSequenceComplete?.Invoke();
                onComplete?.Invoke();
            });
        }

        public void PlaySequenceBackward(Action onComplete = null)
        {
            PlayTweenSequence(false, onComplete);
        }

        public void PlaySequenceToggle(bool forward)
        {
            if (forward)
            {
                PlaySequenceForward();
            }
            else
            {
                PlaySequenceBackward();
            }
        }
        
        public void StopSequence()
        {
            //TODO: DS 19/04/24 Check to see whether need to restart the sequence after pausing
            tweenSequence.Pause();
        }

        public void ResetSequence()
        {
            tweenSequence.Rewind();
        }

        private void PlayTweenSequence(bool forward, Action onComplete)
        {
            if (tweenSequence == null)
            {
                InitialiseSequence();
            }
            
            AddActionOnComplete(forward, onComplete);
            PlaySequence(forward);
        }
        
        private void InitialiseSequence()
        {
            tweenSequence = DOTween.Sequence();
            
            CollectTweens();
            
            if (tweenToSequence == null || tweenToSequence.Length == 0)
            {
                Debug.LogWarning($"No Tween attached to object");
                return;
            }
            
            //TODO: DS 19/04/24 Break up ifs and checks
            foreach (var baseTween in tweenToSequence)
            {
                if (baseTween == null)
                {
                    Debug.LogError($"Null tween in sequence {name}");
                    continue;
                }
                
                if (baseTween.hasOnStart)
                {
                    continue;
                }
                
                if (startAllAtOnce)
                {
                    tweenSequence.Insert(0, baseTween.tween);
                }
                else
                {
                    tweenSequence.Append(baseTween.tween);
                }
            }

            tweenSequence.SetAutoKill(false);
            tweenSequence.onComplete += () => OnSequenceComplete?.Invoke();
            tweenSequence.Pause();
        }

        private void CollectTweens()
        {
            if (tweenToSequence.Length > 0)
                return;
            
            var baseTweens = GetComponentsInChildren<ABSAnimationComponent>();
            tweenToSequence = baseTweens;
        }

        private void AddActionOnComplete(bool forward, Action onComplete)
        {
            if (forward)
            {
                tweenSequence.OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                tweenSequence.OnRewind(() => onComplete?.Invoke());
            }
        }

        private void PlaySequence(bool forward)
        {
            if (forward)
            {
                tweenSequence.PlayForward();
            }
            else
            {
                tweenSequence.PlayBackwards();
            }
        }
    }
}
