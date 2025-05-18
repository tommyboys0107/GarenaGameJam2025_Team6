using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine.InputSystem;

namespace CliffLeeCL
{
    /// <summary>
    /// Control player's movement and rotation by user's input.
    /// </summary>
    public class HeroController : MonoBehaviour
    {
        public enum AnimType
        {
            attack,
            magic01,
            run,
        }
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

        [Header("Hero")]
        [SerializeField]
        private Transform itemRoot;
        [SerializeField]
        private float maxSaturationLevel = 100f; 
        [SerializeField]
        private int maxPowerEnergy = 3;
        [SerializeField]
        private int maxWaterEnergy = 3;
        [SerializeField]
        private float saturationLossPerSecond = 10.0f;
        [SerializeField]
        private float saturationGainPerFood = 40.0f;
        [ShowInInspector, ReadOnly]
        private float currentSaturationLevel;
        [ShowInInspector, ReadOnly]
        private int currentPowerEnergy;
        [ShowInInspector, ReadOnly]
        private int currentWaterEnergy;

        private float CurrentSaturationLevel
        {
            get => currentSaturationLevel;
            set
            {
                currentSaturationLevel = Mathf.Clamp(value, 0, maxSaturationLevel);
                EventManager.Instance.OnSaturationLevelChanged(currentSaturationLevel);
            }
        }
        private int CurrentPowerEnergy
        {
            get => currentPowerEnergy;
            set
            {
                currentPowerEnergy = Mathf.Clamp(value, 0, maxPowerEnergy);
                if (value == currentPowerEnergy)
                {
                    EventManager.Instance.OnPowerEnergyChanged(currentPowerEnergy);
                }
            }
        }
        private int CurrentWaterEnergy
        {
            get => currentWaterEnergy;
            set
            {
                currentWaterEnergy = Mathf.Clamp(value, 0, maxWaterEnergy);
                if (value == currentWaterEnergy)
                {
                    EventManager.Instance.OnWaterEnergyChanged(currentWaterEnergy);
                }
            }
        }

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
        
        [Header("Spine")]
        [SerializeField]
        SkeletonAnimation spineAnimation = null;
        /// <summary>
        /// Time that transition between normalFOV to sprintFOV.
        /// </summary>
        private float normalToSprintTime = 0.0f;
        /// <summary>
        /// Time that transition between sprintFOV to normalFOV.
        /// </summary>
        private float sprintToNormalTime = 0.0f;
        
        private Rigidbody rigid;
        /// <summary>
        /// Is used to get movement related data.
        /// </summary>
        private PlayerStatus status;
        /// <summary>
        /// Is true when player is on the ground.
        /// </summary>
        private bool isGrounded = true;
        /// <summary>
        /// Is true when player is sprinting.
        /// </summary>
        private bool isSprinting = false;
        /// <summary>
        /// Is true when player drain his stamina.
        /// </summary>
        private bool isDrained = false;
        private bool isMoving = false;
        private bool isGameOver = false;
        private bool isHoldingItem = false;
        private Vector3 moveVelocity = Vector3.zero;
        private Vector3 sprintVelocity = Vector3.zero;
        private float originalItemYPosition;
        private Transform interactableItemTransform;
        private AnimType curAnimType;

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            rigid = GetComponent<Rigidbody>();
            Assert.IsTrue(rigid, "Need \"Rigidbody\" component on this gameObject");
            status = GetComponent<PlayerStatus>();
            Assert.IsTrue(status, "Need \"PlayerStatus\" component on this gameObject");

            status.currentStamina = status.maxStamina;
            CurrentSaturationLevel = maxSaturationLevel;
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
            
            if (CurrentSaturationLevel > 0)
            {
                CurrentSaturationLevel -= saturationLossPerSecond * Time.deltaTime;
            }
            else
            {
                isGameOver = true;
                EventManager.Instance.OnGameOver();
            }
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Food"))
            {
                CurrentSaturationLevel += saturationGainPerFood;
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("WaterEnergy"))
            {
                CurrentWaterEnergy++;
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("PowerEnergy"))
            {
                CurrentPowerEnergy++;
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("CraftingTable"))
            {
                var craftingTable = other.GetComponent<CraftingTable>();
                craftingTable.HeroHoldItem(true);
                interactableItemTransform = other.transform.parent;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("CraftingTable"))
            {
                var craftingTable = other.GetComponent<CraftingTable>();
                craftingTable.HeroHoldItem(false);
                interactableItemTransform = null;
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
        /// Transition between different FOVs if needed.
        /// </summary>
        private void UpdateCameraFOV()
        {
            if (isSprinting && (Camera.main.fieldOfView < sprintFOV))
            {
                sprintToNormalTime = 0.0f;
                normalToSprintTime += Time.deltaTime;
                //Not good but enough.
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sprintFOV, normalToSprintTime / FOVTransitionTime);
            }
            else if(!isSprinting && (Camera.main.fieldOfView > normalFOV))
            {
                normalToSprintTime = 0.0f;
                sprintToNormalTime += Time.deltaTime;
                //Not good but enough.
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, normalFOV, sprintToNormalTime / FOVTransitionTime);
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

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (isGameOver || !context.performed) return;
            
            if (interactableItemTransform)
            {
                if (isHoldingItem)
                {
                    interactableItemTransform.parent = null;
                    interactableItemTransform.transform.position = 
                        new Vector3(transform.position.x, originalItemYPosition, transform.position.z - 1.0f);
                    interactableItemTransform = null;
                    isHoldingItem = false;
                }
                else
                {
                    originalItemYPosition = interactableItemTransform.position.y;
                    interactableItemTransform.parent = itemRoot;
                    interactableItemTransform.localPosition = Vector3.zero;
                    isHoldingItem = true;
                }
            }
            else
            {
                    
            }
        }

        public void OnSkillAttack1(InputAction.CallbackContext context)
        {
            if (isGameOver || !context.performed) return;
            
            if (CurrentPowerEnergy > 0)
            {
                CurrentPowerEnergy--;
            }
        }

        public void OnSkillAttack2(InputAction.CallbackContext context)
        {
            if (isGameOver || !context.performed) return;
            
            if (CurrentWaterEnergy > 0)
            {
                CurrentWaterEnergy--;
            }
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
        
        [Button]
        public void ChangeAnimation(AnimType type, bool isLoop = false)
        {
            curAnimType = type;
            spineAnimation.state.SetAnimation(0, type.ToString(), isLoop);
        }

    }
}
