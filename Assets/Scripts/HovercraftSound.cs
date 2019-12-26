using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HovercraftSound : MonoBehaviour {

	public float gain = 3;
    public int stepDiv = 5;
	public float smooth = 1;
	
	public AudioSource audioSource;

    private Rigidbody rb;

    private int step;

    void Start () {
        rb = GetComponent<Rigidbody>();

        step = 0;
    }

    void Update() {
        Enginesound();
    }

    void Enginesound() {
        //Pitch de son en fonction de la vitesse du véhicule (pitch = hauteur du son)
        float speed = rb.velocity.magnitude;
        float speedBarPlus = stepDiv * step;
		float speedBarMinus = stepDiv * (step - 1);
		
		if (speed >= speedBarPlus) {
            step++;
        }else if (speed <= speedBarMinus) {
			step--;
		}

        float reste = stepDiv - (speedBarPlus - speed);

        audioSource.pitch = Mathf.Lerp(audioSource.pitch, (step + reste / 2) / gain, smooth * Time.deltaTime);
    }



}