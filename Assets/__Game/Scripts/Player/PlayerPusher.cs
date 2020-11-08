using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPusher : MonoBehaviour
{
    readonly int SHAPES_LAYER = LayerMask.NameToLayer("Shapes");

    PlayerController _playerController = null;

    private void Awake()
    {
        _playerController = transform.root.GetComponent<PlayerController>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_playerController && collision != null && collision.gameObject.layer == SHAPES_LAYER)
        {
            ProcessPushing(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision collision)
    {
        if (_playerController && collision != null && collision.gameObject.layer == SHAPES_LAYER)
        {
            _playerController.ClearShapePushStatus();
        }
    }

    private void ProcessPushing(Transform shape)
    {
        int playerMovementDir = MathUtils.NumSign(_playerController.HorizontalMovement);
        int shapeDir = MathUtils.NumSign(shape.position.x - _playerController.transform.position.x);

        if (playerMovementDir == shapeDir)
        {
            _playerController.UpdateShapePushStatus(shape);
        }
    }
}
