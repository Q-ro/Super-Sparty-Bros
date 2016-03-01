using UnityEngine;
using System.Collections;

public class EnemyStun : MonoBehaviour {

	// if Player hits the stun point of the enemy, then call Stunned on the enemy
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<CharacterController2D>()._isGroundpounding)
		{

            var parent = this.GetComponentInParent<Enemy>();
			// tell the enemy to be stunned
            if (!parent._isStunned)
            {
                parent.Stunned();
                GraphicHelper.Instance.Slowmo();

                //Make the player bounce off the player
                other.gameObject.GetComponent<CharacterController2D>().EnemyBounce();
            }
		}
	}
}
