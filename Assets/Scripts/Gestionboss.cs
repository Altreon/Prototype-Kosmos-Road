using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gestionboss : Observee
{
    public Transform spawnPhase2Player;

    private int degats; // pv totaux du boss

    private AudioSource audioSource;
    private AudioClip clipMort;
    private AudioClip clipHit;

    public Player player;

    public BossFollowPlayerBehaviour scriptBossFollowPlayerBehaviour;
    public ThrowingTridents scriptThrowingTridents;

    // Start is called before the first frame update
    void Start()
    {
        scriptBossFollowPlayerBehaviour = transform.GetComponent<BossFollowPlayerBehaviour>();
        scriptThrowingTridents = transform.GetComponent<ThrowingTridents>();

        audioSource = this.GetComponent<AudioSource>();
        clipMort = Resources.Load<AudioClip>("Sound/Poseidon/Mort");
        clipHit = Resources.Load<AudioClip>("Sound/Poseidon/Hit");
        degats = 0;
    }

    // démarre ou stop les sripts de suivi et de lancer de tridents du boss
    public void StartStopBoss(bool state)
    {
        scriptBossFollowPlayerBehaviour.enabled = state;
        scriptThrowingTridents.enabled = state;
    }

    public void HitBoss(int damage = 1)
    {
        Debug.Log("Il y a un hit avec le boss");
        degats += damage;
        // Son boss hit
        audioSource.PlayOneShot(clipHit);

        if (degats == 3) // si boss doit mourrir
        {
            Debug.Log("Le boss meurt car il a " + degats + "dégats");
            audioSource.PlayOneShot(clipMort); // son mort du boss
            NotifyObserver(new Message<CinematicManager.type_cinematic>(type_message.cinematic, CinematicManager.type_cinematic.cinematic_fin)); // notifie le gestionnaire de cinématique qu'il doit lancer la cinématique de fin
        }
        else if (degats == 1) // phase 2 (même phase que la première) + source d'eau ou difficulté augmentée éventuelle
        {
            Debug.Log("Passe à la phase 2 car " + degats + "dégats");
            StartPhase2();
            NotifyObserver(new Message<CinematicManager.type_cinematic>(type_message.cinematic, CinematicManager.type_cinematic.cinematic_intermediaire)); // notifie le gestionnaire de cinématique qu'il doit lancer la cinématique de changement de phase
        }
        else if (degats == 2) // phase 3 (phase de course vue de devant)
        {
            Debug.Log("Passe à la phase 3 car " + degats + "dégats");
            player.SetSpawnPoint(spawnPhase2Player);
            player.Respawn();
            NotifyObserver(new Message<CinematicManager.type_cinematic>(type_message.cinematic, CinematicManager.type_cinematic.cinematic_phase_finale));
        }
    }

    private void StartPhase2()
    {
        // Envoi le trident sur le joueur
        scriptThrowingTridents.LengthTridentPlayer = 0;
        // Met le nombre de trident envoyer à 1
        scriptThrowingTridents.TridentsNumbers = 1;
        // On baisse le radius de la zone vidé
        scriptThrowingTridents.ShootingRadius = 10;
        // On réduit la vitesse du trident
        scriptThrowingTridents.ShootPower = 150;
    }
}
