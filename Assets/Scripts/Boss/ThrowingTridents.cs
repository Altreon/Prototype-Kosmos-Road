using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingTridents : MonoBehaviour
{
    // Public parameters
    public GameObject Trident;
    public GameObject Hovercraft;
	public Transform shootPos;
	public bool debugShoot = true;
    public int TridentsNumbers = 10; // Numbers of tridents per shoot
    public float ShotsPerSecound = 0.2f; // Numbers of shoot every secounds
    public float ShootingRadius = 30f; // Radius shooting
    public float LengthTridentPlayer = 70f; // Distance of the shooting zone 
    public float ShootPower = 200f; // Strength magnitude

    // Internal variables
    private List<GameObject> tridents;
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
        tridents = new List<GameObject>();
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

			// Projectile transformation
			projectile.GetComponent<Rigidbody>().MoveRotation(Quaternion.LookRotation(transform.forward, transform.up));
			projectile.transform.position = shootPos.position + _PlayerDirection.normalized;
			
			// Compute the Hovercraft next position
			var distance = Vector3.Distance(Hovercraft.transform.position, shootPos.position);
			var velocity = Hovercraft.GetComponent<Rigidbody>().velocity;
			float t = distance / (ShootPower + velocity.magnitude);
			float ratioDist = Mathf.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.ProjectOnPlane(Hovercraft.transform.forward, Vector3.up), Vector3.up));
			ratioDist = 1.25f - ratioDist/180;
			var futurPos = (Hovercraft.transform.position + (Hovercraft.transform.forward * LengthTridentPlayer * ratioDist))  + velocity * t;

			//Project nextPosition to Terrain
			RaycastHit hitGround;
			if (!Physics.Raycast(shootPos.position, futurPos - shootPos.position, out hitGround, 1000, LayerMask.GetMask("Terrain"))){
				//Si on trouve pas de terrain, on annule l'envoi et on recommencera à la prochaine frame
				return;
			}
			futurPos = hitGround.point;
			Vector3 right = Vector3.Cross(hitGround.normal, Vector3.up).normalized;
			Vector3 forward = Quaternion.AngleAxis(90, right) * hitGround.normal;
			
			// Create a zone of shoot
			float radius = Random.Range(-ShootingRadius, ShootingRadius);
			float angle = Random.Range(0, 2 * Mathf.PI);
			futurPos += right * radius * Mathf.Cos(angle);
			futurPos += forward * radius * Mathf.Sin(angle);
			
			// Compute the strenght to applied
			_PlayerDirection = (futurPos - shootPos.position).normalized;
			var force = (_PlayerDirection * ShootPower) - velocity;
			
			// Trident rotation
			projectile.transform.LookAt(futurPos);
			
			projectile.GetComponent<Trident>().LethalLaunch(_PlayerDirection, Vector3.zero, force.magnitude, 0, false);

            // Add tridents to the list
            tridents.Add(projectile);

			if (_NumTridentsThrow == 0)
            {
                // Launch sound associated
                _audioSource.PlayOneShot(_clip, _audioSource.volume * 0.2f);
            }

            _NumTridentsThrow++;

            if (_NumTridentsThrow == TridentsNumbers)
            {
                _NumTridentsThrow = 0;
            }
        }
    }
	
	void DebugShoot () {
		// Compute the Hovercraft next position
		var distance = Vector3.Distance(Hovercraft.transform.position, shootPos.position);
		var velocity = Hovercraft.GetComponent<Rigidbody>().velocity;
		float t = distance / (ShootPower + velocity.magnitude);
		float ratioDist = Mathf.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.ProjectOnPlane(Hovercraft.transform.forward, Vector3.up), Vector3.up));
		ratioDist = 1.25f - ratioDist/180;
		var futurPos = (Hovercraft.transform.position + (Hovercraft.transform.forward * LengthTridentPlayer * ratioDist))  + velocity * t;

		//Project nextPosition to Terrain
		RaycastHit hitGround;
		if (!Physics.Raycast(shootPos.position, futurPos - shootPos.position, out hitGround, 1000, LayerMask.GetMask("Terrain"))){
			//Si on trouve pas de terrain, on annule l'envoi et on recommencera à la prochaine frame
			return;
		}
		
		futurPos = hitGround.point;
		Vector3 right = Vector3.Cross(hitGround.normal, Vector3.up).normalized;
		Vector3 forward = Quaternion.AngleAxis(90, right) * hitGround.normal;
		
		Debug.DrawRay(shootPos.position, futurPos - shootPos.position, Color.red);
		Debug.DrawRay(futurPos - right * ShootingRadius, right * ShootingRadius * 2, Color.blue);
		Debug.DrawRay(futurPos - forward * ShootingRadius, forward * ShootingRadius * 2, Color.blue);
	}

    public void DeleteTridents()
    {
        foreach(var trident in tridents)
        {
            Destroy(trident);
        }
        tridents.Clear();
    }
}
