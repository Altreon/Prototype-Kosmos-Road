using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTrepiedTridents : MonoBehaviour
{
    // Public parameters
    public GameObject Trident;
	public GameObject TrepiedTrident;
	public Transform shootPos;
	public bool debugShoot = true;
    public int TridentsNumbers = 10; // Numbers of tridents per shoot
    public float ShotsPerSecound = 0.2f; // Numbers of shoot every secounds
    public float ShootingRadius = 70f; // Radius shooting
	public float distTridentFromGround = 3.5f; //distance between trident and ground for last phase

    // Internal variables
    private AudioSource _audioSource;
    private AudioClip _clip;
    private float _TimePreviousShot = 0f;
    private Vector3 _PlayerDirection;
    private int _NumTridentsThrow = 0;
	private Transform _trepiedTrident;

    private void Start()
    {
        _audioSource = this.GetComponent<AudioSource>();
        _clip = Resources.Load<AudioClip>("Sound/Poseidon/Lancer");
    }

    // Update is called once per frame
    void Update()
    {
		if(debugShoot){
			DebugShoot();
		}
		
        if ((Time.time >= _TimePreviousShot + (1 / ShotsPerSecound) && _NumTridentsThrow == 0) || _NumTridentsThrow > 0)
        {
            // Set time previous shot at current time
            _TimePreviousShot = Time.time;

            // Projectiles creation 
			GameObject projectile = Instantiate(Trident);
			Trident trident = projectile.GetComponent<Trident>();
			trident.Start();
			
            projectile.GetComponent<Rigidbody>().useGravity = false;

			
			//Randomize position
			float randX = Random.Range(-ShootingRadius, ShootingRadius);
			
			Vector3 dir = Quaternion.AngleAxis(randX, shootPos.forward) * -shootPos.up;
			
			//Set the projectile position to ground surface
			RaycastHit hitGround;
			if (!Physics.Raycast(shootPos.position, dir, out hitGround, 1000, LayerMask.GetMask("Terrain"))){
				//Si on trouve pas de terrain, on annule l'envoi et on recommencera à la prochaine frame
				return;
			}
			Vector3 normalGround = hitGround.normal;
			Rigidbody rb = projectile.GetComponent<Rigidbody>();
			
			//projectile.GetComponent<Rigidbody>().MoveRotation(Quaternion.LookRotation(transform.right, hitGround.normal));
			projectile.transform.position = hitGround.point + normalGround  * distTridentFromGround;
			
			// Trepied creation 
			if(!_trepiedTrident){
				_trepiedTrident = Instantiate(TrepiedTrident).transform;
				_trepiedTrident.position = new Vector3(0, shootPos.position.y, 0);
			}
			
			projectile.transform.parent = _trepiedTrident;
			
			Vector3 projectionOnNormal = Vector3.ProjectOnPlane(projectile.transform.forward, normalGround);
			Quaternion rotationToNormal = Quaternion.LookRotation(projectionOnNormal, normalGround);
			projectile.transform.rotation = rotationToNormal;
			projectile.transform.Rotate(0, 90, 0, Space.Self);
			
			projectile.GetComponent<Trident>().trepied = true;
			projectile.GetComponent<Trident>().InitFollow();

			if (_NumTridentsThrow == 0)
            {
                // Launch sound associated
                _audioSource.PlayOneShot(_clip, _audioSource.volume * 0.2f);
            }

            _NumTridentsThrow++;

            if (_NumTridentsThrow == TridentsNumbers)
            {
                _NumTridentsThrow = 0;
				_trepiedTrident = null;
            }
        }
    }
	
	void DebugShoot () {
		//TODO
	}
}
