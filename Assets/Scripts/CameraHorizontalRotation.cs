using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHorizontalRotation : MonoBehaviour {

    public float rotationSpeed = .3f;
    public float rotateThreshold = .6f;

    private float oldAngle;
    private bool rotationBegining;

    private CameraMovement classWithLookPoint;

    private void Start()
    {
        rotationBegining = false;
        classWithLookPoint = GetComponent<CameraMovement>();
        if (classWithLookPoint == null)
        {
            Debug.Log("There is no CameraMovement class!");
        }
    }

    void Update () {
		if (Input.touchCount == 2)
        {
            CheckHorizontalRotation();
        }
	}

    private void CheckHorizontalRotation()
    {
        for (int i = 0; i < 2; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                rotationBegining = true;
            }
        }

        // If rotation only began, waits for one frame and preparing old angle
        if (rotationBegining)
        {
            SetOldAngle();
        } else
        {
            Rotate();
        }
    }

    private void SetOldAngle()
    {
        Vector3 oldAngleVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
        oldAngle = Mathf.Atan2(oldAngleVector.x, oldAngleVector.y) * Mathf.Rad2Deg;
        rotationBegining = false;
    }

    private void Rotate()
    {
        Vector3 lookPoint = classWithLookPoint.GetLookPoint;

        Vector3 angleVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
        float newAngle = Mathf.Atan2(angleVector.x, angleVector.y) * Mathf.Rad2Deg;
        float deltaAngle = Mathf.DeltaAngle(newAngle, oldAngle);
        oldAngle = newAngle;

        transform.RotateAround(lookPoint, Vector3.up, deltaAngle * rotationSpeed);
        transform.LookAt(lookPoint);
    }

}
