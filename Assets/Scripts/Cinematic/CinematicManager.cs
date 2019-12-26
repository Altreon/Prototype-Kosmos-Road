using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicManager : Observer
{
    private Animation animationIntroLevel;
    private Animation animationBossCam;
    private Animation animationBoss;
    private Animation animationWater;
    private Animation animationTrepiedBoss;

    public GameObject boss;
    public GameObject HUD;
    public Camera mainCam;
    public GameObject waterBox;

    private CamManager cameras;
    
    private CinematicFXUI cinematicsHUD;

    private Queue<type_cinematic> cinematicQueue = new Queue<type_cinematic>();
    private bool isLaunched = false; // définit si la cinématique en tête de queue est déjà lancé ou non


    [Tooltip("Joue toutes les cinématiques d'affilées")]
    public bool DevMode = false; // permet d'enchaîner toutes les cinématiques (pour les tester)
    public bool PlayIntro = true;
    public bool StartPhase2 = false; // pour cheat et passer directement en phase 2

    public enum type_cinematic
    {
        cinematic_intro,
        cinematic_fin,
        cinematic_intermediaire,
        cinematic_phase_finale
    }

    // Start is called before the first frame update
    void Start()
    {
        animationIntroLevel = transform.Find("TrepiedStartLevel").GetComponent<Animation>();
        animationBossCam = transform.Find("CinematicCamBoss").GetComponent<Animation>();
        animationBoss = boss.transform.GetComponent<Animation>();
        animationTrepiedBoss = boss.transform.parent.GetComponent<Animation>();
        animationWater = waterBox.transform.GetComponent<Animation>();

        cameras = new CamManager();
        cameras.Add("cameraBoss", transform.Find("CinematicCamBoss").GetComponent<Camera>());
        cameras.Add("cameraLevel", transform.Find("TrepiedStartLevel").transform.Find("CinematicCam").GetComponent<Camera>());
        cameras.Add("mainCam", mainCam);
        cameras.Add("camPhase2", boss.transform.parent.parent.Find("TrepiedCamera").transform.Find("camPhase2").GetComponent<Camera>());

        cinematicsHUD = transform.Find("HUDCinematics").GetComponent<CinematicFXUI>();
        
        if(StartPhase2) // si on veut directement commencer en phase 2
        {
            PlayCinematic(type_cinematic.cinematic_phase_finale);
            SkipCurrentCinematic(); // on skippe immédiatement la cinématique
        }

        else if (DevMode) // si on joue toutes les cinématiques
        {
            PlayCinematic(type_cinematic.cinematic_intro);
            PlayCinematic(type_cinematic.cinematic_intermediaire);
            PlayCinematic(type_cinematic.cinematic_phase_finale);
            PlayCinematic(type_cinematic.cinematic_fin);
        }

        else if (PlayIntro) // si on joue l'intro (début de jeu)
        {
            PlayCinematic(type_cinematic.cinematic_intro); // lance l'intro de notre level
        }

        else // si on ne joue pas l'intro, on lance directement le début de la phase 1
        {
            cameras.SetFrontCam("mainCam");
            boss.transform.GetComponent<BossManager>().BeginPhase1();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(cinematicQueue.Count > 0) // si il y a des cinématiques à jouer
        {
            switch (cinematicQueue.Peek()) // prend la cinematique en priorité dans la queue
            {
                case type_cinematic.cinematic_intro:
                    if (!isLaunched)
                        LaunchIntro();
                    break;

                case type_cinematic.cinematic_intermediaire:
                    if (!isLaunched)
                        LaunchPhaseIntermediaire();
                    break;

                case type_cinematic.cinematic_phase_finale:
                    if (!isLaunched)
                        LaunchPhase2();
                    break;

                case type_cinematic.cinematic_fin:
                    if (!isLaunched)
                        LaunchFin();
                    break;
            }

            if (Input.GetButtonDown("Skip")) // si on skip la cinématique
                SkipCurrentCinematic();
        }
    }

    // intérromp la cinématique en cours
    public void SkipCurrentCinematic()
    {
        if (cinematicQueue.Count > 0) // si il y a des cinématiques à jouer
        {
            switch (cinematicQueue.Peek()) // prend la cinematique en priorité dans la queue
            {
                case type_cinematic.cinematic_intro:
                    EndIntro();
                    break;

                case type_cinematic.cinematic_intermediaire:
                    EndPhaseIntermediaire();
                    break;

                case type_cinematic.cinematic_phase_finale:
                    EndPhaseFinale();
                    break;

                case type_cinematic.cinematic_fin:
                    return; // pas skippable
            }

            // arrête les animations en cours (on peut le faire sur toutes car normalement elles ne devrait pas jouer à ce moment là
            animationIntroLevel.Stop();
            animationBossCam.Stop();
            animationBoss.Stop();
            animationWater.Stop();
            animationTrepiedBoss.Stop();

            StopAllCoroutines(); // stoppe toutes les coroutines du script
        }
    }

    // ajoute dans la queue une cinématique à jouer
    public void PlayCinematic(type_cinematic type)
    {
        cinematicQueue.Enqueue(type);
    }

    private void EndCinematique()
    {
        if(cinematicQueue.Count > 0)
            cinematicQueue.Dequeue(); // on enlève de la queue la cinématique
        isLaunched = false; // reset du booléen pour la suivante
    }

    // lance la cinématique d'introduction
    private void LaunchIntro()
    {
        isLaunched = true;
        cinematicsHUD.AfficherPasser(true);

        cinematicsHUD.ShowBars(150, 0.7f); // affiche les barres de cinématiques sur le HUD

        cameras.SetFrontCam("cameraBoss");

        HUD.SetActive(false);

        boss.transform.GetComponent<BossManager>().BeginIntro();

        StartCoroutine(IntroCoroutine());
    }

    // gestion de la cinematique d'intro
    private IEnumerator IntroCoroutine()
    {
        yield return Utils.PlayAnimationsCoroutine(new Anim[] {
            new Anim(animationBossCam, "introBossCam", 0.5f),
            new Anim(animationBoss, "introBoss", 0.5f)
        }); // lance les animations et les animations et attend la fin

        cameras.SetFrontCam("cameraLevel"); // changement de caméra
        cinematicsHUD.ShowBossName(1.0f); // affiche le nom du boss

        yield return Utils.PlayAnimationCoroutine(new Anim(animationIntroLevel, "introLevel", 0.2f)); // joue la cinématique et attend la fin

        EndIntro();
    }

    // fin de l'intro
    private void EndIntro()
    {
        cinematicsHUD.HideBars(0.7f); // enlève les barres de cinématiques sur le HUD
        cinematicsHUD.HideBossName(0.4f); // enlève le nom du boss

        cameras.SetFrontCam("mainCam"); // remet la caméra joueur en priorité
        HUD.SetActive(true); // réactive le HUD

        boss.transform.GetComponent<BossManager>().BeginPhase1(); // lance la phase 1 du boss

        cinematicsHUD.ShowText("Frappez dans les tridents avec votre marteau pour tuer Poisséidon", 1.0f, 3.0f); // instructions
        cinematicsHUD.AfficherPasser(false);

        EndCinematique(); // fin de la cinematique
    }

    // lance la cinématique de changement de phase
    private void LaunchPhaseIntermediaire()
    {
        isLaunched = true;
        cinematicsHUD.AfficherPasser(true);

        cinematicsHUD.ShowBars(150, 0.7f); // affiche les barres de cinématiques sur le HUD

        cameras.SetFrontCam("cameraBoss");

        HUD.SetActive(false);

        if(DevMode)
        {
            boss.transform.GetComponent<BossManager>().SetFollowBehaviour(false);
        }

        StartCoroutine(PhaseIntermediaireCoroutine());
    }

    // gestion de la cinematique de changement de changement de phase
    private IEnumerator PhaseIntermediaireCoroutine()
    {
        yield return Utils.PlayAnimationsCoroutine(new Anim[] {
            new Anim(animationBoss, "bossPhase", 0.2f),
            new Anim(animationBossCam, "phaseBossCam", 0.2f)
        }); // lance les animations et les animations et attend la fin

        EndPhaseIntermediaire();
    }

    // fin de la phase intermediaire
    private void EndPhaseIntermediaire()
    {
        cinematicsHUD.HideBars(0.7f); // enlève les barres de cinématiques sur le HUD

        cameras.SetFrontCam("mainCam"); // remet la caméra joueur en priorité
        HUD.SetActive(true); // réactive le HUD

        // démarre le boss
        boss.transform.GetComponent<BossManager>().BeginPhaseIntermediaire();
        cinematicsHUD.AfficherPasser(false);

        EndCinematique(); // fin de la cinematique

        if (DevMode)
        {
            boss.transform.GetComponent<BossManager>().SetFollowBehaviour(true);
        }
    }

    // lance la cinématique de changement de phase
    private void LaunchPhase2()
    {
        isLaunched = true;
        cinematicsHUD.AfficherPasser(true);

        cinematicsHUD.ShowBars(150, 0.7f); // affiche les barres de cinématiques sur le HUD

        cameras.SetFrontCam("cameraBoss");

        HUD.SetActive(false);

        if (DevMode)
        {
            boss.transform.GetComponent<BossManager>().SetFollowBehaviour(false);
        }

        StartCoroutine(PhaseFinaleCoroutine());
    }

    // gestion de la cinematique de changement de changement de phase
    private IEnumerator PhaseFinaleCoroutine()
    {
        yield return Utils.PlayAnimationsCoroutine(new Anim[] {
            new Anim(animationBoss, "bossPhase2", 0.2f),
            new Anim(animationBossCam, "phase2BossCam", 0.2f)
        }); // lance les animations et les animations et attend la fin

        yield return Utils.PlayAnimationCoroutine(new Anim(animationWater, "mareeMontante", 0.2f));
        
        yield return Utils.PlayAnimationsCoroutine(new Anim[] {
            new Anim(animationTrepiedBoss, "trepiedBossPhase2_arrivee", 0.2f),
            new Anim(animationBossCam, "camBossPhase2_arrivee", 0.2f)
        }); // lance les animations et les animations et attend la fin

        EndPhaseFinale();
    }

    // fin de la phase finale
    private void EndPhaseFinale()
    {
        waterBox.transform.localPosition = new Vector3(waterBox.transform.position.x, -50, waterBox.transform.position.z);// dans un monde parfait n'est pas là mais dans un gamemanager
        
        cinematicsHUD.HideBars(0.7f); // enlève les barres de cinématiques sur le HUD

        cameras.SetFrontCam("mainCam"); // remet la caméra phase 2 en priorité
        HUD.SetActive(true); // réactive le HUD

        //Debut de la phase 2
        boss.transform.GetComponent<BossManager>().BeginPhase2();
        cinematicsHUD.AfficherPasser(false);

        EndCinematique(); // fin de la cinematique

        if (DevMode)
        {
            boss.transform.GetComponent<BossManager>().SetFollowBehaviour(false);
        }
    }

    // lance la cinématique de changement de phase
    private void LaunchFin()
    {
        isLaunched = true;

        cinematicsHUD.ShowBars(150, 0.7f); // affiche les barres de cinématiques sur le HUD

        HUD.SetActive(false);
        
        if (DevMode)
        {
            boss.transform.GetComponent<BossManager>().SetFollowBehaviour(false);
        }

        StartCoroutine(FinCoroutine());
    }

    // gestion de la cinematique de fin
    private IEnumerator FinCoroutine()
    {
        cameras.SetFrontCam("camPhase2");

        yield return new WaitForSeconds(0.2f); // légère tempo

        boss.transform.GetComponent<BossAnimation>().Kill(); // tue le boss

        yield return new WaitForSeconds(2f); // légère tempo

        EndFin();
    }

    // fin de la fin (lol)
    private void EndFin()
    {
        // texte de victoire (temporaire)
        cinematicsHUD.ShowText("Bravo, vous avez tué Poisséidon !\nAppuyez sur start pour recommecer", 1.0f);

        // déclenche la fin du boss
        boss.transform.GetComponent<BossManager>().BeginFin();

        EndCinematique(); // fin de la cinematique
    }

    override
    protected void HandleInfos(BasicMessage message)
    {
        switch(message.type)
        {
            case type_message.cinematic:
                PlayCinematic(((Message<type_cinematic>)message).infos);
                break;
        }
    }
}
