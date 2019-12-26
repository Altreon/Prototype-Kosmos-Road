using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    public float propulsionForce = 200f; // Force à laquelle les trident sont propulsés
    public float torqueForce = 50f; // Force à laquelle les trident tourne quand ils sont éjecté
	
	public bool forwardTurn = true; // Définie si le marteau frappe devant ou derrière le véhicule
	
	public Transform ennemiTarget; // cible a viser (le boss)
	public float angleLimitToTarget; // limite de l'angle pour pouvoir renvoyer le trident sur le boss
	
	public ParticleSystem smashActiveEffect;
	public ParticleSystem smashDesactiveEffect;
	
	public TrailRenderer swingEffect;
	
	public Transform smashEffectPos;
	
	public GameObject TrepiedTrident;
	
	public TimeManager timeManager;

	private Player player;
    private Animator animator;

    private bool smashing;
    private bool isOnRightSide;

    private AudioSource audioSource;
    private AudioClip clipVide;
    private AudioClip clipTrident;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.gameObject.GetComponent<Player>();
        animator = GetComponent<Animator>();
        isOnRightSide = animator.GetBool("rightSide");

        smashing = false;

        audioSource = this.GetComponentInParent<AudioSource>();
        clipVide = Resources.Load<AudioClip>("Sound/Hammer/Vide");
        clipTrident = Resources.Load<AudioClip>("Sound/Hammer/Trident");
		
		animator.SetBool("forwardTurn", forwardTurn);
    }

    // déclenche le marteau
    public void Smash ()
    {
        audioSource.PlayOneShot(clipVide, 2f);
        smashing = true;
		swingEffect.emitting = true;
        animator.SetTrigger("Smash");
    }

    // change le côté du marteau
    public void ChangeSide ()
    {
        isOnRightSide = !isOnRightSide; // change de côté

        if (IsActive()) // si le marteau est actif, on met à jour l'affichage du côté
            animator.SetBool("rightSide", isOnRightSide);
    }
	
	//Tourne le marteau pour frapper en avant ou en arrière du véhicule
	public void Turn () {
		forwardTurn = !forwardTurn;
		animator.SetBool("forwardTurn", forwardTurn);
	}

    // active le marteau
    public void Active()
    {
        gameObject.SetActive(true);
        animator.SetBool("rightSide", isOnRightSide);
		animator.SetBool("forwardTurn", forwardTurn);
    }

    // désactive le marteau (appelé à la fin de son animation)
    public void Unactive()
    {
		if(IsActive()){
			swingEffect.emitting = false;
			gameObject.SetActive(false);
			smashing = false;
		}
    }

    // gestion des collisions avec le marteau
    private void OnTriggerStay(Collider other)
    {
        if (!smashing || !enabled) // si la collision n'a pas lieu d'être
            return;

        if(Utils.CompareLayer(other.gameObject, "Trident") && smashing && player.CanUseHammer()) // si on tape avec un trident et qu'on peut utiliser le marteau
        {
            // Son marteau sur trident
            audioSource.PlayOneShot(clipTrident, 3f);
            // on récupère le trident
            Trident trident = other.GetComponent<Trident>();

            if (trident.IsTridentActive()) // si le trident est actif, on le shoot
            {
				trident.stopTrident(); //Permet de stoper le trident s'il est en l'air, sinon n'as aucun effet.
				
				if(!trident.trepied){
					//Phase 1 et intermédiaire
					
					if (AlignWithTarget()) // si on est aligné avec le boss, le trident par dans sa direction
					{	
						Debug.Log("Le trident par en direction de l'ennemi ciblé");
						
						Vector3 direction = (ennemiTarget.position - other.transform.position).normalized;
						trident.Propulsion(direction, Vector3.Cross(direction, player.GetHovercraft().getNormalGround()), propulsionForce, torqueForce, true);
					}
					else // sinon, il part tout droit
					{
						Debug.Log("Le trident par tout droit");
						
						Vector3 dir = forwardTurn ? transform.parent.forward : -transform.parent.forward;
						Vector3 right = Vector3.Cross(dir, player.GetHovercraft().getNormalGround());
						trident.Propulsion(Quaternion.AngleAxis(10, right) * dir, right, propulsionForce, torqueForce, true);
					}
				}else{
					//Phase 2
					
					// création d'un nouveau trepied, qu'on va faire tourner dans le sens inverse
					Transform trepiedTrident = Instantiate(TrepiedTrident).transform;
					trepiedTrident.position = new Vector3(0, trident.transform.parent.position.y, 0);
					trepiedTrident.GetComponent<TrepiedTrident>().invertSens = true;
					
					//On change le parent du trident
					trident.transform.parent = trepiedTrident;
					
					//On Lui met un effet de rotation
					trident.transform.Translate(Vector3.up * 20);
					trident.Propulsion(Vector3.zero, trident.transform.right, 0, torqueForce, true);
				}
				
				ParticleSystem effect = Instantiate(smashActiveEffect, smashEffectPos.position, new Quaternion()) as ParticleSystem;
				effect.Play();
				
				timeManager.slowMotion();
            }
            else if (!trident.IsTridentActive()) // sinon, on le détruit
            {
                Debug.Log("Le trident est détruit par le marteau");
				
				ParticleSystem effect = Instantiate(smashDesactiveEffect, smashEffectPos.position, new Quaternion()) as ParticleSystem;
				effect.Play();
				
                trident.DestroyTrident();
            }
			smashing = false;
        }
    }

    // vérifie si le joueur est aligné avec le boss (dans la limite de l'angle angleLimitToTarget)
    private bool AlignWithTarget()
    {
        Vector3 direction = ennemiTarget.position - transform.parent.position;
		Vector3 dir = forwardTurn ? transform.parent.forward : -transform.parent.forward;
        double angle = Mathf.Acos(Vector3.Dot(direction, dir) / (direction.magnitude * 1));

        return angle * 180 / Mathf.PI < angleLimitToTarget;
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public bool IsSmashing()
    {
        return smashing;
    }
}
