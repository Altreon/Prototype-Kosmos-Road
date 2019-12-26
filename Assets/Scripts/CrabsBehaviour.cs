using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabsBehaviour : MonoBehaviour
{
    // List of all the point the agent have to go
    public List<Transform> WayPoints;
    public float DistanceMin = 1f;
    public int Speed = 1;

    // Internal variables
    private int _NextPos = 0;
    private int _CurrentPos = 0;
    private bool _goings = true;
    private Vector3 _direction;
    

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = WayPoints[0].position;

    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(WayPoints[_NextPos].position, this.transform.position) < DistanceMin)
        {

            if (_NextPos == 0)
            {
                _NextPos = 1;
                _CurrentPos = 0;
                _goings = true;
            }
            else if (_NextPos == WayPoints.Count - 1)
            {
                _CurrentPos = _NextPos;
                _NextPos = WayPoints.Count - 2;
                _goings = false;
            }
            else
            {
                if (_goings)
                {
                    _CurrentPos = _NextPos;
                    _NextPos++;
                }
                else
                {
                    _CurrentPos = _NextPos;
                    _NextPos--;
                }
            }
            _direction = WayPoints[_NextPos].position - WayPoints[_CurrentPos].position;
        }

        this.transform.position += _direction.normalized * Time.deltaTime * Speed;
    }
}
