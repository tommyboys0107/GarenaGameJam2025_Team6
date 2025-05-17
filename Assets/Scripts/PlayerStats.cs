using UnityEngine;
using System.Collections;
public class PlayerStats : MonoBehaviour
{
    [Header("Combat")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Statistics")]
    public int totalBuildingBreaked;
    public int totalEnemiesKilled;
    public int totalSavedCleaner;
    public int totalGainedEnergy;

    public static PlayerStats Instance;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        BattleUIManager.Instance.UpdateHealthBar(currentHealth);

        if (currentHealth <= 0)
            BattleManager.Instance.EndBattle(false);
    }

    public void RecordDamage(int damage)
    {
        totalBuildingBreaked++;
    }

    public void RecordKill()
    {
        totalEnemiesKilled++;
    }
    public void RecordEnergy()
    {
        totalGainedEnergy++;
    }
    public void totalSaved()
    {
        totalSavedCleaner++;
    }
}
