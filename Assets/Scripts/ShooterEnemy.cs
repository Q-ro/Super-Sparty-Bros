using UnityEngine;
using System.Collections;

public class ShooterEnemy : MonoBehaviour {

	public float startDelay; //How long before starting to attack
	public float attackSpeed; //How muhc time between attacks
	public float projectileSpeed; //Speed of the projectile
	public GameObject spawnLocation; //Where will the projectiles "be born"
	[Range(0.0f,5.0f)]
	public float shotJitter = 4.5f; //How much "jitter" to be aplied to each shot

	public AudioClip shootSFX;

	public enum ShootingDirection
	{
		Left, Right
		//,Up, Down
	}

	public ShootingDirection shootingDirection;
	public GameObject projectileToShot;

	float _attackTimer= 0f; //Keeps track of how much time is left before attacking once again
	float _delayTimer = 0f; //Keeps track of how much time is left before starting to attack
	Animator _animator; //A refference to this gameobject's animator

	bool _isOnCamera = false;

	void OnBecameInvisible() {
		_isOnCamera = false;
	}
	void OnBecameVisible() {
		_isOnCamera = true;
	}

	void Awake()
	{
		_animator = this.GetComponent<Animator> ();
		if (shootingDirection == ShootingDirection.Left) {
			this.transform.Rotate (0, 180, 0);
		}

	}

	// Update is called once per frame
	void Update () 
	{
		
			// If it's not time yet to start attacking, keep waiting to attack
			if (_delayTimer < startDelay) {
				_delayTimer += Time.deltaTime;
			}
		// Else start counting how much time is left for the next shot to be fired
		else {
				_attackTimer += Time.deltaTime;
			}

			//If it's time to shot and the enemy is not stunned
			if (_attackTimer >= attackSpeed && this.gameObject.layer != LayerMask.NameToLayer ("StunnedEnemy")) {
				//reset the counter so that we can keep track of when it's time to shot again
				_attackTimer = 0;


				//Start the attacking animation
				_animator.SetTrigger ("Attack");

			}
	}

	public void shootProjectile()
	{
		if (shootSFX != null && _isOnCamera) {
			this.GetComponent<Enemy> ().playSound (shootSFX);
		}
		//Instantiate the projectile and place it in the right place
		GameObject projectile = Instantiate (projectileToShot, spawnLocation.transform.position, Quaternion.identity)as GameObject;

		if (shootingDirection == ShootingDirection.Left) {
			projectile.GetComponent<Rigidbody2D> ().velocity = new Vector2 (-projectileSpeed,(Random.value*shotJitter));
			projectile.transform.Rotate(0, 180, 0);
		}
		if (shootingDirection == ShootingDirection.Right) {
			projectile.GetComponent<Rigidbody2D> ().velocity = new Vector2 (projectileSpeed,(Random.value*shotJitter));
		}

		// Not need, plus some weird bug is happening and i dont have time to figure it out at this time, gonna look into it later
		//			if (shootingDirection == ShootingDirection.Up) {
		//				projectile.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0,projectileSpeed);
		//				projectile.transform.Rotate(0, 90, 0);
		//			}
		//			if (shootingDirection == ShootingDirection.Down) {
		//				projectile.GetComponent<Rigidbody2D> ().velocity = new Vector2 (0,-projectileSpeed);
		//			}
	}
}
