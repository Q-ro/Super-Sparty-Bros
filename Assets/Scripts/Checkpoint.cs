using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {


	public bool activated = false;

	Animator _animator;

	void Awake()
	{
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if ((other.tag == "Player" ) && (!activated) && (other.gameObject.GetComponent<CharacterController2D>().playerCanMove))
		{
			// mark as active so doesn't get activated multiple times
			activated=true;

			// Trigger the active animation
			_animator.SetBool ("Activated",activated);

			GameManager.Instance.SetCheckpoint(this.transform.position);

		}
	}
}
