using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameOverVisual : MonoBehaviour
{
    [Header("Trap Gizmo Size")]
    [SerializeField] private float width = 1f;
    [SerializeField] private float height = 1f;
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private BoxCollider2D myBox;




    void Update()
    {
        myBox.size = new Vector2(width, height);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        // Centered on the trap’s position
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0.1f));
        // Optional: draw a filled transparent cube
        Gizmos.DrawCube(transform.position, new Vector3(width, height, 0.1f));
    }


}
