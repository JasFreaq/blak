using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasmBody : MonoBehaviour
{
    [SerializeField] float _detectionRadius = 3f;
    [SerializeField] float _duration = 2f;

    Vector3 _targetLocation, _initialLocation, _lastIdleLocation, _targetLastLocation;
    bool _wasInRange = false, _isChasing = false, _isInContact = false;

    ChasmDetector _chasmDetector;

    private void Awake()
    {
        _chasmDetector = GetComponentInChildren<ChasmDetector>();
    }

    private void Start()
    {
        if (_chasmDetector)
            _chasmDetector.SetDetectionRadius(_detectionRadius);
        else
            Debug.LogError("Detector missing");

        _initialLocation = transform.position;
    }

    private void Update()
    {
        Locomotion();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.GetInstanceID() == _chasmDetector.TargetID)
            _isInContact = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetInstanceID() == _chasmDetector.TargetID)
            _isInContact = false;
    }

    private void Locomotion()
    {
        if (_chasmDetector)
        {
            if (_chasmDetector.InRange)
            {
                _targetLocation = _chasmDetector.TargetPosition;

                if (!_isChasing)
                {
                    StopAllCoroutines();
                    _lastIdleLocation = transform.position;
                    _isChasing = true;
                    StartCoroutine(ChaseRoutine());
                }
            }
            else if (_wasInRange)
            {
                _targetLastLocation = transform.position;
                StartCoroutine(ReturnRoutine());
            }

            _wasInRange = _chasmDetector.InRange;
        }
    }

    IEnumerator ChaseRoutine()
    {
        float time = 0;
        while (time - _duration <= Mathf.Epsilon)
        {
            float normalizedTime = time / _duration;
            if (_chasmDetector.InRange)
            {
                transform.position = Vector2.Lerp(_lastIdleLocation, _targetLocation, normalizedTime);
                yield return null;
            }
            else
            {
                _isChasing = false;
                yield break;
            }
            time += Time.unscaledDeltaTime;
        }

        transform.position = _targetLocation;
        if (_isInContact)
        {
            _targetLastLocation = transform.position;
            _chasmDetector.DestroyTarget();
        }
        _isChasing = false;
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        float time = 0;
        while (time - _duration <= Mathf.Epsilon)
        {
            float normalizedTime = time / _duration;
            if (Vector2.Distance(_initialLocation, transform.position) > Mathf.Epsilon)
            {
                transform.position = Vector2.Lerp(_targetLastLocation, _initialLocation, normalizedTime);
                yield return null;
            }
            time += Time.unscaledDeltaTime;
        }

        transform.position = _initialLocation;
    }
}
