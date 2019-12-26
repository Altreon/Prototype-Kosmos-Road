using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchTrident : MonoBehaviour
{
	public float force;
	
    // Start is called before the first frame update
    void Start()
    {
		//GetComponent<Trident>().IsTridentActive();
        //<Trident>().Propulsion(transform.forward, Vector3.zero, force, 0, false);
		GetComponent<Rigidbody>().AddForce(transform.up * force, ForceMode.Impulse);
    }
}
