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
            StartCoroutine(ShapeDisableRoutine(shape));
    }

    protected override void OnTriggerExit2D(Collider2D collision) { }

    IEnumerator ShapeDisableRoutine(Shape shape)
    {
        yield return new WaitForSeconds(0.1f);
        shape.DisableMovement();
    }
}
