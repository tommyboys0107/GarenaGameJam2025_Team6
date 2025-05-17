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
        public event Action onHeroGetFood;
        public event Action onHeroGetWaterEnergy;
        public event Action onHeroGetPowerEnergy;
        public event Action<int> onHeroUseWaterSkill;
        public event Action<int> onHeroUsePowerSkill;
        public event Action<float> onSaturationLevelChanged;
        public event Action<int> onWaterEnergyChanged;
        public event Action<int> onPowerEnergyChanged;
        
        
        public void OnGameOver()
        {
            onGameOver?.Invoke();
            Debug.Log("On Game Over");
        }

        public void OnHeroGetFood()
        {
            onHeroGetFood?.Invoke();
            Debug.Log("On Hero Get Food");
        }

        public void OnHeroGetWaterEnergy()
        {
            onHeroGetWaterEnergy?.Invoke();
            Debug.Log("On Hero Get Water");
        }

        public void OnHeroGetPowerEnergy()
        {
            onHeroGetPowerEnergy?.Invoke();
            Debug.Log("On Hero Get Power");
        }

        public void OnHeroUseWaterSkill(int leftEnergy)
        {
            onHeroUseWaterSkill?.Invoke(leftEnergy);
            Debug.Log($"On Hero Use Water, left {leftEnergy}");
        }

        public void OnHeroUsePowerSkill(int leftEnergy)
        {
            onHeroUsePowerSkill?.Invoke(leftEnergy);
            Debug.Log($"On Hero Use Power, left {leftEnergy}");
        }

        public void OnSaturationLevelChanged(float finalValue)
        {
            onSaturationLevelChanged?.Invoke(finalValue);
            Debug.Log($"On Saturation Level Changed, final {finalValue}");
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

