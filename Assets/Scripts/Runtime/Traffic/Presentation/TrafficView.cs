using System;
using System.Threading;
using UnityEngine;
using TMPro;

using Cysharp.Threading.Tasks;

using NewFrogger.Traffic.Domain;

namespace NewFrogger.Traffic.Presentation
{
    public class TrafficView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _trafficChangedPanel;

        [SerializeField] private TMP_Text _velocityAvgTx;
        [SerializeField] private TMP_Text _weatherTx;
        [SerializeField] private TMP_Text _levelTx;
        [SerializeField] private TMP_Text _predictionTimeTx;

        public void Initialize()
        {
            ResetView();
        }

        public void StartLevel(int level, int time)
        {
            _root.SetActive(true);
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_velocityAvgTx.text));
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_weatherTx.text));

            _levelTx.text = level.ToString();
            _predictionTimeTx.text = time.ToString();
            _levelTx.gameObject.SetActive(true);
        }

        public void ResetView()
        {
            _trafficChangedPanel.SetActive(false);
            _velocityAvgTx.text = "";
            _weatherTx.text = "";
            _predictionTimeTx.text = "";
            _root.SetActive(false);
        }

        public void UpdatePredictionCountdown(int time) => _predictionTimeTx.text = time.ToString();
        public void UpdateLevel(int v) => _levelTx.text = v.ToString();
        public void UpdateTrafficSettings(TrafficSettings settings)
        {
            _velocityAvgTx.text = settings.AverageSpeed.ToString();
            _velocityAvgTx.gameObject.SetActive(!string.IsNullOrEmpty(_velocityAvgTx.text));

            _weatherTx.text = settings.Weather.ToString();
            _weatherTx.gameObject.SetActive(!string.IsNullOrEmpty(_weatherTx.text));
        }

        public async UniTask TimedShowTrafficUpdateWarning(float seconds, CancellationToken ct)
        {
            _trafficChangedPanel.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: ct);
            _trafficChangedPanel.SetActive(false);
        }

        public void EndLevel()
        {
            ResetView();
        }

        private void OnDestroy()
        {
        }
    }
}