using UnityEngine;
using System.Collections;
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public enum BattleState { Preparation, BossFight, GameOver }

    [Header("Settings")]
    public float preparationTime = 10f; // BOSS登場前倒數
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Runtime")]
    public BattleState currentState;
    public float remainingPreparationTime; // 目前剩下時間
    public bool isBossSpawned;

    void Start()
    {
        StartCoroutine(BattleFlow());
    }

    IEnumerator BattleFlow()
    {
        // 階段1：準備階段
        currentState = BattleState.Preparation;
        remainingPreparationTime = preparationTime;

        while (remainingPreparationTime > 0)
        {
            remainingPreparationTime -= Time.deltaTime;
            // UIManager.Instance.UpdateTimer(remainingPreparationTime);
            yield return null;
        }

        // 階段2：BOSS登場
        currentState = BattleState.BossFight;
        IntoBossFight();
    }
    void IntoBossFight()
    {
        UIManager.Instance.bossHealthPanel.SetActive(true);
        Debug.Log("[Battlemanager] into bossfight");
        GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
         // bossObj.GetComponent<BossController>().PlayEntranceAnimation();
        isBossSpawned = true;
    }

    public void EndBattle(bool isVictory)
    {
        currentState = BattleState.GameOver;
        // 展示結算面板
        UIManager.Instance.ShowResultPanel(isVictory);
        Time.timeScale = 0f; // 暫停遊戲邏輯
    }
}
