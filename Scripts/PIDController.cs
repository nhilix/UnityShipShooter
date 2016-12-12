using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PIDController : MonoBehaviour {

    public GameObject parent;
	public double torqueProportionalConstant;
	public double torqueDerivativeConstant;
	public double torqueIntegralConstant;

	public double torqueVelocityProportionalConstant;
	public double torqueVelocityDerivativeConstant;
	public double torqueVelocityIntegralConstant;

	public double anglePIDConstant;
	public double velocityPIDConstant;

	private double lastAngleToTarget;
	private double lastVelocityAngleToTarget;

	private double integratedAngleOffset;
	private double integratedVelocityOffset;

	// Use this for initialization
	void Start () {
        lastAngleToTarget = 0f;
        lastVelocityAngleToTarget = 0f;
        integratedAngleOffset = 0f;
	}
	
    // Update is called once per frame
    public float generateTorque(Vector2 targetPos)
    {
        Vector2 localForward = new Vector2(-Mathf.Sin((parent.transform.rotation.eulerAngles.z / 180f) * Mathf.PI),
                                                                                   Mathf.Cos((parent.transform.rotation.eulerAngles.z / 180f) * Mathf.PI));
        Vector2 vectorToTarget = new Vector2(-(parent.transform.position.x - targetPos.x),
                                                                                       -(parent.transform.position.y - targetPos.y));
        Rigidbody2D RB = parent.GetComponent<Rigidbody2D>();
        //Debug.Log ( string.Format("RB: {0}",RB ));

        double tmpAngleToTarget = lastAngleToTarget;
        double tmpVelocityAngleToTarget = lastVelocityAngleToTarget;
        lastAngleToTarget = Vector2.Angle(localForward, vectorToTarget);
        lastVelocityAngleToTarget = Vector2.Angle(RB.velocity, vectorToTarget);
    
        //Debug.Log ( string.Format("vel: {0}",RB.velocity ));
        //Debug.Log ( string.Format("angle: {0}",lastAngleToTarget ));

        double deltaAngleToTarget = tmpAngleToTarget - lastAngleToTarget;
        double deltaVelocityAngleToTarget = tmpVelocityAngleToTarget - lastVelocityAngleToTarget;

        double torqueFromAngle = (localForward.normalized.x * vectorToTarget.y)
                                                          - (localForward.normalized.y * vectorToTarget.x);
        double torqueFromVelocity = (RB.velocity.x * vectorToTarget.y)
                                                               - (RB.velocity.y * vectorToTarget.x);

        int torqueFromAngleSign = -(int)Mathf.Sign((float)torqueFromAngle);
        int torqueFromVelocitySign = -(int)Mathf.Sign((float)torqueFromVelocity);
        integratedAngleOffset += lastAngleToTarget * torqueFromAngleSign;
        integratedVelocityOffset += lastVelocityAngleToTarget * torqueFromVelocitySign;


        double derivativeAngleCorrection = (torqueDerivativeConstant / 180f) * deltaAngleToTarget * torqueFromAngleSign;
        double integratedAngleCorrection = integratedAngleOffset * torqueIntegralConstant / 180f;
        torqueFromAngle *= torqueProportionalConstant;

        double derivativeVelocityCorrection = (torqueVelocityDerivativeConstant / 180f) * deltaVelocityAngleToTarget * torqueFromVelocitySign;
        double integratedVelocityCorrection = integratedVelocityOffset * torqueVelocityIntegralConstant / 180f;
        torqueFromVelocity *= torqueVelocityProportionalConstant;

        float anglePIDTorque = (float)(torqueFromAngle + derivativeAngleCorrection - integratedAngleCorrection);
        float velocityPIDTorque = (float)(torqueFromVelocity + derivativeVelocityCorrection - integratedVelocityCorrection);
        float distanceFromTarget = Vector2.Distance(parent.transform.position, targetPos);
        float finalTorque =  (float)(anglePIDTorque * anglePIDConstant + velocityPIDTorque * velocityPIDConstant)
                                                       * (parent.transform.localScale.x * parent.transform.localScale.x);
        //Debug.Log(string.Format("torque:{0}", finalTorque));
        return finalTorque;
    }
}
