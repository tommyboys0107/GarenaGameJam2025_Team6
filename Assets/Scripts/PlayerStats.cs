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

    public void RecordBuilding()
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
    public void RecordSaved()
    {
        totalSavedCleaner++;
    }
    int CalculateHeroScore()
    {
        return totalBuildingBreaked * 10
             + totalEnemiesKilled * 100
             + totalSavedCleaner * 200;
    }
    int CalculateCleanerScore()
    {
        return totalGainedEnergy;
    }

    public string GetHeroRank()
    {
        int score = CalculateHeroScore();
        if (score >= 5000) return "S";
        if (score >= 3000) return "A";
        return "B";
    }
    public string GetCleanerRank()
    {
        int score = CalculateCleanerScore();
        if (score >= 30) return "S";
        if (score >= 10) return "A";
        return "B";
    }
}
