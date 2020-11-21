using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    [SerializeField] float _grabBufferDistance = 0.25f;
    [SerializeField] GameObject _pusher = null;

    Rigidbody2D _rigidbody2D = null;

    Transform _grabbedShape = null;
    bool _triedGrabbing = false;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _triedGrabbing = Input.GetButtonDown("Grab");
        if (_triedGrabbing)
        {
            Ungrab();
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_triedGrabbing && !_grabbedShape && collision)
        {
            _grabbedShape = collision.transform;
            _pusher.SetActive(false);

            FixedJoint2D fixedJoint2D = PlayerShapesPool.Instance.GetShape(_grabbedShape.GetInstanceID()).FixedJoint;
            fixedJoint2D.connectedBody = _rigidbody2D;
            float x = _grabbedShape.position.x - transform.position.x;
            float y = _grabbedShape.position.y - transform.position.y;

            fixedJoint2D.connectedAnchor = new Vector2(x, y);
            fixedJoint2D.enabled = true;
        }
    }

    public void Ungrab()
    {
        if (_grabbedShape)
        {
            FixedJoint2D fixedJoint2D = PlayerShapesPool.Instance.GetShape(_grabbedShape.GetInstanceID()).FixedJoint;
            fixedJoint2D.enabled = false;
            fixedJoint2D.connectedBody = null;
            fixedJoint2D.connectedAnchor = Vector2.zero;

            _grabbedShape = null;
            _pusher.SetActive(true);
        }
    }
}
