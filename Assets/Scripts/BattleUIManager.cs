using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;
    [Header("Battle HUD")]
    public Slider healthSlider;
    public Text timerText;
    public GameObject bossHealthPanel;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public Text resultText;
    public Text damageText;
    public Text killsText;
    public Text rankText;

    void Start()
    {
        resultPanel.SetActive(false);
        bossHealthPanel.SetActive(false);
    }

    public void UpdateHealthBar(int health)
    {
        healthSlider.value = health;
    }

    public void UpdateTimer(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        timerText.text = seconds.ToString("00");
    }

    public void ShowResultPanel(bool isVictory)
    {
        resultPanel.SetActive(true);
        resultText.text = isVictory ? "VICTORY" : "DEFEAT";
        // damageText.text = PlayerStats.Instance.totalDamageDealt.ToString();
        // killsText.text = PlayerStats.Instance.totalEnemiesKilled.ToString();

        // 評價系統範例
        int score = CalculateScore();
        rankText.text = GetRank(score);
    }

    int CalculateScore()
    {
        return PlayerStats.Instance.totalBuildingBreaked * 10
             + PlayerStats.Instance.totalEnemiesKilled * 100
             + PlayerStats.Instance.totalSavedCleaner * 200;
    }

    string GetRank(int score)
    {
        if (score >= 5000) return "S";
        if (score >= 3000) return "A";
        return "B";
    }
}
