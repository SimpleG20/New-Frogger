using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NewFrogger.Traffic.Domain
{
    public class Timer
    {
        public event Action<int> OnCountdown;
        public int TimeInSecs { get; private set; }
        public bool Running { get; private set; }

        private bool _paused;
        private Action _onEnd;

        public Timer(int time, Action onEnd = default) 
        {
            TimeInSecs = time; 
            _onEnd = onEnd;
        }

        public async UniTask StartTimer(CancellationToken ct)
        {
            if (Running) return;
            Running = true;

            try
            {
                while (Running && TimeInSecs > 0)
                {
                    if (_paused)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    await UniTask.Delay(1000, cancellationToken: ct);
                    TimeInSecs--;
                    OnCountdown?.Invoke(TimeInSecs);
                }

                _onEnd?.Invoke();
            }
            finally
            {
                Running = false;
            }
        }

        public void Pause() => _paused = true;
        public void Resume() => _paused = false;
        public void Stop() => Running = false;

        public void Dispose()
        {
            _onEnd = null;
            OnCountdown = null;
        }
    }
}
