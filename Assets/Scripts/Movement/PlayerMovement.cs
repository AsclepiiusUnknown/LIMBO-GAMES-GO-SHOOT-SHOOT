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

        #region ||Movement
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5;
        [SerializeField] private float runSpeed = 10;
        [SerializeField] private bool canAirWalk;
        [Range(0f, 1f)]
        [SerializeField] private float runStepLength = .7f;
        [SerializeField] private float midAirDamping;
        [SerializeField] private float movementDamping;

        //*PRIVATE//
        [HideInInspector] public bool _isWalking = true;
        [HideInInspector] public bool _canRun = true;
        private Vector2 _moveInput;
        private Vector3 _moveDir = Vector3.zero;
        #endregion

        #region ||Jumping
        [Header("Jumping")]
        [SerializeField] private float jumpSpeed = 10;
        [SerializeField] private float jumpCut = .5f;
        [SerializeField] private float coyoteJump = .2f;
        [SerializeField] private float grndRem = .2f;

        //*PRIVATE
        private bool _jumping;
        private float _coyoteJump;
        private float _grndRem;
        private bool _isGrounded;
        #endregion

        #region ||Mouse
        [Header("Mouse")]
        [SerializeField] private MouseLook mouseLook;

        //*PRIVATE//
        private Camera _cam;
        #endregion

        #region ||Gravity
        [Header("Gravity")]
        [SerializeField] private float gravityScale = 2;
        [SerializeField] private float groundForce = 10;

        //*PRIVATE
        /// <summary>
        /// A bool to check/control whether or not this script should be applying gravity to this player at any point in time.
        /// </summary>
        [HideInInspector] public bool _useGravity = true;
        #endregion

        #region ||Audio
        [Header("Audio")]
        [SerializeField] private AudioClip jumpSound; // the sound played when character leaves the ground.
        [SerializeField] private AudioClip landSound; // the sound played when character touches back on ground.
        [SerializeField] private AudioClip[] footstepSounds; // an array of footstep sounds that will be randomly selected from.

        //*PRIVATE//
        private AudioSource _audioSource;
        #endregion

        #region ||Effects
        [Header("Effects")]
        [SerializeField] private bool useFovKick = true;
        [SerializeField] private FOVKick fovKick = new FOVKick();
        [SerializeField] private bool useHeadBob = true;
        [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
        [SerializeField] private float stepInterval;
        #endregion

        #region ||Collision
        [Header("Collision")]
        [SerializeField] private float impactForceMultiplier = .2f;
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

        public bool IsSetup { get { return isSetup; } }
        private bool isSetup = false;

        #endregion

        public void Setup()
        {
            #region |Component Gathering
            _controller = GetComponent<CharacterController>();
            _audioSource = GetComponent<AudioSource>();
            _cam = GetComponentInChildren<Camera>();
            _cam.enabled = true;
            GetComponentInChildren<Canvas>().enabled = true;
            #endregion

            #region |Component Setup
            if (_cam == null) return;
            _ogCameraPos = _cam.transform.localPosition;
            fovKick.Setup(_cam);
            headBob.Setup(_cam, stepInterval);
            _nextStep = _stepCycle / 2f;
            mouseLook.Init(transform, _cam.transform);
            #endregion

            #region |Cursor Setup
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            #endregion

            isSetup = true;
        }

        private void Update()
        {
            if (!isSetup)
                return;

            #region |Timers

            _coyoteJump -= Time.deltaTime;
            _grndRem -= Time.deltaTime;

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
            if (Input.GetButtonDown("Jump"))
                _coyoteJump = coyoteJump;

            //Update the Ground Remember timer for late input
            if (_controller.isGrounded)
                _grndRem = grndRem;

            _isGrounded = (_grndRem > 0);

            #endregion

            #region ||Just Landing

            if (!_prevGrounded && _isGrounded)
            {
                StartCoroutine(jumpBob.DoBobCycle()); //Animate the landing
                PlayLandingSound(); //Play landing sound effect
                _moveDir.y = 0f; //We are no longer moving moving on the Y axis (up/down)
                _jumping = false; //we are no longer jumping
            }

            #endregion

            #region ||Just Jumping

            //??????
            if (!_isGrounded && !_jumping && _prevGrounded)
                _moveDir.y = 0f;

            #endregion

            //Update grounding variable for next frames checks
            _prevGrounded = _isGrounded;

            #endregion

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _isWalking = !_isWalking;
            }
        }

        private void FixedUpdate()
        {
            if (!isSetup)
                return;

            #region |Movement

            #region ||Input

            //Perform input checks and get the correct speed for movements
            GetInput(out var speed);
            // Always move along the cameras local forward as it is the direction that the player is facing
            // ReSharper disable once Unity.InefficientPropertyAccess
            var desiredMove = transform.forward * _moveInput.y + transform.right * _moveInput.x;

            #endregion

            #region ||Ground Normal Calculations

            // Get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, _controller.radius, Vector3.down, out var hitInfo, _controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore); desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            #endregion

            #region ||Movement Damping

            _moveDir.x += desiredMove.x * speed;
            _moveDir.z += desiredMove.z * speed;
            _moveDir.x *= Mathf.Pow(1 - movementDamping, Time.deltaTime * 10);
            _moveDir.z *= Mathf.Pow(1 - movementDamping, Time.deltaTime * 10);
            #endregion

            #region ||Ground Checks

            //! if (_useGravity)
            // {
            if (_controller.isGrounded)
            {
                //Apply a set force to ensure we remain on the ground
                _moveDir.y = -groundForce;
            }
            else
            {
                // Apply gravity modified by the custom gravity scale and time in order to bring us down
                _moveDir += Physics.gravity * (gravityScale * Time.fixedDeltaTime);
            }
            // }

            // If we are grounded
            if (_isGrounded && _coyoteJump > 0)
            {
                _moveDir.y = jumpSpeed; //Apply the jump speed to the overall vertical movement
                PlayJumpSound(); //Play a jump sound effect
                _coyoteJump = 0; //Reset cause we have run the jump
                _jumping = true; //We are now jumping
            }

            #endregion

            #region ||Mid-Air Movement
            //if we are not (theoretically) grounded 
            if (!_isGrounded)
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
                    _moveDir.x = velocity.x;
                    _moveDir.z = velocity.z;
                }
            }

            #endregion

            //Apply movement calculations using character controller
            _collisionFlags = _controller.Move(_moveDir * Time.fixedDeltaTime);

            #endregion

            #region |Jump Cutting

            //If the jump is cancelled while still rising then cut the velocity by a set amount
            if (Input.GetButtonUp("Jump") && _moveDir.y > 0) _moveDir.y *= jumpCut;

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
            _moveInput = new Vector2(horizontal, vertical);

            // Normalize the move input if it exceeds the max of 1
            if (_moveInput.sqrMagnitude > 1)
                _moveInput.Normalize();

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
            if (_controller.velocity.magnitude > 0 && (grndRem > 0))
            {
                var camTransform = _cam.transform;
                camTransform.localPosition =
                    headBob.DoHeadBob(_controller.velocity.magnitude + speed * (_isWalking ? 1f : runStepLength));
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
            float moveAvg = (Mathf.Abs(_moveDir.x) + Mathf.Abs(_moveDir.z)) / 2;
            if (_controller.velocity.sqrMagnitude > 0 && (_moveInput.x != 0 || _moveInput.y != 0))
                _stepCycle += (_controller.velocity.magnitude + moveAvg * (_isWalking ? 1f : runStepLength)) *
                Time.fixedDeltaTime;

            if (!(_stepCycle > _nextStep)) return;

            _nextStep = _stepCycle + stepInterval;

            PlayFootStepAudio();
        }

        private void PlayFootStepAudio()
        {
            if (!_controller.isGrounded) return;

            if (footstepSounds.Length > 0)
            {
                // pick & play a random footstep sound from the array,
                // excluding sound at index 0
                var n = Random.Range(1, footstepSounds.Length);
                _audioSource.clip = footstepSounds[n];
                _audioSource.PlayOneShot(_audioSource.clip);

                // move picked sound to index 0 so it's not picked next time
                footstepSounds[n] = footstepSounds[0];
                footstepSounds[0] = _audioSource.clip;
            }
        }

        #endregion

        #endregion
    }
}