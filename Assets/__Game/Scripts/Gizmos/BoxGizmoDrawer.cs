using UnityEngine;
using Gizmos = Popcron.Gizmos;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class BoxGizmoDrawer : MonoBehaviour
{

    public Material material = null;
    BoxCollider2D collider2D;

    Camera camera;

    private void Start()
    {
        camera = Camera.main;
        collider2D = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        //use custom material, if null it uses a default line material
        Gizmos.Material = material;

        //toggle gizmo drawing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Gizmos.Enabled = !Gizmos.Enabled;
        }

        //can also draw from update
        Gizmos.Square(transform.position, collider2D ? collider2D.size.x : 0, Color.white);
    }
}
