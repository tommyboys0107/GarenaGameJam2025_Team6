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
    [SerializeField] GameObject[] waterEnergies;
    [SerializeField] GameObject[] powerEnergies;
    [SerializeField] Image waterEnergyIcon;
    [SerializeField] Image powerEnergyIcon;
    [SerializeField] Sprite grayWaterIcon;
    [SerializeField] Sprite grayEnergyIcon;
    [SerializeField] Sprite normalWaterIcon;
    [SerializeField] Sprite normalEnergyIcon;
    [SerializeField] Image heroHealthFillImage;
    [SerializeField] float playerMaxHealth = 200f;

    [Header("Result Panel")]
    [SerializeField] Sprite victorySprite;
    [SerializeField] Sprite loseSprite;
    [SerializeField] GameObject vTitleImage;
    [SerializeField] GameObject lTitleImage;
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
        CliffLeeCL.EventManager.Instance.onWaterEnergyChanged += UpdateWaterEnergy;
        CliffLeeCL.EventManager.Instance.onPowerEnergyChanged += UpdatePowerEnergy;
        CliffLeeCL.EventManager.Instance.onSaturationLevelChanged += UpdateSaturationLevel;
    }
    void OnDestroy()
    {
        CliffLeeCL.EventManager.Instance.onWaterEnergyChanged -= UpdateWaterEnergy;
        CliffLeeCL.EventManager.Instance.onPowerEnergyChanged -= UpdatePowerEnergy;
        CliffLeeCL.EventManager.Instance.onSaturationLevelChanged -= UpdateSaturationLevel;
    }

    public void UpdateHealthBar(int health)
    {
        healthSlider.value = health;
    }
    void UpdateWaterEnergy(int currentWaterEnergy)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < currentWaterEnergy)
                waterEnergies[i].SetActive(true);
            else
                waterEnergies[i].SetActive(false);
        }
        if (currentWaterEnergy == 0)
        {
            waterEnergyIcon.sprite = grayWaterIcon;
        }
        else
        {
            waterEnergyIcon.sprite = normalWaterIcon;
        }
    }
    void UpdateSaturationLevel(float saturationLevel)
    {
        heroHealthFillImage.fillAmount = Mathf.Clamp01(saturationLevel / playerMaxHealth);
    }
    void UpdatePowerEnergy(int currentPowerEnergy)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < currentPowerEnergy)
                powerEnergies[i].SetActive(true);
            else
                powerEnergies[i].SetActive(false);
        }
        if (currentPowerEnergy == 0)
        {
            powerEnergyIcon.sprite = grayEnergyIcon;
        }
        else
        {
            powerEnergyIcon.sprite = normalEnergyIcon;
        }
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
            vTitleImage.SetActive(true);
            lTitleImage.SetActive(false);
            BattleManager.Instance.PlaySE(victoryClip);
        }
        else
        {
            vTitleImage.SetActive(false);
            lTitleImage.SetActive(true);
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
