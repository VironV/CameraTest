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

    [Header("Zoomimg parameters")]
    public float minOrthoSize = 50f;
    public float maxOrthoSize = 150f;
    public float orthoZoomSpeed = .5f;
    public float thresholdBeforeZoom = .5f;

    [Header("Rotate world stuff")]
    public float rotateWorldSpeed = .3f;
    public float rotateThreshold = .6f;

    [Header("Up Angle stuff")]
    public float upAngleSpeed = .5f;
    public float minAngle = 30;
    public float maxAngle = 80;
    public float maxRelativeSize = 4f;

    private Camera cameraComponent;

    private Touch[] touches;

    private float oldAngle;
    private Vector3 lookPoint;
    private bool rotationBegining;

    private bool isAfterMoving;
    private Vector3 vectorAfterMoving;
    private float afterMovingSpeed;

    private void Start()
    {
        touches = new Touch[2];

        isAfterMoving = false;
        rotationBegining = false;
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
        {
            Debug.Log("There is no camera object for cameras moves script!");
        }

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
        else if (Input.touchCount == 2)
        {
            DoubleTouchCheck();
        }
	}

    //
    // BLOCK: Look point
    //
    bool SetLookPoint()
    {
        bool foundLookPoint = false;

        RaycastHit[] hitPoints = Physics.RaycastAll(transform.position, transform.forward);
        foreach (RaycastHit hit in hitPoints)
        {
            if (hit.transform.tag == earthTag)
            {
                foundLookPoint = true;
                lookPoint = hit.transform.position;
                break;
            }
        }

        return foundLookPoint;
    }

    //
    // BLOCK: camera moves (zoom, rotation)
    //
    private void DoubleTouchCheck()
    {
        for (int i = 0; i < 2; i++)
        {
            touches[i] = Input.GetTouch(i);
        }

        CheckZoom();
        CheckWorldRotate();
        CheckUpAngleDiff();
    }

    private void CheckZoom()
    {
        Vector2[] prevPositions = new Vector2[2];
        for (int i = 0; i < 2; i++)
        {
            prevPositions[i] = touches[i].position - touches[i].deltaPosition;
        }

        float prevDistance = (prevPositions[1] - prevPositions[0]).magnitude;
        float currentDistance = (touches[1].position - touches[0].position).magnitude;
        float deltaDistance = currentDistance - prevDistance;
        if (Mathf.Abs(deltaDistance) > thresholdBeforeZoom)
        {
            if (cameraComponent.orthographic)
            {
                cameraComponent.orthographicSize -= deltaDistance * orthoZoomSpeed;
                cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize, minOrthoSize, maxOrthoSize); 
            }
        }
    }

    private void CheckUpAngleDiff()
    {
        float maxVectorsMagnitude = Mathf.Max(touches[0].deltaPosition.magnitude,touches[1].deltaPosition.magnitude);
        float sumMagnitude = (touches[0].deltaPosition + touches[1].deltaPosition).magnitude;
        float relativeSize = touches[0].deltaPosition.magnitude / touches[1].deltaPosition.magnitude;

        if (sumMagnitude > maxVectorsMagnitude && (relativeSize > 1f / maxRelativeSize) && (relativeSize < 1f * maxRelativeSize))
        {
            float averageRotate = (touches[0].deltaPosition.y + touches[1].deltaPosition.y) / 2;

            transform.RotateAround(lookPoint, transform.right, -averageRotate * upAngleSpeed);
            if (transform.rotation.eulerAngles.x > maxAngle || transform.rotation.eulerAngles.x < minAngle)
            {
                transform.RotateAround(lookPoint, transform.right, averageRotate * upAngleSpeed);
            }
        }
    }

    private void CheckWorldRotate()
    {
        for (int i = 0; i < 2; i++)
        {
            if (touches[i].phase == TouchPhase.Began)
            {
                rotationBegining = true;
            }
        }

        if (!rotationBegining)
        {
            Vector3 angleVector = touches[1].position - touches[0].position;
            float newAngle = Mathf.Atan2(angleVector.x, angleVector.y) *Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(newAngle, oldAngle);
            oldAngle = newAngle;

            transform.RotateAround(lookPoint, Vector3.up, deltaAngle * rotateWorldSpeed);
        }
        else
        {
            Vector3 oldAngleVector = touches[1].position - touches[0].position;
            oldAngle = Mathf.Atan2(oldAngleVector.x, oldAngleVector.y) * Mathf.Rad2Deg;
            rotationBegining = false;
        }   
    }

    //
    // BLOCK: Moving
    //

    private void MovingCamera()
    {
        touches[0] = Input.GetTouch(0);
        transform.Translate(-touches[0].deltaPosition * movingSpeed);

        if (SetLookPoint() == false)
        {
            transform.Translate(touches[0].deltaPosition * movingSpeed);
        }
        else if (touches[0].phase == TouchPhase.Ended)
        {
            vectorAfterMoving = -touches[0].deltaPosition;
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
}
