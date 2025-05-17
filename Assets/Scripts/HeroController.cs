using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace CliffLeeCL
{
    /// <summary>
    /// Control player's movement and rotation by user's input.
    /// </summary>
    public class HeroController : MonoBehaviour
    {
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

        private InputSystem_Actions inputActions;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
           inputActions.Disable(); 
        }

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
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            UpdateSprintEffect();
            UpdateStamina();
        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        /// </summary>
        void FixedUpdate()
        {
            UpdateMovement();
            UpdateIsGrounded();
            UpdateJump();
        }

        /// <summary>
        /// The function is used to emit sprint effect paticle when the player is sprinting.
        /// </summary>
        void UpdateSprintEffect()
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
        void UpdateStamina()
        {
            if (isSprinting)
            {
                if (status.currentStamina > 0.0f)
                    status.currentStamina -= status.staminaLossPerSecond * Time.deltaTime;
                else
                {
                    status.currentStamina = 0.0f;
                    isDrained = true;
                    StartCoroutine("Rest", status.restTimeWhenDrained);
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
        IEnumerator Rest(float restTime)
        {
            yield return new WaitForSeconds(restTime);
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
        
        /// <summary>
        /// Update player's movement via assigning new velocity.
        /// </summary>
        private void UpdateMovement()
        {
            var moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            var isSprintKeyPressed = inputActions.Player.Sprint.IsPressed(); 
            var moveVelocity = new Vector3(moveInput.x, 0, moveInput.y) * (status.movingSpeed * Time.fixedDeltaTime);
            var sprintVelocity = new Vector3(moveInput.x, 0, moveInput.y) * (status.sprintSpeed * Time.fixedDeltaTime);
            
            if (canSprint && isSprintKeyPressed && (status.currentStamina > 0))
            {
                var newPosition = rigid.position + sprintVelocity;
                rigid.MovePosition(newPosition);
                isSprinting = sprintVelocity.sqrMagnitude > Mathf.Epsilon; // Is really moving.
            }
            else
            {
                var newPosition = rigid.position + moveVelocity;
                rigid.MovePosition(newPosition);                 
                isSprinting = false;
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
            var isJumping = inputActions.Player.Jump.triggered; 

            if(canJump && isGrounded && isJumping)
            {
                rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, 0f, rigid.linearVelocity.z);
                rigid.AddForce(new Vector3(0f, status.jumpForce, 0f), ForceMode.Impulse);
                isGrounded = false;
            }
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
}
