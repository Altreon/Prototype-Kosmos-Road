using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationSmog : MonoBehaviour
{
	public float maxDistanceWithTrident = 3.7f; // la distance entre les emetteur et le centre du trident
	public float minDistanceWithTrident = 1.5f; // la distance entre les emetteur et le centre du trident
	
    private Transform leftEmit;
	private Transform rightEmit;
	
	private ParticleSystem leftParticle;
	private ParticleSystem rightParticle;
	
	private Trident trident;
	
    void Start()
    {
        leftEmit = transform.GetChild(0);
		rightEmit = transform.GetChild(1);
		
		leftParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
		rightParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
		
		trident = transform.parent.GetComponent<Trident>();
    }

    public void UpdatePos(Vector3 lookDirection)
    {
		
		//on tourne vers la même direction que le joueur
        Vector3 projection = Vector3.ProjectOnPlane(lookDirection, transform.up);
		transform.LookAt(transform.position + projection.normalized, transform.up);
		
		//On calcule une distance adéquate du trident;
		float angleWithTrident = Vector3.Angle(transform.forward, transform.parent.up);
		if(angleWithTrident > 90){
				angleWithTrident = 180 - angleWithTrident;
		}
		float distance = Mathf.Clamp((angleWithTrident / 90) * maxDistanceWithTrident, minDistanceWithTrident, maxDistanceWithTrident);
		
		//On écarte les émetteur en fonction de l'orientation du trident
		leftEmit.position = Vector3.MoveTowards(leftEmit.position, transform.position + transform.right * distance, 1);
		rightEmit.position = Vector3.MoveTowards(rightEmit.position, transform.position - transform.right * distance, 1);
    }
	
	public bool CanEmit () {
		return trident.IsTridentActive();
	}
	
	public void StartEmit() {
		leftParticle.Play();
		rightParticle.Play();
	}
	
	public void StopEmit() {
		leftParticle.Stop();
		rightParticle.Stop();
	}
}
