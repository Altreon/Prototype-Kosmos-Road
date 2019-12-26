using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : Observee {
	
	//Attend qu'on lui dise de pouvoir démarrer (être controlable)
	public bool waitStart = true;
    
    // cooldown choisi des capacités
    public float HamerCooldown = 2f;
    public float BoostCooldown = 2f;
	public float JumpCooldown = 2f;
	
	public int seuilVitesseShootTrident = 100; // vitesse en dessous de laquelle le joueur ne peut pas détruire ou renvoyé un trident (utiliser le marteau)
    public int seuilVitesseMort = 60; // vitesse en dessous de laquelle le joueur meurt si il est ralenti
	
	// cooldown d'invicibilité en secondes (ne peut pas prendre d'accélération également)
	public float invicibilityTime = 1f;
	
	// cooldown d'invicibilité en secondes (ne peut pas prendre d'accélération également)
	public float invicibilityRespawnTime = 2f;
	
	// cooldown d'impossibilité de prendre une accélération après une précédente
	public float SpeedImpossibleTime = 1f;
	
	//accélération bonus par étape speedStep (en km/h)
	public int speedBonus = 40;
	
	//Le rayon de detection des tridents à supprimer au respawn
	public float clearRespawnRadius = 50f;
	
	public bool invert = false;

    // gestionnaire de Timer
    private TimerManager timers;
	
	private bool isInvincible = false;
	private bool canSpeed = true;

    private Hovercraft hovercraft;
    public Hammer hammer;
	
	private bool isAxisHammerInUse = false;
	
	//Effet de particules du joueur
	public ParticleSystem thrusterEffect;
	public ParticleSystem thrusterBoostEffect;
	public ParticleSystem accelerationEffect;
	public ParticleSystem hurtEffect;
	public ParticleSystem particuleDeathSpawn;
	public ParticleSystem driftFriction;
	
	
    public Transform spawnPoint;
	public Music music; // placer le composant Music de main cam
    public Renderer rend;
    public CinematicFXUI hudEffect;

	public AudioSource SoundSource;
    private AudioClip clipJump;
    private AudioClip clipBoost;
    private AudioClip clipMort;
    private AudioClip clipRespawn;
    private AudioClip clipHit;
    private AudioClip clipIndisponible;
    private AudioClip clipRalentissement;
    private AudioClip clipDrift;
	
	private Track currentTrack;

    private bool isInputEnabled = true;
    private CameraHovercraft cameraHovercraft;


    // Use this for initialization
    void Awake ()
    {
		hovercraft = GetComponent<Hovercraft>();
        cameraHovercraft = music.GetComponent<CameraHovercraft>();

        ///////////////////
        // MODIF ICI
        //////////////////
        //rend.material.color = Color.red;
        //Color col = rend.material.color;
        //col.a = 0.01f;
        //rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 0f);
        //Debug.Log(rend.material.color.a);
        /////////


        timers = new TimerManager();
        timers.Add("Jump", new Timer());
        timers.Add("Boost", new Timer());
        timers.Add("Hammer", new Timer());
		timers.Add("Invicibility", new Timer());
		timers.Add("CanSpeed", new Timer());

        clipJump = Resources.Load<AudioClip>("Sound/Player/Saut");
        clipBoost = Resources.Load<AudioClip>("Sound/Player/Boost");
        clipMort = Resources.Load<AudioClip>("Sound/Player/Mort");
        clipRespawn = Resources.Load<AudioClip>("Sound/Player/Respawn");
        clipHit = Resources.Load<AudioClip>("Sound/Player/Hit");
        clipIndisponible = Resources.Load<AudioClip>("Sound/Player/Indisponible");
        clipRalentissement = Resources.Load<AudioClip>("Sound/Player/Ralentissement");
		clipDrift = Resources.Load<AudioClip>("Sound/Player/Drift");

		if(waitStart){
			//On stop le joueur pour l'intro
			Stop();
		}
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateInput(); // met à jour les input
        timers.UpdateTimers(Time.deltaTime); // met à jour les timers

        UpdateHammer(); // mise à jour du marteau
		
		//met à jour le status d'invicibilité et de non-speed
		UpdateStatus();
		
		UpdateMusic();
		
		UpdateFX();
    }    

    void UpdateInput () {

        if (!isInputEnabled)
        {
            hovercraft.setRotate(0f);
            hovercraft.setTruster(0f);
            return;
        }

		hovercraft.setTruster(Input.GetAxis("Trigger"));

        if(Input.GetButton("Brake")) {
            SoundSource.PlayOneShot(clipRalentissement);
            hovercraft.setBrake(true);
        }else{
            hovercraft.setBrake(false);
        }

        if (Input.GetButton("Drift")) {
            hovercraft.setDrift(true);
            SoundSource.PlayOneShot(clipDrift);
            if (!driftFriction.isPlaying){
				driftFriction.Play();
			}
        }
        else {
            hovercraft.setDrift(false);
			if(driftFriction.isPlaying){
				driftFriction.Stop();
			}
        }

		if(!invert){
			hovercraft.setRotate(Input.GetAxis("Horizontal"));
		}else{
			hovercraft.setRotate(-Input.GetAxis("Horizontal"));
		}

        if (Input.GetButtonDown("Jump") && hovercraft.onGround()) {
            if (timers["Jump"].IsFinished())
            {
				hovercraft.jump();
				
                SoundSource.PlayOneShot(clipJump); // lance le son
                timers["Jump"].Start(JumpCooldown); // lance le timer
                NotifyObserver(new Message<float>(type_message.cooldown_start_jump, JumpCooldown)); // notifie l'HUD
            }
            else
            {
                // A IMPLEMENTER : PETITE ANIMATION, SON, FEEDBACK SUR LE FAIT QUE CAPACITE PAS DISPO
                SoundSource.PlayOneShot(clipIndisponible);
            }
        }

        if (Input.GetButtonDown("Boost"))
        {
            if(timers["Boost"].IsFinished() && hovercraft.canBoost())
            {
				hovercraft.boost();
				
                timers["Boost"].Start(BoostCooldown); // lance le timer
                NotifyObserver(new Message<float>(type_message.cooldown_start_boost, BoostCooldown)); // notifie l'HUD
                SoundSource.PlayOneShot(clipBoost);
            }
            else
            {
                // A IMPLEMENTER : PETITE ANIMATION, SON, FEEDBACK SUR LE FAIT QUE CAPACITE PAS DISPO
                SoundSource.PlayOneShot(clipIndisponible);
            }
        }
		
		if( Input.GetAxisRaw("Hammer") == 1)
		{
			if(isAxisHammerInUse == false)
			{	
				if (timers["Hammer"].IsFinished() && CanUseHammer())
				{
					hammer.Smash(); //Le marteau se désactive tout seul en fin d'animation
					
					timers["Hammer"].Start(HamerCooldown); // lance le timer
					NotifyObserver(new Message<float>(type_message.cooldown_start_hammer, HamerCooldown)); // notifie l'HUD
				}
				else
				{
					// POLISH
					// A IMPLEMENTER : PETITE ANIMATION, SON, FEEDBACK SUR LE FAIT QUE CAPACITE PAS DISPO
					SoundSource.PlayOneShot(clipIndisponible);
				}
				
				isAxisHammerInUse = true;
			}
		}
		if( Input.GetAxisRaw("Hammer") == 0)
		{
			isAxisHammerInUse = false;
		}

        if (Input.GetButtonDown("HammerSide"))
        {
            hammer.ChangeSide();
        }

        if (Input.GetButton("RollLeft")) {
            hovercraft.setRoll(-1);
        }else if (Input.GetButton("RollRight")) {
			hovercraft.setRoll(1);
		}else {
			hovercraft.setRoll(0);
		}
			

        if (Input.GetButtonDown("Restart")) {
			Application.LoadLevel(Application.loadedLevel);
        }

        //if (Input.GetButtonDown("Quit")) {
        //    Application.Quit();
        //}
	}

    public Hovercraft GetHovercraft()
    {
        return hovercraft;
    }

    // tue le player (en soit le reset juste)
    public void Kill()
    {
        isInputEnabled = false;
        //particuleDeathSpawn.startColor = Color.gray;
        //particuleDeathSpawn.Play();

        hovercraft.GetRigidbody().detectCollisions = false;
        hovercraft.GetRigidbody().velocity = Vector3.zero;
        StartCoroutine(Flicker(4));

        


        SoundSource.PlayOneShot(clipMort);
		
        StartCoroutine(WaitForRespawn());

        // autre truc (sons, infos, compteur de mort, etc)
    }
	
	//Desactive totalement le joueur
	public void Stop () {
		enabled = false;
		hovercraft.Stop();
	}
	
	//démarre le joueur
	public void Run () {
		enabled = true;
		hovercraft.Run();
	}
	
	//Tourne le marteau pour frapper en avant en arrière
	public void TurnHammer () {
		hammer.Turn();
	}

    IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(1f);
        hudEffect.ShowWhiteScreen(1.2f, 0.4f);
        yield return new WaitForSeconds(1f);
        Respawn();		
        yield return new WaitForSeconds(1f);
        hovercraft.GetRigidbody().detectCollisions = true;
        isInputEnabled = true;
        
    }


    IEnumerator Flicker(float nbIter)
    {
        for (int i = 0; i < nbIter; i++)
        {
            yield return new WaitForSeconds(0.2f);
            rend.enabled = false;
            yield return new WaitForSeconds(0.2f);
            rend.enabled = true;
        }
    }


    // fait respawn le player
    // note : consulter Antoine avant de modifier / supprimer pls
    public void Respawn()
    {
		//On détruit les trident environnent la zone de spawnPoint
		Collider[] hits = Physics.OverlapSphere(spawnPoint.position, clearRespawnRadius, LayerMask.GetMask("Trident"));
		foreach(Collider hit in hits){
				Destroy(hit.gameObject);
		}
		
		
        SoundSource.PlayOneShot(clipRespawn); // Lance le son de respawn
        hovercraft.goToCheckpoint(spawnPoint); // téléporte le player à son spawn
        NotifyObserver(new BasicMessage(type_message.restart_hud_timers)); // reset les cooldown du HUD
        timers.ResetTimers(); // reset les timers de cooldown
		
		//TODO à changer, c'est pour faire ça vite
		GameObject.Find("Poseidon").GetComponent<BossManager>().PlayerRespawn();

        hovercraft.GetComponent<Rigidbody>().AddForce(hovercraft.transform.forward * 0.00001f, ForceMode.Impulse);
        StartCoroutine(Flicker(10));
        //particuleDeathSpawn.startColor = Color.white; //set de la couleur des particules
        //particuleDeathSpawn.Play();
		
		StartInvicibleRespawnTime();
    }

	public void ChangeCheckpoint(Transform checkpoint){
		spawnPoint = checkpoint;
	}

    // ralentit le joueur (sauf si il est en dessous de sa vitesse de survie, le tue)
    // renvoi si le joueur est tué
    public bool Hurt()
    {
		
		bool killed = false;
        if(Utils.ConvertSpeedToKmph(hovercraft.GetVitesse()) > seuilVitesseMort) // si le joueur est assez rapide pour ne pas mourrir
        {
            hovercraft.Slow(speedBonus);
        }
        else // sinon on le tue
        {
            Kill();
            killed = true;
        }
		
		StartInvicibleTime();

        SoundSource.PlayOneShot(clipHit);

        ParticleSystem effect = Instantiate(hurtEffect, transform.position, new Quaternion()) as ParticleSystem;
		effect.Play();
		
		return killed;
    }
    
    // fait accélérer le joueur
    public void Heal()
    {
		//Si le vehicule est à vitesse max, on accélère pas;
		if(hovercraft.isMaxSpeed()){
			return;
		}
		
		StartSpeedImpossibleTime();
		
		accelerationEffect.Play();
        hovercraft.Speed(speedBonus);
    }
	
	// True si le vitesse courante du vehicle dépasse ou égale au seuil
	public bool SpeedReachThreshold()
    {
        return hovercraft.GetVitesseKMH() >= seuilVitesseShootTrident;
    }

	// True si le vitesse courante ET maximale du vehicle dépasse ou égale au seuil
    public bool CanUseHammer()
    {
        return SpeedReachThreshold() && hovercraft.GetTrusterVitesseKMH() >= seuilVitesseShootTrident;
    }

    // met à jour les mécaniques relatives au marteau
    void UpdateHammer()
    {
        // Reactivation du marteau si inactif et cooldown écoulé et vitesse suffisante
        if (!hammer.IsActive() && timers["Hammer"].IsFinished() && CanUseHammer())
        {
            hammer.Active();
        }

        // Désactive le marteau en dessous de la vitesse min pour dégommer les tridents
        if (hammer.IsActive() && !CanUseHammer())
        {
            hammer.Unactive();
        }
    }
	
	// met à jour la piste jouée par la musique
	void UpdateMusic() {
		if((music.CurrentMusicTrack() == Track.Phase1 || music.CurrentMusicTrack() == Track.Phase2) && hovercraft.GetTrusterVitesseKMH() >= seuilVitesseShootTrident){
			currentTrack = music.CurrentMusicTrack();
			music.changeMusicTrack(Track.HammerActive);
		}else if(music.CurrentMusicTrack() == Track.HammerActive && hovercraft.GetTrusterVitesseKMH() < seuilVitesseShootTrident) {
			//TODO récupérer l'info sur quelle phase on est pour relancer la bonne musique
			music.changeMusicTrack(currentTrack);
		}
	}
	
	void UpdateStatus() 
	{
		if(isInvincible && timers["Invicibility"].IsFinished()){
            isInvincible = false;
        }

		if(!canSpeed && timers["CanSpeed"].IsFinished()){
            canSpeed = true;
        }		
	}
	
	void UpdateFX () {
		if(!thrusterBoostEffect.isPlaying && SpeedReachThreshold()){
			thrusterBoostEffect.Play();
		}else if(thrusterBoostEffect.isPlaying && !SpeedReachThreshold()){
			thrusterBoostEffect.Stop();
		}
	}
	
	//Rend le joueur invicible pendant en temps
	public void StartInvicibleTime () {
		isInvincible = true;
		timers["Invicibility"].Start(invicibilityTime);
	}
	
	//Rend le joueur invicible pendant en temps
	public void StartInvicibleRespawnTime () {
		isInvincible = true;
		timers["Invicibility"].Start(invicibilityRespawnTime);
	}
	
	//Rend le joueur impossible à accélérer pendant un temps
	public void StartSpeedImpossibleTime () {
		canSpeed = false;
		timers["CanSpeed"].Start(SpeedImpossibleTime);
	}
	
	public bool IsInvincible () {
		return isInvincible;
	}
	
	public bool CanSpeed () {
		return canSpeed;
	}

    public int GetSeuilVitesseMort()
    {
        return seuilVitesseMort;
    }
    
    public int GetSeuilVitesseMarteau()
    {
        return seuilVitesseShootTrident;
    }

    // change le spawnPoint du joueur
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }
}