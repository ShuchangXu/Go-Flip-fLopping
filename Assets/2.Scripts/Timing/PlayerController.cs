using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {

	//Move Speed for different States
	public float runSpeed = 7.0f;
	public float swimSpeed = 4.5f;
	public float jumpFromGroundSpeed = 5.5f;
	public float jumpOutOfWaterSpeed = 6.0f;

	//Jump Force for different States
	public float groundJumpForce = 700f;
	public float waterJumpForce = 300f;
	public float doubleJumpForce = 400f;

	//Public Components
	public MovingMap map;
	public EnemyChasing enemy;
	public Transform[] groundCheckPoints;
	public LayerMask whatIsGround;
	public LayerMask whatIsWater;

	//Public SFXs
	public AudioClip coinSFX;
	public AudioClip dieSFX;

	//private Components
	SpriteRenderer _spriteRender;
	Transform _transform;
	Animator _animator;
	Rigidbody2D _rigidbody;
	AudioSource _audio;

	//private Mechanical Properties for Setup Level
	float _vy;
	float _jumpForce;
	float _runSpeedReg;

	//damping Factor for Hold Time Control
	float dampingY = 0.1f;

	//Y Bound for Hold Time
	float maxY = 4.22f;
	float minY = -2.78f;

	//private Mechanical Properties for Hold Level
	float _currentVelocity_Y;

	//States On Land
	[SerializeField]
	bool _isOnPoop = false;
	[SerializeField]
	bool _isGrounded = false;
	[SerializeField]
	bool _isInWater = false;

	//States in the air
	[SerializeField]
	bool _canDoubleJump = false;

	//States Alive
	public bool _isDuringSetup = true;
	bool _isDead = false;

	//Collections Counter
	int _bronzeCoinCount = 0;
	int _silverCoinCount = 0;
	int _goldCoinCount = 0;

	//The time the player finish setup level
	int _finishedSetupTimeCount = 0;

	//Layers
	// store the layer the player is on (setup in Awake)
	int _playerLayer;
	// number of layer that Platforms are on (setup in Awake)
	int _platformLayer;

	void Start () {
		_runSpeedReg = runSpeed;

		// get a reference to the components we are going to be changing and store a reference for efficiency purposes
		_transform = GetComponent<Transform> ();

		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");

		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) // if Rigidbody is missing
			Debug.LogError("Rigidbody2D component missing from this gameobject");

		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}

		_spriteRender = GetComponent<SpriteRenderer> ();

		// determine the player's specified layer
		_playerLayer = this.gameObject.layer;

		// determine the platform's specified layer
		_platformLayer = LayerMask.NameToLayer("Platform");
	}
		
	void Update () {
		_animator.SetBool ("isDead", _isDead);
		_animator.SetBool ("isMoving", _isDuringSetup);
		if (!_isDead) {
			if (_isDuringSetup) {
				SetupPlayingUpdate ();
			} else {
				HoldPlayingUpdate ();
			}
		}
	}

	void HoldPlayingUpdate(){
		_rigidbody.isKinematic = true;
		float targetPosY = Mathf.Clamp (Input.mousePosition.y / Screen.height * 10.0f - 5.0f, minY, maxY);
		float newPosY = Mathf.SmoothDamp (transform.position.y, targetPosY, ref _currentVelocity_Y, dampingY);
		transform.Translate (0, newPosY - transform.position.y, 0);
	}

	void SetupPlayingUpdate(){
		_rigidbody.isKinematic = false;
		// Check to see if character is grounded by raycasting from the middle of the player
		// down to the groundCheck position and see if collected with gameobjects on the
		// whatIsGround layer
		_isGrounded=false;
		_isInWater=true;

		foreach(Transform groundCheck in groundCheckPoints){
			_isGrounded |= Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);
			_isInWater &= Physics2D.Linecast(_transform.position, groundCheck.position, whatIsWater);
		}

		//Set Animator State
		_animator.SetBool("isGround",_isGrounded);
		_animator.SetBool("isInWater",_isInWater);

		// get the current vertical velocity from the rigidbody component
		_vy = _rigidbody.velocity.y;

		if (_isOnPoop) {
			_canDoubleJump = true;
			map.moveSpeed = 0.0f;
			_jumpForce = groundJumpForce;
		} else if (_isInWater) {
			_canDoubleJump = true;
			map.moveSpeed = swimSpeed;
			_jumpForce = waterJumpForce;
		} else if (_isGrounded && _vy <= 0.0f) {
			_canDoubleJump = true;
			runSpeed = _runSpeedReg;
			map.moveSpeed = runSpeed;
			_jumpForce = groundJumpForce;
		} else {
			map.moveSpeed = runSpeed;
			_jumpForce = doubleJumpForce;
		}


		if (CrossPlatformInputManager.GetButtonDown ("Jump")) {
			if (_isGrounded) {
				runSpeed = jumpFromGroundSpeed;
				DoJump ();
			} else if (_isInWater) {
				runSpeed = jumpOutOfWaterSpeed;
				DoJump ();
			} else if (_canDoubleJump) {
				DoJump ();
				_canDoubleJump = false;
			}
		}

		// If the player stops jumping mid jump and player is not yet falling
		// then set the vertical velocity to 0 (he will start to fall from gravity)
		if(CrossPlatformInputManager.GetButtonUp("Jump") && _vy>0f)
		{
			_vy = 0f;
		}

		_rigidbody.velocity = new Vector2(0, _vy);

		// if moving up then don't collide with platform layer
		// this allows the player to jump up through things on the platform layer
		// NOTE: requires the platforms to be on a layer named "Platform"
		Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_vy > 0.0f)); 

		if (enemy) {
			enemy.updateMoveSpeed (map.moveSpeed);
			enemy.updatePlayerPos (transform.position);
		}
	}


	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "StopMovingOnCollision") 
		{
			_isOnPoop = true;
			_animator.SetBool ("isStuck", _isOnPoop);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "House") 
		{

			SetupWin ();
		}
	}
		
	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.tag=="StopMovingOnCollision")
		{
			_isOnPoop = false;
			_animator.SetBool ("isStuck", _isOnPoop);
		}
	}

	//Utility Functions
	void PlaySound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}

	void DoJump(){
		// reset current vertical motion to 0 prior to jump
		_vy = 0f;
		// add a force in the up direction
		_rigidbody.AddForce (new Vector2 (0, _jumpForce));
	}

	void SetupWin(){
		_isDuringSetup = false;
		_finishedSetupTimeCount++;
	}

	public int getSetupFinishCount(){
		return _finishedSetupTimeCount;
	}
		
	public void HoldWin(){
		_isDuringSetup = true;
	}

	public void Die(){
		_isDuringSetup = false;
		_isDead = true;
		_animator.SetTrigger("Die");
		PlaySound (dieSFX);
	}

	public bool checkState(string stateName){
		switch (stateName) {
		case "dead":
			return _isDead;
		case "setup":
			return _isDuringSetup;
		default:
			return false;
		}
	}

	public void Collect(string collectionType){
		if (collectionType == "bronze") {
			_bronzeCoinCount++;
			PlaySound(coinSFX);
		} else if (collectionType == "silver") {
			_silverCoinCount++;
			PlaySound(coinSFX);
		} else if (collectionType == "gold") {
			_goldCoinCount++;
			PlaySound(coinSFX);
		}
	}

	public int getCollectionCount(string collectionType){
		if (collectionType == "bronze") {
			return _bronzeCoinCount;
		} else if (collectionType == "silver") {
			return _silverCoinCount;
		} else if (collectionType == "gold") {
			return _goldCoinCount;
		}
		return 0;
	}

	public void resetCollectionCount(){
		_bronzeCoinCount = 0;
		_silverCoinCount = 0;
		_goldCoinCount = 0;
	}

	public void SetSetupTransform(){
		Vector3 pos = transform.position;
		pos.x = 0.0f;
		pos.y = -2.78f;
		transform.position = pos;
		_spriteRender.flipX = false;
	}

	public void SetHoldTransform(){
		Vector3 pos = transform.position;
		pos.x = 5.0f;
		transform.position = pos;
		_spriteRender.flipX = true;
	}
}
