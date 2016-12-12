using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CockpitControl : MonoBehaviour {

	public bool bHasPlayer;
	public int throttleSpeed;
	public GameObject target;
    public int missilesFired;
    public List<Color> pathLinesColors;

    public List<List<Vector2>> pathLinesOriginList;
    public List<List<Vector2>> pathLinesEndList;

	private Vector2 forceCorrection;
	private int throttle;


	public struct ForceToApply {
		public Vector2 thrustDir;
		public Vector2 thrustPos;
	}

	void Start () {
		throttle = 50;
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
        {
           Physics2D.IgnoreCollision(collider, GetComponent<Collider2D>());
        }
        pathLinesOriginList = new List<List<Vector2>>();
        pathLinesEndList = new List<List<Vector2>>();
        pathLinesColors = new List<Color>();
        missilesFired = 0;
	}

	// Update is called once per frame
	void FixedUpdate () {
        for (int i = 0; i < pathLinesEndList.Count; i++)
        {
            for (int j = 0; j < pathLinesEndList[i].Count; j++)
            {
                Debug.DrawLine(pathLinesOriginList[i][j], pathLinesEndList[i][j], pathLinesColors[i]);
                Vector2 targetToUse = pathLinesEndList[i][j];
                float xSize = 0.05f;
                Debug.DrawLine(targetToUse + new Vector2(xSize, 0f), targetToUse + new Vector2(-xSize, 0f), Color.yellow);
            }
        }

        forceCorrection = new Vector2(-Mathf.Sin((transform.rotation.eulerAngles.z / 180f) * Mathf.PI),
                                             Mathf.Cos((transform.rotation.eulerAngles.z / 180f) * Mathf.PI));

		if (bHasPlayer) {
			if ( Input.GetKey ( "w" ) ) {
				foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
					if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsAftThruster) {
						engine.applyForceThisUpdate = true;
					}
				}
			}
			if ( Input.GetKey ( "d" ) ) {
				foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
					if ( engine.gameObject.transform.localPosition.y > 0f ) {
						if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsStarboardThruster) {
							engine.applyForceThisUpdate = true;
						}
					} else {
						if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsPortThruster) {
							engine.applyForceThisUpdate = true;
						}
					}
				}
			}
			if ( Input.GetKey ( "a" ) ) {
				foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
					if ( engine.gameObject.transform.localPosition.y > 0f ) {
						if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsPortThruster) {
							engine.applyForceThisUpdate = true;
						}
					} else {
						if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsStarboardThruster) {
							engine.applyForceThisUpdate = true;
						}
					}
				}
			}
			if ( Input.GetKey ( "s" ) ) {
				foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
					if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsForeThruster) {
						engine.applyForceThisUpdate = true;
					}
				}
			}
			if ( Input.GetKey( "q" ) ) {
				foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
					if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsPortThruster) {
						engine.applyForceThisUpdate = true;
					}
				}
			}
			if ( Input.GetKey( "e" ) ) {
				foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
					if (engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsStarboardThruster) {
						engine.applyForceThisUpdate = true;
					}
				}
			}
			// apply engine forces for all flagged to apply this update
			foreach( ModularEngine engine in GetComponentsInChildren<ModularEngine> () ) {
				if ( engine.applyForceThisUpdate )
					applyEngineForce( engine );
				engine.applyForceThisUpdate = false;
			}
		}

	}

	void applyEngineForce( ModularEngine engine ) {
		ForceToApply newForce = new ForceToApply();
		// Quaternion.Euler will rotate our thrust direction around the z-axis by the local rotation of the engine
		// corrected for its current gimbal amount.  The vector we are rotating is the current "foward" vector of
		// the ship.  Finally we scale the strength by the current engine thrust amount
		float currGimbal = 0f;
		if ( engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsAftThruster ) {
			currGimbal = engine.getGimbal();
		}
		if ( engine.thrusterState == ModularEngine.ThrusterStateEnum.bIsForeThruster ) {
			currGimbal = - engine.getGimbal();
		}
		newForce.thrustDir = Quaternion.Euler( 0f, 0f, engine.transform.localRotation.eulerAngles.z - currGimbal ) 
							 * forceCorrection
							 * engine.getThrust();
		// The force is applied from the current position of the engine
		newForce.thrustPos = engine.transform.position;
		GetComponent<Rigidbody2D> ().AddForceAtPosition( newForce.thrustDir, newForce.thrustPos );
	}

	void Update() {
		if (Input.GetKey ("r")) {
			throttle += throttleSpeed;
		}
		if (Input.GetKey ("f")) {
			throttle -= throttleSpeed;
		}
		if (Input.GetKey ("x")) {
			throttle = 0;
		}
		throttle = Mathf.Max ( Mathf.Min (throttle, 100), 0 );
        int tmpMissilesFired = 0;
        foreach (ModularWeapon weapon in GetComponentsInChildren<ModularWeapon>())
        {
            tmpMissilesFired += weapon.missilesFired;
        }
        if (tmpMissilesFired > missilesFired) missilesFired = tmpMissilesFired;
	}

	public float getThrottlePercent() {
		return (float) throttle / 100f;
	}

	public Vector2 getShipDirection() {
		return forceCorrection;
	}
}
