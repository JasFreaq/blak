using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Shape
{
    protected override void Awake()
    {
        base.Awake();

        _shapeType = ShapeType.Square;
    }
}
