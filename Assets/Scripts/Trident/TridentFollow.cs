using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TridentFollow : MonoBehaviour
{
	public Transform follow;
	private float speed = 800f;
	private float rotateSpeed = 0.17f;
	private float rotateNormaleSpeed = 60f;
	private float slowPourcent = 0.99f;
	
	//Force de lévitation
    private float hoverForceUp = 200f;
    private float hoverForceDown = 10f;
    private float hoverAltitude = 2f;
    private float hoverDistanceDetection = 10f;
	
	//Gravité
    private float gravity = 15f;
	
	private Rigidbody rb;
	
	private bool hoverActive;
	
	private float distanceGround;
    private Vector3 normalGround;
	
	private float integral;
	private float lastProportional;
	
	private float integral2;
	private float lastProportional2;
	
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
	
	float SpeedToForce (float speed) {
		return speed / 7.18f;
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		Sensor();
		UpdateForce();
        UpdateHover();
    }
	
	void Sensor() {
		RaycastHit groundSensor;

		Vector3 down = transform.right;
		
        //Affichage d'une ligne dans l'éditeur.
        Debug.DrawRay(transform.position, down * hoverDistanceDetection, Color.green);

        //Si le véhicule trouve une surface avec le layer "Terrain" sous la véhicule en dessous de la distance hoverDistanceDetection
        if (Physics.Raycast(transform.position, down, out groundSensor, hoverDistanceDetection, LayerMask.GetMask("Terrain"))){;
            distanceGround = groundSensor.distance;
            normalGround = groundSensor.normal.normalized;
			Debug.DrawRay(groundSensor.point, normalGround * hoverDistanceDetection, Color.red);
			
            hoverActive = true; //Active la lévitation
        }else{
            hoverActive = false; //Désactive la lévitation
        }
	}
	
	void UpdateForce(){
		//On annule les ancienne force de rotation
		rb.angularVelocity = Vector3.zero;
		
        //On projete cette normale par rapport au plan du véhicule.
        Vector3 projectionOnNormal = Vector3.ProjectOnPlane(transform.forward, normalGround);
		
        //On créer une rotation pour que le véhicule soit perpendiculaire à cette normale (et donc à la surface du terrain)
		Quaternion rotationToNormal = Quaternion.LookRotation(projectionOnNormal, normalGround);
		
        //On applique cette rotation qu'on interpolise avec la vitesse rotateNormaleSpeed pour smoother la rotation
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, rotationToNormal, Time.fixedDeltaTime * rotateNormaleSpeed));
		rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, -90)));
		
		//On Tourne en direction du joueur
		Vector3 projection = Vector3.ProjectOnPlane(follow.position - transform.position, transform.forward);
		float angle = Vector3.SignedAngle(transform.forward, projection, follow.up);
		float pid = PID2(0, angle);
		rb.AddRelativeTorque(rotateSpeed * pid - rb.angularVelocity.x, 0, 0, ForceMode.VelocityChange);
		
        //On pousse le véhicule vers son cotée avec la même force que le cotée opposé pour empécher le véhicule de "glisser" lors d'un virage
        float sideSpeed = Vector3.Dot(rb.velocity, transform.up);
		rb.AddForce(-transform.up * (sideSpeed / Time.fixedDeltaTime), ForceMode.Acceleration);
		
		//Force de frottement qui ralentie le véhicule.
		rb.velocity *= slowPourcent;
		
		rb.AddForce(transform.forward * SpeedToForce(speed), ForceMode.Acceleration);
	}
	
	void UpdateHover () {
		if(hoverActive){
            float pid = PID(hoverAltitude, distanceGround);
            rb.AddForce(normalGround * hoverForceUp * pid, ForceMode.Acceleration);
        }else{
            rb.AddForce(transform.right * gravity, ForceMode.Acceleration);
        }
	}
	
	//Cette fonctionn'est pas de moi, cela permet de calculer un correcteur PID, utile en asservissement (ici en hauteur)
    //Elle a été simplifié par rapport à la vrai fonction plus complète et plus complexe
    private float pCoeff = .8f; //gain proportionnel
	private float iCoeff = .0002f; //gain integrateur
	private float dCoeff = .2f;//gain dérivateur
	public float PID(float seekValue, float currentValue) {
		float proportional = seekValue - currentValue;

		float derivative = (proportional - lastProportional) / Time.fixedDeltaTime;
		integral += proportional * Time.fixedDeltaTime;
		lastProportional = proportional;

		//application du du PID (fonction de transfert)
		float value = pCoeff * proportional + iCoeff * integral + dCoeff * derivative;

        value = Mathf.Clamp(value, -1, 1);
		return value;
	}
	
	public float PID2(float seekValue, float currentValue) {
		float proportional = seekValue - currentValue;

		float derivative = (proportional - lastProportional2) / Time.fixedDeltaTime;
		integral2 += proportional * Time.fixedDeltaTime;
		lastProportional2 = proportional;

		//application du du PID (fonction de transfert)
		float value = pCoeff * proportional + iCoeff * integral2 + dCoeff * derivative;

        value = Mathf.Clamp(value, -1, 1);
		return value;
	}
}
