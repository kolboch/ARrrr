﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnProjectiles : MonoBehaviour
{

	public GameObject firePoint;
	public List<GameObject> vfx = new List<GameObject>();
	
	private GameObject effectToSpawn;
	private float timeToFire = 0f;
	
	// Use this for initialization
	void Start ()
	{
		if (vfx.Count > 0)
		{
			effectToSpawn = vfx[0];	
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0) && Time.time >= timeToFire)
		{
			timeToFire = Time.time + 0.7f; // delay next launch
			SpawnVFX();
		}
	}

	private void SpawnVFX()
	{
		GameObject vfx;
		if (firePoint != null)
		{
			vfx = Instantiate(effectToSpawn, firePoint.transform.position, Quaternion.identity);
			
		}
		else
		{
			Debug.Log("Fire point is null");
		}
	}
}
