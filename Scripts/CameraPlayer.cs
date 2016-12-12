using UnityEngine;
using System.Collections;

public class CameraPlayer : MonoBehaviour {

	public GameObject shipToFollow;

	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position - shipToFollow.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = shipToFollow.transform.position;//+ offset;
		transform.position = new Vector3( transform.position.x, transform.position.y, -10f );

		float zoom = Input.GetAxis ("Mouse ScrollWheel");
		Camera cam = GetComponent<Camera> ();
		cam.orthographicSize += zoom * 5f;
		cam.orthographicSize = Mathf.Max (3.5f, Mathf.Min (cam.orthographicSize, 50f));

	}
}
