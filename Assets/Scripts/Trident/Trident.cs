using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Trident : MonoBehaviour
{
    // gestion des textures (actif/inactif)
    public Material[] materials;
    public Renderer rend;

    public int speedBoost = 40; // pourcentage d'augmentation de la vitesse quand on passe proche

    private Timer timerActivity;
    public float timeToInactive = 10f; // temps avant inactivité (en secondes)
	
	private Timer timerDestroy;
    public float timeToDestroy = 100f; // temps avant depop (en secondes)
	public float timeToDisapear = 2f; // temps entre le depop et la destruction (en secondes)
	
	public bool trepied = false; //défini si le trident est accroché à un trépié (phase 2)
	
	public ParticleSystem HitGroundEffect;
	public ParticleSystem HitBossEffect;
	public ParticleSystem FrictionEffect;

    private Rigidbody rbTrident;

    private AudioSource audioSource;
    private AudioClip clipBam;
    private AudioClip clipAcceleration;
    private AudioClip clipPlanter;


    private bool lethality = true; // si le trident tue ou non
    private bool isActive = true; // définit si le trident est inactif (true) ou actif (oui)
    private bool canBoostPlayer = true; // définit si le joueur peut ou non être accéléré par le trident
	private bool isDestroying = false; // si le trident est en train d'être détruit

	private bool lanceurJoueur = false; // définit si c'est le joueur qui a lancé le trident
	
	public float UnstopableTime = 0.1f;
    private Timer UnstopableTimer; //temps de non-collision avec le sol après un lancer du joueur
	
	private bool follow = false;

    // Start is called before the first frame update
    public void Start()
    {
        rbTrident = GetComponent<Rigidbody>();

        //rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[0];

        timerActivity = new Timer(timeToInactive);
		timerDestroy = new Timer(timeToDestroy);
		
		timerActivity.Start();
		timerDestroy.Start();
		
		UnstopableTimer = new Timer(UnstopableTime);

        audioSource = this.GetComponent<AudioSource>();
		
		if(this.GetComponent<TridentFollow>()){
			follow = true;
		}

        clipBam = Resources.Load<AudioClip>("Sound/Trident/Bam");
        clipAcceleration = Resources.Load<AudioClip>("Sound/Trident/Acceleration");
        clipPlanter = Resources.Load<AudioClip>("Sound/Trident/Planter");
    }

    // Update is called once per frame
    void Update()
    {
        timerActivity.Tick(Time.deltaTime);
		timerDestroy.Tick(Time.deltaTime);
		UnstopableTimer.Tick(Time.deltaTime);

        if (IsTridentActive() && timerActivity.IsFinished())
        {
            Desactivate(); // désactive le trident
        }
		
		if (!isDestroying && timerDestroy.IsFinished())
        {
			isDestroying = true;
            StartCoroutine(End()); // détruit le trident
        }
    }

    // désactive le trident
    public void Desactivate()
    {
        rend.sharedMaterial = materials[1];
        isActive = false;
    }

    // gestion des collisions (entrée)
    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) { // si la collision n'a pas lieu d'être
            return;
		}
		
		
        if (!lanceurJoueur && other.CompareTag("Hovercraft")) // si on tape avec l'hovercraft
        {
            Player playerRef = other.transform.parent.gameObject.GetComponent<Player>();

			// Si le joueur n'est pas invicible
			if(!playerRef.IsInvincible()) {
				canBoostPlayer = false; // quand on tape, on ne peut pas booster en même temps
				
				if (lethality) // si le trident tue
				{
					Debug.Log("Joueur tué par un trident");
					playerRef.Kill();
				}
				else // sinon, il blesse
				{
					if (IsTridentActive()) // si trident actif, lance son associé + réduction de vitesse (pour trident actif)
					{
						Debug.Log("Ralenti vitesse active");

						audioSource.PlayOneShot(clipBam, audioSource.volume * 0.2f);


						playerRef.Hurt();
					}
					else // si trident innactif, lance son associé + réduction de vitesse (pour trident inactif) + casse le trident
					{
						Debug.Log("Ralenti vitesse inactive et destruction du trident");

						audioSource.PlayOneShot(clipBam, audioSource.volume * 0.2f);

						playerRef.Hurt();
					}
					
					StartCoroutine(End()); // détruit le trident
				}
			}
        }
        else if (other.CompareTag("HovercraftLeftCollider") || other.CompareTag("HovercraftRightCollider") && canBoostPlayer) // si on tape avec une des hitbox latéral du vaisseau et qu'on peut booster le joueur
        {
            Player playerRef = other.transform.parent.gameObject.GetComponent<Player>();

            if (canBoostPlayer && !playerRef.IsInvincible() && playerRef.CanSpeed())
            {
                if (IsTridentActive()) // si trident actif, lance son associé + réduction de vitesse (pour trident actif)
                {
                    Debug.Log("Accélération !!!");

                    audioSource.PlayOneShot(clipAcceleration);

                    playerRef.Heal();
					
					if(!follow){
						//On desactive le trident après passage
						Desactivate();
					}else{
						Destroy(gameObject);
					}
                }
            }
        }
        else if (Utils.CompareLayer(other.gameObject, "Terrain") && UnstopableTimer.IsFinished()) // si on touche le terrain (attérissage), lance un son associé
        {
			if(lanceurJoueur){
				transform.LookAt(ImpactPoint());
			}
            audioSource.PlayOneShot(clipPlanter, audioSource.volume * 0.2f);
            stopTrident();
			rbTrident.isKinematic = true; // trident devient statique
			
			HitGroundEffect.transform.LookAt(ImpactPoint());
			HitGroundEffect.Play();
        }
        else if (!rbTrident.isKinematic && lanceurJoueur && Utils.CompareLayer(other.gameObject, "Boss"))
        {
			
			rbTrident.detectCollisions = false;
			
			transform.LookAt(ImpactPoint());

			BossManager bossManager = other.transform.parent.parent.parent.gameObject.GetComponent<BossManager>();
			if(!bossManager){ //si pas trouvé c'est l'autre collider (phase 2)
				bossManager = other.transform.parent.parent.gameObject.GetComponent<BossManager>();
			}
            bossManager.HitBoss();

            stopTrident();
			Destroy(gameObject);
			
			HitBossEffect.Play();
        }else if(rbTrident.isKinematic && Utils.CompareLayer(other.gameObject, "Boss"))
		{
			StartCoroutine(End()); // détruit le trident
		}
    }
	
	// désactive le trident et le détruit au bout de X secondes
	IEnumerator End () {
		BoxCollider[] colliders = GetComponents<BoxCollider>();
		foreach(BoxCollider c in colliders) {
			c.enabled = false;
		}
		
		rbTrident.detectCollisions = false;
		rend.enabled = false;
		
		yield return new WaitForSeconds(timeToDisapear);
		
		Destroy(gameObject);
	}
	
	private Vector3 ImpactPoint () {
		RaycastHit hit;
		if (Physics.Raycast(transform.position, rbTrident.velocity, out hit))
		{
			return hit.point; 
		}
		return Vector3.zero;
	}

	
	public void stopTrident () {
		lethality = false; // le trident n'est plus léthal
		rbTrident.velocity = Vector3.zero; // arrêt du trident
		rbTrident.angularVelocity = Vector3.zero; //arrêt de la rotation du trident
		
		//on arrète l'effet de particule friction si elle est jouée
		if(FrictionEffect.isPlaying){
			FrictionEffect.Stop(); //on arrète l'effet de particule friction si elle est jouée
			FrictionEffect.Clear();
		}
	}
	
    // gestion des collisions (sortie)
    private void OnTriggerExit(Collider other)
    {
        if (!enabled) // si la collision n'a pas lieu d'être
            return;

        if (Utils.CompareLayer(other.gameObject, "Hovercraft")) // si on sort de la collision avec l'hovercraft
        {
            canBoostPlayer = true; // on peut à nouveau être boosté par le trident
        }
    }

    // détruit le trident
    public void DestroyTrident()
    {
        Destroy(gameObject);
    }

    // renvoi si le trident est actif ou non
    public bool IsTridentActive()
    {
        return isActive;
    }

    // propulse le trident selon les forces passées en paramètre
    public void Propulsion(Vector3 dirPropulsion, Vector3 dirTorque, float propulsionForce, float torqueForce, bool isLanceurJoueur)
    {
        rbTrident.isKinematic = false; // le trident peut bouger (non statique)

        // application des forces
        rbTrident.maxAngularVelocity = torqueForce;
        rbTrident.AddForce(dirPropulsion * propulsionForce, ForceMode.Impulse);
        rbTrident.AddTorque(dirTorque * torqueForce, ForceMode.Impulse);
		
		//on dit qui lance
		lanceurJoueur = isLanceurJoueur;
		
		if(lanceurJoueur) {
			UnstopableTimer.Start();
		}
		
		if(follow){
			//pour l'instant, instant touch
			rbTrident.detectCollisions = false;

            GameObject.Find("Poseidon").GetComponent<BossManager>().HitBoss();

            Destroy(gameObject);
		}
    }

    // idem "Propulsion()" mais pour un lancer mortel
    public void LethalLaunch(Vector3 dirPropulsion, Vector3 dirTorque, float propulsionForce, float torqueForce, bool isLanceurJoueur)
    {
        lethality = true; // le trident tue
        Propulsion(dirPropulsion, dirTorque, propulsionForce, torqueForce, isLanceurJoueur);
    }
	
	public void InitFollow(){
		lethality = false;
		rbTrident.isKinematic = false; // le trident peut bouger (non statique)
		lanceurJoueur = false;
	}
}

