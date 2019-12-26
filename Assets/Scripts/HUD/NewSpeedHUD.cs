using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewSpeedHUD : MonoBehaviour
{
    public Player player;

    private Image jauge; // image de la jauge

    private Image[] graduations; // tableau des graduations neutre (en nb de tridents)

    // les 3 graduations symbolés (mort, marteau, max)
    private Image graduationMort;
    private Image graduationMarteau;
    private Image graduationMax;

    private Text textComponentValue;

    private float treshHoldMinJauge, treshHoldMaxJauge, treshHoldJauge; // la jauge n'est pas un cercle complet, il faut donc la valeur min et max pour la remplire
    private float treshHoldGraduations, treshHoldMinGraduations, treshHoldMaxGraduations; // idem mais en rotation de rotation pour les graduations

    private float vitesseMort, vitesseMarteau, vitesseCourante, vitesseMax, vitesseMaxAbsolue;

    private float nbGraduations; // nombre de graduations sur le compteur

    // Start is called before the first frame update
    void Start()
    {
        textComponentValue = transform.Find("SpeedValue").GetComponent<Text>();
        vitesseCourante = 0;

        treshHoldMinJauge = 0.095f;
        treshHoldMaxJauge = 0.89f;
        treshHoldJauge = treshHoldMaxJauge - treshHoldMinJauge;

        treshHoldMinGraduations = 135;
        treshHoldMaxGraduations = -125;
        treshHoldGraduations = treshHoldMaxGraduations - treshHoldMinGraduations;

        jauge = transform.Find("Speedometer_jauge").GetComponent<Image>();

        graduationMort = transform.Find("graduation_mort").GetComponent<Image>();
        graduationMarteau = transform.Find("graduation_marteau").GetComponent<Image>();
        graduationMax = transform.Find("graduation_max").GetComponent<Image>();

        Setup(); // met en place le compteur (disposition des graduations, etc)
    }

    // met en place la graduation, etc
    void Setup()
    {
        jauge.fillAmount = 0f;

        vitesseMort = player.GetSeuilVitesseMort();
        vitesseMarteau = player.GetSeuilVitesseMarteau();
        vitesseMaxAbsolue = player.GetHovercraft().GetMaxVitesseKMH();

        nbGraduations = convertSpeedToLevel(vitesseMaxAbsolue);

        SetGraduation(graduationMort, convertSpeedToLevel(vitesseMort));
        SetGraduation(graduationMarteau, convertSpeedToLevel(vitesseMarteau));
        SetGraduation(graduationMax, convertSpeedToLevel(vitesseMax));

        GameObject barreGraduation = transform.Find("Speedometer_barre_courte").gameObject;
        graduations = new Image[(int)nbGraduations];

        graduations[0] = barreGraduation.GetComponent<Image>();
        SetGraduation(graduations[0], 1);

        for (int i = 2; i < nbGraduations; i++) // créer chacune des sous-graduations
        {
            GameObject newObject = Object.Instantiate(barreGraduation, transform);
            graduations[i-1] = newObject.GetComponent<Image>();
            SetGraduation(graduations[i-1], i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        vitesseMax = player.GetHovercraft().GetRelativeMaxSpeed(); // récupère la vitesse max courante
        SetGraduation(graduationMax, convertSpeedToLevel(vitesseMax), true); // met à jour la graduation de la vitesse max courante

        vitesseCourante = (float) player.GetHovercraft().GetVitesseKMH();
        SetVitesseTexte(vitesseCourante); // met à jour la vitesse textuelle
        SetJauge(vitesseCourante); // change la jauge
    }

    public void SetVitesseTexte(float speed)
    {
        textComponentValue.text = speed.ToString("000"); // met à jour la vitesse textuelle
    }

    // renvoi la couleur de la jauge en fonction de si le joueur va assez vite pour ne pas mourrir (vert) ou non (rouge)
    private Color GetColor(float speedKMH)
    {
        if (speedKMH > player.GetSeuilVitesseMarteau()) // si il va assez vite pour le marteau, en bleu
        {
            return Color.blue;
        }
        else if (speedKMH > player.GetSeuilVitesseMort()) // si il va assez vite, en vert
        {
            return Color.green;
        }
        else // sinon en rouge
        {
            return Color.red;
        }
    }

    // gestion de la jauge de vitesse
    void SetJauge(float speed)
    {
        float ratio = treshHoldMinJauge + (treshHoldJauge * (speed / vitesseMaxAbsolue)); // ratio de remplissage de la jauge

        jauge.color = GetColor(speed); // change la couleur de la jauge
        jauge.fillAmount = ratio; // change le remplissage de la jauge
    }

    // convert une vitesse en KMH en unité de vitesse bonus (trident ou autre)
    float convertSpeedToLevel(float speedKMH)
    {
        return speedKMH / (float) player.speedBonus;
    }

    float convertLevelToRotation(float level)
    {
        return treshHoldMinGraduations + (treshHoldGraduations * (level / nbGraduations));
    }

    void SetGraduation(Image graduation, float level, bool lerp = false)
    {
        Vector3 newRotation = graduation.GetComponent<RectTransform>().eulerAngles;

        if(lerp) // si on lerp l'angle
        {
            newRotation.z = Mathf.LerpAngle(newRotation.z, convertLevelToRotation(level), 3 * Time.deltaTime);
        }
        else
        {
            newRotation.z = convertLevelToRotation(level);
        }
        graduation.GetComponent<RectTransform>().eulerAngles = newRotation;
    }
}