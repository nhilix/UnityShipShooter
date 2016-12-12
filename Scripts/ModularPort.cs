using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModularPort : MonoBehaviour {

	public ModularShipPart parentPart;
	public ModularPort connection;
	public float powerCapacity;
	public int sizeClass;
	public bool bIsParent;
	public bool bCanTransferPlayer;

	private float currPowerSupply;
	private Dictionary< int, float > sizeClassToStrengthMap;	

	// Use this for initialization
	void Start () {
		parentPart = gameObject.GetComponent<ModularShipPart> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
