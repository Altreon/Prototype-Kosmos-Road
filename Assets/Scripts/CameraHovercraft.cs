using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHovercraft : MonoBehaviour
{
    public Hovercraft hovercraft; // hovercraft à suivre

    [Tooltip("Mettre un point de référence sur le trepied du boss que l'on fixera")]
    public Transform refBoss; // référence du boss à suivre

    [Tooltip("Distance à l'hovercraft (valable en avant comme en arière")]
    public float distanceFromHovercraft; // la distance caméra/véhicule

    [Tooltip("Hauteur de la caméra par rapport à l'hovercraft")]
    public float heightFromHovercraft; // la hauteur caméra/véhicule

    [Tooltip("Inclinaison de la caméra (angle de rotation)")]
    public float inclinaison = 0f;

    public bool orientationZ = true;
	
	private Rigidbody rb;
    
    private float speedEffect; // puissance de l'effet de vitesse (FOV + recul caméra)
    private float rotationSmoothing; // adoucir la rotation caméra
    private float defaultFOV; // FOV par défaut de la caméra

    private bool phase2; // passage de la caméra en phase 2

    // Start is called before the first frame update
    void Start()
    {
		rb = GetComponent<Rigidbody>();
		
        rotationSmoothing = 5.0f;
        speedEffect = 20.0f;
        defaultFOV = 50;

        phase2 = false; 
    }

    void FixedUpdate()
    {
        if (!phase2) // en tant normal
            StandardUpdate();

        else // en phase 2
            Phase2Update();
    }

    private void StandardUpdate()
    {
        // vecteur avant de l'hovercraft tenant compte du sens
        Vector3 forwardHovercraft = hovercraft.transform.forward * (int)hovercraft.GetSens();

        // définit la position voulue de la caméra
        Vector3 wantedPosition = hovercraft.transform.position;
        wantedPosition.y += heightFromHovercraft; // tient compte de la hauteur à la caméra
        wantedPosition -= forwardHovercraft * distanceFromHovercraft; // tient compte du sens de l'hovercraft et de la distance voulue à l'hovercraft

        Vector3 newPosition = Vector3.Lerp(transform.position, wantedPosition, speedEffect * Time.fixedDeltaTime);

        // définit la rotation voulue de la caméra
        Quaternion newRotation;

        newRotation = Quaternion.LookRotation(forwardHovercraft) * (orientationZ ? Quaternion.Euler(inclinaison, 0, hovercraft.transform.eulerAngles.z) : Quaternion.Euler(inclinaison, 0, 0));
        newRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSmoothing * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);
		rb.MoveRotation(newRotation);

        // gestion FOV
        float wantedFOV = defaultFOV + (float)hovercraft.GetVitesse() * 0.1f;
        transform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(transform.GetComponent<Camera>().fieldOfView, wantedFOV, speedEffect * Time.fixedDeltaTime);
    }

    private void Phase2Update()
    {
        // vecteur du joueur vers le boss
        Vector3 headingToBoss = refBoss.position - hovercraft.transform.position;
		//On projete par rapport au plan du sol
		headingToBoss = Vector3.ProjectOnPlane(headingToBoss, hovercraft.getNormalGround());
		Debug.DrawRay(hovercraft.transform.position, headingToBoss, Color.red);
		//On normalise pour avoir un vecteur de longueur 1
		headingToBoss = headingToBoss.normalized;
        
        // vecteur de l'hovercraft vu de face 
        //Vector3 forwardHovercraft = hovercraft.transform.forward * (int) hovercraft.GetSens() * -1;
		Vector3 forwardHovercraft = headingToBoss * (int) hovercraft.GetSens();

        // définit la position voulue de la caméra
        Vector3 wantedPosition = hovercraft.transform.position;
        wantedPosition.y += heightFromHovercraft; // tient compte de la hauteur à la caméra
        wantedPosition -= forwardHovercraft * distanceFromHovercraft; // tient compte du sens de l'hovercraft et de la distance voulue à l'hovercraft

        Vector3 newPosition = Vector3.Lerp(transform.position, wantedPosition, speedEffect * Time.fixedDeltaTime);

        // définit la rotation voulue de la caméra
        Quaternion newRotation;

        newRotation = Quaternion.LookRotation(headingToBoss) * (orientationZ ? Quaternion.Euler(inclinaison, 0, refBoss.transform.parent.Find("Poseidon").transform.eulerAngles.z * -1) : Quaternion.Euler(inclinaison, 0, 0));
        newRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSmoothing * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);
		rb.MoveRotation(newRotation);

        // gestion FOV
        float wantedFOV = defaultFOV + (float)hovercraft.GetVitesse() * 0.1f;
        transform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(transform.GetComponent<Camera>().fieldOfView, wantedFOV, speedEffect * Time.fixedDeltaTime);
    }

    // passage en phase 2
    public void EnablePhase2Cam()
    {
        phase2 = true;
    }

    // passage par défaut
    public void DisablePhase2Cam()
    {
        phase2 = false;
    }
}
