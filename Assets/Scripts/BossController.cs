using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
public class BossController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] Transform player;
    [SerializeField] float followSpeed = 3.5f;
    [SerializeField] float skillCooldown = 15f;
    [SerializeField]
    float speed = 3f;
    [SerializeField]
    float stopDistance = 10f;
    [SerializeField]
    float hitDistance = 10f;
    [SerializeField]
    float hitDuration = 1.0f;

    [Header("Combat")]
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] int maxHealth = 500;
    // [SerializeField] float knockbackForce = 15f;
    [SerializeField] float stunDuration = 2f;
    [SerializeField] float breakTime = 1f;
    [SerializeField] float hitDamage = 10f;
    float currentHealth;
    [SerializeField]
    int obstacleCollisionCount;
    bool isStunned;

    [Header("UI")]
    [SerializeField] private Image fillImage;

    void Start()
    {

        currentHealth = maxHealth;

        StartCoroutine(SkillRoutine());
    }

    void Update()
    {
        if (!isStunned)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > stopDistance)
            {
                // 朝 player 前進
                Vector3 direction = (player.position - transform.position).normalized;
                // y 軸不動
                direction.y = 0;
                transform.position += direction * followSpeed * Time.deltaTime;
            }
            // 否則停下來什麼都不做
        }
    }

    System.Collections.IEnumerator SkillRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(skillCooldown);
            CreateObstacleCircle();
        }
    }

    void CreateObstacleCircle()
    {
        int obstacleCount = 8;
        float radius = 5f;

        for (int i = 0; i < obstacleCount; i++)
        {
            float angle = i * Mathf.PI * 2 / obstacleCount;
            Vector3 pos = player.position + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Instantiate(obstaclePrefab, pos, Quaternion.identity);
        }
    }

    public void TakeDamage()
    {
        if (obstacleCollisionCount < 3) return;

        currentHealth = Mathf.Max(0, currentHealth - hitDamage);
        fillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        if (currentHealth <= 0)
        {
            BattleManager.Instance.EndBattle(true);
        }
    }
    [Button]
    public void OnHit()
    {
        // isHit = true;
        if (isStunned)
            TakeDamage();
        else
            StartCoroutine(ShowHitAnimAndMove());
    }

    IEnumerator ShowHitAnimAndMove()
    {
        // 朝 player 反向前進
        Vector3 direction = (player.position - transform.position).normalized * hitDistance;
        HitMove().Forget();

        yield return new WaitForSeconds(breakTime);
    }

    public async UniTaskVoid HitMove()
    {
        // 計算目標位置（反向移動 hitDistance）
        Vector3 start = transform.localPosition;
        Vector3 end = start - (player.position - transform.position).normalized * hitDistance;

        float timer = 0f;

        // 執行動畫（手動插值）
        while (timer < hitDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / hitDuration);
            transform.localPosition = Vector3.Lerp(start, end, t);
            await UniTask.Yield(); // 等下一幀
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("[Boss] enter OnTriggerEnter" + collision.gameObject);
        if (collision.CompareTag("Build"))
        {
            Debug.Log("[Boss] enter build");
            obstacleCollisionCount++;
            if (obstacleCollisionCount >= 3)
            {
                StartCoroutine(StunRoutine());
            }
        }
    }

    System.Collections.IEnumerator StunRoutine()
    {
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        obstacleCollisionCount = 0;
    }
}
