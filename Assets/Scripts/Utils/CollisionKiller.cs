using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// définit une classe très simple, tuant le joueur lorsqu'il entre en collision avec
public class CollisionKiller : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) // si la collision n'a pas lieu d'être
            return;

        if (other.CompareTag("Hovercraft")) // si collision avec un hovercraft, on le tue
        {
            other.transform.parent.GetComponent<Player>().Kill();
        }
    }
}
