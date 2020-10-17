using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShapesPool : MonoBehaviour
{ 
    [SerializeField] GameObject _boxPrefab;
    [SerializeField] GameObject _balloonPrefab;
    [SerializeField] GameObject _prismPrefab;

    GameObject[] _boxes = new GameObject[PlayerLight.MAX_LIGHT_LIMIT];
    GameObject[] _balloons = new GameObject[PlayerLight.MAX_LIGHT_LIMIT];
    GameObject[] _prisms = new GameObject[PlayerLight.MAX_LIGHT_LIMIT];

    Vector2 _boxSize = Vector2.zero;
    float _balloonRadius = 0f;
    
    public Vector2 BoxSize { get { return _boxSize; } }
    public float BalloonRadius { get { return _balloonRadius; } }

    private void Awake()
    {
        if (_boxPrefab)
        { 
            BoxCollider2D boxCollider = _boxPrefab.GetComponent<BoxCollider2D>();
            if (boxCollider)
            {
                _boxSize = boxCollider.size;
            }
        }

        if (_balloonPrefab)
        {
            CircleCollider2D circleCollider = _balloonPrefab.GetComponent<CircleCollider2D>();
            if (circleCollider)
            {
                _balloonRadius = circleCollider.radius;
            }
        }
    }

    private void Start()
    {
        if (_boxPrefab)
            SetupBoxPool();
        if (_balloonPrefab)
            SetupBalloonPool();
        if (_prismPrefab)
            SetupPrismPool();
    }

    private void SetupBoxPool()
    {
        for (int i = 0; i < 4; i++)
        {
            _boxes[i] = Instantiate(_boxPrefab, transform.position, Quaternion.identity);
            _boxes[i].transform.parent = transform;
            _boxes[i].SetActive(false);
        }
    }
    
    private void SetupBalloonPool()
    {
        for (int i = 0; i < 4; i++)
        {
            _balloons[i] = Instantiate(_boxPrefab, transform.position, Quaternion.identity);
            _balloons[i].transform.parent = transform;
            _balloons[i].SetActive(false);
        }
    }
    
    private void SetupPrismPool()
    {
        for (int i = 0; i < 4; i++)
        {
            _prisms[i] = Instantiate(_boxPrefab, transform.position, Quaternion.identity);
            _prisms[i].transform.parent = transform;
            _prisms[i].SetActive(false);
        }
    }

    public GameObject GetBox()
    {
        foreach (GameObject box in _boxes)
        {
            if (!box.activeInHierarchy)
            {
                return box;
            }
        }

        Debug.LogError("More Boxes being requested from pool than instantiated.");
        return null;
    }
    
    public GameObject GetBalloon()
    {
        foreach (GameObject balloon in _balloons)
        {
            if (!balloon.activeInHierarchy)
            {
                return balloon;
            }
        }

        Debug.LogError("More Balloons being requested from pool than instantiated.");
        return null;
    }
    
    public GameObject GetPrism()
    {
        foreach (GameObject prism in _prisms)
        {
            if (!prism.activeInHierarchy)
            {
                return prism;
            }
        }

        Debug.LogError("More Boxes being requested from pool than instantiated.");
        return null;
    }
}
