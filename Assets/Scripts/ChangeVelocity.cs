using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeVelocity : MonoBehaviour
{
    public float velocityFactor = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddForce(velocityFactor * other.attachedRigidbody.velocity);
    }

}
