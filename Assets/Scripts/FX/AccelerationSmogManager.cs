using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationSmogManager : MonoBehaviour
{
	public float coneAngle = 45f; // l'angle du cone de detection des effect
	
	//La liste des effect dont on doit afficher l'effet
	private List<AccelerationSmog> effects;
	
    // Start is called before the first frame update
    void Start()
    {
		effects = new List<AccelerationSmog>();
    }

    // Update is called once per frame
    void Update()
    {
		List<AccelerationSmog> deleteEffects = new List<AccelerationSmog>();
		
		//On update la position de touts les effets AccelerationSmog des effects de la liste
        foreach(AccelerationSmog effect in effects){
			//Si inactif ou plus dans le cone, on l'enlève de la liste
			if(!effect.CanEmit() || Vector3.Angle(transform.forward, effect.transform.position - transform.position) > coneAngle){
				deleteEffects.Add(effect);
			}else{
				effect.UpdatePos(transform.forward);
			}
		}
		
		foreach(AccelerationSmog effect in deleteEffects){
			removeEffect(effect);
		}
    }
	
    private void OnTriggerStay(Collider other)
    {
		//On vérifie que c'est un trident
		if(Utils.CompareLayer(other.gameObject, "Trident")) {
			AccelerationSmog effect = other.transform.Find("AccelerationSmog").GetComponent<AccelerationSmog>();
			
			//Puis qu'il n'est pas déjà dans la liste et et qu'il est dans le cone de detection
			if(!effects.Contains(effect) && Vector3.Angle(transform.forward, other.transform.position - transform.position) <= coneAngle) {
				addEffect(effect);
			}
		}
	}
	
	private void OnTriggerExit(Collider other)
    {
		//On vérifie que c'est un trident
		if(Utils.CompareLayer(other.gameObject, "Trident")) {
			AccelerationSmog effect = other.transform.Find("AccelerationSmog").GetComponent<AccelerationSmog>();
			
			//Puis s'il est dans la liste on le supprime
			if(effects.Contains(effect)) {
				removeEffect(effect);
			}
		}
	}
	
	private void addEffect (AccelerationSmog effect) {
		effect.StartEmit();
		effects.Add(effect);
	}
	
	private void removeEffect (AccelerationSmog effect) {
		effect.StopEmit();
		effects.Remove(effect);
	}
}
