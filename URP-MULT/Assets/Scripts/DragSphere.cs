using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragSphere : MonoBehaviour
{
    // Reference to the object to rotate
    public GameObject objectToRotate;

    private Transform objectTransform;

    private float rotationSpeed = 5f;

    private void Start()
    {
        objectTransform = GetComponent<Transform>();

        // Check if objectToRotate is set
        if (objectToRotate == null)
        {
            Debug.LogError("ObjectToRotate is not set in the DragSphere script.");
            return;
        }
    }

    private void OnMouseDrag()
    {
        float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
        float rotationY = -Input.GetAxis("Mouse Y") * rotationSpeed;  // Inverted for natural rotation

        objectToRotate.transform.Rotate(Vector3.up, rotationX, Space.World);
        //objectTransform.Rotate(Vector3.right, rotationY, Space.World);  // Commented out, not used
    }
}


