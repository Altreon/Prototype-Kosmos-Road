using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeplacementBehaviour : MonoBehaviour
{
    private BossManager bossManager;
    private Transform pivotBoss;
    private Transform boss;

    private float initialRotationPivot; // rotation iniatiale du pivot
    private float initialBossHeight; // hauteur initiale du boss

    public Transform player;
    public float maxAngle = 20; // angle entre le boss et le joueur partant de l'origine du pivot
    public float speed = 20.0f;
    public float intervale = 4.0f;
    private Timer intervalTimer;

    public float durationHide = 1.0f; // durée de l'animation où le boss va se cacher
    public float durationShow = 1.0f; // durée de l'animation où le boss va se montrer
    private bool visible;

    private bool isMoving; // définit si le boss se déplace ou non

    public float offsetBoss = 160; // variation de hauteur dans le déplacement du boss (caché/visible)

    // Start is called before the first frame update
    void Start()
    {
        bossManager = transform.GetComponent<BossManager>();
        boss = transform;
        pivotBoss = transform.parent.parent.transform;

        initialBossHeight = -690;
        initialRotationPivot = 0;

        StopAllCoroutines();

        isMoving = false;
        visible = true;

        intervalTimer = new Timer(intervale);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            float wantedAngle = pivotBoss.eulerAngles.y;

            Vector3 bossFromPivot = boss.position - pivotBoss.position;
            Vector3 playerFromPivot = player.position - pivotBoss.position;

            float diffAngle = Vector3.Angle(bossFromPivot, playerFromPivot) - maxAngle;

            if (diffAngle > 0) // si le boss est plus loin qu'il ne le devrait du joueur
            {
                wantedAngle -= diffAngle;
            }
            else
            {
                wantedAngle -= (Time.deltaTime * speed);
            }

            Vector3 newRotation = pivotBoss.eulerAngles;
            newRotation.y = wantedAngle;

            pivotBoss.eulerAngles = newRotation;

            intervalTimer.Tick(Time.deltaTime); // écoulement du timer

            if (intervalTimer.IsFinished()) // changement caché / visible
            {
                intervalTimer.Start(intervale); // relance compteur

                if (visible)
                    HideBoss();
                else
                    ShowBoss();
            }
        }
    }

    // démarre le déplacement du boss en phase 2
    public void Init()
    {
        pivotBoss.eulerAngles = new Vector3(pivotBoss.eulerAngles.x, initialRotationPivot, pivotBoss.eulerAngles.z);
        boss.localPosition = new Vector3(boss.localPosition.x, initialBossHeight, boss.localPosition.z);

        visible = true;
        StartMoving();
    }

    // lance le déplacement du boss
    public void StartMoving()
    {
        isMoving = true;
        intervalTimer.Start(intervale);
    }

    // stoppe le déplacement du boss
    public void StopMoving()
    {
        isMoving = false;
        intervalTimer.Stop();
    }

    // fait remonter le boss sur la scène
    public void ShowBoss()
    {
        StartCoroutine(ShowBossCoroutine(durationShow));
    }

    IEnumerator ShowBossCoroutine(float time)
    {
        float desiredPosition = boss.localPosition.y + offsetBoss;

        float step = offsetBoss / time;

        while (boss.localPosition.y < desiredPosition) // animation du boss
        {
            Vector3 newPosition = boss.localPosition;
            newPosition.y += Time.deltaTime * step;
            boss.localPosition = newPosition;

            yield return null;
        }

        bossManager.SetThrowingBehaviour(true); // activation du script de lancer de trident
        visible = true;
    }

    // fait descendre le boss
    public void HideBoss()
    {
        bossManager.SetThrowingBehaviour(false); // désactivation du script de lancer de trident
        StartCoroutine(HideBossCoroutine(durationHide));
    }

    IEnumerator HideBossCoroutine(float time)
    {
        float desiredPosition = boss.localPosition.y - offsetBoss;

        float step = offsetBoss / time;

        while (boss.localPosition.y > desiredPosition) // animation du boss
        {
            Vector3 newPosition = boss.localPosition;
            newPosition.y -= Time.deltaTime * step;
            boss.localPosition = newPosition;

            yield return null;
        }

        visible = false;
    }
}
