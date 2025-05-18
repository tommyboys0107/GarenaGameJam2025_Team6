using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public enum BattleState { Preparation, BossFight, GameOver }

    [Header("Settings")]
    [SerializeField] float preparationTime = 10f; // BOSS登場前倒數
    [SerializeField] GameObject bossPrefab;
    public Transform heroPoint;
    // [SerializeField] Transform bossSpawnPoint;
    [SerializeField] float bossSpawnRadius = 5.0f;
    [SerializeField] float cautionTime = 3.0f; //警告聲時間
    [SerializeField] AudioSource battleAudioSource;
    [SerializeField] AudioClip[] hitSEs;
    [SerializeField] AudioClip introSE;

    [Header("Runtime")]
    public BattleState currentState;
    public float remainingPreparationTime; // 目前剩下時間
    public bool isBossSpawned;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CliffLeeCL.EventManager.Instance.onGameOver += GameLose;
        StartCoroutine(BattleFlow());
    }
    void OnDestroy()
    {
        CliffLeeCL.EventManager.Instance.onGameOver -= GameLose;
    }
    void GameLose()
    {
        EndBattle(false);
    }
    private void Update()
    {
        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            EndBattle(true); 
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            EndBattle(false); 
        }
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
        PlaySE(introSE);
        yield return new WaitForSeconds(cautionTime);
        IntoBossFight();
    }
    void IntoBossFight()
    {
        BattleUIManager.Instance.bossHealthPanel.SetActive(true);
        Debug.Log("[Battlemanager] into bossfight");
        float angle = Mathf.PI * 2 / Random.Range(1, 15);
            Vector3 bossSpawnPoint = heroPoint.position + new Vector3(
                Mathf.Cos(angle) * bossSpawnRadius,
                0,
                Mathf.Sin(angle) * bossSpawnRadius
            );
        GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint, Quaternion.identity);
         // bossObj.GetComponent<BossController>().PlayEntranceAnimation();
        isBossSpawned = true;
    }
    public void HitSE()
    {
        int index = Random.Range(0, 1); // 0或1
        battleAudioSource.PlayOneShot(hitSEs[index]);
    }
    public void PlaySE(AudioClip ac)
    {
        battleAudioSource.PlayOneShot(ac);
    }
        
    public void EndBattle(bool isVictory)
    {
        if (currentState == BattleState.GameOver)
            return;
        
        currentState = BattleState.GameOver;
        
        // 展示結算面板
        BattleUIManager.Instance.ShowResultPanel(isVictory);
        Time.timeScale = 0f; // 暫停遊戲邏輯
    }
}
