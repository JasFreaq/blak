using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FixedJoint2D))]
public class Shape : MonoBehaviour
{
    protected ShapeType _shapeType = ShapeType.None;

    private Rigidbody2D _rigidbody2D = null;
    private FixedJoint2D _fixedJoint = null;
    private bool _canMove = true;


    public FixedJoint2D FixedJoint
    {
        get
        {
            if (_canMove)
                return _fixedJoint;

            return null;
        }
    }

    protected virtual void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _fixedJoint = GetComponent<FixedJoint2D>();
        _fixedJoint.enabled = false;
    }

    private void OnEnable()
    {
        PlayerEventsHandler.InvokeOnShapeForm();
        EnableMovement();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        PlayerEventsHandler.InvokeOnShapeAbsorb();
    }

    void EnableMovement()
    {
        _canMove = true;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void DisableMovement()
    {
        _canMove = false;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }
}
