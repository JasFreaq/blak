using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Run State")]
    [SerializeField] float _maxRunSpeed = 6f;
    [SerializeField] AnimationCurve _runAccelerationCurve = new AnimationCurve();
    [Tooltip("No. of times the acceleration speed is compared to deceleration.")]
    [SerializeField] float _runAccelerationToDecelerationRatio = 2f;
    [SerializeField] float _runAccelerationTime = 0.2f;

    bool _wasRunningLastFrame = false;
    bool _wasStoppingRunLastFrame = false;
    float _runTimeCounter = 0f;
    int _runDir = 0;

    [Header("Jump")]
    [SerializeField] float _playerHeight = 2.56f; //May derive from BoxCollider, decide later
    [Tooltip("This value multiplied by the PlayerHeight is the MaxJumpHeight of the player.")]
    [SerializeField] int _maxJumpHeightToPlayerHeightRatio = 3;
    [SerializeField] float _jumpHeight = 8f;
    [SerializeField] float _wallJumpHeight = 1.5f;
    [SerializeField] AnimationCurve _jumpCurve = new AnimationCurve();

    [Header("Locomotion Affectors")]
    [SerializeField] float _gravity = 20f;
    [Tooltip("Controls the factor by which the jump height will reduce when the player let's up the jump button.")]
    [SerializeField] float _jumpButtonReleaseAttentuationFactor = 0.5f;
    [SerializeField] float _coyoteTimeDuration = 0.5f;

    [Header("Shape Abilities")]
    [SerializeField] float _powerJumpHeight = 10f;
    [SerializeField] float _glideAmount = 2f;

    //Cache Components
    CharacterController2D _characterController2D = null;

    //Internals
    CharacterController2D.CharacterCollisionState2D _collisionStateFlag;


    bool _isGrounded = true;
    bool _isJumping = false;
    bool _isWallJumping = false;
    bool _isCoyoteTime = true;
    bool _isGliding = false;
    bool _isPowerJumping = false;

    bool _canPowerJump = true;

    bool _canGlide = true;
    bool _canStartGliding = false;

    float _coyoteTimeCounter = 0f;

    Vector3 _movementDirection = Vector3.zero;

    #endregion

    #region Lifecycle Functions

    private void Awake()
    {
        _characterController2D = GetComponent<CharacterController2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (_characterController2D)
        {
            Locomotion();
        }
        else
        {
            Debug.LogError("PlayerController is missing CharacterController2D.");
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

        if (_canGlide && Input.GetAxisRaw("Vertical") > 0 && _characterController2D.velocity.y > 0.2f) 
        {
            _isGliding = true;
            if (_canStartGliding)
            {
                _movementDirection.y = 0;
                _canStartGliding = false;
            }
            _movementDirection.y -= _glideAmount * Time.deltaTime;
        }
        else
        {
            _isGliding = false;
            _canStartGliding = true;
            _movementDirection.y -= _gravity * Time.deltaTime;
        }
        

        //Move
        _characterController2D.move(_movementDirection * Time.deltaTime);

        //Update Flags
        _collisionStateFlag = _characterController2D.collisionState;
        _isGrounded = _collisionStateFlag.below;
        
        ProcessCoyoteTime();

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
        ProcessRun();

        if (_isGrounded)
        {
            _movementDirection.y = 0;
            _isJumping = false;
            _isWallJumping = false;
            _isCoyoteTime = false;

            ProcessJump();
        }
        else if (_isCoyoteTime) //Coyote Time!
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
            _movementDirection.x = horizontalInput * _maxRunSpeed * horizontalMultiplier;
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
            _movementDirection.y = _canPowerJump ? _jumpHeight + _powerJumpHeight : _jumpHeight;
            _isPowerJumping = _canPowerJump;
            _isJumping = true;
        }
    }

    private void ProcessCoyoteTime()
    {
        if (!_isGrounded && !_isJumping)
        {
            if (Mathf.Abs(_coyoteTimeCounter) < Mathf.Epsilon)
            {
                _coyoteTimeCounter = Time.time;
                _isCoyoteTime = true;
            }
            else if (Mathf.Abs(Time.time - _coyoteTimeCounter) > _coyoteTimeDuration)
            {
                _isCoyoteTime = false;
            }
        }
        else
        {
            _coyoteTimeCounter = 0f;
            _isCoyoteTime = false;
        }
    }
    
    private void ProcessWallJumpAndInput()
    {
        if (!_isGrounded && Input.GetButtonDown("Jump") && !_isWallJumping) 
        {
            _movementDirection.y = _jumpHeight * _wallJumpHeight;
            _isWallJumping = true;
        }
    }

    #endregion
}
