using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enum
//Les phase du boss
public enum PhaseBoss
{
    Intro = 0,
    Phase1 = 1,
    PhaseIntermediaire = 2,
    Phase2 = 3,
    Fin = 4
};

public class BossManager : Observee
{
    public CameraHovercraft maincam;

    public Player player;
    public Music music;
    public Transform spawnPhase2Player;

    public int pv = 3; // pv totaux du boss

    //PV de changement de phase de boss
    public int pvPhaseIntermediaire = 2;
    public int pvPhase2 = 1;
    //Rajouter ici si d'autre phase

    public MeshCollider colliderBossNormal;
    public BoxCollider colliderBossPhase2;

    private PhaseBoss currentPhase = PhaseBoss.Intro; //Phase actuel du boss

    private AudioSource audioSource;
    private AudioClip clipMort;
    private AudioClip clipHit;

    private BossFollowPlayerBehaviour follow;
    private ThrowingTridents throwing;
    private ThrowingTrepiedTridents throwingTrepied;
    private DeplacementBehaviour deplacement;

    public ObserverHUD hud;

    // Start is called before the first frame update
    void Start()
    {
        follow = transform.GetComponent<BossFollowPlayerBehaviour>();
        throwing = transform.GetComponent<ThrowingTridents>();
        throwingTrepied = transform.GetComponent<ThrowingTrepiedTridents>();
        deplacement = transform.GetComponent<DeplacementBehaviour>();

        audioSource = this.GetComponent<AudioSource>();
        clipMort = Resources.Load<AudioClip>("Sound/Poseidon/Mort");
        clipHit = Resources.Load<AudioClip>("Sound/Poseidon/Hit");
    }

    public void Update()
    {
        if (Input.GetButtonDown("CheatHitBoss"))
        {
            HitBoss();
        }
    }

    public void HitBoss()
    {
        Debug.Log("Il y a un hit avec le boss");
        pv--;
        // Son boss hit
        audioSource.PlayOneShot(clipHit);

        if (pv == pvPhaseIntermediaire)
        {
            EndPhase1();
        }

        if (pv == pvPhase2)
        {
            EndPhaseIntermediaire();
        }

        if (pv == 0)
        {
            EndPhase2();
        }
    }

    public PhaseBoss getPhase()
    {
        return currentPhase;
    }

    //---------------------Phase Intro------------------------
    public void BeginIntro()
    {
        // mise en place du boss
        transform.localPosition = new Vector3(0, -1000, 0);
        transform.eulerAngles = new Vector3(60, 0, 0);

        Debug.Log("DÃ©but de l'intro du boss");
        currentPhase = PhaseBoss.Intro;
        SetThrowingBehaviour(false);
        SetFollowBehaviour(false);
    }

    //---------------------Phase 1------------------------
    public void BeginPhase1()
    {
        //activation/dÃ©sactivation collider
        colliderBossNormal.enabled = true;
        colliderBossPhase2.enabled = false;

        UpdateCoeurHUD(pv - pvPhase2); // initialise les pv hud

        // mise en place du boss
        transform.localPosition = new Vector3(0, -670, 0);
        transform.eulerAngles = new Vector3(0, 0, 0);

        // modification paramÃ¨tres
        Debug.Log("DÃ©but de la phase 1 du boss");
        SetThrowingBehaviour(true);
        SetFollowBehaviour(true);
        player.Run();
        currentPhase = PhaseBoss.Phase1;
    }

    public void EndPhase1()
    {
        //dÃ©sativation collider
        colliderBossNormal.enabled = false;

        // mise en place du boss
        transform.localPosition = new Vector3(0, -670, 0);
        transform.eulerAngles = new Vector3(0, 360, 0);

        pv = pvPhaseIntermediaire; // force (en cas d'appel externe à la logique du boss)
        
		
		UpdateCoeurHUD(pv - pvPhase2); // met à jour les pv hud

        Debug.Log("Fin de la phase 1 du boss");
        SetThrowingBehaviour(false);
        SetFollowBehaviour(false);
        player.Stop();

        music.changeMusicTrack(Track.Phase1);

        // notifie le gestionnaire de cinÃ©matique qu'il doit lancer la cinÃ©matique de changement de phase
        NotifyObserver(new Message<CinematicManager.type_cinematic>(type_message.cinematic, CinematicManager.type_cinematic.cinematic_intermediaire));
    }

    //--------------------- Phase intermÃ©diaire ------------------------
    public void BeginPhaseIntermediaire()
    {
        //activation/dÃ©sactivation collider
        colliderBossNormal.enabled = true;
        colliderBossPhase2.enabled = false;

        // mise en place du boss
        transform.localPosition = new Vector3(0, -670, 0);
        transform.eulerAngles = new Vector3(0, 0, 0);

        // Modification lancer de trident
        throwing.ShootingRadius = 40;
        throwing.LengthTridentPlayer = 80;
        throwing.ShotsPerSecound = 1;
        throwing.TridentsNumbers = 10;
        throwing.ShootPower = 300;

        Debug.Log("DÃ©but de la phase intermÃ©diaire du boss");
        SetThrowingBehaviour(true);
        SetFollowBehaviour(true);
        player.Respawn();
        player.Run();
        currentPhase = PhaseBoss.PhaseIntermediaire;
    }

    public void EndPhaseIntermediaire()
    {
        //dÃ©sactivation collider
        colliderBossNormal.enabled = false;

        // mise en place du boss
        transform.localPosition = new Vector3(0, -670, 0);
        transform.eulerAngles = new Vector3(0, 0, 0);

        pv = pvPhase2; // force (en cas d'appel externe Ã  la logique du boss)
        
		UpdateCoeurHUD(0); // met à jour les pv hud

        Debug.Log("Fin de la phase intermÃ©diaire du boss");
        SetThrowingBehaviour(false);
        SetFollowBehaviour(false);
        player.Stop();

        // On supprimes les tridents prÃ©cÃ©dent
        throwing.DeleteTridents();

        music.changeMusicTrack(Track.Phase1);


        // notifie le gestionnaire de cinÃ©matique qu'il doit lancer la cinÃ©matique de changement de phase
        NotifyObserver(new Message<CinematicManager.type_cinematic>(type_message.cinematic, CinematicManager.type_cinematic.cinematic_phase_finale));
    }

    //---------------------Phase 2------------------------
    public void BeginPhase2()
    {
        //activation/dÃ©sactivation collider
        colliderBossNormal.enabled = false;
        colliderBossPhase2.enabled = true;
		
		UpdateCoeurHUD(pvPhase2); // met à jour les pv hud

        // mise en place du boss
        transform.parent.transform.eulerAngles = new Vector3(44.5f, 0, 0); // changement du pivot
        transform.localPosition = new Vector3(0, -690, 0);
        transform.localEulerAngles = new Vector3(0, 90, 0);
        transform.GetComponent<BossAnimation>().Run();

        Debug.Log("DÃ©but de la phase 2 du boss");
        SetFollowBehaviour(false);
        deplacement.Init();

        //On bouge la position d'envoie des trident
        throwing.shootPos.Translate(-Vector3.right * 15);

        player.transform.Find("ColliderLeft").localPosition = new Vector3(0, 0, 3f);
        player.transform.Find("ColliderLeft").localScale = new Vector3(3, 4, 1);
        player.transform.Find("ColliderRight").localPosition = new Vector3(0, 0, 3f);
        player.transform.Find("ColliderRight").localScale = new Vector3(3, 4, 1);


        player.ChangeCheckpoint(spawnPhase2Player);
		player.invert = true;
        player.TurnHammer();
        player.Respawn();
        player.Run();
        currentPhase = PhaseBoss.Phase2;

        SetThrowingBehaviour(true);

        music.changeMusicTrack(Track.Phase2);

        // change les paramÃ¨tres de la camÃ©ra
        maincam.EnablePhase2Cam();
        maincam.distanceFromHovercraft = 6;


    }

    public void EndPhase2()
    {
        //dÃ©sactivation collider
        colliderBossPhase2.enabled = false;

        Debug.Log("Fin de la phase 2 du boss");
        SetThrowingBehaviour(false);
        SetFollowBehaviour(false);
        player.Stop();

        pv = 0; // force (en cas d'appel externe Ã  la logique du boss)
        UpdateCoeurHUD(0);


        //TODO music victoire

        audioSource.PlayOneShot(clipMort); // son mort du boss

        // notifie le gestionnaire de cinÃ©matique qu'il doit lancer la cinÃ©matique de fin
        NotifyObserver(new Message<CinematicManager.type_cinematic>(type_message.cinematic, CinematicManager.type_cinematic.cinematic_fin));
    }

    //---------------------Phase Fin------------------------
    public void BeginFin()
    {
        Debug.Log("Le boss est mort");
        currentPhase = PhaseBoss.Fin;
        follow.enabled = false;
        throwingTrepied.enabled = false;
    }

    public void SetFollowBehaviour(bool etat)
    {
        follow.enabled = etat;
    }

    public void SetThrowingBehaviour(bool etat)
    {
		if (currentPhase == PhaseBoss.Fin){
				return;
		}
		
        if (currentPhase == PhaseBoss.Phase2)
        {
            throwingTrepied.enabled = etat;
        }
        else
        {
            throwing.enabled = etat;
        }
    }

    public void PlayerRespawn()
    {
        if (currentPhase == PhaseBoss.Phase2)
        {
            deplacement.Init();
        }
    }

    // affiche le nombre de coeur passé en paramètre sur le HUD
    public void UpdateCoeurHUD(int nb)
    {
        hud.GetLifeManagerHUD().SetNbCoeur(nb);
    }

    //---------------------Pour la phase intermÃ©diare------------------------
    /*
		Pour l'instant, pas de phase intermÃ©diare
		Ce qui est commentÃ© servira pour la phase intermÃ©diare
		
		// Envoi le trident sur le joueur
        throwing.LengthTridentPlayer = 0;
        // Met le nombre de trident envoyer Ã  1
        throwing.TridentsNumbers = 1;
        // On baisse le radius de la zone vidÃ©
        throwing.ShootingRadius = 10;
        // On rÃ©duit la vitesse du trident
        throwing.ShootPower = 150;
		*/
}
