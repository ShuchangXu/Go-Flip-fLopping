﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCollections : MonoBehaviour {

	public string collectionType;
	public bool taken = false;
	public GameObject explosion;

	// if the player touches the coin, it has not already been taken, and the player can move (not dead or victory)
	// then take the coin
	void OnTriggerEnter2D (Collider2D other)
	{
		if ((other.gameObject.tag == "Player" ) && (!taken))
		{
			// mark as taken so doesn't get taken multiple times
			taken=true;

			// if explosion prefab is provide, then instantiate it
			if (explosion)
			{
				Instantiate (explosion, new Vector3 (transform.position.x, transform.position.y, 0), transform.rotation, transform.parent);
			}

			// do the player collect coin thing
			other.gameObject.GetComponent<PlayerController>().Collect(collectionType);
			// destroy the coin
			DestroyObject(this.gameObject);
		}
	}
}
