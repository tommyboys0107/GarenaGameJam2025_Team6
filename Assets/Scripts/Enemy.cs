using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    float speed = 3f;
    [SerializeField]
    float stopDistance = 10f;
    [SerializeField]
    float hitDistance = 20f;
    [SerializeField]
    float hitDuration = 1.0f;
    // 暈倒時間
    [SerializeField]
    float breakTime = 30f;
    [SerializeField]
    GameObject enemyObj = null;
    [SerializeField]
    GameObject deadBodyObj = null;
    
    bool isHit;
    bool isDead;
    Tweener hitMove;
    Transform player;
    Coroutine hitCoroutine;
    Rigidbody rigidbody;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rigidbody = GetComponent<Rigidbody>();
        enemyObj.SetActive(true);
        deadBodyObj.SetActive(false);
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        if (!isHit)
        {

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > stopDistance)
            {
                // 朝 player 前進
                Vector3 direction = (player.position - transform.position).normalized;
                // y 軸不動
                direction.y = 0;
                transform.position += direction * speed * Time.deltaTime;
            }
            // 否則停下來什麼都不做
        }
    }

    [Button]
    public void OnHit(int damage, float pushForce)
    {
        isHit = true;
        hitCoroutine = StartCoroutine(ShowHitAnim(pushForce));
    }

    IEnumerator ShowHitAnim(float pushForce)
    {
        // 朝 player 反向前進
        Vector3 direction = (transform.position - player.position).normalized * pushForce;
        rigidbody.AddForce(direction, ForceMode.Impulse);
        // HitMove().Forget();
        
        yield return new WaitForSeconds(breakTime);
    }
    
    public async UniTaskVoid HitMove()
    {
        // 計算目標位置（反向移動 hitDistance）
        Vector3 start = transform.localPosition;
        Vector3 end = start - (player.position - transform.position).normalized * hitDistance;

        float timer = 0f;

        // 執行動畫（手動插值）
        while (timer < hitDuration && !isDead)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / hitDuration);
            transform.localPosition = Vector3.Lerp(start, end, t);
            await UniTask.Yield(); // 等下一幀
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (!isDead && isHit && other.gameObject.CompareTag("Build"))
        {
            var build = other.transform.parent.gameObject.GetComponent<Build>();
            build?.Collapse();
            isDead = true;
            enemyObj.SetActive(false);
            deadBodyObj.SetActive(true);
            Debug.Log("[Enemy] Break down!");
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

}
