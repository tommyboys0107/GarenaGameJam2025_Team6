using System;
using CliffLeeCL;
using UnityEngine;
using UnityEngine.InputSystem;

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
    Vector3 moveVelocity = Vector3.zero;
    Vector3 sprintVelocity = Vector3.zero;

    #endregion

    [Header("道具相關")]
    [SerializeField]
    Transform itemRoot = null;
    Item curItem = null;

    void Start()
    {
        curItem = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            curItem = other.GetComponent<Item>();
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            var item = other.GetComponent<Item>();
            if (curItem == item)
            {
                curItem = null;
            }
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
            if (curItem != null)
            {
                curItem.PlayerGetItem();
                curItem.transform.parent = itemRoot;
                curItem.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            isinteractive = false;
        }
    }

}