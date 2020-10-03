using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Locomotion Direct")]
    [SerializeField] private float _walkSpeed = 6f;
    [SerializeField] private float _jumpHeight = 8f;
    [SerializeField] private float _wallJumpHeight = 1.5f;
    [Header("Locomotion Indirect ")]
    [SerializeField] private float _gravity = 20f;
    [Tooltip("Controls the factor by which the jump height will reduce when the player let's up the jump button.")]
    [SerializeField] private float _jumpButtonReleaseAttentuationFactor = 0.5f;
    [SerializeField] private float _coyoteTime = 0.5f;


    //Cache Components
    private CharacterController2D _characterController2D = null;

    //Internal
    private CharacterController2D.CharacterCollisionState2D _collisionStateFlag;

    private bool _isGrounded = true;
    private bool _isJumping = false;
    private bool _isWallJumping = false;
    private bool _isCoyoteTime = true;

    float _coyoteTimeCounter = 0f;

    private Vector3 _movementDirection = Vector3.zero;

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

    private void Locomotion()
    {
        ProcessBasicInputs();

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
        if (!_isGrounded && !_isJumping)
        {
            if (Mathf.Abs(_coyoteTimeCounter) < Mathf.Epsilon)
            {
                _coyoteTimeCounter = Time.time;
                _isCoyoteTime = true;
            }
            else if (Mathf.Abs(Time.time - _coyoteTimeCounter) > _coyoteTime) 
            {
                _isCoyoteTime = false;
            }
        }
        else
        {
            _coyoteTimeCounter = 0f;
            _isCoyoteTime = false;
        }

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

    private void ProcessBasicInputs()
    {
        _movementDirection.x = Input.GetAxis("Horizontal");
        _movementDirection.x *= _walkSpeed;

        if (_isGrounded)
        {
            _movementDirection.y = 0;
            _isJumping = false;
            _isWallJumping = false;
            _isCoyoteTime = false;

            if (Input.GetButtonDown("Jump"))
            {
                _movementDirection.y = _jumpHeight;
                _isJumping = true;
            }
        }
        else if (_isCoyoteTime) //Coyote Time!
        {
            if (Input.GetButtonDown("Jump"))
            {
                _movementDirection.y = _jumpHeight;
                _isJumping = true;
            }
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

    private void ProcessWallJumpAndInput()
    {
        if (!_isGrounded && Input.GetButtonDown("Jump") && !_isWallJumping) 
        {
            _movementDirection.y = _jumpHeight * _wallJumpHeight;
            _isWallJumping = true;
        }
    }


}
