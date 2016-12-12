using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public int dmg;
	public string dmgType;
	public int currentLife;
	public int lifeTime;
	public GameObject explosion;
	public bool bIsDying;
   public GameObject whoFiredMe;

	// Use this for initialization
	void Start () {
		currentLife = 50 * lifeTime;
      foreach (Collider2D collider in whoFiredMe.GetComponentsInChildren<Collider2D>())
      {
         Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collider);
      }
	}
	
	// Update is called once per frame
	void Update () {
		if (currentLife <= 0 && bIsDying == false) {
			spawnHitEvent (transform.position);
			KillMeNicely ();
		} else {
			currentLife -= 1;
		}
		if (bIsDying == true) {
			transform.localScale = Vector3.Scale( transform.localScale, 0.85f * Vector3.one ); 
		}
		if (transform.localScale.x <= 0.05) {
			Destroy ( gameObject );
		}
	}

	void spawnHitEvent( Vector2 pos ) {
		Instantiate( explosion, pos, transform.rotation );
	}

	void OnCollisionStay2D( Collision2D collision ) {
		bool bDoNotDestroy = false;
		if (collision.gameObject.GetComponent<Projectile> () != null) {
			if (collision.gameObject.GetComponent<Projectile> ().dmgType == "PulseLaser" ) {
				bDoNotDestroy = true;
			}
		}
      if ( collision.gameObject.tag == "PlayerShip" && gameObject.tag != "Missile" )
      {
			bDoNotDestroy = true;
      }
		if (!bDoNotDestroy) {
			foreach (ContactPoint2D contact in collision.contacts) {
				spawnHitEvent (contact.point);
			}
			KillMeNicely ();
		}
	}

	void OnCollisionEnter2D( Collision2D collision ) {
      currentLife -= 20;
		bool bDoNotDestroy = false;
		if (collision.gameObject.GetComponent<Projectile> () != null) {
			if (collision.gameObject.GetComponent<Projectile> ().dmgType == "PulseLaser" ) {
				bDoNotDestroy = true;
			}
		}
      if ( collision.gameObject.tag == "PlayerShip" && gameObject.tag != "Missle" )
      {
			bDoNotDestroy = true;
      }
		if (!bDoNotDestroy) {
			foreach (ContactPoint2D contact in collision.contacts) {
				spawnHitEvent (contact.point);
			}
			KillMeNicely ();
		}
	}

	void KillMeNicely () {
		bIsDying = true;
		if (dmgType == "MissileExplosive") {
			Destroy( gameObject );
		}
	}
}
