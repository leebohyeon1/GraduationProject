using System.Reflection;
using UnityEngine;
using BH_Lib;
using BH_Lib.DI;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    // public EventManager EventManager { get; private set; }
    public enum GameState { Title, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Title;

    private SoundManager soundManager;
    private DIContainer container;

    private void Awake()
    {
        // EventManager = new EventManager();
        // container = new DIContainer();
        
        // container.Register(soundManager);
        // container.Register(EventManager);
        // AutoRegisterServices();
        // container.ResolveAllWithInjection();
    }

    // private void AutoRegisterServices()
    // {
    //     foreach (var mono in FindObjectsOfType<MonoBehaviour>(true))
    //     {
    //         var type = mono.GetType();
    //         if (type.GetCustomAttribute<AutoRegisterAttribute>() != null)
    //             container.Register(type, mono);
    //     }
    // }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
    }
}
