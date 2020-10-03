using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Locomotion Base")]
    [SerializeField] float _walkSpeed = 6f;
    [SerializeField] float _jumpHeight = 8f;
    [SerializeField] float _wallJumpHeight = 1.5f;

    [Header("Locomotion Affectors")]
    [SerializeField] float _gravity = 20f;
    [Tooltip("Controls the factor by which the jump height will reduce when the player let's up the jump button.")]
    [SerializeField] float _jumpButtonReleaseAttentuationFactor = 0.5f;
    [SerializeField] float _coyoteTimeDuration = 0.5f;

    [Header("Slope Sliding")]
    [SerializeField] float _slopeSlideSpeed = 4f;
    [SerializeField] float _slopeMinHorizontalSpeed = 1f;
    [Tooltip("In Degrees.")]
    [SerializeField] float _slopeLimit = 30f;

    //Cache Components
    CharacterController2D _characterController2D = null;

    //Internals
    CharacterController2D.CharacterCollisionState2D _collisionStateFlag;

    bool _isGrounded = true;
    bool _isJumping = false;
    bool _isWallJumping = false;
    bool _isCoyoteTime = true;

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
        if (_characterController2D)
        {
            _characterController2D.SlopeLimit = _slopeLimit;
        }
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

        _movementDirection.y -= _gravity * Time.deltaTime;
        

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
        Vector3 tempMovement = Vector3.zero;
//      
        _movementDirection.x = Input.GetAxis("Horizontal");
        _movementDirection.x *= _walkSpeed;
        
        if (_isGrounded)
        {
            _movementDirection.y = 0;
            _isJumping = false;
            _isWallJumping = false;
            _isCoyoteTime = false;

            ProcessSlopeSliding();
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

    private void ProcessSlopeSliding()
    {
        RaycastHit2D raycastHit = _characterController2D.VerticalRaycastHit;
        if (raycastHit)
        {
            float slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
            Vector3 slopeGradient = raycastHit.normal;

            if (_slopeLimit - slopeAngle < Mathf.Epsilon) 
            {
                Vector3 slopeMovement = slopeGradient * slopeAngle;
                float horizontal = _movementDirection.x + (slopeMovement.normalized.x * _slopeSlideSpeed);

                if (MathUtils.NumSign(slopeMovement.x) == 1)
                    horizontal = Mathf.Max(horizontal, _slopeMinHorizontalSpeed);
                else
                    horizontal = Mathf.Min(horizontal, -_slopeMinHorizontalSpeed);

                _movementDirection = new Vector3(horizontal, -slopeMovement.normalized.y * _slopeSlideSpeed, 0f);
            }
        }
    }

    private void ProcessJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _movementDirection.y = _jumpHeight;
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
