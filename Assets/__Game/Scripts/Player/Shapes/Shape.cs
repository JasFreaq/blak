using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FixedJoint2D))]
public class Shape : MonoBehaviour
{
    protected ShapeType _shapeType = ShapeType.None;
    protected FixedJoint2D _fixedJoint = null;

    public FixedJoint2D FixedJoint { get { return _fixedJoint; } }

    protected virtual void Awake()
    {
        _fixedJoint = GetComponent<FixedJoint2D>();
        _fixedJoint.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
