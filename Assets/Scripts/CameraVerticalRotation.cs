using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVerticalRotation : MonoBehaviour {

    public float rotationSpeed = .5f;
    public float maxAngleSpeed = 9f;
    public float minAngle = 30;
    public float maxAngle = 80;
    public float maxRelativeSize = 4f;

    private CameraMovement classWithLookPoint;

    private void Start()
    {
        classWithLookPoint = GetComponent<CameraMovement>();
        if (classWithLookPoint == null)
        {
            Debug.Log("There is not CameraMovement class!");
        }
    }

    void Update () {
		if (Input.touchCount == 2)
        {
            CheckVerticleRotation();
        }
	}

    // Check if swipes was in relatively same direction
    private void CheckVerticleRotation()
    {
        float maxVectorsMagnitude = Mathf.Max(Input.GetTouch(0).deltaPosition.magnitude, Input.GetTouch(1).deltaPosition.magnitude);
        float sumMagnitude = (Input.GetTouch(0).deltaPosition + Input.GetTouch(1).deltaPosition).magnitude;
        float relativeSize = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(1).deltaPosition.magnitude;

        if (sumMagnitude > maxVectorsMagnitude && (relativeSize > 1f / maxRelativeSize) && (relativeSize < 1f * maxRelativeSize))
        {
            Rotate();
        }
    }

    private void Rotate()
    {
        Vector3 lookPoint = classWithLookPoint.GetLookPoint;

        float averageRotate = (Input.GetTouch(0).deltaPosition.y + Input.GetTouch(1).deltaPosition.y) / 2;

        // Limits maximum speed of rotation
        float currentSpeed = averageRotate * rotationSpeed;
        if (Mathf.Abs(currentSpeed) > maxAngleSpeed)
        {
            currentSpeed = currentSpeed < 0 ? -maxAngleSpeed : maxAngleSpeed;
        }

        transform.RotateAround(lookPoint, transform.right, -currentSpeed);

        SetInBorders(lookPoint);
    }

    // If angle is too big or too small, revert
    private void SetInBorders(Vector3 lookPoint)
    {
        if (transform.localRotation.eulerAngles.x > maxAngle)
        {
            while (transform.rotation.eulerAngles.x >= maxAngle)
            {
                transform.RotateAround(lookPoint, transform.right, -.1f);
            }
        }
        else if (transform.localRotation.eulerAngles.x < minAngle)
        {
            while (transform.rotation.eulerAngles.x <= minAngle)
            {
                transform.RotateAround(lookPoint, transform.right, .1f);
            }
        }
        transform.LookAt(lookPoint);
    }
}
