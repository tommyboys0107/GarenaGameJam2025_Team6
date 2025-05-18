using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.Collections;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
public class BossController : MonoBehaviour
{
    [Header("Movement")]
    // [SerializeField] Transform player;
    [SerializeField] float followSpeed = 3.5f;
    [SerializeField] float skillCooldown = 15f;
    // [SerializeField] float speed = 3f;
    [SerializeField] float stopDistance = 0f;
    [SerializeField] float hitDistance = 10f;
    [SerializeField] float hitDuration = 1.0f;

    [Header("Combat")]
    [SerializeField] int obstacleCount = 8;
    [SerializeField] float radius = 5f;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] int maxHealth = 500;
    // [SerializeField] float knockbackForce = 15f;
    [SerializeField] float stunDuration = 2f;
    [SerializeField] float breakTime = 1f;
    [SerializeField] float hitDamage = 10f;
    [SerializeField] float attackDelay = 4f;
    float currentHealth;
    [SerializeField]
    int obstacleCollisionCount;
    bool isStunned;
    bool isHitFly;
    bool isAttack;
    [SerializeField] SpriteRenderer bossSpriteRenderer;
    [SerializeField] Sprite normalBossImage;
    [SerializeField] Sprite stunnedBossImage;
    [Header("SE")]
    [SerializeField] AudioClip StunSE;
    [SerializeField] AudioClip magicSE;
    [SerializeField] AudioClip buildingExplodeSE;
    [Header("Spine")]
    private BossAnimType curAnimType;
    [SerializeField] SkeletonAnimation spineAnimation = null;
    public enum BossAnimType
    {
        stay,
        animation
    }


    void Start()
    {

        currentHealth = maxHealth;
        CreateObstacleCircle();
    }

    void Update()
    {
        if (Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            TestDamage(50); 
        }
        if (!isStunned && !isAttack)
        {
            float distance = Vector3.Distance(transform.position, BattleManager.Instance.heroPoint.position);

            if (distance > stopDistance)
            {
                // 朝 player 前進
                Vector3 direction = (BattleManager.Instance.heroPoint.position - transform.position).normalized;
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
    [Button]
    void CreateObstacleCircle()
    {
        BattleManager.Instance.PlaySE(magicSE);
        for (int i = 0; i < obstacleCount; i++)
        {
            float angle = i * Mathf.PI * 2 / obstacleCount;
            Vector3 pos = BattleManager.Instance.heroPoint.position + new Vector3(
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
        BattleManager.Instance.HitSE();
        BattleUIManager.Instance.bossHealthBar.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        if (currentHealth <= 0)
        {
            BattleManager.Instance.EndBattle(true);
        }
    }
    public void TestDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        BattleManager.Instance.HitSE();
        BattleUIManager.Instance.bossHealthBar.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
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
        {
            isHitFly = true;
            StartCoroutine(ShowHitAnimAndMove());
        }
    }

    IEnumerator ShowHitAnimAndMove()
    {
        // 朝 player 反向前進
        Vector3 direction = (BattleManager.Instance.heroPoint.position - transform.position).normalized * hitDistance;
        HitMove().Forget();

        yield return new WaitForSeconds(breakTime);
        isHitFly = false;
    }

    public async UniTaskVoid HitMove()
    {
        // 計算目標位置（反向移動 hitDistance）
        Vector3 start = transform.localPosition;
        Vector3 end = start - (BattleManager.Instance.heroPoint.position - transform.position).normalized * hitDistance;
        // 後續會使用物理的話，則不需要透過座標固定。
        // end.y = 1f;

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

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[Boss] enter OnTriggerEnter" + collision.gameObject);
        if (collision.gameObject.CompareTag("Build") && isHitFly)
        {
            obstacleCollisionCount++;

            // 觸發破壞建築(改到建築物端製作)
            // BreakBuilding(collision.gameObject);

            if (obstacleCollisionCount == 3 && !isStunned)
            {
                Debug.Log("[Boss] obstacleCollisionCount" + obstacleCollisionCount);
                obstacleCollisionCount = 0;
                StartCoroutine(StunRoutine());
            }
        }
        else if(collision.gameObject.CompareTag("Player") && !isHitFly && !isStunned && !isAttack)
        {
            isAttack = true;
            StartCoroutine(AttackRoutine());
        }
    }
    IEnumerator AttackRoutine()
    {
        Debug.Log("[Boss] AttackRoutine");
        ChangeAnimation(BossAnimType.animation);
        yield return new WaitForSeconds(attackDelay);
        isAttack = false;
        Debug.Log("[Boss] AttackRoutine false");
        ChangeAnimation(BossAnimType.stay);

    }

    System.Collections.IEnumerator StunRoutine()
    {
        isStunned = true;
        Debug.Log("[Boss] Stun start");
        BattleManager.Instance.PlaySE(StunSE);
        bossSpriteRenderer.sprite = stunnedBossImage;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        bossSpriteRenderer.sprite = normalBossImage;
        Debug.Log("[Boss] Stun ends");
        obstacleCollisionCount = 0;
    }
    [Button]
    public void ChangeAnimation(BossAnimType type, bool isLoop = false)
    {
        curAnimType = type;
        spineAnimation.state.SetAnimation(0, type.ToString(), isLoop);
    }
}
