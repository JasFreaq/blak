using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidDoor : VoidWeak
{
    [SerializeField] int _powerToOpen = 1;
    int _shapesAbsorbedCount = 0;

    protected override void ProcessTimeStop()
    {
        for (int i = 0; i < _shapes.Count; i++)
        {
            _shapes[i].gameObject.SetActive(false);
            _shapesAbsorbedCount++;

            if (_shapesAbsorbedCount >= _powerToOpen)
                Destroy(gameObject);
        }
    }
}
