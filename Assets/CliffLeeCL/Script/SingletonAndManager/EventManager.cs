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
        public event Action<float> onSaturationLevelChanged;
        public event Action<int> onWaterEnergyChanged;
        public event Action<int> onPowerEnergyChanged;
        
        
        public void OnGameOver()
        {
            onGameOver?.Invoke();
            Debug.Log("On Game Over");
        }

        public void OnSaturationLevelChanged(float finalValue)
        {
            onSaturationLevelChanged?.Invoke(finalValue);
        }

        public void OnWaterEnergyChanged(int finalValue)
        {
            onWaterEnergyChanged?.Invoke(finalValue);
            Debug.Log($"On Water Energy Changed, final {finalValue}");
        }

        public void OnPowerEnergyChanged(int finalValue)
        {
            onPowerEnergyChanged?.Invoke(finalValue);
            Debug.Log($"On Power Energy Changed, final {finalValue}");
        }
    }
}

