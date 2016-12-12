using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissileController2 : MonoBehaviour {
	
	public GameObject target;
	public int fuel;
	public float maxThrust;
	public float torqueProportionalConstant;
	public float torqueDerivativeConstant;
	public float torqueIntegralConstant;
	public float torqueVelocityProportionalConstant;
	public float torqueVelocityDerivativeConstant;
	public float torqueVelocityIntegralConstant;
	public double anglePIDConstant;
	public double velocityPIDConstant;
	public struct PID {

		public Vector2 var;
		public Vector2 firstDerivative;
		public Vector2 secondDerivative;

		public Vector2 lastVar;
		public Vector2 lastDerivative;

		public Vector2 targetVar;
		public Vector2 targetFirstDerivative;
		public Vector2 targetSecondDerivative;

		public Vector2 lastTargetVar;
		public Vector2 lastTargetDerivative;

		public Vector2 varForward;
		public Vector2 derivForward;
		public Vector2 targetForward;

		public double p;
		public double d;
		public double i;
		public double integratedOffset;

		public double p_Constant;
		public double i_Constant;
		public double d_Constant;
		
		public double weight;

		public PID( double p_constant, double i_constant, double d_constant, double pid_constant  ) {

			this.integratedOffset = 0f;

			this.var = Vector2.zero;
			this.firstDerivative = Vector2.zero;
			this.secondDerivative = Vector2.zero;

			this.lastVar = Vector2.zero;
			this.lastDerivative =  Vector2.zero;

			this.targetVar = Vector2.zero;
			this.targetFirstDerivative = Vector2.zero;
			this.targetSecondDerivative = Vector2.zero;
			
			this.lastTargetVar = Vector2.zero;
			this.lastTargetDerivative = Vector2.zero;

			this.varForward = Vector2.zero;
			this.derivForward = Vector2.zero;
			this.targetForward = Vector2.zero;

			this.p = 0f;
			this.d = 0f;
			this.i = 0f;

			this.p_Constant = p_constant;
			this.i_Constant = i_constant;
			this.d_Constant = d_constant;
			this.weight = pid_constant;
		}
	}
	private float thrust;
	private Transform[] transforms;
	private List<Transform> engines;
	private PID anglePID;
	private PID velocityPID;
	private Rigidbody2D RB;
	private Projectile projectileController;
	private float lastAngle;
	private float lastVelocityAngle;
	
	// Use this for initialization
	void Start () {
		projectileController = GetComponent<Projectile> ();
		RB = GetComponent<Rigidbody2D> ();

		anglePID = new PID( torqueProportionalConstant,
		                   						 torqueIntegralConstant,
		                   						 torqueDerivativeConstant,
		                   						 anglePIDConstant );

		velocityPID = new PID(torqueVelocityProportionalConstant,
		                      						torqueVelocityIntegralConstant,
		                      						torqueVelocityDerivativeConstant,
		                      						velocityPIDConstant );

		thrust = maxThrust;
		transforms = GetComponentsInChildren<Transform> ();
		engines = new List<Transform> ();
		foreach ( Transform child in transforms ) {
			if (child.gameObject.tag == "Flame") {
				engines.Add( child );
			}
		}
		fuel *= 50;
		lastVelocityAngle = 0f;
		lastAngle = 0f;
	}

	// Update is called once per frame
	void FixedUpdate () {
		// Some useful variables that need to update each frame
		Vector2 localForward = new Vector2 (-Mathf.Sin ((transform.rotation.eulerAngles.z / 180f) * Mathf.PI),
		                                    									   Mathf.Cos ((transform.rotation.eulerAngles.z / 180f) * Mathf.PI));
		Vector2 vectorToTarget = new Vector2 ( - (transform.position.x - target.transform.position.x),
		                                     										   - (transform.position.y - target.transform.position.y));

		// Calculate the variables
		anglePID.var = localForward;
		anglePID.targetVar = vectorToTarget;

		velocityPID.var = RB.velocity;
		velocityPID.targetVar = localForward;


		//velocityPID.var = (RB.velocity.normalized.x * vectorToTarget.normalized.y)
														   // - (RB.velocity.normalized.y * vectorToTarget.normalized.x);
		//velocityPID.var *= RB.velocity.magnitude;


		// first derivative
		anglePID.firstDerivative = anglePID.var - anglePID.lastVar;
		anglePID.lastVar = anglePID.var;
		anglePID.targetFirstDerivative = anglePID.targetVar - anglePID.lastTargetVar;
		anglePID.lastTargetVar = anglePID.targetVar;

		velocityPID.firstDerivative = anglePID.var - velocityPID.lastVar;
		velocityPID.lastVar = velocityPID.var;
		velocityPID.targetFirstDerivative = velocityPID.targetVar - velocityPID.lastTargetVar;
		velocityPID.lastTargetVar = velocityPID.targetVar;

		// second derivative
		anglePID.secondDerivative = anglePID.firstDerivative - anglePID.lastDerivative;
		anglePID.lastDerivative = anglePID.firstDerivative;
		anglePID.targetSecondDerivative = anglePID.targetFirstDerivative - anglePID.lastTargetDerivative;
		anglePID.lastTargetDerivative = anglePID.targetFirstDerivative; 

		velocityPID.secondDerivative = velocityPID.firstDerivative - velocityPID.lastDerivative;
		velocityPID.lastDerivative = velocityPID.firstDerivative;
		velocityPID.targetSecondDerivative = velocityPID.targetFirstDerivative - velocityPID.lastTargetDerivative;
		velocityPID.lastTargetDerivative = velocityPID.targetFirstDerivative; 
		 
		// calculate the timestep prediction
		anglePID.varForward = anglePID.var + anglePID.firstDerivative;
		anglePID.targetForward = anglePID.targetVar + anglePID.targetFirstDerivative;
		anglePID.derivForward = anglePID.firstDerivative + anglePID.secondDerivative;

		velocityPID.varForward = velocityPID.var + velocityPID.firstDerivative;
		velocityPID.targetForward = velocityPID.targetVar + velocityPID.targetFirstDerivative;
		velocityPID.derivForward = velocityPID.firstDerivative + velocityPID.secondDerivative;

		// find the difference between our predicted values
		float angle = Vector2.Angle( anglePID.varForward , anglePID.targetForward );
		float velocityAngle = Vector2.Angle( velocityPID.varForward, velocityPID.targetForward );

		float angleSign = Mathf.Sign( anglePID.varForward.x*anglePID.targetForward.y - anglePID.targetForward.x*anglePID.varForward.y );
		angle *= angleSign;
		float deltaAngle = angle - lastAngle;
		lastAngle = angle;

		float velocitySign = Mathf.Sign( velocityPID.varForward.x*velocityPID.targetForward.y - velocityPID.targetForward.x*velocityPID.varForward.y );
		velocityAngle *= velocitySign;
		float deltaVelocityAngle = velocityAngle - lastVelocityAngle;
		lastVelocityAngle = velocityAngle;

		// build the integration offset
		anglePID.integratedOffset += angle;
		velocityPID.integratedOffset += velocityAngle;
		
		if (fuel > 0) {
			if (projectileController.currentLife < (projectileController.lifeTime * 50 - 15)) {

				// formulate components of our PID's
				anglePID.p = angle * anglePID.p_Constant;
				anglePID.d =  deltaAngle * anglePID.d_Constant;
				anglePID.i = anglePID.integratedOffset * anglePID.i_Constant;

				velocityPID.p = velocityAngle * velocityPID.p_Constant;
				velocityPID.d = deltaVelocityAngle * velocityPID.d_Constant;
				velocityPID.i = velocityPID.integratedOffset * velocityPID.i_Constant;

				// calculate final result of each PID controller
				float anglePIDTorque = (float)( anglePID.p + anglePID.d + anglePID.i ) / 180f; 
				float velocityPIDTorque = -(float)( velocityPID.p - velocityPID.d + velocityPID.i ) / 180f; 

				// then apply our PID torque
				RB.AddTorque ( (float)( anglePIDTorque * anglePID.weight
				        								  + velocityPIDTorque * velocityPID.weight ) );
			}
			RB.AddForce (localForward * thrust);
			fuel -= 1;
		} else {
			foreach ( Transform engine in engines ) {
				engine.gameObject.GetComponent<SpriteRenderer> ().enabled = false;
			}
			GetComponent<MissileControl> ().enabled = false;
		}
	}
	
	void Update () {
		
	}
}
