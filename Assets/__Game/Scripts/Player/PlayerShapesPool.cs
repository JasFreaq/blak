using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShapesPool : MonoBehaviour
{
    static PlayerShapesPool _instance = null;
    public static PlayerShapesPool Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<PlayerShapesPool>();
            }

            return _instance;
        }
    }

    [SerializeField] GameObject _boxPrefab;
    [SerializeField] GameObject _balloonPrefab;
    [SerializeField] GameObject _prismPrefab;

    GameObject[] _boxes = new GameObject[PlayerLight.MAX_LIGHT_LIMIT];
    GameObject[] _balloons = new GameObject[PlayerLight.MAX_LIGHT_LIMIT];
    GameObject[] _prisms = new GameObject[PlayerLight.MAX_LIGHT_LIMIT];

    Dictionary<int, Shape> _shapes = new Dictionary<int, Shape>();

    Vector2 _boxSize = Vector2.zero;
    float _balloonRadius = 0f;

    private bool _initialized = false;

    public Vector2 BoxSize { get { return _boxSize; } }
    public float BalloonRadius { get { return _balloonRadius; } }

    private void Awake()
    {
        PlayerShapesPool[] instances = FindObjectsOfType<PlayerShapesPool>();
        if (instances.Length > 1)
        {
            for (int i = 1, size = instances.Length; i < size; i++)
            {
                Destroy(instances[i].gameObject);
            }
        }
        else
        {
            _instance = this;
        }

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

    public bool Initialize()
    {
        if (!_initialized)
        {
            if (_boxPrefab)
                SetupBoxPool();
            if (_balloonPrefab)
                SetupBalloonPool();
            if (_prismPrefab)
                SetupPrismPool();

            _initialized = true;
            return true;
        }

        return false;
    }

    private void SetupBoxPool()
    {
        for (int i = 0; i < 4; i++)
        {
            _boxes[i] = Instantiate(_boxPrefab, transform.position, Quaternion.identity);
            _boxes[i].transform.parent = transform;
            _boxes[i].SetActive(false);

            Shape box = _boxes[i].GetComponent<Shape>();
            if (box)
            {
                _shapes.Add(_boxes[i].transform.GetInstanceID(), box);
            }
        }
    }
    
    private void SetupBalloonPool()
    {
        for (int i = 0; i < 4; i++)
        {
            _balloons[i] = Instantiate(_boxPrefab, transform.position, Quaternion.identity);
            _balloons[i].transform.parent = transform;
            _balloons[i].SetActive(false);

            Shape balloon = _balloons[i].GetComponent<Shape>();
            if (balloon)
            {
                _shapes.Add(_balloons[i].transform.GetInstanceID(), balloon);
            }
        }
    }
    
    private void SetupPrismPool()
    {
        for (int i = 0; i < 4; i++)
        {
            _prisms[i] = Instantiate(_boxPrefab, transform.position, Quaternion.identity);
            _prisms[i].transform.parent = transform;
            _prisms[i].SetActive(false);

            Shape prism = _prisms[i].GetComponent<Shape>();
            if (prism)
            {
                _shapes.Add(_prisms[i].transform.GetInstanceID(), prism);
            }
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

    public Shape GetShape(int iD)
    {
        if (_shapes.ContainsKey(iD))
        {
            return _shapes[iD];
        }

        return null;
    }
}
