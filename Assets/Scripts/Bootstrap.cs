// Space Trader 5000 - Android/Unity Port
// Unity entry point: bootstraps the GameState singleton and starts a new game
// when no save exists. Attach this MonoBehaviour to a persistent GameObject
// in the Main scene.

using UnityEngine;

namespace SpaceTrader
{
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            // GameState self-registers as a singleton in its own Awake.
            // We only need to ensure it exists before anything else runs.
            if (GameState.Instance == null)
            {
                var go = new GameObject("GameState");
                go.AddComponent<GameState>();
            }
        }

        void Start()
        {
            if (!GameState.Instance.GameLoaded)
                TravelerSystem.StartNewGame();
        }
    }
}
