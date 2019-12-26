using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Observer : MonoBehaviour
{

    // Update is called once per frame
    public void Notify(BasicMessage message)
    {
        HandleInfos(message);
    }

    protected abstract void HandleInfos(BasicMessage message);
}
