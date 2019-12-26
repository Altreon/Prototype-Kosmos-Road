using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
	public float slowFactor = 0.001f; // entre 0 et 1, 0 = temps arrété, 1 = temps normal
	public float slowLength = 1f; // l'étirement du slow mode dans le temps
	
	public Rigidbody playerRB;
	
	void Start () {
		Application.targetFrameRate = 60;
	}

    // Update is called once per frame
    void Update()
    {
		if(!PauseMenu.GameIsPaused && isSlowMotion()){
			float newTime = Time.timeScale + (1f / slowLength) * Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Clamp(newTime, 0f, 1f);
			if(!isSlowMotion()){
				Resume();
			}
		}
    }
	
	public void Pause () {
		Time.timeScale = 0f;
	}
	
	public void Resume () {
		Time.timeScale = 1f;
		//playerRB.interpolation = RigidbodyInterpolation.None;
	}
	
	public void slowMotion () {
		Time.timeScale = slowFactor;
		//playerRB.interpolation = RigidbodyInterpolation.Interpolate;
	}
	
	//Permet de savoir si un slowMotion est en cours
	public static bool isSlowMotion () {
		return Time.timeScale != 1f;
	}
}
