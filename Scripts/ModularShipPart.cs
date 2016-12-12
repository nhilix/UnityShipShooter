using UnityEngine;
using System.Collections;

public class ModularShipPart : MonoBehaviour {


	public float generatedPower;
	public int maxPorts;
	public int maxShields;
	public int maxHullHealth;
	public int maxOxygen;
	public int maxHullStrength;
	public int maxFuel;
	public int crewCapacity;
	public int mass;

	private HealthController health;
	private ModularPort[] ports;
	private float temp;
	private float enduredPressure;
	private float currPowerDraw;
	private float currFuel;


	// Use this for initialization
	void Start () {
		ports = new ModularPort[maxPorts];

		health = (HealthController) gameObject.AddComponent<HealthController> ();
		health.currShields = maxShields;
		health.currHull = maxHullHealth;
		health.currOxygen = maxOxygen;
	}
	
	// Update is called once per frame
	void Update () {
		updateMass ( GetComponent<Rigidbody2D> () );
	}

	void updateMass ( Rigidbody2D RB ) {
		float newMass = 0;
		Vector2 massLoc = Vector2.zero;
		ModularShipPart[] parts = GetComponentsInChildren<ModularShipPart>();
		foreach (ModularShipPart part in parts) {
			newMass += part.mass;
			if ( part.tag != "PlayerShip" && part.tag != "EnemyShip" ) {
				massLoc.x += part.mass * part.transform.localPosition.x;
				massLoc.y += part.mass * part.transform.localPosition.y;
			}
		}
		RB.mass = newMass;
		RB.centerOfMass = massLoc / RB.mass; 
	}

	void makeConnections( ModularShipPart targetConnection ) {
		// function to define the ports from one modular
		// part to another ( power supply, size class, etc.)
	}
}
