using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidWeak : MonoBehaviour
{
    [System.Serializable]
    protected class AbsorberStruct
    {
        public GameObject gameObject;

        public AbsorberStruct GetAbsorberStruct(GameObject gameObject)
        {
            if (this.gameObject == gameObject)
                return this;

            return null;
        }
    }

    [SerializeField] private LayerMask ShapeLayer;

    protected List<AbsorberStruct> _shapes = new List<AbsorberStruct>();
    
    private void OnEnable()
    {
        PlayerEventsHandler.RegisterOnTimeStop(ProcessTimeStop);
    }

    private void OnDisable()
    {
        PlayerEventsHandler.DeregisterOnTimeStop(ProcessTimeStop);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(ShapeLayer == (ShapeLayer | (1 << collision.gameObject.layer))))
        {
            return;
        }

        AbsorberStruct absorberStruct = new AbsorberStruct();
        absorberStruct.gameObject = collision.gameObject;
        _shapes.Add(absorberStruct);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (!(ShapeLayer == (ShapeLayer | (1 << collision.gameObject.layer))))
        {
            return;
        }

        for (int i = 0; i < _shapes.Count; i++)
        {
            if (_shapes[i].gameObject.GetInstanceID() == collision.gameObject.GetInstanceID())
                _shapes.RemoveAt(i);
        }
    }

    protected virtual void ProcessTimeStop()
    {
        for (int i = 0; i < _shapes.Count; i++)
        {
            Destroy(_shapes[i].gameObject);
        }

        _shapes.Clear();
    }
}
