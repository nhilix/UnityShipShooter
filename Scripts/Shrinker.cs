using UnityEngine;
using System.Collections;

public class Shrinker : MonoBehaviour {
	
	public Vector3 startScale;

	// Use this for initialization
	void Start () {
		startScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 oldScale = transform.localScale;
		transform.localScale = Vector3.Scale (oldScale, Vector3.one * 0.85f);
		if (transform.localScale.x <= 0.05) {
			transform.localScale = startScale;
			GetComponent<SpriteRenderer> ().enabled = false;
			gameObject.GetComponent<Shrinker> ().enabled = false;
		}
	}

	public void resetScale () {
		transform.localScale = startScale;
	}
}
