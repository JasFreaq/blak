using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Base Locomotion")]
    [SerializeField] private float _walkSpeed = 6f;
    [SerializeField] private float _jumpHeight = 8f;
    [Tooltip("Controls the factor by which the jump height will reduce when the player let's up the jump button.")]
    [SerializeField] private float _jumpButtonReleaseAttentuationFactor = 0.5f;
    [SerializeField] private float _gravity = 20f;
    [Header("Wall Jump")]
    [SerializeField] private Vector2 _wallJumpAmounts = new Vector2(0.5f, 1.5f);
    [SerializeField] private float _timeBetweenWallJumps = 0.5f;
    [SerializeField] private float _wallGrabTimeLimit = 3f;

    //Cache Components
    private CharacterController2D _characterController2D = null;

    //Internal
    private CharacterController2D.CharacterCollisionState2D _collisionStateFlag;

    private bool _isGrounded = true;
    private bool _isJumping = false;
    private bool _justWallJumped = false;
    private bool _isWallGrabbed = false;

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
        Locomotion();
        print(_collisionStateFlag.right);
    }

    #endregion

    private void Locomotion()
    {
        if (!_justWallJumped)
        {
            _movementDirection.x = Input.GetAxis("Horizontal");
            _movementDirection.x *= _walkSpeed;
        }

        if (_isGrounded)
        {
            _movementDirection.y = 0;
            _isJumping = false;

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

        if (_movementDirection.x > Mathf.Epsilon)
            transform.eulerAngles = new Vector3(0, 0, 0);
        else if (_movementDirection.x < -Mathf.Epsilon)
            transform.eulerAngles = new Vector3(0, 180, 0);

        if (!_isWallGrabbed)
            _movementDirection.y -= _gravity * Time.deltaTime;

        if (!_characterController2D)
        {
            Debug.LogError("PlayerController is missing CharacterController2D.");
            return;
        }

        _characterController2D.move(_movementDirection * Time.deltaTime);
        _collisionStateFlag = _characterController2D.collisionState;

        _isGrounded = _collisionStateFlag.below;
        if (_isGrounded)
            _isWallGrabbed = false;

        if (_collisionStateFlag.above)
        {
            _movementDirection.y = 0;
        }

        if ((_collisionStateFlag.right || _collisionStateFlag.left) && !_isGrounded) 
        {
            if (Input.GetButtonDown("Jump") && !_justWallJumped)
            {
                ProcessWallJump();
            }
            else if (Input.GetButton("Grab"))
            {
                if (!_isWallGrabbed)
                    StartCoroutine(ProcessWallGrabRoutine());
            }
        }
    }

    private void ProcessWallJump()
    {
        if (_movementDirection.x > Mathf.Epsilon)
        {
            _movementDirection.x = -_jumpHeight * _wallJumpAmounts.x;
            _movementDirection.y = _jumpHeight * _wallJumpAmounts.y;
        }
        else if (_movementDirection.x < -Mathf.Epsilon)
        {
            _movementDirection.x = _jumpHeight * _wallJumpAmounts.x;
            _movementDirection.y = _jumpHeight * _wallJumpAmounts.y;
        }

        StartCoroutine(WallJumpTimerRoutine());
    }

    IEnumerator WallJumpTimerRoutine()
    {
        _justWallJumped = true;
        yield return new WaitForSeconds(_timeBetweenWallJumps);
        _justWallJumped = false;
    }

    IEnumerator ProcessWallGrabRoutine()
    {
        _isWallGrabbed = true;
        _movementDirection.y = 0;

        float timeCounter = _wallGrabTimeLimit;
        while (timeCounter > Mathf.Epsilon)
        {
            if (Input.GetButtonUp("Grab") || !(_collisionStateFlag.right || _collisionStateFlag.left))
                break;

            yield return null;
            timeCounter -= Time.deltaTime;
        }

        _isWallGrabbed = false;
    }
}
