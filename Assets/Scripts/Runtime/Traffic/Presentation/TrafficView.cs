using System;
using System.Threading;
using UnityEngine;
using TMPro;

using Cysharp.Threading.Tasks;

using NewFrogger.Gameplay.Domain;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Traffic.Domain;
using NewFrogger.Traffic.Domain.Entities;

namespace NewFrogger.Traffic.Presentation
{
    public class TrafficView : MonoBehaviour
    {
        public event Action OnPanelHid;

        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _trafficChangedPanel;

        [SerializeField] private TMP_Text _velocityAvgTx;
        [SerializeField] private TMP_Text _weatherTx;
        [SerializeField] private TMP_Text _levelTx;
        [SerializeField] private TMP_Text _timeTx;


        private CancellationTokenSource _cts;

        public void Initialize(TrafficModel traffic)
        {
            traffic.OnTrafficSettingsChanged += HandleOnTrafficSettingsChanged;
            traffic.OnStartNewLevel += HandleOnStartNewLevel;
            traffic.OnStopLevel += HandleOnStopLevel;
            traffic.OnCountdownChanged += HandleOnCountdownChanged;

            ResetView();
        }

        public void ResetView()
        {
            _trafficChangedPanel.SetActive(false);
            _velocityAvgTx.text = "";
            _weatherTx.text = "";
            _timeTx.text = "";
            _root.SetActive(false);
        }

        private void HandleOnCountdownChanged(int time)
        {
            _timeTx.text = time.ToString();
        }

        private void HandleOnStopLevel()
        {
            _cts?.Cancel();
            ResetView();
        }

        private void HandleOnStartNewLevel(int level, int time)
        {
            _root.SetActive(true);
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_velocityAvgTx.text));
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_weatherTx.text));

            _levelTx.text = level.ToString();
            _timeTx.text = time.ToString();
            _levelTx.gameObject.SetActive(true);
        }

        private void HandleOnTrafficSettingsChanged(TrafficSettings obj)
        {
            _trafficChangedPanel.SetActive(true);

            _velocityAvgTx.text = obj.AverageSpeed.ToString();
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_velocityAvgTx.text));

            _weatherTx.text = obj.Weather.ToString();
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_weatherTx.text));

            _cts = new CancellationTokenSource();
            _ = UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: _cts.Token).ContinueWith(() =>
            {
                _trafficChangedPanel.SetActive(false);
                OnPanelHid?.Invoke();
            });
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            ResetView();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}