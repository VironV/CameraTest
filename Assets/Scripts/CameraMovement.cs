using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    [Header("Tag for world space")]
    public string earthTag = "Plane";

    [Header("Moving parameters")]
    public float movingSpeed = .5f;
    public float afterMovingSlowdown = .5f;
    public float afterMovingDivisionSpeed = 2f;

    private Vector3 lookPoint;

    private bool isAfterMoving;
    private Vector3 vectorAfterMoving;
    private float afterMovingSpeed;

    public Vector3 GetLookPoint { get { return lookPoint; } } 

    private void Start()
    {
        isAfterMoving = false;
        SetLookPoint();
    }

    void Update () {
        if (isAfterMoving)
        {
            AfterMoving();
        }

        if (Input.touchCount == 1)
        {
            MovingCamera();
        }
	}

    /*
     * Changes look point and checks if its on world space
     * If point not found, returns false. Otherwise returns true
     */
    bool SetLookPoint()
    {
        bool foundLookPoint = false;

        RaycastHit[] hitPoints = Physics.RaycastAll(transform.position, transform.forward);
        foreach (RaycastHit hit in hitPoints)
        {
            if (hit.transform.tag == earthTag)
            {
                foundLookPoint = true;
                lookPoint = hit.point;
                break;
            }
        }

        return foundLookPoint;
    }
    

    /*
     * Moves camera
     * Not goes beyond border
     */
    private void MovingCamera()
    {
        transform.Translate(-Input.GetTouch(0).deltaPosition * movingSpeed);

        // If false - new point is beyond border, so we revert
        if (SetLookPoint() == false)
        {
            transform.Translate(Input.GetTouch(0).deltaPosition * movingSpeed);
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            vectorAfterMoving = -Input.GetTouch(0).deltaPosition;
            afterMovingSpeed = movingSpeed / afterMovingDivisionSpeed;
            isAfterMoving = true;
        }
    }

    /*
     * Moves camera after end of touch input
     * The faster user swipes, the farther camera moves after
     */
    private void AfterMoving()
    {
        afterMovingSpeed -= afterMovingSlowdown * Time.deltaTime;
        if (afterMovingSpeed <= 0)
        {
            isAfterMoving = false;
        }
        else
        {
            transform.Translate(vectorAfterMoving * afterMovingSpeed);

            if (SetLookPoint() == false)
            {
                transform.Translate(-vectorAfterMoving * afterMovingSpeed);
                isAfterMoving = false;
            }
        }
    }

    private void SpeedChange(float change)
    {
        movingSpeed *= change;
        afterMovingSlowdown *= change;
    }

    private void OnEnable()
    {
        CameraZooming.OnZoomChange += SpeedChange;
    }

    private void OnDisable()
    {
        CameraZooming.OnZoomChange -= SpeedChange;
    }
}
