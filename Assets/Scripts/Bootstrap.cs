// Space Trader 5000 - Android/Unity Port
// Unity entry point: bootstraps the GameState singleton and starts a new game
// when no save exists. Attach this MonoBehaviour to a persistent GameObject
// in the Main scene.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SpaceTrader
{
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            // GameState self-registers as a singleton in its own Awake.
            if (GameState.Instance == null)
            {
                var go = new GameObject("GameState");
                go.AddComponent<GameState>();
            }

            // Unity UI requires an EventSystem to process all input events.
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                DontDestroyOnLoad(esGo);
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        void Start()
        {
            if (!GameState.Instance.GameLoaded)
                TravelerSystem.StartNewGame();
        }
    }
}
