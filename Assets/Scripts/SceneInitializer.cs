// Space Trader 5000 – Android/Unity Port
// Bootstraps all persistent game objects before the first scene loads.
// [RuntimeInitializeOnLoadMethod] runs automatically; no scene attachment needed.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SpaceTrader
{
    public static class SceneInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            // GameState singleton (DontDestroyOnLoad in its own Awake)
            var gsGo = new GameObject("GameState");
            Object.DontDestroyOnLoad(gsGo);
            gsGo.AddComponent<GameState>();

            // UIManager singleton (DontDestroyOnLoad in its own Awake)
            var uiGo = new GameObject("UIManager");
            Object.DontDestroyOnLoad(uiGo);
            uiGo.AddComponent<UI.UIManager>();

            // EventSystem using the new Input System UI module
            var esGo = new GameObject("EventSystem");
            Object.DontDestroyOnLoad(esGo);
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }
    }
}
