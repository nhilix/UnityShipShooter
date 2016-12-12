using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	private bool bIsExploding;
	private Vector3 startScale;
	public float size;

	// Use this for initialization
	void Start () {
		bIsExploding = true;
		startScale = transform.localScale;
		transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);
	}
	
	// Update is called once per frame
	void Update () {
		if (bIsExploding) {
			transform.localScale = Vector3.Scale (transform.localScale, Vector3.one * 1.15f);
			if (transform.localScale.x >= startScale.x * size ) {
				bIsExploding = false;
			}
		} else {
			transform.localScale = Vector3.Scale (transform.localScale, Vector3.one * .85f);
			if (transform.localScale.x <= .05)
				Destroy (gameObject);
		}
	}
}
