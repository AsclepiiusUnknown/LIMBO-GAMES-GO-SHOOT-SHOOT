using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LIMBO.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : Player
    {
        #region |VARIABLES

        #region ||Base Movement
        [Header("Base Movement")]
        public float walkSpeed = .7f;
        public float runSpeed = 1.25f;
        [Range(0f, 1f)] public float stepLength = .7f;

        //*PRIVATE//
        [HideInInspector] public bool _isWalking = true;
        public bool _canRun = true;
        private Vector2 moveInput;
        private Vector3 moveDir = Vector3.zero;
        #endregion

        #region ||Damping
        public float airDamping = .5f;
        public float grndDamping = .5f;
        #endregion

        #region ||Jumping
        [Header("Jumping")] public float jumpSpeed = 9;
        public float jumpCut = .5f;
        public float coyoteJump = .2f;
        public float grndRemValue = .2f;
        public bool canAirWalk = true;

        //*PRIVATE
        private bool isJumping;
        private float coyoteKeeper;
        private float grndRemKeeper;
        private bool isGrounded;
        #endregion

        #region ||Mouse
        [Header("Mouse")] public MouseLook mouseLook;

        //*PRIVATE//
        private Camera _cam;
        #endregion

        #region ||Gravity
        [Header("Gravity")] public float gravityScale = 2;
        public float groundForce = 10;

        //*PRIVATE
        [HideInInspector] public bool _useGravity = true;
        #endregion

        #region ||Audio
        [Header("Audio")] public AudioClip jumpSound; // the sound played when character leaves the ground.
        public AudioClip landSound; // the sound played when character touches back on ground.
        public AudioClip[] footstepSounds; // an array of footstep sounds that will be randomly selected from.

        //*PRIVATE//
        private AudioSource _audioSource;
        #endregion

        #region ||Effects
        [Header("Effects")] private bool useFovKick = true;
        private FOVKick fovKick = new FOVKick();
        private bool useHeadBob = true;
        private CurveControlledBob headBob = new CurveControlledBob();
        private LerpControlledBob jumpBob = new LerpControlledBob();
        private float stepInterval;
        #endregion

        #region ||Collision
        [Header("Collision")] public float impactForceMultiplier = .2f;
        #endregion

        #region ||Misc
        [Header("Misc.")]
        //*PRIVATE//
        private float _yRotation;
        private CharacterController _controller;
        private CollisionFlags _collisionFlags;
        private bool _prevGrounded;
        private Vector3 _ogCameraPos;
        private float _stepCycle;
        private float _nextStep;
        #endregion

        #endregion

        private void Awake()
        {
            #region |Component Gathering
            _controller = GetComponent<CharacterController>();
            _audioSource = GetComponent<AudioSource>();
            _cam = Camera.main;
            #endregion
        }

        private void Start()
        {
            #region |Component Setup
            if (_cam == null) return;
            _ogCameraPos = _cam.transform.localPosition;
            fovKick.Setup(_cam);
            print(stepInterval);
            headBob.Setup(_cam, stepInterval);
            _nextStep = _stepCycle / 2f;
            mouseLook.Init(transform, _cam.transform);
            #endregion

            #region |Cursor Setup
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            #endregion
        }

        private void Update()
        {
            #region |Timers

            coyoteKeeper -= Time.deltaTime;
            grndRemKeeper -= Time.deltaTime;

            #endregion

            #region |Rotate View

            if (mouseLook != null)
                mouseLook.LookRotation(transform, _cam.transform);
            else
                print("**NULL**");

            #endregion

            #region |Jumping / Landing

            #region ||Update Jump/Ground Input

            //Update Coyote timer for early input
            if (Input.GetKeyDown(KeyCode.Space))
                coyoteKeeper = coyoteJump;

            //Update the Ground Remember timer for late input
            if (_controller.isGrounded)
                grndRemKeeper = grndRemValue;

            isGrounded = (grndRemKeeper > 0);

            #endregion

            #region ||Just Landing

            if (!_prevGrounded && isGrounded)
            {
                StartCoroutine(jumpBob.DoBobCycle()); //Animate the landing
                PlayLandingSound(); //Play landing sound effect
                moveDir.y = 0f; //We are no longer moving moving on the Y axis (up/down)
                isJumping = false; //we are no longer jumping
            }

            #endregion

            #region ||Just Jumping

            //??????
            if (!isGrounded && !isJumping && _prevGrounded)
                moveDir.y = 0f;

            #endregion

            //Update grounding variable for next frames checks
            _prevGrounded = isGrounded;

            #endregion

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _isWalking = !_isWalking;
            }
        }

        private void FixedUpdate()
        {
            #region |Movement

            #region ||Input

            //Perform input checks and get the correct speed for movements
            GetInput(out var speed);
            // Always move along the cameras local forward as it is the direction that the player is facing
            // ReSharper disable once Unity.InefficientPropertyAccess
            var desiredMove = transform.forward * moveInput.y + transform.right * moveInput.x;

            #endregion

            #region ||Ground Normal Calculations

            // Get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, _controller.radius, Vector3.down, out var hitInfo,
                _controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            #endregion

            #region ||Movement Damping

            moveDir.x += desiredMove.x * speed;
            moveDir.z += desiredMove.z * speed;
            moveDir.x *= Mathf.Pow(1 - grndDamping, Time.deltaTime * 10);
            moveDir.z *= Mathf.Pow(1 - grndDamping, Time.deltaTime * 10);
            #endregion

            #region ||Ground Checks

            //! if (_useGravity)
            // {
            if (_controller.isGrounded)
            {
                //Apply a set force to ensure we remain on the ground
                moveDir.y = -groundForce;
            }
            else
            {
                // Apply gravity modified by the custom gravity scale and time in order to bring us down
                moveDir += Physics.gravity * (gravityScale * Time.fixedDeltaTime);
            }
            // }

            // If we are grounded
            if (isGrounded && coyoteKeeper > 0)
            {
                moveDir.y = jumpSpeed; //Apply the jump speed to the overall vertical movement
                PlayJumpSound(); //Play a jump sound effect
                coyoteKeeper = 0; //Reset cause we have run the jump
                isJumping = true; //We are now jumping
            }

            #endregion

            #region ||Mid-Air Movement
            //if we are not (theoretically) grounded 
            if (!isGrounded)
            {
                // If we can change velocity mid-air
                if (canAirWalk)
                {
                    // !_moveDir.x *= Mathf.Pow(1 - midAirDamping, Time.deltaTime * 10);
                    // !_moveDir.z *= Mathf.Pow(1 - midAirDamping, Time.deltaTime * 10);
                }
                else //if not
                {
                    // Make the move direction the same as our current velocity
                    var velocity = _controller.velocity;
                    moveDir.x = velocity.x;
                    moveDir.z = velocity.z;
                }
            }

            #endregion

            //Apply movement calculations using character controller
            _collisionFlags = _controller.Move(moveDir * Time.fixedDeltaTime);

            #endregion

            #region |Jump Cutting

            //If the jump is cancelled while still rising then cut the velocity by a set amount
            if (Input.GetKeyUp(KeyCode.Space) && moveDir.y > 0) moveDir.y *= jumpCut;

            #endregion

            //Update the step cycle to allow for accurate footstep audio
            ProgressStepCycle(speed);
            //Update the cameras position if we are using head bob animations
            UpdateCameraPosition(speed);

            mouseLook.UpdateCursorLock();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (_collisionFlags == CollisionFlags.Below) return;

            if (body == null || body.isKinematic) return;
            var avgScale = (body.transform.localScale.x + body.transform.localScale.y + body.transform.localScale.z) / 3;
            body.AddForceAtPosition(_controller.velocity / (body.mass * avgScale) * impactForceMultiplier, hit.point,
                ForceMode.Impulse);
        }

        #region |Input
        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool wasWalking = _isWalking;

            if (!_canRun)
            {
                _isWalking = true;
            }

            // Set the correct speed based off the running input
            speed = _isWalking ? walkSpeed : runSpeed;
            moveInput = new Vector2(horizontal, vertical);

            // Normalize the move input if it exceeds the max of 1
            if (moveInput.sqrMagnitude > 1)
                moveInput.Normalize();

            // If the player has just changed movement (run/walk) and we have FOV kick enabled
            if (_isWalking != wasWalking && useFovKick && _controller.velocity.sqrMagnitude > 0)
            {
                //Reset and then perform the appropriate kick
                StopAllCoroutines();
                StartCoroutine(!_isWalking ? fovKick.FOVKickUp() : fovKick.FOVKickDown());
            }
        }
        #endregion

        #region |Camera Effects
        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!useHeadBob) return;
            if (_controller.velocity.magnitude > 0 && (grndRemValue > 0))
            {
                var camTransform = _cam.transform;
                camTransform.localPosition = headBob.DoHeadBob(_controller.velocity.magnitude + speed * (_isWalking ? 1f : stepLength));
                newCameraPosition = camTransform.localPosition;
                newCameraPosition.y = camTransform.localPosition.y - jumpBob.Offset();
            }
            else
            {
                newCameraPosition = _cam.transform.localPosition;
                newCameraPosition.y = _ogCameraPos.y - jumpBob.Offset();
            }

            _cam.transform.localPosition = newCameraPosition;
        }
        #endregion

        #region |Sound Effects

        #region ||Landing

        private void PlayLandingSound()
        {
            _audioSource.clip = landSound;
            _audioSource.Play();
            _nextStep = _stepCycle + .5f;
        }

        #endregion

        #region ||Jumping

        private void PlayJumpSound()
        {
            _audioSource.clip = jumpSound;
            _audioSource.Play();
        }

        #endregion

        #region ||Footsteps

        private void ProgressStepCycle(float speed)
        {
            float moveAvg = (Mathf.Abs(moveDir.x) + Mathf.Abs(moveDir.z)) / 2;
            if (_controller.velocity.sqrMagnitude > 0 && (moveInput.x != 0 || moveInput.y != 0))
                _stepCycle += (_controller.velocity.magnitude + moveAvg * (_isWalking ? 1f : stepLength)) *
                              Time.fixedDeltaTime;

            if (!(_stepCycle > _nextStep)) return;

            _nextStep = _stepCycle + stepInterval;

            PlayFootStepAudio();
        }

        private void PlayFootStepAudio()
        {
            if (!_controller.isGrounded) return;
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            var n = Random.Range(1, footstepSounds.Length);
            _audioSource.clip = footstepSounds[n];
            _audioSource.PlayOneShot(_audioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = _audioSource.clip;
        }

        #endregion

        #endregion
    }
}