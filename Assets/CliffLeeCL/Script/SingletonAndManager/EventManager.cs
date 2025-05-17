using System;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// This singleton class manage all events in the game.
    /// </summary>
    /// <code>
    /// // Usage in other class example:\n
    /// void Start(){\n
    ///     EventManager.instance.onGameOver += LocalFunction;\n
    /// }\n
    /// \n
    /// // If OnEnable function will cause error, try listen to events in Start function.\n
    /// void OnEnable(){\n
    ///     EventManager.instance.onGameOver += LocalFunction;\n
    /// }\n
    /// \n
    /// void OnDisable(){\n
    ///     EventManager.instance.onGameOver -= LocalFunction;\n
    /// }\n
    /// \n
    /// void LocalFunction(){\n
    ///     //Do something here\n
    /// }
    /// </code>
    public class EventManager : Singleton<EventManager>
    {
        public event Action onGameOver;
        public event Action onGetFood;
        public event Action onGetWaterEnergy;
        public event Action onGetPowerEnergy;
        
        public void OnGameOver()
        {
            onGameOver?.Invoke();
            Debug.Log("OnGameOver event is invoked!");
        }

        public void OnGetFood()
        {
            onGetFood?.Invoke();
        }

        public void OnGetWaterEnergy()
        {
            onGetWaterEnergy?.Invoke();
        }

        public void OnGetPowerEnergy()
        {
            onGetPowerEnergy?.Invoke();
        }
    }
}

