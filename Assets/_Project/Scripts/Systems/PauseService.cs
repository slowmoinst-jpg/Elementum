using System;
using Elementum.Project.Core;
using UnityEngine;

namespace Elementum.Project.Systems
{
    public sealed class PauseService : MonoBehaviour
    {
        [SerializeField]
        private SignalBusSO _signalBus;

        private bool _isAdPauseActive;
        private float _cachedTimeScale = 1f;
        private bool _cachedAudioPause;

        public event Action OnAdOpened;
        public event Action OnAdClosed;
        public event Action OnRewardGranted;

        public void Construct(SignalBusSO signalBus)
        {
            _signalBus = signalBus;
        }

        public void NotifyAdOpened()
        {
            if (_isAdPauseActive)
            {
                return;
            }

            _cachedTimeScale = Time.timeScale;
            _cachedAudioPause = AudioListener.pause;

            Time.timeScale = 0f;
            AudioListener.pause = true;
            _isAdPauseActive = true;

            OnAdOpened?.Invoke();
            _signalBus?.RaiseAdOpened();
        }

        public void NotifyAdClosed()
        {
            if (!_isAdPauseActive)
            {
                return;
            }

            RestoreTimeAndAudio();

            OnAdClosed?.Invoke();
            _signalBus?.RaiseAdClosed();
        }

        public void NotifyRewardGranted()
        {
            OnRewardGranted?.Invoke();
            _signalBus?.RaiseRewardGranted();
        }

        private void OnDisable()
        {
            if (_isAdPauseActive)
            {
                RestoreTimeAndAudio();
            }
        }

        private void RestoreTimeAndAudio()
        {
            // Важно восстанавливать предыдущее состояние, чтобы не ломать паузу других систем.
            Time.timeScale = _cachedTimeScale;
            AudioListener.pause = _cachedAudioPause;
            _isAdPauseActive = false;
        }
    }
}
