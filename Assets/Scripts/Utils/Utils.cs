using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static bool CompareLayer(GameObject obj, string layer)
    {
        return (obj.layer == LayerMask.NameToLayer(layer));
    }

    static public int ConvertSpeedToKmph(double speedMeterPerSecond)
    {
        return (int)(speedMeterPerSecond * 3.6);
    }

    static public int ConvertSpeedToMph(double speedMeterPerSecond)
    {
        return (int)(speedMeterPerSecond * 2.237);
    }
	
	static public float ConvertKmphToSpeed(float kmph)
    {
        return kmph / 3.6f;
    }

    static public float ConvertMphToSpeed(float mph)
    {
        return mph / 2.237f;
    }

    static public float ForceInterpolationThrow(float distance)
    {
        if (distance <= 400)
        {
            return (float) ((400 - distance) / 400 + 1.1 * (distance / 400));
        }
        else if (distance <= 800)
        {
            return (float)( 1.1 * ((800 - distance) / 400) + 1.3 * ((distance - 400) / 400));
        }
        else
        {
            return (float)(1.5 * ((3000 - distance) / 2200) + 2.5 * ((distance - 800) / 2200));
        }
    }

    public static bool PlayAnimation(Animation anim, string animationName, float speed = 1.0f)
    {
        if(!anim.IsPlaying(animationName))
        {
            anim[animationName].speed = speed;
            anim.Play(animationName);
            return true;
        }
        return false;
    }

    public static IEnumerator PlayAnimationCoroutine(Anim animation)
    {
        if (!animation.IsPlaying()) // si l'animation ne joue pas déjà
        {
            animation.Play(); // lance l'animation
            yield return new WaitWhile(() => animation.IsPlaying()); // attend qu'elle soit finie
        }
        else // si elle joue déjà
        {
            yield return new WaitWhile(() => animation.IsPlaying()); // attend qu'elle soit finie
            yield return PlayAnimationCoroutine(animation); // on la joue
        }
    }

    public static IEnumerator PlayAnimationsCoroutine(Anim[] animations)
    {
        foreach (Anim animation in animations)
            animation.Play(); // lance l'animation

        yield return new WaitWhile(() => {
            foreach (Anim animation in animations)
            {
                if (animation.IsPlaying()) // si au moins UNE animation n'est pas terminée, on continue
                    return true;
            }
            return false;
        }); // attend qu'elle soit finie
    }
}

public struct Anim
{
    public Animation AnimObj { get; set; }
    public string AnimationName { get; set; }
    public float Speed { get; set; }

    public Anim(Animation anim, string animationName, float speed = 1.0f)
    {
        AnimObj = anim;
        AnimationName = animationName;
        Speed = speed;
    }

    public void Play()
    {
        AnimObj[AnimationName].speed = Speed; // on définit la vitesse de l'animation
        AnimObj.Play(AnimationName); // on la joue
    }

    public bool IsPlaying()
    {
        return AnimObj.IsPlaying(AnimationName);
    }
}