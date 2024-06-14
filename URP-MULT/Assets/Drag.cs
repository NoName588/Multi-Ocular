using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{
    private Vector3 startPosition; // Store the initial position of the object

    void OnMouseDown() // Called when the mouse is pressed down on the object
    {
        startPosition = transform.position; // Record the initial position
    }

    void OnMouseDrag() // Called while the mouse is dragging over the object
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position in world space
        mousePosition.y = transform.position.y; // Maintain object's Y position

        transform.position = mousePosition; // Update the object's position
    }
}

