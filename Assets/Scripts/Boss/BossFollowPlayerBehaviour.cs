using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFollowPlayerBehaviour : MonoBehaviour
{
    public Transform target; // le player à regarder

    private float rotationSmoothing; // adoucit le regard du boss (sa manière de se tourner dans une direction
    private Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        rotationSmoothing = 0.8f;
        previousPosition = Vector3.forward;
    }

    void LateUpdate()
    {
        
        Vector3 newPosition = Vector3.Lerp(previousPosition, GetPosition(), rotationSmoothing * Time.deltaTime);
        newPosition.y = transform.position.y;
        transform.LookAt(newPosition);

        previousPosition = GetPosition();
    }

    private Vector3 GetPosition()
    {
        return target.position;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
