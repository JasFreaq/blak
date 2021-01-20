using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadeDoor : MonoBehaviour
{
    [SerializeField] int _powerToOpen = 1;
    [SerializeField] float _durationToOpen = 1;
    [SerializeField] float _durationWaitBeforeClose = 1.5f;
    [SerializeField] Transform _openPosTransform;

    Vector3 _closedPos, _openPos;
    bool _isOpen = false;

    private void Start()
    {
        _closedPos = transform.position;
        _openPos = _openPosTransform.position;
    }

    public void Open(int power)
    {
        if (power >= _powerToOpen && !_isOpen)
        {
            StartCoroutine(OpenRoutine());
            _isOpen = true;
        }
    }

    IEnumerator OpenRoutine()
    {
        float t = 0;
        while (t <= 1) 
        {
            t += Time.deltaTime / _durationToOpen;
            transform.position = Vector3.Lerp(_closedPos, _openPos, t);

            yield return null;
        }
        transform.position = _openPos;

        yield return new WaitForSeconds(_durationWaitBeforeClose);

        t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / _durationToOpen;
            transform.position = Vector3.Lerp(_openPos, _closedPos, t);

            yield return null;
        }
        transform.position = _closedPos;
        _isOpen = false;
    }
}
