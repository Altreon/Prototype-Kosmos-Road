using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimation : MonoBehaviour
{
	private Animator animator;
    private Animation animations;
	
    // Start is called before the first frame update
    void Start()
    {
        animator = transform.Find("Model").GetComponent<Animator>();
        animations = transform.GetComponent<Animation>();
    }

    public void Hit () {
		animator.Play("Hit");
	}
	
	public void Stun () {
		animator.Play("Stun");
	}
	
	public void Run () {
		animator.Play("Run");
	}
	
	public void StopRun () {
		animator.Play("Idle");
	}
	
	public void Attack () {
		animator.Play("Attack");
	}

    public void Kill()
    {
        animator.Play("Dead");
    }

    public Animation GetAnimations()
    {
        return animations;
    }
}
