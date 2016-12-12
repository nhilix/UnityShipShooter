using UnityEngine;
using System.Collections;

public class ModularEngine : ModularShipPart {

	public int maxThrust;
	[Range(0,50)]
	public float maxGimbal;
	public enum ThrusterStateEnum
	{
		bIsAftThruster,
		bIsPortThruster,
		bIsStarboardThruster,
		bIsForeThruster,
	}
	public ThrusterStateEnum thrusterState;
	public bool applyForceThisUpdate;
	private int currThrust;
	private float currGimbal;
	private SpriteRenderer[] spriteRenderers;

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody2D> ().mass = mass;

		currThrust = maxThrust;
		float zAngle = transform.localRotation.eulerAngles.z;
		if (15f <= zAngle && zAngle < 120f) {
			thrusterState  = ThrusterStateEnum.bIsPortThruster;
		}
		if (120f <= zAngle && zAngle < 240f) {
			thrusterState = ThrusterStateEnum.bIsForeThruster;
		}
		if (240f <= zAngle && zAngle < 345f) {
			thrusterState = ThrusterStateEnum.bIsStarboardThruster;
		}
		if ( ( 0 <= zAngle && zAngle < 15f ) || ( 345f <= zAngle && zAngle < 360f ) ) {
			thrusterState = ThrusterStateEnum.bIsAftThruster;
		}

		spriteRenderers = GetComponentsInChildren<SpriteRenderer> ();

	}
	
	// Update is called once per frame
	void Update () {
		if ( GetComponentInParent<CockpitControl> ().bHasPlayer ) {
			foreach (SpriteRenderer sRenderer in spriteRenderers) {
				if ( sRenderer != GetComponent<SpriteRenderer> () ) {
					Shrinker shrinker = sRenderer.gameObject.GetComponent<Shrinker> ();
					// logic for rear thruster flames
					if ( thrusterState == ThrusterStateEnum.bIsAftThruster ) {
						if (Input.GetKey ("w")) {
							sRenderer.enabled = true;
							shrinker.resetScale();
							shrinker.enabled = false;
						}
						if (Input.GetKeyUp ("w")){
							shrinker.enabled = true;
						}
					}
					// logic for front thruster flames
					if ( thrusterState == ThrusterStateEnum.bIsForeThruster ) {
						if (Input.GetKey ("s")) {
							sRenderer.enabled = true;
							shrinker.resetScale();
							shrinker.enabled = false;
						}
						if (Input.GetKeyUp ("s")){
							shrinker.enabled = true;
						}
					}
					// logic for right( Port ) thruster flames
					if ( thrusterState == ThrusterStateEnum.bIsPortThruster ) {
						if (transform.localPosition.y > 0 ) {
							if (Input.GetKey ("a")) {
								sRenderer.enabled = true;
								shrinker.resetScale();
								shrinker.enabled = false;
							}
							if (Input.GetKeyUp ("a")){
								shrinker.enabled = true;
							}
						} else {
							if (Input.GetKey ("d")) {
								sRenderer.enabled = true;
								shrinker.resetScale ();
								shrinker.enabled = false;
							}
							if (Input.GetKeyUp ("d")){
								shrinker.enabled = true;
							}
						}
						if (Input.GetKey ("q")) {
							sRenderer.enabled = true;
							shrinker.resetScale();
							shrinker.enabled = false;
						}
						if (Input.GetKeyUp ("q")){
							shrinker.enabled = true;
						}
					}
					// logic for left ( Starboard ) thruster flames
					if ( thrusterState == ThrusterStateEnum.bIsStarboardThruster ) {
						if (transform.localPosition.y > 0 ) {
							if (Input.GetKey ("d")) {
								sRenderer.enabled = true;
								shrinker.resetScale();
								shrinker.enabled = false;
							}
							if (Input.GetKeyUp ("d")){
								shrinker.enabled = true;
							}
						} else {
							if (Input.GetKey ("a")) {
								sRenderer.enabled = true;
								shrinker.resetScale();
								shrinker.enabled = false;
							}
							if (Input.GetKeyUp ("a")){
								shrinker.enabled = true;
							}
						}
						if (Input.GetKey ("e")) {
							sRenderer.enabled = true;
							shrinker.resetScale();
							shrinker.enabled = false;
						}
						if (Input.GetKeyUp ("e")){
							shrinker.enabled = true;
						}
					}
				}
			}
		}
		if (Input.GetKeyDown ("a")) {
			currGimbal += maxGimbal; // degrees, because Quaternion.Euler( Vector3 rotation ) is in degrees 
		}
		if (Input.GetKeyUp ("a")) {
			currGimbal -= maxGimbal;
		}
		if (Input.GetKeyDown ("d")) {
			currGimbal -= maxGimbal;
		}
		if (Input.GetKeyUp ("d")) {
			currGimbal += maxGimbal;
		}
		currThrust = (int) (maxThrust * GetComponentInParent<CockpitControl> ().getThrottlePercent () * Mathf.Sign ( currThrust ) );
	}

	public int getThrust() {
		return currThrust;
	}
	
	public float getGimbal() {
		return currGimbal;
	}
}
