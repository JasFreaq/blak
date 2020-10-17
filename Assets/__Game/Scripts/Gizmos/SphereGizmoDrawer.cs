using UnityEngine;
using Gizmos = Popcron.Gizmos;

[ExecuteAlways]
public class SphereGizmoDrawer : MonoBehaviour
{
    public float radius = 0;

    private void Update()
    {
        //use custom material, if null it uses a default line material
        Gizmos.Material = null;

        //toggle gizmo drawing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Gizmos.Enabled = !Gizmos.Enabled;
        }

        //can also draw from update
        Gizmos.Circle(transform.position, radius, Quaternion.identity, Color.cyan);
    }
}
