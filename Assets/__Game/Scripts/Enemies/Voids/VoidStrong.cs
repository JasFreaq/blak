using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidStrong : VoidWeak
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        Shape shape = PlayerShapesPool.Instance.GetShape(collision.transform.GetInstanceID());
        if (shape) 
            shape.DisableMovement();
    }

    protected override void OnTriggerExit2D(Collider2D collision) { }
}
