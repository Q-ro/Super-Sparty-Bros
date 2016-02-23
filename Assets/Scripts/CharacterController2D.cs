using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class CharacterController2D : MonoBehaviour {

	// player controls
	[Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
	public float moveSpeed = 3f;

	[Range(0.0f, 20.0f)] // create a slider in the editor and set limits on moveSpeed
	public float runSpeed = 5f;

	public float jumpForce = 600f;

	// player health
	public int playerHealth = 1;

	// LayerMask to determine what is considered ground for the player
	public LayerMask whatIsGround;

	// Transform just below feet for checking if player is grounded
	public Transform groundCheck;

	// can player move?
	// we want this public so other scripts can access it but we don't want to show in editor as it might confuse designer
	[HideInInspector]
	public bool playerCanMove = true;
	// Is the player invisible ?
	[HideInInspector]
	public bool playerIsInvinsible = false;

    //Attack damage of the player
    [Range(0.0f,10.0f)]
    public int attackDamage = 1;
    //Attack speed of the player
    [Range(0.0f,2.0f)]
    public float attackSpeed = 0.2f;

    //public int health;
    //int _maxHealth;

	// SFXs
	public AudioClip coinSFX;
	public AudioClip deathSFX;
	public AudioClip fallSFX;
	public AudioClip jumpSFX;
	public AudioClip victorySFX;
	public AudioClip swordAttackSFX;
	public AudioClip swordClinkSFX;

	public CamShakeSimple cam;

	// private variables below

	// store references to components on the gameObject
	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;
	CircleCollider2D _characterCollider;

	// hold player motion in this timestep
	float _vx;
	float _vy;

	// player tracking
	bool _facingRight = true;
	bool _isGrounded = false;
	bool _isRunning = false;
	bool _isAttacking= false;
	bool _canDoubleJump = false;

	// store the layer the player is on (setup in Awake)
	int _playerLayer;

	// number of layer that Platforms are on (setup in Awake)
	int _platformLayer;
	
	void Awake () {
		// get a reference to the components we are going to be changing and store a reference for efficiency purposes
		_transform = GetComponent<Transform> ();
		
		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) // if Rigidbody is missing
			Debug.LogError("Rigidbody2D component missing from this gameobject");
		
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");
		
		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}

		_characterCollider = GetComponent<CircleCollider2D> ();

		// determine the player's specified layer
		_playerLayer = this.gameObject.layer;

		// determine the platform's specified layer
		_platformLayer = LayerMask.NameToLayer("Platform");
	}

	// this is where most of the player controller magic happens each game event loop
	void Update()
	{
		// exit update if player cannot move or game is paused
		if (!playerCanMove || (Time.timeScale == 0f))
			return;

		// determine horizontal velocity change based on the horizontal input
		_vx = CrossPlatformInputManager.GetAxisRaw ("Horizontal");

		// Determine if running based on the horizontal movement
		if (_vx != 0) 
		{
			_isRunning = true;
		} else {
			_isRunning = false;
		}

		// set the running animation state
		_animator.SetBool("Running", _isRunning);

		// get the current vertical velocity from the rigidbody component
		_vy = _rigidbody.velocity.y;

		// Check to see if character is grounded by raycasting from the middle of the player
		// down to the groundCheck position and see if collected with gameobjects on the
		// whatIsGround layer
		_isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);  

		// Set the grounded animation states
		_animator.SetBool("Grounded", _isGrounded);

		// Reset Doublejump
		if (_isGrounded) 
		{
			_canDoubleJump = true;
		}

		// If grounded AND jump button pressed, then allow the player to jump
		if (_isGrounded && CrossPlatformInputManager.GetButtonDown ("Jump")) {
			_animator.SetTrigger("Jumping");

			DoJump ();
		}
		// If we can double jump, allow the player to double jump.
		else if (_canDoubleJump && CrossPlatformInputManager.GetButtonDown ("Jump")) {
			DoJump ();
			//Disable double jump after one use.
			_canDoubleJump = false;
		}
	
		// If the player stops jumping mid jump and player is not yet falling
		// then set the vertical velocity to 0 (he will start to fall from gravity)
		if(CrossPlatformInputManager.GetButtonUp("Jump") && _vy>0f)
		{
			_vy = 0f;
		}

		// Change the actual velocity on the rigidbody
		_rigidbody.velocity = new Vector2(_vx * moveSpeed, _vy);

		if (CrossPlatformInputManager.GetButtonDown ("Attack") && !_isAttacking) 
		{
			//If the player is grounded and pressing the attack button do a grounded attack
			if (_isGrounded) {
				_isAttacking = true;
				playerIsInvinsible = true;
				StartCoroutine (PlayerGroundedAttack ());
			} 
			else 
			{
				//If the player is in the air and pressing the attack button and the down button do a ground pound attack
				if (CrossPlatformInputManager.GetAxisRaw("Vertical") < 0)
				{
					StartCoroutine (PlayerGroundpoundAttack ());
				}
				//If the player is in the air and pressing the attack button do an air attack
				else 
				{
					StartCoroutine (PlayerAirAttack ());
				}
			}
		}

		// if moving up then don't collide with platform layer
		// this allows the player to jump up through things on the platform layer
		// NOTE: requires the platforms to be on a layer named "Platform"
		Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_vy > 0.0f)); 
	}

	// Checking to see if the sprite should be flipped
	// this is done in LateUpdate since the Animator may override the localScale
	// this code will flip the player even if the animator is controlling scale
	void LateUpdate()
	{
		// get the current scale
		Vector3 localScale = _transform.localScale;

		if (_vx > 0) // moving right so face right
		{
			_facingRight = true;
		} else if (_vx < 0) { // moving left so face left
			_facingRight = false;
		}

		// check to see if scale x is right for the player
		// if not, multiple by -1 which is an easy way to flip a sprite
		if (((_facingRight) && (localScale.x<0)) || ((!_facingRight) && (localScale.x>0))) {
			localScale.x *= -1;
		}

		// update the scale
		_transform.localScale = localScale;
	}

	// if the player collides with a MovingPlatform, then make it a child of that platform
	// so it will go for a ride on the MovingPlatform
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag=="MovingPlatform")
		{
			this.transform.parent = other.transform;
		}
	}

	// if the player exits a collision with a moving platform, then unchild it
	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.tag=="MovingPlatform")
		{
			this.transform.parent = null;
		}
	}

	/// <summary>
	/// Makes the player Jump
	/// </summary>
	void DoJump()
	{
		// reset current vertical motion to 0 prior to jump
		_vy = 0f;
		// add a force in the up direction
		_rigidbody.AddForce (new Vector2 (0, jumpForce));
		// play the jump sound
		PlaySound(jumpSFX);
	}

	// do what needs to be done to freeze the player
 	void FreezeMotion() {
		playerCanMove = false;
		_rigidbody.isKinematic = true;
	}

	// do what needs to be done to unfreeze the player
	void UnFreezeMotion() {
		playerCanMove = true;
		_rigidbody.isKinematic = false;
	}

	// play sound through the audiosource on the gameobject
	void PlaySound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}

	// public function to apply damage to the player
	public void ApplyDamage (int damage) {
		if (playerCanMove && !playerIsInvinsible) {
			playerHealth -= damage;

			//CamShakeSimple camrea = GameObject.Find ("Main_Camera").GetComponent<Camera>().gameObject.GetComponents<CamShakeSimple>();

			cam.PlayShake ();

			if (playerHealth <= 0) { // player is now dead, so start dying
				PlaySound(deathSFX);
				StartCoroutine (KillPlayer ());
			}
		}
	}

	// public function to kill the player when they have a fall death
	public void FallDeath () {
		if (playerCanMove) {
			playerHealth = 0;
			PlaySound(fallSFX);
			StartCoroutine (KillPlayer ());
		}
	}

	IEnumerator PlayerGroundedAttack()
	{
		//Debug.Log ("ground atacking");
		_animator.SetTrigger ("GroundAttack");
		PlaySound(swordAttackSFX);
		yield return new WaitForSeconds (attackSpeed);
		_isAttacking = false;
		playerIsInvinsible = false;
	}

	IEnumerator PlayerAirAttack()
	{
		Debug.Log ("Air atacking");
		yield return new WaitForSeconds (attackSpeed);
	}

	IEnumerator PlayerGroundpoundAttack()
	{
		Debug.Log ("Air pound atacking");
		yield return new WaitForSeconds (attackSpeed);
	}

	// coroutine to kill the player
	IEnumerator KillPlayer()
	{
		if (playerCanMove)
		{
			// freeze the player
			FreezeMotion();

			// play the death animation
			_animator.SetTrigger("Death");

			// Disable the circle colider to prevent enemies from colliding with it
			_characterCollider.enabled = false;

			// After waiting tell the GameManager to reset the game
			yield return new WaitForSeconds(2.0f);

			if (GameManager.Instance) // if the gameManager is available, tell it to reset the game
				GameManager.Instance.ResetGame();
			else // otherwise, just reload the current level
				//Application.LoadLevel(Application.loadedLevelName);
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

	public void CollectCoin(int amount) {
		PlaySound(coinSFX);

		if (GameManager.Instance) // add the points through the game manager, if it is available
			GameManager.Instance.AddPoints(amount);
	}

	// public function on victory over the level
	public void Victory() {
		PlaySound(victorySFX);
		FreezeMotion ();
		_animator.SetTrigger("Victory");

		_characterCollider.enabled = false;

		if (GameManager.Instance) // do the game manager level compete stuff, if it is available
			GameManager.Instance.LevelCompete();
	}

	// public function to respawn the player at the appropriate location
	public void Respawn(Vector3 spawnloc) {
		UnFreezeMotion();
		playerHealth = 1;
		_transform.parent = null;
		_transform.position = spawnloc;
		_animator.SetTrigger("Respawn");
		_characterCollider.enabled = true;
	}

	public void EnemyBounce()
	{
		//Make the player jump
		DoJump ();

		//reset the double jump
		_canDoubleJump = true;
	}
}
