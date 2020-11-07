using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    public const int MAX_LIGHT_LIMIT = 4;

    [Header("Light")]
    [SerializeField] float[] _lightRadii = new float[MAX_LIGHT_LIMIT + 1];
    [SerializeField] [Range(0, MAX_LIGHT_LIMIT)] int _lightLevel = 2;

    [Header("Shaping")]
    [SerializeField] PlayerShapesPool _shapesPool;
    [SerializeField] Transform _shapeFormingMarker;
    [SerializeField] LayerMask _shapeCollisionLayers;

    CircleCollider2D _circleCollider;
    SphereGizmoDrawer _sphereGizmoDrawer;

    ShapeType _currentShape = ShapeType.None;

    bool _timeStopped = false;
    float _lightRadius = 0f;
    int _shapesInWorld = 0;

    public float LightRadius { get { return _lightRadius; } }
    public int LightLevel { get { return _lightLevel; } set { _lightLevel = value; } }


    private void Awake()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _sphereGizmoDrawer = GetComponent<SphereGizmoDrawer>();

        _lightRadius = _lightRadii[0];
        _currentShape = ShapeType.Square;
    }
    
    private void Update()
    {
        _sphereGizmoDrawer.radius = LightRadius;

        Vector3 worldPos = GetMousePointerToWorldPos();
        if (Vector2.Distance(worldPos, transform.position) - LightRadius <= Mathf.Epsilon)
        {
            _shapeFormingMarker.position = new Vector3(worldPos.x, worldPos.y, -2);
        }

        if (Input.GetButtonDown("Toggle Time Stop"))
        {
            if (_timeStopped)
            {
                Time.timeScale = 1;
                _timeStopped = false;
            }
            else
            {
                Time.timeScale = 0;
                _timeStopped = true;
            }
            PlayerEventsHandler.InvokeOnToggleTimeStop(_timeStopped);
        }

        if (_timeStopped)
        {
            if (_lightLevel > 0 && Input.GetKeyDown(KeyCode.R))
            {
                FormShape();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                AbsorbShape();
            }
        }
    }

    #region Light Collision Handling

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Detector Door")
        {
            //collision.GetComponent<DetectorDoor>().Open(_player.LightLevel);
        }
    }

    #endregion

    #region Shape Processes

    private void FormShape()
    {
        GameObject shape = null;
        bool canForm = false;
        
        switch (_currentShape)
        {
            case ShapeType.Square:
                shape = _shapesPool.GetBox();
                canForm = !CheckBoxCollision();
                break;
            case ShapeType.Circle:
                shape = _shapesPool.GetBalloon();
                canForm = !CheckBalloonCollision();
                break;
            case ShapeType.Triangle:
                shape = _shapesPool.GetPrism();
                //canForm = CheckPrismCollision();
                break;
        }

        if (shape && _shapeFormingMarker && canForm)
        {
            Vector3 worldPosition = GetMousePointerToWorldPos();
            if (Vector2.Distance(worldPosition, transform.position) - LightRadius <= Mathf.Epsilon)
            {
                shape.transform.position = new Vector3(_shapeFormingMarker.position.x, _shapeFormingMarker.position.y, 0);
                shape.transform.rotation = Quaternion.identity;
                shape.SetActive(true);

                _lightLevel--;
                _shapesInWorld++;
                AdjustLightRadius();
            }
        }
    }

    public void AbsorbShape()
    {
        Vector3 worldPosition = GetMousePointerToWorldPos();
        if (Vector2.Distance(worldPosition, transform.position) - LightRadius <= Mathf.Epsilon)
        {
            LayerMask mask = LayerMask.GetMask("Shapes");
            RaycastHit2D hit2D = Physics2D.Raycast(worldPosition, Camera.main.transform.forward, 0f, mask);

            if (hit2D)
            {
                _lightLevel++;
                _shapesInWorld--;
                AdjustLightRadius();

                hit2D.transform.gameObject.SetActive(false);
            }
        }
    }

    bool CheckBoxCollision()
    {
        RaycastHit2D hit2D = Physics2D.BoxCast(_shapeFormingMarker.transform.position, 
            _shapesPool.BoxSize, 0f, Vector2.zero, 0f, _shapeCollisionLayers);

        if (hit2D)
            return true;

        return false;
    }

    bool CheckBalloonCollision()
    {
        RaycastHit2D hit2D = Physics2D.CircleCast(_shapeFormingMarker.transform.position,
            _shapesPool.BalloonRadius, Vector2.zero, 0f, _shapeCollisionLayers);

        if (hit2D)
            return true;

        return false;
    }

    void AdjustLightRadius()
    {
        _lightRadius = _lightRadii[_shapesInWorld];
        _circleCollider.radius = _lightRadius;
    }

    Vector3 GetMousePointerToWorldPos()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0;

        return worldPosition;
    }

    #endregion
}
