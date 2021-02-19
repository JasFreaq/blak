using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    #region Variables
    
    [Header("Run State")]
    [SerializeField] float _maxRunSpeed = 14f;
    [SerializeField] float _walkSpeed = 6f;
    [SerializeField] AnimationCurve _runAccelerationCurve = new AnimationCurve();
    [Tooltip("No. of times the acceleration speed is compared to deceleration.")]
    [SerializeField] float _runAccelerationToDecelerationRatio = 2f;
    [SerializeField] float _runAccelerationTime = 0.2f;

    bool _wasRunningLastFrame = false;
    bool _wasStoppingRunLastFrame = false;
    float _runTimeCounter = 0f;
    int _runDir = 0;

    [Header("Jump State")]
    [SerializeField] float _jumpHeight = 8f;
    [SerializeField] float _wallJumpHeight = 1.5f;
    [SerializeField] float _wallJumpBufferTime = 0.3f;
    [SerializeField] float _gravity = 20f;
    [Tooltip("Controls the factor by which the jump height will reduce when the player let's up the jump button.")]
    [SerializeField] float _jumpButtonReleaseAttentuationFactor = 0.5f;
    [SerializeField] float _coyoteTimeDuration = 0.5f;
    [SerializeField] float _inAirJumpInputBufferRayLength = 0.5f;
    [SerializeField] float _inAirJumpInputBufferTime = 0.5f;

    float _coyoteTimeTracker = 0f;
    float _wallJumpBufferTimeTracker = 0;
    float _jumpBufferTimeCounter = 0f;

    [Header("Shape Abilities")]
    [SerializeField] float _powerJumpHeight = 10f;
    [Tooltip("Increase Time.timeScale to this amount at the beginning of the PowerJump.")]
    [SerializeField] float _powerJumpIncreasedTimeScale = 5f;
    [Tooltip("Reset Time.timeScale after this many seconds.")]
    [SerializeField] float _powerJumpResetTimeScaleTime = 0.1f;

    //[SerializeField] float _glideAmount = 2f;

    //Cache Components
    CharacterController2D _characterController2D = null;
    PlayerGrabber _playerGrabber = null;
    PlayerLight _playerLight = null;

    //Internals
    CharacterController2D.CharacterCollisionState2D _collisionStateFlag;

    bool _isGrounded = true;
    bool _isJumping = false;
    bool _isWallJumping = false;
    bool _isCoyoteTime = false;
    bool _isWithinJumpBuffer = false;

    bool _isPushingShape = false;
    Transform _pushedShape = null;
    
    //bool _isGliding = false;
    bool _isPowerJumping = false;

    bool _canGlide = true;
    bool _canStartGliding = false;

    Vector3 _movementDirection = Vector3.zero;
    Vector3 _deltaMovement = Vector3.zero;

    #endregion

    #region Lifecycle Functions

    private void Awake()
    {
        _characterController2D = GetComponent<CharacterController2D>();
        _playerGrabber = GetComponentInChildren<PlayerGrabber>();
        _playerLight = GetComponentInChildren<PlayerLight>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (_characterController2D && _playerLight && !_playerLight.IsTimeStopped)
        {
            Locomotion();

            if (!_collisionStateFlag.below && _collisionStateFlag.wasGroundedLastFrame)
            {
                _playerGrabber.Ungrab();
            }
        }
        else
        {
            if (!_characterController2D)
                Debug.LogError("PlayerController is missing CharacterController2D.");

            if (!_playerLight)
                Debug.LogError("PlayerController is missing PlayerLight.");
        }
    }

    #endregion

    #region Movement/Locomotion Functions

    private void Locomotion()
    {
        ProcessBaseLocomotionAndInputs();

        //Calculations
        
        if (_movementDirection.x > Mathf.Epsilon)
            transform.eulerAngles = new Vector3(0, 0, 0);
        else if (_movementDirection.x < -Mathf.Epsilon)
            transform.eulerAngles = new Vector3(0, 180, 0);

        //if (_canGlide && Input.GetAxisRaw("Vertical") > 0 && _characterController2D.velocity.y > 0.2f) 
        //{
        //    _isGliding = true;
        //    if (_canStartGliding)
        //    {
        //        _movementDirection.y = 0;
        //        _canStartGliding = false;
        //    }
        //    _movementDirection.y -= _glideAmount * Time.deltaTime;
        //}
        //else
        //{
        //    _isGliding = false;
        //    _canStartGliding = true;
            _movementDirection.y -= _gravity * Time.deltaTime;
        //}


        //Move
        _deltaMovement = _characterController2D.move(_movementDirection * Time.deltaTime);

        //Update Flags
        _collisionStateFlag = _characterController2D.collisionState;
        _isGrounded = _collisionStateFlag.below;
        
        ProcessCoyoteTime();
        ProcessJumpBufferTime();

        //Clear upward movement when upward path is blocked
        if (_collisionStateFlag.above)
        {
            _movementDirection.y = 0;
        }

        if (_collisionStateFlag.right || _collisionStateFlag.left)
        {
            ProcessWallJumpAndInput();
        }
    }
       
    private void ProcessBaseLocomotionAndInputs()
    {
        if (!_isPowerJumping)
        {
            if (_characterController2D.AboveBox)
            {
                _movementDirection.x = Input.GetAxis("Horizontal");
                _movementDirection.x *= _maxRunSpeed;
            }
            else
                ProcessRun();
        }
        else
            _movementDirection.x = 0;

        if (_isGrounded)
        {
            _movementDirection.y = 0;
            _isJumping = false;
            _isWallJumping = false;
            _isCoyoteTime = false;


            ProcessJump();
        }
        else if (_isCoyoteTime || _isWithinJumpBuffer) //Coyote Time!
        {
            ProcessJump();
        }
        else //Player is in the air
        {
            if (Input.GetButtonUp("Jump"))
            {
                if (_movementDirection.y > Mathf.Epsilon)
                    _movementDirection.y *= _jumpButtonReleaseAttentuationFactor;
            }
        }
    }

    private void ProcessRun()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        if (horizontalInput != 0)
        {
            _runDir = MathUtils.NumSign(horizontalInput);

            if (!_wasRunningLastFrame)
            {
                _runTimeCounter = 0;
                _wasRunningLastFrame = true;
            }

            float curveTime = Mathf.Clamp(_runTimeCounter / _runAccelerationTime, 0, 1);
            float horizontalMultiplier = _runAccelerationCurve.Evaluate(curveTime);
            _movementDirection.x = _runDir * (_maxRunSpeed * horizontalMultiplier);
        }
        else
        {
            if (_wasRunningLastFrame)
            {
                if (!_wasStoppingRunLastFrame)
                {
                    _runTimeCounter = 0;
                    _wasStoppingRunLastFrame = true;
                }

                float curveTime = Mathf.Clamp((_runTimeCounter / _runAccelerationTime) * _runAccelerationToDecelerationRatio, 0, 1);
                float horizontalMultiplier = _runAccelerationCurve.Evaluate(1 - curveTime);
                _movementDirection.x = _runDir * _maxRunSpeed * horizontalMultiplier;

                if (Mathf.Abs(_movementDirection.x) == 0)
                {
                    _wasRunningLastFrame = false;
                    _wasStoppingRunLastFrame = false;
                    _runDir = 0;
                }
            }
        }

        _runTimeCounter += Time.deltaTime;
    }



    private void ProcessJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (_characterController2D.AboveBox)
            {
                StartCoroutine(ProcessPowerJumpRoutine());
            }
            else
            {
                _movementDirection.y = _jumpHeight;                
            }

            _wallJumpBufferTimeTracker = Time.time;
            _isJumping = true;
        }
    }

    private void ProcessCoyoteTime()
    {
        if (!_isGrounded && !_isJumping)
        {
            if (Mathf.Abs(_coyoteTimeTracker) < Mathf.Epsilon)
            {
                _coyoteTimeTracker = Time.time;
                _isCoyoteTime = true;
            }
            else if (Mathf.Abs(Time.time - _coyoteTimeTracker) > _coyoteTimeDuration)
            {
                _isCoyoteTime = false;
            }
        }
        else
        {
            _coyoteTimeTracker = 0f;
            _isCoyoteTime = false;
        }
    }

    private void ProcessJumpBufferTime()
    {
        if (!_isGrounded) 
        {
            if (_jumpBufferTimeCounter > _inAirJumpInputBufferTime)
            {
                RaycastHit2D hit_1 = Physics2D.Raycast(_characterController2D.RaycastOrigins.bottomRight, Vector2.down,
                _inAirJumpInputBufferRayLength, _characterController2D.platformMask);
                RaycastHit2D hit_2 = Physics2D.Raycast(_characterController2D.RaycastOrigins.bottomLeft, Vector2.down,
                    _inAirJumpInputBufferRayLength, _characterController2D.platformMask);

                Debug.DrawRay(_characterController2D.RaycastOrigins.bottomRight, Vector2.down * _inAirJumpInputBufferRayLength, Color.magenta);
                Debug.DrawRay(_characterController2D.RaycastOrigins.bottomLeft, Vector2.down * _inAirJumpInputBufferRayLength, Color.magenta);


                if (hit_1 || hit_2)
                {
                    _isWithinJumpBuffer = true;
                }
                else
                {
                    _isWithinJumpBuffer = false;
                }
            }

            _jumpBufferTimeCounter += Time.deltaTime;
        }
        else
        {
            _isWithinJumpBuffer = false;
            _jumpBufferTimeCounter = 0;
        }
    }
    
    private void ProcessWallJumpAndInput()
    {
        if (!_isGrounded && Input.GetButtonDown("Jump") && !_isWallJumping && Time.time - _wallJumpBufferTimeTracker > _wallJumpBufferTime)    
        {
            _movementDirection.y = _jumpHeight + _wallJumpHeight;
            _isWallJumping = true;
        }
    }

    IEnumerator ProcessPowerJumpRoutine()
    {
        _movementDirection.y = _powerJumpHeight;
        Time.timeScale = _powerJumpIncreasedTimeScale;
        _isPowerJumping = true;
        _playerLight.CanTimeStop = false;

        yield return new WaitForSecondsRealtime(_powerJumpResetTimeScaleTime);

        Time.timeScale = 1f;
        _isPowerJumping = false;
        _playerLight.CanTimeStop = true;
    }

    #endregion
}
