using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    [SerializeField] GameObject _pusher = null;
    [SerializeField] float _grabBufferXDist = 0.5f;
    [SerializeField] float _grabBufferXDuration = 0.2f;

    PlayerController _playerController = null;

    Transform _grabbedShape = null;
    bool _triedGrabbing = false;

    float _initialXDist = 0;

    private void Awake()
    {
        _playerController = transform.root.GetComponent<PlayerController>();
    }

    private void Update()
    {
        _triedGrabbing = Input.GetButtonDown("Grab");
        if (_grabbedShape)
        {
            if (_triedGrabbing)
            {
                _grabbedShape = null;
                _pusher.SetActive(true);
                return;
            }

            _grabbedShape.position = new Vector3(_grabbedShape.position.x + _playerController.HorizontalMovement * Time.deltaTime,
                _grabbedShape.position.y, _grabbedShape.position.z);

            float xDist = GetXDistance();
            if (xDist > _initialXDist) 
            {
                _grabbedShape.position = new Vector3(_playerController.transform.position.x + _initialXDist,
                    _grabbedShape.position.y, _grabbedShape.position.z);
            }

            ProcessPushing();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_triedGrabbing && !_grabbedShape && collision)
        {
            _grabbedShape = collision.transform;
            _initialXDist = GetXDistance();
            _pusher.SetActive(false);
        }
    }

    float GetXDistance()
    {
        if (_grabbedShape && _playerController)
        {
            return Mathf.Abs(_grabbedShape.position.x - _playerController.transform.position.x);
        }

        return 0;
    }

    private void ProcessPushing()
    {
        int playerMovementDir = MathUtils.NumSign(_playerController.HorizontalMovement);
        int shapeDir = MathUtils.NumSign(_grabbedShape.position.x - _playerController.transform.position.x);

        if (playerMovementDir == shapeDir)
        {
            _playerController.UpdateShapePushStatus(_grabbedShape);
        }
        else
        {
            _playerController.ClearShapePushStatus();
        }
    }
}
