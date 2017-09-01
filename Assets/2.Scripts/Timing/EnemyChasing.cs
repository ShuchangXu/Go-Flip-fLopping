using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChasing : MonoBehaviour {

	public float leftBoundaryX = -8.5f;
	public float moveSpeedRelativeToGround = 6.0f;

	public float dampingX = 0.5f;
	public float dampingY_Inital = 0.5f;
	[SerializeField]
	float dampingY;

	[SerializeField]
	float _idealMoveSpeedRelativeToCamera;

	Vector3 _playerPos;
	float _currentVelocity_X;
	float _currentVelocity_Y;
	float _currentAcceleration_X;

	bool _isAccelerating = false;

	void Awake(){
		dampingY = dampingY_Inital;
	}

	void Update(){
		if (gameObject.transform.position.x > 10.0f) {
			Destroy (gameObject);
		}
	}

	void LateUpdate () {
		
		_currentVelocity_X = Mathf.SmoothDamp (_currentVelocity_X, _idealMoveSpeedRelativeToCamera, ref _currentAcceleration_X, dampingX);
		dampingY = Mathf.Clamp ((_playerPos.x - transform.position.x), 0.05f, dampingY_Inital);
		float newPosY = Mathf.SmoothDamp (transform.position.y, _playerPos.y, ref _currentVelocity_Y, dampingY);
		gameObject.transform.Translate (_currentVelocity_X * Time.deltaTime, newPosY - transform.position.y, 0);

	}

	void OnTriggerEnter2D (Collider2D other){
		if (other.gameObject.tag == "Player") {
			other.gameObject.GetComponent<PlayerController> ().Die ();
		}
	}

	public void updateMoveSpeed(float mapMoveSpeed){
		if (_isAccelerating) {
			if (gameObject.transform.position.x > -7.0f) {
				_isAccelerating = false;
				_idealMoveSpeedRelativeToCamera = moveSpeedRelativeToGround - mapMoveSpeed;
			} else {
				_idealMoveSpeedRelativeToCamera = 1.5f;
			}
		} else {
			if (gameObject.transform.position.x < leftBoundaryX) {
				_isAccelerating = true;
				_idealMoveSpeedRelativeToCamera = 1.5f;
			} else {
				_idealMoveSpeedRelativeToCamera = moveSpeedRelativeToGround - mapMoveSpeed;
			}
		}
	}

	public void updatePlayerPos(Vector3 playerPos){
		_playerPos = playerPos;
	}

	public void resetPos(){
		Vector3 pos = transform.position;
		pos.x = -8.0f;
		transform.position = pos;
	}
}
