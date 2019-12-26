using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// code prit du tutoriel "Unity Tutorial - Cinematic Bars" by "Code Monkey" (https://www.youtube.com/watch?v=nNbM40HFyCs)
public class CinematicFXUI : MonoBehaviour
{
    private RectTransform topbar, bottombar;
    private RectTransform bossName;

    private Image whiteScreen; // pour l'écran blanc de mort
    private Image textPasser; // passer les cinématiques

    private Text textComponent;
    private Outline outlineText;

    private float targetSizeBars, changeSizeAmountBars; // propriété pour les barres
    private float targetSizeBossName, changeSizeAmountBossName; // propriété pour le nom du boss

    private bool isActiveBars = false, isActiveBossName = false;

    private void Awake()
    {
        // créer une barre en haut du canvas
        GameObject gameObject = new GameObject("topbar", typeof(Image));
        gameObject.transform.SetParent(transform, false);
        gameObject.GetComponent<Image>().color = Color.black;
        topbar = gameObject.GetComponent<RectTransform>();
        topbar.anchorMin = new Vector2(0, 1);
        topbar.anchorMax = new Vector2(1, 1);
        topbar.sizeDelta = new Vector2(0, 0);

        // créer une barre en bas du canvas
        gameObject = new GameObject("bottombar", typeof(Image));
        gameObject.transform.SetParent(transform, false);
        gameObject.GetComponent<Image>().color = Color.black;
        bottombar = gameObject.GetComponent<RectTransform>();
        bottombar.anchorMin = new Vector2(0, 0);
        bottombar.anchorMax = new Vector2(1, 0);
        bottombar.sizeDelta = new Vector2(0, 0);

        bossName = transform.Find("BossName").GetComponent<RectTransform>();

        textComponent = transform.Find("TextHUD").GetComponent<Text>();
        outlineText = transform.Find("TextHUD").GetComponent<Outline>();
        

        // écran blanc pour la mort du joueur
        gameObject = new GameObject("whiteScreen", typeof(Image));
        gameObject.transform.SetParent(transform, false);
        whiteScreen = gameObject.GetComponent<Image>();
        whiteScreen.color = new Color(1, 1, 1, 0);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(transform.GetComponent<Canvas>().GetComponent<RectTransform>().rect.width, transform.GetComponent<Canvas>().GetComponent<RectTransform>().rect.height);

        transform.Find("Texte_passer").GetComponent<Image>().enabled = false;
        GameObject newObject = Object.Instantiate(transform.Find("Texte_passer").gameObject, transform);
        textPasser = newObject.GetComponent<Image>();
        textPasser.enabled = false;
    }

    // animation faisant apparaitre les barres
    public void ShowBars(float targetSize, float time)
    {
        this.targetSizeBars = targetSize;
        changeSizeAmountBars = (targetSize - topbar.sizeDelta.y) / time;
        isActiveBars = true;
    }

    // animation faisant disparaitre les barres
    public void HideBars(float time)
    {
        targetSizeBars = 0;
        changeSizeAmountBars = (targetSizeBars - topbar.sizeDelta.y) / time;
        isActiveBars = true;
    }

    public void ShowBossName(float time)
    {
        targetSizeBossName = 0;
        changeSizeAmountBossName = (targetSizeBossName - bossName.anchoredPosition.x) / time;
        isActiveBossName = true;
    }

    public void HideBossName(float time)
    {
        targetSizeBossName = bossName.sizeDelta.x;
        changeSizeAmountBossName = (targetSizeBossName - bossName.anchoredPosition.x) / time;
        isActiveBossName = true;
    }

    // affiche du texte pendant un certain temps avant de le faire disparaitre
    public void ShowText(string texte, float time, float duration = -1)
    {
        StartCoroutine(CoroutineFadeInText(texte, time, duration));
    }

    public void HideText(float time)
    {
        StartCoroutine(CoroutineFadeOutText(time));
    }


    public IEnumerator CoroutineFadeInText(string texte, float time, float duration)
    {
        textComponent.text = texte;
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0);
        outlineText.effectColor = new Color(outlineText.effectColor.r, outlineText.effectColor.g, outlineText.effectColor.b, 0);
        while (textComponent.color.a < 1.0f)
        {
            Color newColor = textComponent.color;
            newColor.a += (Time.deltaTime / time);
            textComponent.color = newColor;

            newColor = outlineText.effectColor;
            newColor.a += (Time.deltaTime / time);
            outlineText.effectColor = newColor;

            yield return null;
        }
        if(duration != -1) // si la durée est définie
        {
            yield return new WaitForSeconds(duration); // attends la durée avant de lancer la coroutine
            HideText(time); // fait disparaitre le texte
        }
    }

    public IEnumerator CoroutineFadeOutText(float time)
    {
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1);
        outlineText.effectColor = new Color(outlineText.effectColor.r, outlineText.effectColor.g, outlineText.effectColor.b, 1);
        while (textComponent.color.a > 0.0f)
        {
            Color newColor = textComponent.color;
            newColor.a -= (Time.deltaTime / time);
            textComponent.color = newColor;

            newColor = outlineText.effectColor;
            newColor.a -= (Time.deltaTime / time);
            outlineText.effectColor = newColor;

            yield return null;
        }
    }

    // affiche un écran blanc qui apparait pendant duration secondes (-1 = infini) et apparait et disparait en fade secondes
    public void ShowWhiteScreen(float duration = -1, float fade = 0.2f)
    {
        StartCoroutine(CoroutineShowWhiteScreen(duration, fade));
    }

    private IEnumerator CoroutineShowWhiteScreen(float duration, float fade)
    {
        whiteScreen.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0);
        // apparition
        while (whiteScreen.color.a < 1.0f)
        {
            Color newColor = whiteScreen.color;
            newColor.a += (Time.deltaTime / fade);
            whiteScreen.color = newColor;

            yield return null;
        }
        if (duration != -1) // si la durée est définie
        {
            yield return new WaitForSeconds(duration); // attends la durée avant de lancer la coroutine

            // disparition
            while (whiteScreen.color.a > 0.0f)
            {
                Color newColor = whiteScreen.color;
                newColor.a -= (Time.deltaTime / fade);
                whiteScreen.color = newColor;

                yield return null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveBars)
        {
            Vector2 sizeDelta = topbar.sizeDelta;
            sizeDelta.y += changeSizeAmountBars * Time.deltaTime;

            if (changeSizeAmountBars > 0)
            {
                if (sizeDelta.y >= targetSizeBars)
                {
                    sizeDelta.y = targetSizeBars;
                    isActiveBars = false;
                }
            }
            else
            {
                if (sizeDelta.y <= targetSizeBars)
                {
                    sizeDelta.y = targetSizeBars;
                    isActiveBars = false;
                }
            }

            bottombar.sizeDelta = sizeDelta;
            topbar.sizeDelta = sizeDelta;
        }

        if (isActiveBossName)
        {
            Vector2 positionDelta = bossName.anchoredPosition;
            positionDelta.x += changeSizeAmountBossName * Time.deltaTime;

            if (changeSizeAmountBossName > 0)
            {
                if (positionDelta.x >= targetSizeBossName)
                {
                    positionDelta.x = targetSizeBossName;
                    isActiveBossName = false;
                }
            }
            else
            {
                if (positionDelta.x <= targetSizeBossName)
                {
                    positionDelta.x = targetSizeBossName;
                    isActiveBossName = false;
                }
            }

            bossName.anchoredPosition = positionDelta;
        }
    }

    // affiche ou non le texte "Appuyer sur 'A' pour passer" (pour les cinématiques)
    public void AfficherPasser(bool etat)
    {
        textPasser.enabled = etat;
    }
}
