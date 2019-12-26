using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleKill : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled) // si la collision n'a pas lieu d'être
            return;

        if (collision.transform.CompareTag("Hovercraft")) // si collision avec un hovercraft, on le tue
        {
            collision.transform.GetComponent<Player>().Kill();
        }
    }
}
