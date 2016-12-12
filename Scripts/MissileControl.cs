using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MissileControl : MonoBehaviour {

	public GameObject target;
	public int fuel;
	public float maxThrust;
    public int missileId;
	public double torqueProportionalConstant;
	public double torqueDerivativeConstant;
	public double torqueIntegralConstant;
	public double torqueVelocityProportionalConstant;
	public double torqueVelocityDerivativeConstant;
	public double torqueVelocityIntegralConstant;
	public double anglePIDConstant;
	public double velocityPIDConstant;
    public PIDController targetPID;

	private float startDistance;
	private float thrust;
    private float framesSinceLastAvoid;
    private Color lineColor;
    private Vector2 lastTargetToAvoid;
    private Vector2 targetToAvoidCollision;
	private Transform[] transforms;
	private List<Transform> engines;
    private List<Vector2> pathLinesOrigin;
    private List<Vector2> pathLinesEnd;

    //Wrapper for Debug.Log usage: Log(foo,bar,baz);
    public static string Log(params object[] data)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append("\t");
        }
        string s = sb.ToString();
        Debug.Log(s);
        return s;
    }
	// Use this for initialization
	void Start () {
		thrust = maxThrust;
		transforms = GetComponentsInChildren<Transform> ();
		engines = new List<Transform> ();
		foreach ( Transform child in transforms ) {
			if (child.gameObject.tag == "Flame") {
				engines.Add( child );
			}
		}
		fuel *= 50;
        targetPID = gameObject.AddComponent<PIDController>();
        configPID(targetPID);
        pathLinesOrigin = new List<Vector2>();
        pathLinesEnd = new List<Vector2>();
        lineColor = new Color(Random.Range(0, 255) / 255f, Random.Range(0, 255) /255f, Random.Range(0, 255) / 255f);
        missileId = GetComponent<Projectile>().whoFiredMe.GetComponent<CockpitControl> ().missilesFired;
    }
    void configPID(PIDController pid, float vScale=1f, float aScale=1f )
    {
        pid.parent = gameObject;
        pid.torqueProportionalConstant=torqueProportionalConstant ;
        pid.torqueDerivativeConstant = torqueDerivativeConstant;
        pid.torqueIntegralConstant = torqueIntegralConstant;
        pid.torqueVelocityProportionalConstant = torqueVelocityProportionalConstant;
        pid.torqueVelocityDerivativeConstant = torqueVelocityDerivativeConstant;
        pid.torqueVelocityIntegralConstant = torqueVelocityIntegralConstant;
        pid.anglePIDConstant = anglePIDConstant;
        pid.velocityPIDConstant = velocityPIDConstant;
    }
	// Update is called once per frame
    bool validHit(RaycastHit2D hit)
    {   bool validHit;
        validHit = hit;
        if (validHit)
        {
            validHit &= hit.transform != transform;
            validHit &= hit.transform != target.transform;
            
            if (hit.transform.parent) validHit &= hit.transform.parent.transform != target.transform;
            validHit &= hit.transform != GetComponent<Projectile>().whoFiredMe.transform;
            if (validHit)
            {
                validHit &= Vector2.Distance(transform.position, target.transform.position) > Vector2.Distance(transform.position, hit.transform.position);
            }
            if (validHit)
            {
                validHit &= bWillHitTarget(hit);
                //validHit &= Physics2D.Raycast(transform.position, target.transform.position);
            }
        }
        return validHit;
    }
    static Vector2 RotateVector(Vector2 v, float degrees)
    {
        Vector2 result = new Vector2();
        float theta = ( degrees / 180f ) * Mathf.PI;
        result.x = v.x * Mathf.Cos(theta) - v.y * Mathf.Sin(theta);
        result.y = v.x * Mathf.Sin(theta) + v.y * Mathf.Cos(theta);
        return result;
    }
    bool bWillHitTarget(RaycastHit2D hit)
    {
        return true;
    }
	void FixedUpdate () {
		if (fuel > 0) {
            Vector2 localForward = new Vector2 (-Mathf.Sin ((transform.rotation.eulerAngles.z / 180f) * Mathf.PI),
                                                                       Mathf.Cos ((transform.rotation.eulerAngles.z / 180f) * Mathf.PI));
            Rigidbody2D RB = GetComponent<Rigidbody2D>();
            Vector2 targetPos = new Vector2(target.transform.position.x, target.transform.position.y);
            Vector2 transPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 accel = localForward.normalized * thrust;
            transPos = transPos + RB.velocity * Time.fixedDeltaTime + 0.5f * accel * Time.fixedDeltaTime * Time.fixedDeltaTime;
            float targetDis = Vector2.Distance(targetPos, transPos);
            Vector2 toTarget = targetPos - transPos;
            Vector2 scanOrigin = RB.velocity + accel * Time.fixedDeltaTime;

            // Draw lines along path
            pathLinesOrigin.Add(transPos);
            pathLinesEnd.Add(transPos + localForward.normalized * 0.15f);

            /*
            CockpitControl shipControl = GetComponent<Projectile>().whoFiredMe.GetComponent<CockpitControl>();
            if (missileId >= shipControl.pathLinesOriginList.Count)
            {
                shipControl.pathLinesOriginList.Add(new List<Vector2> () );
                shipControl.pathLinesEndList.Add(new List<Vector2> () );
                shipControl.pathLinesColors.Add(lineColor);
            }
            shipControl.pathLinesOriginList[missileId - 1] = pathLinesOrigin;
            shipControl.pathLinesEndList[missileId - 1] = pathLinesEnd;
            */

             // LASER SCANNING :)
            // Find Raycast deviation from velocity that first clears obstacle
            // hitL and hitR are most recent raycast of deviated velocity
       
            lastTargetToAvoid = targetToAvoidCollision;
            targetToAvoidCollision = Vector2.zero;
            float scanLength = 10000f; 
            float degreeStepSize = 1f;
            // degreeOffset is the angle we deviate velocity for raycast
            float degreeOffset = -1f;

            RaycastHit2D raycastAvoidObstacle = new RaycastHit2D();
            RaycastHit2D raycastAvoidObstacleL = new RaycastHit2D();
            RaycastHit2D raycastAvoidObstacleR = new RaycastHit2D();
            
            float degreeOffsetL = 0f;
            float degreeOffsetR = 0f;

            // Extend deviation until a raycast avoids the obstacle
            RaycastHit2D hit = Physics2D.Raycast(transPos, scanOrigin);
            if (validHit(hit))
            {
                RaycastHit2D hitL = hit;
                RaycastHit2D hitR = hit;
                while ((validHit(hitR) || validHit(hitL)) && degreeOffset < 90f)
                {
                    degreeOffset += degreeStepSize;

                    // Calculate deviated vectors for raycast
                    // Left is counter-clockwise, Right is clock-wise
                    Vector2 leftCastTarget = RotateVector(scanOrigin, degreeOffset);
                    Vector2 rightCastTarget = RotateVector(scanOrigin, -degreeOffset);

                    // Raycast along deviated targets
                    hitL = Physics2D.Raycast(transPos, leftCastTarget, scanLength);
                    hitR = Physics2D.Raycast(transPos, rightCastTarget, scanLength);
                    if (hitR) Debug.DrawLine(transPos, hitR.point, Color.red);
                    if (hitL) Debug.DrawLine(transPos, hitL.point, Color.blue);

                    if (!validHit(hitL) && degreeOffsetL == 0)
                    {
                        leftCastTarget = RotateVector(scanOrigin, degreeOffset - 1);
                        raycastAvoidObstacleL = Physics2D.Raycast(transPos, leftCastTarget, scanLength);
                        degreeOffsetL = degreeOffset;
                    }
            
                    if (!validHit(hitR) && degreeOffsetR == 0)
                    {
                        rightCastTarget = RotateVector(scanOrigin, -(degreeOffset - 1));
                        raycastAvoidObstacleR = Physics2D.Raycast(transPos, rightCastTarget, scanLength);
                        degreeOffsetR = -degreeOffset;
                    }
                }

                Vector2 offset;
                Vector2 offsetL = Vector2.zero;
                Vector2 offsetR = Vector2.zero; 
                Vector2 offsetLclear = targetPos;
                Vector2 offsetRclear = targetPos;
                Vector2 toHit;

                float distFromPathToTargetL = 0f;
                float distFromPathToTargetR = 0f;

                raycastAvoidObstacle = raycastAvoidObstacleL;

                toHit = (Vector2)raycastAvoidObstacle.point - transPos;

                offsetL = (Vector2)raycastAvoidObstacle.point
                                         + RotateVector(scanOrigin, 90f);
                offsetLclear = (Vector2)raycastAvoidObstacle.point
                                         + RotateVector(scanOrigin, 90f);

                Debug.DrawLine(transPos, offsetL, Color.green);
                Debug.DrawLine(transPos, offsetLclear, Color.cyan);
                Debug.DrawLine(raycastAvoidObstacle.point, offsetL, Color.blue);
                Debug.DrawLine(raycastAvoidObstacle.point, transPos);

                raycastAvoidObstacle = raycastAvoidObstacleR;

                toHit = (Vector2)raycastAvoidObstacle.point - transPos;

                offsetR = (Vector2)raycastAvoidObstacle.point
                                         + RotateVector(scanOrigin, -90f);
                          
                offsetRclear = (Vector2)raycastAvoidObstacle.point
                                         + RotateVector(scanOrigin, -90f);

                Debug.DrawLine(transPos, offsetR, Color.yellow);
                Debug.DrawLine(transPos, offsetRclear, Color.magenta);
                Debug.DrawLine(raycastAvoidObstacle.point, offsetR, Color.red);
                Debug.DrawLine(raycastAvoidObstacle.point, transPos);

                distFromPathToTargetL = (offsetL - transPos).magnitude
                                                      * Mathf.Sin( Mathf.Deg2Rad 
                                                      * Vector2.Angle(offsetL - transPos, targetPos - transPos));
                distFromPathToTargetR = (offsetR - transPos).magnitude 
                                                      * Mathf.Sin( Mathf.Deg2Rad
                                                      * Vector2.Angle(offsetR - transPos, targetPos - transPos));

                float angleToVelL = Vector2.Angle(offsetR - transPos, scanOrigin);
                float angleToVelR = Vector2.Angle(offsetL - transPos, scanOrigin);
                if (distFromPathToTargetL < distFromPathToTargetR)
                {
                    offset = offsetL;
                    raycastAvoidObstacle = raycastAvoidObstacleL;
                }
                else
                {
                    offset = offsetR;
                    raycastAvoidObstacle = raycastAvoidObstacleR;
                }

                targetToAvoidCollision = offset;
                // DEBUG LINES
                //if (targetToAvoidCollision != Vector2.zero) Debug.Break();
                Debug.DrawLine(localForward + transPos, transPos, Color.white);
                Debug.DrawLine(transPos, transPos + scanOrigin * 10f, Color.black);
               
            }
            
            if (targetToAvoidCollision == Vector2.zero && lastTargetToAvoid != Vector2.zero)
            {
                targetToAvoidCollision = targetPos + 0.9f * (lastTargetToAvoid - targetPos);
            } 
            Vector2 targetToUse = targetToAvoidCollision != Vector2.zero ? targetToAvoidCollision : targetPos;
            Debug.DrawLine(targetToUse + new Vector2( 0.3f, 0.3f), targetToUse + new Vector2(-0.3f, -.3f), Color.yellow);
            Debug.DrawLine(targetToUse + new Vector2( 0.3f, -0.3f), targetToUse + new Vector2(-0.3f, 0.3f), Color.yellow);
            float torque = targetPID.generateTorque(targetToUse);
            RB.AddTorque(torque);
			RB.AddForce(localForward.normalized * thrust);
			fuel -= 1;
		} else {
			foreach ( Transform engine in engines ) {
				engine.gameObject.GetComponent<SpriteRenderer> ().enabled = false;
			}
			GetComponent<MissileControl> ().enabled = false;
		}
	}
}
