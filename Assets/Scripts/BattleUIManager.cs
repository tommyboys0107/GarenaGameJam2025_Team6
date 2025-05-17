using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;
    [Header("Battle HUD")]
    public Slider healthSlider;
    public Text timerText;
    public GameObject bossHealthPanel;
    public Image bossHealthBar;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text titleText;
    public TMP_Text heroResultText;
    public TMP_Text cleanerResultText;
    public TMP_Text buildingsDamageText;
    public TMP_Text energyText;
    public TMP_Text killsText;
    public AudioClip victoryClip;
    public AudioClip loseClip;
    void Awake()
    {
        Instance = this;
    }
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
        if (isVictory)
        {
            titleText.text = "VICTORY";
            BattleManager.Instance.PlaySE(victoryClip);
        }
        else
        {
            titleText.text = "Lose";
            BattleManager.Instance.PlaySE(loseClip);
        }

        // 評價系統範例
        heroResultText.text = PlayerStats.Instance.GetHeroRank();
        cleanerResultText.text = PlayerStats.Instance.GetCleanerRank();
        buildingsDamageText.text = PlayerStats.Instance.totalBuildingBreaked.ToString();
        killsText.text = PlayerStats.Instance.totalEnemiesKilled.ToString();
        energyText.text = PlayerStats.Instance.totalGainedEnergy.ToString();
    }

    
}
