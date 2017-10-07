using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public string earthTag = "Plane";

    public float movingSpeed = .5f;
    public float thresholdBeforeZoom = .5f;

    [Header("Rotate world stuff")]
    public float rotateWorldSpeed = .3f;
    public float rotateThreshold = .6f;

    [Header("Up Angle stuff")]
    public float upAngleSpeed = .5f;
    public float minAngle = 30;
    public float maxAngle = 80;
    public float maxRelativeSize = 4f;

    [Header("Orthografic camera parameters")]
    public float minOrthoSize = .5f;
    public float orthoZoomSpeed = .5f;

    private Camera camera;
    private float oldAngle;
    private Vector3 lookPoint;
    private bool swipeBegan;

    private void Start()
    {
        swipeBegan = false;
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
            for (int i = 0; i < 2; i++)
            {
                touches[i] = Input.GetTouch(i);
            }

            CheckZoom(touches);
            CheckWorldRotate(touches);
            CheckUpAngleDiff(touches);
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

    private void CheckZoom(Touch[] touches)
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
        }
    }

    private void CheckUpAngleDiff(Touch[] touches)
    {
        if (touches[0].phase != TouchPhase.Moved || touches[1].phase != TouchPhase.Moved)
        {
            return;
        }

        float maxVectorsMagnitude = Mathf.Max(touches[0].deltaPosition.magnitude,touches[1].deltaPosition.magnitude);
        float sumMagnitude = (touches[0].deltaPosition + touches[1].deltaPosition).magnitude;
        float relativeSize = touches[0].deltaPosition.magnitude / touches[1].deltaPosition.magnitude;

        if (sumMagnitude > maxVectorsMagnitude && (relativeSize > 1f / maxRelativeSize) && (relativeSize < 1f * maxRelativeSize))
        {
            float averageRotate = (touches[0].deltaPosition.y + touches[1].deltaPosition.y) / 2;

            transform.RotateAround(lookPoint, transform.right, -averageRotate * upAngleSpeed);
            if (transform.rotation.eulerAngles.x > maxAngle
                || transform.rotation.eulerAngles.x < minAngle)
            {
                transform.RotateAround(lookPoint, transform.right, averageRotate * upAngleSpeed);
            }
        }
    }

    private void CheckWorldRotate(Touch[] touches)
    {
        for (int i = 0; i < 2; i++)
        {
            if (touches[i].phase == TouchPhase.Began)
            {
                swipeBegan = true;
            }
        }

        if (!swipeBegan)
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
            swipeBegan = false;
        }   
    }
}
