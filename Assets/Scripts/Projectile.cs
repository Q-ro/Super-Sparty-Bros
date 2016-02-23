using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	[Tooltip("How long should we wait before being destroying the object on cllision with another object")]
	public float collisionTimeToDestroy; //How long should we wait before being destroying the object on cllision with another object
	[Range(0.0f, 10.0f)]
	[Tooltip("How long will the object persist in the screen before being destroyed")]
	public float lifeSpan = 6.0f; //How long will the object persist in the screen before being destroyed
	[Range(1, 20)]
	public int damage = 1;


	// A list of possible types the projectyles can be
	//piercing does damange, exploding explodes on contact, flaming can set things on fire
	public enum ProjectileType
	{
		Piercing, Exploding, Flaming
	}

	[Tooltip("The type of projectile this is, piercing does damange, exploding explodes on contact, flaming can set things on fire")]
	public ProjectileType projectileType;

	Animator _animator; //A reference to the gameobject's animator
	bool _isActive = true; //A falg to make ruse the object doesn't collide twice

	// Use this for initialization
	void Awake () {
		_animator = this.GetComponent<Animator> ();
		StartCoroutine (DestroyObjectOnTimeOut());
	}

	// Update is called once per frame
	void OnTriggerEnter2D (Collider2D other) 
	{
		if (_isActive) {
			
			_isActive = false;

			// If the projectile collided with the player, deal damage.
			if (other.tag == "Player") {

				//Slow down time
				GraphicHelper.Instance.Slowmo ();

				other.GetComponent<CharacterController2D> ().ApplyDamage (damage);
			}

			// Play the "Destroy" animation if an animator is present
			if (this.GetComponent<Animator> () != null) {
				_animator.SetTrigger ("Destroy");
				StartCoroutine (DestroyObject ());
			}
			//Else just destroy the object
			else 
			{
				Destroy (gameObject);
			}
		}
	}

	// Destroys the game object after a short delay allowing the "destroy animation" to play
	IEnumerator DestroyObject()
	{
		yield return new WaitForSeconds (collisionTimeToDestroy);
		Destroy (gameObject);
	}

	// Destoys the game object once it reaches it's max "life span"
	IEnumerator DestroyObjectOnTimeOut()
	{
		yield return new WaitForSeconds (lifeSpan);
		Destroy (gameObject);
	}


}
