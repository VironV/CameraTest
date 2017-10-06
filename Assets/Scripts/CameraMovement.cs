using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public string earthTag = "Plane";

    public float movingSpeed = .5f;
    public float thresholdBeforeZoom = .5f;

    [Header("Rotate world stuff")]
    public float rotateWorldSpeed = .3f;

    [Header("Up Angle stuff")]
    public float upAngleSpeed = .5f;
    public float minAngle = 30;
    public float maxAngle = 80;
    public float maxRelativeSize = 4f;

    [Header("Orthografic camera parameters")]
    public float minOrthoSize = .5f;
    public float orthoZoomSpeed = .5f;

    private Camera camera;
    private Vector3 lookPoint;

    private void Start()
    {
        camera = GetComponent<Camera>();
        if (camera == null)
        {
            Debug.Log("There is no camera object for cameras moves script!");
        }
    }

    private void FixedUpdate()
    {
        CheckLookPoint();
    }

    void Update () {
		if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            transform.Translate(-touch.deltaPosition * movingSpeed);

            if (CheckLookPoint() == false)
            {
                transform.Translate(touch.deltaPosition * movingSpeed);
            }
        } else if (Input.touchCount == 2)
        {
            Touch[] touches = new Touch[2];
            for (int i=0; i<2; i++)
            {
                touches[i] = Input.GetTouch(i);
            }

            if (!CheckZoom(touches))
            {
                CheckUpAngleDiff(touches);
                CheckWorldRotate(touches);
            }
            
        }
	}

    bool CheckLookPoint()
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

    private bool CheckZoom(Touch[] touches)
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
            if (camera.orthographic)
            {
                camera.orthographicSize -= deltaDistance * orthoZoomSpeed;
                camera.orthographicSize = Mathf.Max(minOrthoSize, camera.orthographicSize);
            }
            return true;
        }
        return false;
    }

    private bool CheckUpAngleDiff(Touch[] touches)
    {
        if (touches[0].phase != TouchPhase.Moved || touches[1].phase != TouchPhase.Moved)
        {
            return false;
        }

        float maxVectorsMagnitude = Mathf.Max(touches[0].deltaPosition.magnitude,touches[1].deltaPosition.magnitude);
        float sumMagnitude = (touches[0].deltaPosition + touches[1].deltaPosition).magnitude;
        float relativeSize = touches[0].deltaPosition.magnitude / touches[1].deltaPosition.magnitude;

        Debug.Log(maxVectorsMagnitude);
        Debug.Log(sumMagnitude);
        Debug.Log(relativeSize);

        if ( sumMagnitude > maxVectorsMagnitude && (relativeSize > 1f/maxRelativeSize) && (relativeSize < 1f*maxRelativeSize))
        {
            float averageRotate = (touches[0].deltaPosition.y + touches[1].deltaPosition.y) / 2;

            transform.RotateAround(lookPoint, transform.right, averageRotate * upAngleSpeed);
            if (transform.rotation.eulerAngles.x > maxAngle
                || transform.rotation.eulerAngles.x < minAngle)
            {
                transform.RotateAround(lookPoint, transform.right, -averageRotate * upAngleSpeed);
            }
            return true;
        }

        return false;
    }

    private bool CheckWorldRotate(Touch[] touches)
    {

        float maxVectorsMagnitude = Mathf.Max(touches[0].deltaPosition.magnitude, touches[1].deltaPosition.magnitude);
        float sumMagnitude = (touches[0].deltaPosition + touches[1].deltaPosition).magnitude;

        Debug.Log(maxVectorsMagnitude);
        Debug.Log(sumMagnitude);

        if (sumMagnitude < maxVectorsMagnitude 
            || touches[0].phase == TouchPhase.Stationary
            || touches[1].phase == TouchPhase.Stationary)
        {
            int direction = CheckDirection(touches);

            float sumRotate = touches[0].deltaPosition.magnitude + touches[1].deltaPosition.magnitude;

            transform.RotateAround(lookPoint, Vector3.up, sumMagnitude * upAngleSpeed * direction);
            return true;
        }
        return false;
    }

    private int CheckDirection(Touch[] touches)
    {
        Touch left = touches[1];
        Touch right = touches[0];

        if (touches[0].position.x == touches[1].position.x)
        {
            if (touches[0].position.y < touches[1].position.y)
            {
                left = touches[0];
                right = touches[1];
            }

            if (left.deltaPosition.x < 0 || right.deltaPosition.x > 0)
            {
                return -1;
            }
            return 1;
        }
        else
        {
            if (touches[0].position.x < touches[1].position.x)
            {
                left = touches[0];
                right = touches[1];
            }


            if (left.deltaPosition.y > 0 || right.deltaPosition.y < 0)
            {
                return -1;
            }
            return 1;
        }
    }
}
