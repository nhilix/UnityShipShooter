using UnityEngine;
using System.Collections;

public class ModularWeapon : ModularShipPart {

	public int powerReqForFire;
	public int weaponPoints;
    public int weaponSize;
    public int missilesFired;

	public GameObject bulletPreFab;

	// Use this for initialization
	void Start () {
      if (weaponSize == 0 || weaponSize == null)
      {
         weaponSize = 1;
      }
		GetComponent<Rigidbody2D> ().mass = mass;
      Physics2D.IgnoreCollision(GetComponentInParent<Collider2D>(), GetComponent<Collider2D>());
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Fire ();
		}
	}

	void Fire() {
		foreach (Transform shotSpawn in GetComponentInChildren<Transform> ()) {
			if (shotSpawn.gameObject.tag == "ShotSpawn") {
				GameObject shot = (GameObject)Instantiate( bulletPreFab, shotSpawn.transform.position, shotSpawn.transform.rotation );
				string dmgType = shot.GetComponent<Projectile> ().dmgType;
				if ( dmgType == "MissileExplosive" ) {
                    missilesFired += 1;
					shot.GetComponent<MissileControl> ().target = GetComponentInParent<CockpitControl> ().target;
					shot.GetComponent<Rigidbody2D> ().velocity = GetComponentInParent<CockpitControl> ().getShipDirection() * 6f
                                                                                       + GetComponentInParent<Rigidbody2D> ().velocity;
                    shot.transform.localScale *= weaponSize;
                    shot.GetComponent<Rigidbody2D>().mass *= weaponSize;
                    shot.GetComponent<Projectile>().dmg *= weaponSize;
                    shot.GetComponent<Rigidbody2D>().velocity = shot.GetComponent<Rigidbody2D>().velocity / weaponSize;
				} else {
                   Vector2 dir = new Vector2 ( -Mathf.Sin ( ( transform.rotation.eulerAngles.z / 180f ) * Mathf.PI ),
                                                                Mathf.Cos ( ( transform.rotation.eulerAngles.z / 180f ) * Mathf.PI ) ); 
                   shot.GetComponent<Rigidbody2D> ().velocity = dir * 10f;
                   shot.transform.localScale *= weaponSize;
                   shot.GetComponent<Rigidbody2D>().mass *= weaponSize;
                   shot.GetComponent<Projectile>().dmg *= weaponSize;
				}
            shot.GetComponent<Projectile>().whoFiredMe = GetComponentInParent<CockpitControl> ().gameObject;
			}
		}
	}

   public virtual void OnCollisionEnter2D(Collision2D collision)
   {
      /*
      Rigidbody2D m2 = collision.gameObject.GetComponent<Rigidbody2D>();
      Rigidbody2D m1 = GetComponentInParent<CockpitControl>().gameObject.GetComponent<Rigidbody2D>();

      float phi = m1.transform.rotation.z - m2.transform.rotation.z;
      float theta = Mathf.Atan(                                ( m2.mass * m2.velocity.magnitude * Mathf.Sin(phi) )                        /
      //                                    ---------------------------------------------------------------------------------------------                                 
                                             ( m1.mass*m1.velocity.magnitude + m2.mass * m2.velocity.magnitude * Mathf.Cos(phi) ) );

      float u = m2.mass * m2.velocity.magnitude * Mathf.Sin(phi) /
      //           -----------------------------------------------------
                       ( ( m1.mass + m2.mass ) * Mathf.Sin( theta ) );
      Debug.Log(theta);
      m1.velocity = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * u;
      m2.velocity = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * u;

      float C = 1f;
      //                                             Momentum 2 body inellastic collision
    
      Vector3 v1 = ( C * m2.mass * ( m2.velocity - m1.velocity ) + m1.mass * m1.velocity + m2.mass * m2.velocity ) /  
      //                  --------------------------------------------------------------------------------------------------
                                                                             ( m1.mass + m2.mass );

      Vector3 v2 = ( C * m1.mass * ( m1.velocity - m2.velocity ) + m1.mass * m1.velocity + m2.mass * m2.velocity ) /  
      //                  --------------------------------------------------------------------------------------------------
                                                                             ( m1.mass + m2.mass );

      m1.velocity = v1;
      m2.velocity = v2;
      */
   }
}
