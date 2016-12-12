using UnityEngine;
using System.Collections;

public class MouseAimWeapon : ModularWeapon {

   private GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public override void Update () {
      target = GetComponentInParent<CockpitControl>().target;
      float weaponAngle = 0f;
      Vector3 dir = Vector3.zero;
      if (target == null)
      {
         Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
         dir = Input.mousePosition - screenPoint;
      }
      else
      {
         dir = target.transform.position - transform.position;
      }
      weaponAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
      transform.rotation = Quaternion.AngleAxis(weaponAngle - 90, Vector3.forward);
      base.Update();
	}
}
