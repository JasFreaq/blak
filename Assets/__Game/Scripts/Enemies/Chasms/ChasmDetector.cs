using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasmDetector : MonoBehaviour
{
    [SerializeField] private LayerMask ShapeLayer;

    bool _inRange = false;
    Transform _target;

    CircleCollider2D _circleCollider;
    SphereGizmoDrawer _sphereGizmoDrawer;

    public bool InRange { get { return _inRange; } }
    public Vector3 TargetPosition { get { return _target.position; } }
    public int TargetID 
    { 
        get
        {
            if (_target)
                return _target.GetInstanceID();
            return 0;
        } 
    }

    private void Awake()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _sphereGizmoDrawer = GetComponent<SphereGizmoDrawer>();
    }

    public void SetDetectionRadius(float radius)
    {
        _circleCollider.radius = radius;
        _sphereGizmoDrawer.radius = radius * 2;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ShapeLayer == (ShapeLayer | (1 << collision.gameObject.layer)))
        {
            _target = collision.transform;
            _inRange = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (ShapeLayer == (ShapeLayer | (1 << collision.gameObject.layer)) && !_inRange)
        {
            _target = collision.transform;
            _inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ShapeLayer == (ShapeLayer | (1 << collision.gameObject.layer)) && collision.transform == _target) 
        {
            _target = null;
            _inRange = false;
        }
    }

    public void DestroyTarget()
    {
        _target.gameObject.SetActive(false);
    }
}
