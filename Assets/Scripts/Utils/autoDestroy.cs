using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autoDestroy : MonoBehaviour
{
	public float timeToAlive = 2f;
	private Timer timerAlive;
	
    // Start is called before the first frame update
    void Start()
    {
        timerAlive = new Timer(timeToAlive);
		 timerAlive.Start();
    }

    // Update is called once per frame
    void Update()
    {
		 timerAlive.Tick(Time.deltaTime);
        if( timerAlive.IsFinished()){
			Destroy(this.gameObject);
		};
    }
}
