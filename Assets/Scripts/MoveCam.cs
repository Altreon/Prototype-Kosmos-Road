using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCam : MonoBehaviour
{
    public Transform hovercraftT;
    public Rigidbody rb;

    public float speed;
	public float rotateSpeed;
    public float angle;
    public float distance;
	
	private Hovercraft hovercraft;

    // Start is called before the first frame update
    void Start()
    {
        hovercraft = hovercraftT.gameObject.GetComponent<Hovercraft>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Vecteur de vélocité du véhicule
        Vector3 velocity = rb.velocity;

        //Calcul si le véhicule avance ou recule
        int side = 0;
        if (Vector3.Angle(hovercraftT.forward, velocity) < 90) {
            side = 1;
        }
        else {
            side = -1;
        }

        //Nouvelle position pour la caméra
        Vector3 pos = hovercraftT.position - Quaternion.AngleAxis(angle * side, hovercraftT.right) * Vector3.Normalize(rb.velocity) * distance;

        //Mise à jour de la position avec interpolation (vitesse définie par la variable publique speed)
        transform.position = Vector3.Lerp(transform.position, pos, speed);


        //Oriente la caméra en suivant le vecteur velocité du véhicule
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rb.velocity, hovercraft.getNormalGround()), rotateSpeed);
    }
}
