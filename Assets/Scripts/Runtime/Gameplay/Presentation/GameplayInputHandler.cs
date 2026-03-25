using UnityEngine.InputSystem;
using NewFrogger.Player.Domain;

namespace NewFrogger.Gameplay.Presentation
{
    public class GameplayInputHandler : System.IDisposable
    {
        private PlayerModel _player;
        private PlayerInput _input;

        public GameplayInputHandler(PlayerModel player, PlayerInput input)
        {
            _player = player ?? throw new System.ArgumentNullException(nameof(player));
            _input = input ?? throw new System.ArgumentNullException(nameof(input));

            _input.actions["Move"].performed += HandleOnMovement;
        }

        private void HandleOnMovement(InputAction.CallbackContext ctx)
        {
            var direction = ctx.ReadValue<UnityEngine.Vector2>();
            _player.Move(direction);
        }

        public void Dispose()
        {
            if (_input != null)
            {
                _input.actions["Move"].performed -= HandleOnMovement;
            }
            _input = null;
            _player = null;
        }
    }
}
