using System;
using System.Collections.Generic;
using CliffLeeCL;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Supporter : MonoBehaviour
{

    #region CL

    /// <summary>
    /// Decide whether the player can sprint or not.
    /// </summary>
    public bool canSprint = true;

    /// <summary>
    /// Is used to emit sprint effect particle.
    /// </summary>
    public ParticleSystem sprintEffect;

    /// <summary>
    /// Decide whether the player can jump or not.
    /// </summary>
    public bool canJump = true;

    /// <summary>
    /// Define where the checker should be relatively to player.
    /// </summary>
    [Header("Ground check")]
    public Vector3 checkerPositionOffset = Vector3.zero;

    /// <summary>
    /// Define how large the checker is.
    /// </summary>
    public float checkerRadius = 0.1f;

    /// <summary>
    /// Define how many layers the checker will check.
    /// </summary>
    public LayerMask checkLayer;


    [Header("FOV transition")]
    /// <summary>
    /// Define normal FOV of camera.
    /// </summary>
    public float normalFOV = 60.0f;

    /// <summary>
    /// Define FOV of camera when the player is sprinting.
    /// </summary>
    public float sprintFOV = 65.0f;

    /// <summary>
    /// Define the transition time between FOVs.
    /// </summary>
    public float FOVTransitionTime = 0.5f;

    /// <summary>
    /// Time that transition between normalFOV to sprintFOV.
    /// </summary>
    float normalToSprintTime = 0.0f;

    /// <summary>
    /// Time that transition between sprintFOV to normalFOV.
    /// </summary>
    float sprintToNormalTime = 0.0f;

    Rigidbody rigid;

    /// <summary>
    /// Is used to get movement related data.
    /// </summary>
    PlayerStatus status;

    /// <summary>
    /// Is true when player is on the ground.
    /// </summary>
    bool isGrounded = true;

    /// <summary>
    /// Is true when player is sprinting.
    /// </summary>
    bool isSprinting = false;

    /// <summary>
    /// Is true when player drain his stamina.
    /// </summary>
    bool isDrained = false;

    bool isMoving = false;
    bool isinteractive = false;
    private bool isGameOver = false;
    Vector3 moveVelocity = Vector3.zero;
    Vector3 sprintVelocity = Vector3.zero;

    #endregion

    [Header("道具相關")]
    [SerializeField]
    Transform itemRoot = null;
    Item encounterItem = null;
    Item curOriginItem = null;
    Item curAdvancedItem = null;
    CraftingTable craftingTable = null;

    List<Item> itemList = new List<Item>(); 

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        Assert.IsTrue(rigid, "Need \"Rigidbody\" component on this gameObject");
        status = GetComponent<PlayerStatus>();
        Assert.IsTrue(status, "Need \"PlayerStatus\" component on this gameObject");
        encounterItem = null;
    }

    private void OnEnable()
    {
        EventManager.Instance.onGameOver += OnGameOver;
    }


    private void OnDisable()
    {
        EventManager.Instance.onGameOver -= OnGameOver;
    }

    private void OnGameOver()
    {
        isGameOver = true;
    }
        
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if (isGameOver)
        {
            return;
        }
        UpdateSprintEffect();
        UpdateStamina();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void FixedUpdate()
    {
        if (isGameOver)
        {
            return;
        }
            
        UpdateIsGrounded();
        UpdateJump();
        if (isMoving)
        {
            var newPosition = rigid.position + (isSprinting ? sprintVelocity : moveVelocity);
            rigid.MovePosition(newPosition);   
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Item") || other.CompareTag("Food") || other.CompareTag("PowerEnergy") || other.CompareTag("WaterEnergy"))
            && encounterItem == null && curAdvancedItem == null)
        {
            var item = other.GetComponent<Item>();
            if (encounterItem == null)
            {
                encounterItem = item;
                item.SetHint(true);
            }
            // if (!itemList.Contains(item))
            // {
            //     itemList.Add(item);
            // }
        }
        else if (other.CompareTag("CraftingTable"))
        {
            craftingTable = other.GetComponent<CraftingTable>();
            if (!craftingTable.CheckIsHold() && curOriginItem != null)
            {
                craftingTable.SetHint(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item") || other.CompareTag("Food") || other.CompareTag("PowerEnergy") || other.CompareTag("WaterEnergy"))
        {
            var item = other.GetComponent<Item>();
            // if (itemList.Contains(item))
            if (encounterItem == item)
            {
                item.SetHint(false);
                // itemList.Remove(item);
                encounterItem = null;
            }
        }
        else if (other.CompareTag("CraftingTable"))
        {
            craftingTable?.SetHint(false);
            craftingTable = null;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var moveInput = context.ReadValue<Vector2>();
            moveVelocity = new Vector3(moveInput.x, 0, moveInput.y) * (status.movingSpeed * Time.fixedDeltaTime);
            sprintVelocity = new Vector3(moveInput.x, 0, moveInput.y) * (status.sprintSpeed * Time.fixedDeltaTime);
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (canSprint && context.performed && (status.currentStamina > 0))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isinteractive = true;
            InteractObj();

        }
        else
        {
            isinteractive = false;
        }
    }

    [Button]
    public void AttackTest()
    {
        InteractObj();
    }

    void InteractObj()
    {
        if (encounterItem != null)
        {
            encounterItem.PlayerGetItem();
            encounterItem.transform.parent.parent = itemRoot;
            encounterItem.transform.parent.localPosition = Vector3.zero;
            switch (encounterItem.GetType())
            {
                case Item.ItemType.DeadBody:
                case Item.ItemType.Water:
                case Item.ItemType.Power:
                    curOriginItem = encounterItem;
                    break;
                case Item.ItemType.Food:
                case Item.ItemType.WaterEnergy:
                case Item.ItemType.PowerEnergy:
                    curAdvancedItem = encounterItem;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            encounterItem = null;
        }
        else if (craftingTable != null && !craftingTable.CheckIsHold() && curOriginItem != null)
        {
            craftingTable.CraftItem(curOriginItem);
            curOriginItem = null;
        }
        else if (curOriginItem != null)
        {
            var itemParent = curOriginItem.transform.parent;
            itemParent.parent = null;
            
            // 隨機移動到附近距離單位 2 的地方
            Vector3 direction = moveVelocity != Vector3.zero ? moveVelocity : Vector3.back;
            // direction.y = 0;
            direction = direction.normalized * 2;
            Vector3 startPos = itemParent.localPosition;
            Vector3 endPos = new Vector3(startPos.x, 0, startPos.z);
            Vector3 controlPoint = new Vector3(startPos.x + direction.x / 4f, startPos.y * 1.5f, startPos.z + direction.z / 4f);
            Vector3[] path = new Vector3[] { controlPoint, endPos + direction};

            float timer = 0;
            // 使用 DOTween 的 DOLocalPath 搭配動畫曲線
            var duration = 0.5f;
            itemParent.DOLocalPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    curOriginItem.PlayerThrowItem();
                    curOriginItem = null;
                });
        }
        else if (curAdvancedItem != null)
        {
            var itemParent = curAdvancedItem.transform.parent;
            itemParent.parent = null;
            
            // 隨機移動到附近距離單位 2 的地方
            Vector3 randomDirection = Random.onUnitSphere;
            randomDirection.y = 0;
            randomDirection = randomDirection.normalized * 2;
            Vector3 startPos = itemParent.localPosition;
            Vector3 endPos = new Vector3(startPos.x, 0, startPos.z);
            Vector3 controlPoint = new Vector3(startPos.x + randomDirection.x / 4f, startPos.y * 1.5f, startPos.z + randomDirection.z / 4f);
            Vector3[] path = new Vector3[] { controlPoint, endPos + randomDirection};

            float timer = 0;
            // 使用 DOTween 的 DOLocalPath 搭配動畫曲線
            var duration = 0.5f;
            itemParent.DOLocalPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    curAdvancedItem.PlayerThrowItem();
                    curAdvancedItem = null;
                });
        }
    }


    /// <summary>
    /// The function is used to emit sprint effect paticle when the player is sprinting.
    /// </summary>
    private void UpdateSprintEffect()
    {
        if (isSprinting)
        {
            if (sprintEffect)
                sprintEffect.Play();
        }
        else
        {
            if (sprintEffect)
                sprintEffect.Stop();
        }
    }

    /// <summary>
    /// Recover or decrease player's stamina in conditions.
    /// </summary>
    private void UpdateStamina()
    {
        if (isSprinting)
        {
            if (status.currentStamina > 0.0f)
                status.currentStamina -= status.staminaLossPerSecond * Time.deltaTime;
            else
            {
                status.currentStamina = 0.0f;
                isDrained = true;
                isSprinting = false;
                Rest(status.restTimeWhenDrained).Forget();
            }
        }
        else if(!isDrained) // Have to rest a while if the player is drained.
        {
            if (status.currentStamina < status.maxStamina)
                status.currentStamina += status.staminaRecoveryPerSecond * Time.deltaTime;
            else
                status.currentStamina = status.maxStamina;
        }
    }

    /// <summary>
    /// Coroutine that will makes the player to wait for restTime to recover its stamina.
    /// </summary>
    /// <param name="restTime">The time the player needs to rest.</param>
    /// <returns>Interface that all coroutines use.</returns>
    private async UniTaskVoid Rest(float restTime)
    {
        await UniTask.WaitForSeconds(restTime);
        isDrained = false;
    }
    /// <summary>
    /// Check whether the player is grounded.
    /// </summary>
    private void UpdateIsGrounded()
    {
        if (Physics.CheckSphere(transform.position + checkerPositionOffset, checkerRadius, checkLayer, QueryTriggerInteraction.Ignore))
            isGrounded = true;
        else
            isGrounded = false;
    }

    /// <summary>
    /// Update player's jump via adding impulse force.
    /// </summary>
    private void UpdateJump()
    {
        //
        // if(canJump && isGrounded && isJumping)
        // {
        //     rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, 0f, rigid.linearVelocity.z);
        //     rigid.AddForce(new Vector3(0f, status.jumpForce, 0f), ForceMode.Impulse);
        //     isGrounded = false;
        // }
    }

    /// <summary>
    /// Implement this OnDrawGizmosSelected if you want to draw gizmos only if the object is selected.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + checkerPositionOffset, checkerRadius);
    }
}