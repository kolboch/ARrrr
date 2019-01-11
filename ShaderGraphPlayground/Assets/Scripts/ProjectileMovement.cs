using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{

	public float speed = 1.0f;
	public float fireRate = 2.0f; 
	
	// Update is called once per frame
	void Update () {
		if (speed != 0f)
		{
			transform.position += (transform.forward * speed * Time.deltaTime);
		}
	}
}
