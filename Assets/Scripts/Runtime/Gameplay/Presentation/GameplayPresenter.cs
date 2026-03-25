using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

using CustomLogger;

using NewFrogger.Gameplay.Composition;
using NewFrogger.Gameplay.Data;
using NewFrogger.Player.Domain;
using NewFrogger.Player.Presentation;
using NewFrogger.Traffic.Domain;
using NewFrogger.Traffic.Presentation;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayPresenter : MonoBehaviour
    {
        [SerializeField] private string _apiBaseUrl = "https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io";
        [SerializeField] private GameplaySettingsSO _gameplaySettingsSO;
        [SerializeField] private GameplayView _gameplayView;

        [Header("Controllers")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private TrafficController _trafficController;

        [Header("Player")]
        [SerializeField] private PlayerInput _input;

        private GameplayLifecycle _lifecycle;
        private GameplayInputHandler _gameplayInput;
        private GameplayCompositionRoot _compositionRoot;

        private CancellationTokenSource _generalCTS;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            _gameplayView.Initialize();
            _gameplayView.OnStart += StartGameplay;
            _gameplayView.OnStop += ForceEndGameplay;

            _compositionRoot = new GameplayCompositionRoot(_apiBaseUrl);

            var playerModel = new PlayerModel(_gameplaySettingsSO, _compositionRoot.GetTimeProvider());
            var trafficModel = new TrafficModel(_gameplaySettingsSO, _compositionRoot.GetTrafficStatsService());

            _playerController.Initialize(playerModel);
            _trafficController.Initialize(trafficModel);

            _generalCTS = new CancellationTokenSource();
            _gameplayInput = new GameplayInputHandler(playerModel, _input);
            _lifecycle = new GameplayLifecycle(_gameplayView, _playerController, _trafficController, _generalCTS);

            Log.log("Gameplay Presenter Initialized");
        }

        private void StartGameplay() => _lifecycle.StartGameplay();
        private void ForceEndGameplay() => _lifecycle.EndGameplay();

        private void OnDestroy()
        {
            _generalCTS?.Cancel();
            _generalCTS?.Dispose();

            _lifecycle?.Dispose();
            _gameplayInput?.Dispose();
            _compositionRoot?.Dispose();
        }
    }
}