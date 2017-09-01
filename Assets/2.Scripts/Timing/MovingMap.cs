using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingMap : MonoBehaviour {

	public float moveSpeed = 0.1f;
	public float obstacleError = -0.15f;

	public GameObject Ground;
	public GameObject GroundWater;
	public GameObject WaterGround;
	public GameObject Water;

	public GameObject singleFloatingPlatform;
	public GameObject combinedFloatingPlatform;

	public GameObject obstacle;
	public GameObject treasure;

	public GameObject destination;

	public GameObject[] collections;

	public float leftBoundaryX = -10.0f;

	public int maxAdjoinBlockCount=12;

	public int maxMapLength = 150;

	float _currentMapEndX;

	[SerializeField]
	int _mapLength = 0;

	//public GameObject FloatingR;

	void Awake(){
		_currentMapEndX = leftBoundaryX;
		AddMap (5);
	}

	void LateUpdate () {
		gameObject.transform.Translate (-moveSpeed * Time.deltaTime, 0, 0);

		_currentMapEndX = _currentMapEndX - moveSpeed * Time.deltaTime;

		if (_currentMapEndX < 15.0f) {
			AddMap (1);
		}

		if (_mapLength > maxMapLength) {
			AddDestination ();
			_mapLength = 0;
		}

		foreach (Transform child in transform) {
			if (child.position.x < leftBoundaryX ) {
				Destroy (child.gameObject);
			}
		}
			
	}

	void AddMap(int waterNum){
		for (int i = 0; i < 2*waterNum; i++) {
			int num = Random.Range (1, maxAdjoinBlockCount);
			if (i % 2 == 0) {
				AddGroundBlock (WaterGround);
				AddBarriers (_currentMapEndX, num);
				for (int k = 0; k < num; k++) {
					AddGroundBlock (Ground);
				}
			} else {
				AddGroundBlock (GroundWater);
				AddFloatingPlatform(_currentMapEndX,num);
				AddTreasure (_currentMapEndX, num);
				AddWaterCoins (_currentMapEndX,num);
				for (int k = 0; k < num; k++) {
					AddGroundBlock (Water);
				}
			}
		}
	}

	void AddDestination(){
		Instantiate (destination, new Vector3 (_currentMapEndX, 0.52f, 0), new  Quaternion (), gameObject.transform);
	}

	void AddGroundBlock(GameObject Obj){
		_mapLength = _mapLength + 1;
		_currentMapEndX += 1.0f;
		Instantiate (Obj, new Vector3 (_currentMapEndX, -4, 0), new  Quaternion (), gameObject.transform);
	}

	void AddFloatingPlatform(float startX, int offset){

		float currentEndX = startX - 1.0f;
		float rightBoundary = startX + offset;

		//Randomly Genrating Blocks When spaces are enough
		while (rightBoundary - currentEndX > 3.0f) {
			int x = Random.Range (0, 2);
			if (x == 0) {
				//Generate Single Platform
				currentEndX = currentEndX + 2.0f;
				Instantiate (singleFloatingPlatform, new Vector3 (currentEndX, -1, 0), new  Quaternion (), gameObject.transform);

				//Randomly generate Collections or Obstacles
				int type = Random.Range (0, collections.Length+1);
				if (type == collections.Length) {
					Instantiate (obstacle, new Vector3 (currentEndX, obstacleError, 0), new  Quaternion (), gameObject.transform);
				} else {
					Instantiate (collections [type], new Vector3 (currentEndX, 0, 0), new  Quaternion (), gameObject.transform);
				}

			} else {
				
				//Generate Long Platform
				currentEndX = currentEndX + 4.0f;
				Instantiate (combinedFloatingPlatform, new Vector3 (currentEndX - 1.0f, -1, 0), new  Quaternion (), gameObject.transform);
				//Randomly generate Collections or Obstacles
				for (int i = 0; i < 3; i++) {
					int k = Random.Range (0, 3);
					if (k != 2) {
						int type = Random.Range (0, collections.Length+1);
						if (type == collections.Length) {
							Instantiate (obstacle, new Vector3 (currentEndX - 2.0f + i, obstacleError, 0), new  Quaternion (), gameObject.transform);
						} else {
							Instantiate (collections [type], new Vector3 (currentEndX - 2.0f + i, 0, 0), new  Quaternion (), gameObject.transform);
						}
					}
				}

			}
		}
			
		//When at end, decide whether generating single blocks
		if (rightBoundary - currentEndX > 1.0f) {
			currentEndX = currentEndX + 2.0f;
			Instantiate (singleFloatingPlatform, new Vector3 (currentEndX, -1, 0), new  Quaternion (), gameObject.transform);
			//Randomly generate Collections or Obstacles
			int type = Random.Range (0, collections.Length+1);
			if (type == collections.Length) {
				Instantiate (obstacle, new Vector3 (currentEndX, obstacleError, 0), new  Quaternion (), gameObject.transform);
			} else {
				Instantiate (collections [type], new Vector3 (currentEndX, 0, 0), new  Quaternion (), gameObject.transform);
			}
		}
	}

	/*Legacy!!! void AddFloatingPlatform(float startX, int offset){
		float currentEndX = startX - 1.0f;
		int longBlockNum = Random.Range (0, (offset + 1) / 4 + 1);
		int singleBlockNum = (offset - longBlockNum * 4) / 2;

		while (longBlockNum >= 0 && singleBlockNum >= 0) {
			int x = Random.Range (0, 2);
			if (x == 0) {
				currentEndX = currentEndX + 2.0f;
				Instantiate (singleFloatingPlatform, new Vector3 (currentEndX, -1, 0), new  Quaternion (), gameObject.transform);
				singleBlockNum--;
			} else {
				currentEndX = currentEndX + 4.0f;
				Instantiate (combinedFloatingPlatform, new Vector3 (currentEndX - 1.0f, -1, 0), new  Quaternion (), gameObject.transform);
				longBlockNum--;
			}
		}

		while (longBlockNum >= 0) {
			currentEndX = currentEndX + 4.0f;
			Instantiate (combinedFloatingPlatform, new Vector3 (currentEndX - 1.0f, -1, 0), new  Quaternion (), gameObject.transform);
			longBlockNum--;
		}

		while (singleBlockNum > 0){
			currentEndX = currentEndX + 2.0f;
			Instantiate (singleFloatingPlatform, new Vector3 (currentEndX, -1, 0), new  Quaternion (), gameObject.transform);
			singleBlockNum--;
		}
	}*/

	void AddWaterCoins(float startX, int offset){
		for (int i = 0; i < offset; i++) {
			int x = Random.Range (0, 2);
			if (x == 0) {
				int type = Random.Range (0, collections.Length);
				Instantiate (collections [type], new Vector3 (startX + i + 1, -4, 0), new  Quaternion (), gameObject.transform);
			}
		}
	}

	void AddBarriers (float startX, int offset){
		for (int i = 0; i < offset; i++) {
			int x = Random.Range (0, 5);
			if (x == 0) {
				Instantiate (obstacle, new Vector3 (startX + i + 1, -3.0f+obstacleError, 0), new  Quaternion (), gameObject.transform);
			}
		}
	}

	void AddTreasure (float startX, int offset){
		if (offset > 2) {
			int x = Random.Range (0, 2);
			if (x == 0) {
				
				float posX = startX + (offset + 1.0f) / 2;
				Instantiate (treasure, new Vector3 (posX, +3, 0), new  Quaternion (), gameObject.transform);
				Instantiate (singleFloatingPlatform, new Vector3 (posX, +2, 0), new  Quaternion (), gameObject.transform);
			}
		}
	}

	public float getMapPercentage(){
		return (_mapLength - _currentMapEndX) /200.0f;
	}
}
