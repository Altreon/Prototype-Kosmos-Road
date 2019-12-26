using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Observee : MonoBehaviour
{
    public Observer observer; // observateur de l'observé

    // Start is called before the first frame update
    protected void Init()
    {
        observer = this.GetComponent<Observer>(); // récupération de l'observateur
    }

    protected void NotifyObserver(BasicMessage message)
    {
        observer.Notify(message);
    }
}
