using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hovercraft : MonoBehaviour {
	
	//Enum
	enum ActiveStat {Active, Transition, Desactive};
    public enum sens_hovercraft
    {
        avance = 1,
        recule = -1
    };
	
	//Contrôle du véhicule
	
	//Valeur Initiale
	public float trusterSpeed = 100f;
    public float backRatioSpeed = 0.5f; //le pourcentage à laquelle le joueur recule par rapport à sa vitesse moteur
	
	//Valeur Max
	public float maxTrusterSpeed = 300f;
	
    public float thresholdSens = 1.0f; // seuil d'inversion du sens
    public float rotateSpeed = 1f;
	public float rotateAirSpeed = 1f;
    public float rotateNormaleSpeed = 6f;
    public float rollAngle = 20f;
	public float rollAirSpeed = 5f;
	public float driftRollAngle = 50f;
    public float driftFactor = 0.01f; //entre 0 et 1
	public float driftCompensateFactor = 1f; //entre 0 et 1
	public float driftRotateSpeed = 2f;
    public float slowPourcent = 0.99f;
    public float brakePourcent = 0.9f;

	//Force de lévitation
    public Transform hoverPos;
    public float hoverForceUp = 50f;
    public float hoverForceDown = 10f;
    public float hoverAltitude = 2f;
    public float hoverDistanceDetection = 4f;
	
	//Seuil de perte définitive de vitesse
	public float lossSpeedThreshold = 1f;
	//Vitesse minimale à laquel le moteur ne passe pas en dessous
	public float minTrusterSpeed = 40f;
	
	//Capacité spéciale
    public float jumpForce = 20f;
    public float boostForce = 40f;

	//Gravité
    public float gravity = 30f;
	
	//PRIVATE
    private Rigidbody rb;
    private Transform hover;
	private float truster;
    private bool brake;
    private bool hoverActive;
    private bool drift;
	private int roll;
	
    private float distanceGround;
    private Vector3 normalGround;

    private ActiveStat hoverStat;
	private ActiveStat boostStat;

    private float rotate;

    private float integral;
	private float lastProportional;

    //savoir si on avance ou on recule
    private sens_hovercraft forward = sens_hovercraft.avance;
	
	//Utiliser pour le compenser la décélèration du drift;
	private float oldVelocityM;
	
	//Utiliser pour la perte de vitesse définitive;
	private float reachSpeed = 0;
	
	private float iniTrusterSpeed;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
        hover = transform.GetChild(0);
		
        hoverStat = ActiveStat.Active;
		boostStat = ActiveStat.Active;
		
		iniTrusterSpeed = trusterSpeed;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
        Sensor();

        //On fait tourner le vaisseau (par rapport à l'axe Y) en fonction que si le joueur veut aller à gauche ou à droite
        forward = (rb.velocity.magnitude < thresholdSens ? sens_hovercraft.avance : Vector3.Angle(transform.forward, rb.velocity) < 90 ? sens_hovercraft.avance : sens_hovercraft.recule);
		
        if(hoverActive){
            ApplyForce();
        }
        else
        {
            AirControl();
        }
		
        ApplyGravity();
	}
	
	public bool onGround () {
		return hoverActive;
	}

    public void setTruster (float truster){
        this.truster = truster;
    }

    public void setBrake (bool brake){
       this.brake = brake;
    }

    public void setRotate (float rotate){
        this.rotate = rotate; //* forward;
    }
	
	public Vector3 getNormalGround () {
		return normalGround;
	}
	
	public void Stop () {
		enabled = false;
		
		if(rb){
			rb.detectCollisions = false;
			rb.velocity = Vector3.zero;
		}
	}
	
	public void Run () {
		enabled = true;
		
		if(rb){
			rb.detectCollisions = true;
		}
	}

    public void setDrift(bool drift) {
		
		//Si on sort d'un drift, on redirige la la vélocité vers l'avant du vaisseau
		if(this.drift && !drift){
			//Comme le nez de véhicule point légèrement vers le bas, on pousse dans le référenciel de la normal du sol
			Vector3 projectionOnNormal = Vector3.ProjectOnPlane(transform.forward, normalGround);
			rb.velocity = projectionOnNormal.normalized * rb.velocity.magnitude;
		}
		
		//Quand on commence à drifter, on commence à stocker la magnitude de la frame précédente
		if(!this.drift && drift){
			oldVelocityM = rb.velocity.magnitude;
		}
		
        this.drift = drift;
    }
	
	public void setRoll(int roll) {
        this.roll = roll;
    }

    private void stop () {
        rb.velocity = Vector3.zero;
		oldVelocityM = 0;
		reachSpeed = 0;
    }

    public void jump () {
		oldVelocityM = rb.velocity.magnitude;
		
		if(hoverStat == ActiveStat.Active){
			hoverStat = ActiveStat.Transition;
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}
    }

    public void boost()
    {
		if(boostStat == ActiveStat.Active){
			if(!hoverActive){
				boostStat = ActiveStat.Desactive;
			}
			rb.AddForce(transform.forward * (boostForce/7.0f), ForceMode.Impulse);
		}
    }
	
	public bool canBoost () {
		return boostStat == ActiveStat.Active;
	}
	
    // fait aller le hovercraft au checkpoint passé en paramètre
	public void goToCheckpoint(Transform checkpoint) {
		if(!rb){
			Start();
		}
		
		stop();
		
		trusterSpeed = iniTrusterSpeed;
        transform.position = checkpoint.position;
        transform.rotation = checkpoint.rotation;
	}

    //Récupère les informations du sol en dessous du véhicule
    void Sensor () {
        RaycastHit groundSensor;

		Vector3 down = Vector3.zero;
		if(normalGround == Vector3.zero){
				down = hoverPos.TransformDirection(Vector3.down);
		}else{
			down = -normalGround;
		}
		
        //Affichage d'une ligne dans l'éditeur.
        Debug.DrawRay(hoverPos.position, down * hoverDistanceDetection, Color.green);

        //Si le véhicule trouve une surface avec le layer "Terrain" sous la véhicule en dessous de la distance hoverDistanceDetection
        if (Physics.Raycast(hoverPos.position, down, out groundSensor, hoverDistanceDetection, LayerMask.GetMask("Terrain"))){;
            distanceGround = groundSensor.distance;
            normalGround = groundSensor.normal.normalized;
			Debug.DrawRay(groundSensor.point, normalGround * hoverDistanceDetection, Color.red);
			
            hoverActive = true; //Active la lévitation
			
			if(hoverStat == ActiveStat.Desactive){
				hoverStat = ActiveStat.Active;
				
				//On fait repartir le véhicule à la bonne vitesse
				Vector3 projectionOnNormal = Vector3.ProjectOnPlane(transform.forward, normalGround);
				rb.velocity = projectionOnNormal.normalized * oldVelocityM;
			}
			
			if(boostStat == ActiveStat.Desactive){
				boostStat = ActiveStat.Active;
			}
        }else{
            hoverActive = false; //Désactive la lévitation
			if(hoverStat == ActiveStat.Transition){
				hoverStat = ActiveStat.Desactive;
			}
        }

        //Correction pour modifier la direction droite/gauche quand la surface est orienté vers le bas par rapport au référentielle du monde
        if(groundSensor.point.y > transform.position.y){
            //rotate *= -1;
        }

    }

    //Applique les force physique
    void ApplyForce () {
        //Frein
        if (brake){
            rb.velocity *= brakePourcent;
        }
		
		//On annule les ancienne force de rotation
		rb.angularVelocity = Vector3.zero;

        //On tourne la normale de la surface du terrain pour s'adapter à l'orientation du véhicule
		float angle = 0;
		if(drift){
			angle = -driftRollAngle * rotate;
		}else{
			angle = -rollAngle * rotate;
		}

        Vector3 normal = Quaternion.AngleAxis(angle, transform.forward) * normalGround;

        //On projete cette normale par rapport au plan du véhicule.
        Vector3 projectionOnNormal = Vector3.ProjectOnPlane(transform.forward, normalGround);

		Debug.DrawRay(hoverPos.position, projectionOnNormal * hoverDistanceDetection, Color.black);
		
        //On créer une rotation pour que le véhicule soit perpendiculaire à cette normale (et donc à la surface du terrain)
		Quaternion rotationToNormal = Quaternion.LookRotation(projectionOnNormal, normal);
		
        //On applique cette rotation qu'on interpolise avec la vitesse rotateNormaleSpeed pour smoother la rotation
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, rotationToNormal, Time.fixedDeltaTime * rotateNormaleSpeed));

        //float virage = rb.velocity.magnitude >= 20f ? 0.015f * rb.velocity.magnitude : 0.2f;
		float virageRef = SpeedToForce(maxTrusterSpeed);
		//float virage = (virageRef*2 - rb.velocity.magnitude) / virageRef;
		float virage = 1;
        if (drift){
			rb.AddRelativeTorque(0, (int) forward * driftRotateSpeed * rotate - rb.angularVelocity.y, 0, ForceMode.VelocityChange);
		}else{
			rb.AddRelativeTorque(0, (int) forward * rotateSpeed * rotate * virage - rb.angularVelocity.y, 0, ForceMode.VelocityChange);
		}

        //On pousse le véhicule vers son cotée avec la même force que le cotée opposé pour empécher le véhicule de "glisser" lors d'un virage
        float sideSpeed = Vector3.Dot(rb.velocity, transform.right);
		
		if(drift){
			float forceToCompensate = oldVelocityM - rb.velocity.magnitude;
			rb.AddForce(-transform.right * (sideSpeed / (Time.fixedDeltaTime / driftFactor)), ForceMode.Acceleration);
			rb.AddForce(transform.forward * forceToCompensate * 100 * driftCompensateFactor, ForceMode.Acceleration);
			//On compense la perte de velocité :
			//rb.AddForce(rb.velocity.normalized * 10, ForceMode.Acceleration);
		}else{
			rb.AddForce(-transform.right * (sideSpeed / Time.fixedDeltaTime), ForceMode.Acceleration);

        }
		
		//Force de frottement qui ralentie le véhicule.
		rb.velocity *= slowPourcent;
		
        //Force de propulsion du moteur
        Engine();
		
		ProcessLossSpeed();
	}
	
	void ProcessLossSpeed () {
		if(!TimeManager.isSlowMotion()){
			if(rb.velocity.magnitude > reachSpeed){
				reachSpeed = rb.velocity.magnitude;
			}
			
			//Si le joueur ralentie, le truster l'enregistre pour que ce soit définitif
			if(GetVitesseKMH() > minTrusterSpeed && reachSpeed - rb.velocity.magnitude > lossSpeedThreshold){
				trusterSpeed = Utils.ConvertSpeedToKmph(rb.velocity.magnitude);
				reachSpeed = rb.velocity.magnitude;
			}
		}
	}

    void AirControl() {
		//On annule les ancienne force de rotation
		rb.angularVelocity = Vector3.zero;
		
        //On fait tourner le vaisseau (par rapport à l'axe Y) en fonction que si le joueur veut aller à gauche ou à droite
        rb.AddRelativeTorque(0, (int) forward * rotateAirSpeed * rotate - rb.angularVelocity.y, 0, ForceMode.VelocityChange);
		
		//On fait tourner le vaisseau (par rapport à l'axe Z) en fonction que si le joueur veut faire rouller le vaisseau vers la gauche ou la droite
        rb.AddRelativeTorque(0, 0, rollAirSpeed * roll - rb.angularVelocity.y, ForceMode.VelocityChange);

        //On pousse le véhicule vers son cotée avec la même force que le cotée opposé pour empécher le véhicule de "glisser" lors d'un virage
        //float sideSpeed = Vector3.Dot(rb.velocity, transform.right);
		//rb.AddForce(-transform.right * (sideSpeed / Time.fixedDeltaTime), ForceMode.Acceleration);
		
		float sideSpeed = Vector3.Dot(rb.velocity, transform.right);
		//float forceToCompensate = oldVelocityM - rb.velocity.magnitude;
		rb.AddForce(-transform.right * (sideSpeed / Time.fixedDeltaTime), ForceMode.Acceleration);
		//rb.AddForce(transform.forward * forceToCompensate * 100 * driftCompensateFactor, ForceMode.Acceleration);
		
		//Force de frottement qui ralentie le véhicule.
        rb.velocity *= slowPourcent;
		
		//Force de propulsion du moteur
        Engine();
    }
	
	//utilisation du propulseur du véhicule
	void Engine () {
		if(Vector3.Angle(transform.forward, rb.velocity) < 90) {
			rb.AddForce(transform.forward * truster * SpeedToForce(trusterSpeed), ForceMode.Acceleration);
		}else{
			rb.AddForce(transform.forward * truster * SpeedToForce(trusterSpeed) * backRatioSpeed, ForceMode.Acceleration);
		}
	}

    //On applique une force de gravité quand le véhicule n'est pas attaché à une surface
    void ApplyGravity() {
        if(hoverActive && hoverStat == ActiveStat.Active){
            //Utilisation d'un PID pour éviter l'accélération 
            float pid = PID(hoverAltitude, distanceGround);
            rb.AddForce(normalGround * hoverForceUp * pid, ForceMode.Acceleration);
        }else{
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
    }
	
	void OnCollisionStay (Collision collision) {
		Vector3 forceColToUp = Vector3.Dot(collision.impulse, transform.up) * transform.up;
		rb.AddForce(-forceColToUp, ForceMode.Impulse);
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

    private double angleBetweenVector(Vector3 v1, Vector3 v2){
		return Mathf.Acos((float)(Vector3.Dot(v1, v2) / (norme(v1) * norme(v2))));
	}
	
	private double norme (Vector3 vect) {
		return Mathf.Sqrt(Mathf.Pow(vect.x,2) + Mathf.Pow(vect.y,2) + Mathf.Pow(vect.z,2));
	}

    public Rigidbody GetRigidbody()
    {
        return transform.GetComponent<Rigidbody>();
    }

    public double GetVitesse()
    {
        return GetRigidbody().velocity.magnitude;
    }
	
	public int GetVitesseKMH() {
		return Utils.ConvertSpeedToKmph(GetVitesse());
	}
	
	public float GetMaxVitesseKMH() {
		return maxTrusterSpeed;
	}
	
	public float GetTrusterVitesseKMH() {
		return trusterSpeed;
	}
	
	float SpeedToForce (float speed) {
		return speed / 7.18f;
	}
	
	float ForceToKM (float force) {
		return force * 7.18f;
	}
	
	public bool isMaxSpeed () {
		return trusterSpeed >= maxTrusterSpeed;
	}

    // ralentit l'hovercraft d'un certain pourcentage
    public void Slow(float reductor)
    {
        /*float ratio = (float) (100 - reductor) / 100;
        rb.velocity *= ratio;*/
		rb.AddForce(-transform.forward * Utils.ConvertKmphToSpeed(reductor), ForceMode.Impulse);
		if(trusterSpeed - reductor <= 0){
			trusterSpeed = 0;
		}else{
			trusterSpeed -= reductor;
		}
    }

    // accélère l'hovercraft en ajoutant une vitesse (en km/h)
    public void Speed(float amplifier)
    {
        /*float ratio = (float) (100 + amplifier) / 100;
        rb.velocity *= ratio;*/
		rb.AddForce(transform.forward * Utils.ConvertKmphToSpeed(amplifier), ForceMode.Impulse);
		if(trusterSpeed + amplifier >= maxTrusterSpeed){
			trusterSpeed = maxTrusterSpeed;
		}else{
			trusterSpeed += amplifier;
		}
    }

    // renvoi le sens dans lequel l'hovercraft se dirige (avant, arrière)
    public sens_hovercraft GetSens()
    {
        return forward;
    }

    // renvoi la vitesse maximale de l'hovercraft tenant compte de son sens
    public float GetRelativeMaxSpeed()
    {
        switch(forward)
        {
            case sens_hovercraft.avance:
                return GetTrusterVitesseKMH();
            case sens_hovercraft.recule:
                return GetTrusterVitesseKMH() * backRatioSpeed;
        }
        return -1; // echec
    }
}