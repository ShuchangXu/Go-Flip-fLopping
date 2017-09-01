using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaveForm{
	Linear,
	Sin,
	Sawtooth,
	Square
}

public struct ParameterList{
	public WaveForm waveform;
	public float[] parameters;
}

public static class RandomParameterGenerator{
	public static ParameterList GenerateRandomParameters(){
		int x = Random.Range (0, 4);
		switch (x) {
		case 0:
			return GenerateLinearParameters ();
		case 1:
			return GenerateSinParameters ();
		case 2:
			return GenerateSawtoothParameters ();
		case 3:
			return GenerateSquareParameters ();
		default:
			return GenerateLinearParameters ();
		}
	}

	public static ParameterList GenerateLinearParameters(){
		float Vx0 = Random.Range (5.0f, 7.0f);
		float Ax = Random.Range (0.0f, 2.0f);
		float Vy0 = Random.Range (-2.0f, 2.0f);
		float Ay = -Mathf.Sign (Vy0) * Random.Range (-2.0f, 2.0f);

		ParameterList myList = new ParameterList ();
		myList.waveform = WaveForm.Linear;
		myList.parameters = new float[4]{ Vx0, Ax, Vy0, Ay };
		return myList;
	}

	public static ParameterList GenerateSinParameters(){
		float Vx0 = Random.Range (5.0f, 7.0f);
		float Ax = Random.Range (0.0f, 1.5f);
		float Vy0 = Random.Range (-1.0f, 1.0f);
		float Ay_Amp = Random.Range (0.4f, 2.0f);
		float Ay_Period = Random.Range (0.05f, 0.2f);

		ParameterList myList = new ParameterList ();
		myList.waveform = WaveForm.Sin;
		myList.parameters = new float[5]{ Vx0, Ax, Vy0, Ay_Amp, Ay_Period };
		return myList;
	}

	public static ParameterList GenerateSawtoothParameters(){
		float Vx0 = Random.Range (5.0f, 7.0f);
		float Ax = Random.Range (0.0f, 1.5f);
		float Oy_Amp = Random.Range (0.4f, 2.0f);
		float Oy_Period = Random.Range (0.2f, 1.0f);
		float UpRatio = Random.Range (0.01f, 0.99f);

		ParameterList myList = new ParameterList ();
		myList.waveform = WaveForm.Sawtooth;
		myList.parameters = new float[5]{ Vx0, Ax, Oy_Amp, Oy_Period, UpRatio };
		return myList;
	}

	public static ParameterList GenerateSquareParameters(){
		float Vx0 = Random.Range (4.0f, 6.0f);
		float Ax = Random.Range (0.0f, 1.0f);
		float Oy_Amp = Random.Range (0.4f, 2.0f);
		float Oy_Period = Random.Range (0.05f, 0.4f);

		ParameterList myList = new ParameterList ();
		myList.waveform = WaveForm.Square;
		myList.parameters = new float[4]{ Vx0, Ax, Oy_Amp, Oy_Period };
		return myList;
	}
}

public class EnemyFiring : MonoBehaviour {

	//Particle Effects
	public GameObject shieldExplosion;
	public GameObject houseExplosion;

	//Mechanical Parameters
	//Common
	WaveForm _waveform = WaveForm.Sin;
	float _timeSinceSpawn = 0.0f;

	float _Vx = 5.0f;
	float _Acceleration_x = 0.0f;
	float _Vy = 0.0f;
	float _Acceleration_y = 0.0f;

	float _maxY = 4.0f;
	float _minY = -3.0f;

	//Sin & Square
	float _Acceleration_y_Amp = 0.0f;
	float _Acceleration_y_Period = 0.2f;

	//Sawtooth
	float _Vy_Up = 0.0f;
	float _Vy_Down = 0.0f;

	TrailRenderer _trail;
	SpriteRenderer _sprite;
	BulletSpawner _parentSpawner;

	void Awake(){
		if (transform.parent) {
			_parentSpawner = transform.parent.gameObject.GetComponent<BulletSpawner> ();
		}
		_sprite = GetComponent<SpriteRenderer> ();
		_trail = GetComponent<TrailRenderer> ();
		_trail.sortingLayerName = "Foreground";
	}

	void FixedUpdate () {
		float timeDelta = Time.deltaTime;

		//Update Velocity
		UpdateVelocity (timeDelta);

		//Update Position
		Vector3 prePos = transform.position;
		Vector3 tmpPos = prePos;
		tmpPos.x = tmpPos.x + _Vx * timeDelta;
		tmpPos.y = Mathf.Clamp (tmpPos.y + _Vy * timeDelta, _minY - 0.1f, _maxY + 0.1f);
		transform.position = tmpPos;
		Vector3 displacement = tmpPos - prePos;

		//Update Velocity Angle
		Vector3 tmp = transform.eulerAngles;
		tmp.z = Mathf.Atan2 (displacement.y, displacement.x) * Mathf.Rad2Deg;
		transform.eulerAngles = tmp;

		//There's something wrong with translate
		//transform.Translate (_Vx * timeDelta, _Vy * timeDelta, 0);
	}

	void UpdateVelocity(float timeDelta){
		//Update Time
		_timeSinceSpawn = _timeSinceSpawn + timeDelta;

		_Vx = _Vx + _Acceleration_x * timeDelta;

		switch (_waveform) {
		case WaveForm.Linear:
			if (transform.position.y > _maxY) {
				_Vy = -_Vy;
				_Acceleration_y = -_Acceleration_y;
			} else if (transform.position.y < _minY) {
				_Vy = -_Vy;
				_Acceleration_y = -_Acceleration_y;
			} else {
				_Vy = _Vy + _Acceleration_y * timeDelta;
			}
			break;
		case WaveForm.Sin:
			if (transform.position.y > _maxY) {
				_Vy = -_Vy;
				_Acceleration_y_Amp = -_Acceleration_y_Amp;
			} else if (transform.position.y < _minY) {
				_Vy = -_Vy;
				_Acceleration_y_Amp = -_Acceleration_y_Amp;
			} else {
				_Acceleration_y = -_Acceleration_y_Amp * Mathf.Cos (_timeSinceSpawn / _Acceleration_y_Period);
				_Vy = _Vy + _Acceleration_y * timeDelta;
			}
			break;
		case WaveForm.Sawtooth:
			if (transform.position.y > _maxY) {
				_Vy = _Vy_Down;
			} else if (transform.position.y < _minY) {
				_Vy = _Vy_Up;
			}
			break;
		case WaveForm.Square:
			_Acceleration_y = - _Acceleration_y_Amp * Mathf.Cos (_timeSinceSpawn / _Acceleration_y_Period);
			_Vy = _Vy + _Acceleration_y * timeDelta;
			break;
		default:
			_Vy = _Vy + _Acceleration_y * timeDelta;
			break;
		}
	}

	public void SetParameters(ParameterList pl){
		switch (pl.waveform) {
		case WaveForm.Linear:
			SetLinear (pl.parameters [0], pl.parameters [1], pl.parameters [2], pl.parameters [3]);
			break;
		case WaveForm.Sin:
			SetSin (pl.parameters [0], pl.parameters [1], pl.parameters [2], pl.parameters [3], pl.parameters [4]);
			break;
		case WaveForm.Sawtooth:
			SetSawtooth (pl.parameters [0], pl.parameters [1], pl.parameters [2], pl.parameters [3], pl.parameters [4]);
			break;
		case WaveForm.Square:
			SetSquare (pl.parameters [0], pl.parameters [1], pl.parameters [2], pl.parameters [3]);
			break;
		}
	}

	public void SetLinear(float Velocity_x0, float Acceleration_x, float Velocity_y0, float Acceleration_y){
		_waveform = WaveForm.Linear;
		_timeSinceSpawn = 0.0f;
		_Vx = Velocity_x0;
		_Acceleration_x = Acceleration_x;
		_Vy = Velocity_y0;
		_Acceleration_y = Acceleration_y;
	}

	public void SetSin(float Velocity_x0, float Acceleration_x, float Velocity_y0, float Oscillate_y_Amp, float Oscillate_y_Period){
		_waveform = WaveForm.Sin;
		_timeSinceSpawn = 0.0f;
		_Vx = Velocity_x0;
		_Acceleration_x = Acceleration_x;
		_Vy = Velocity_y0;
		_Acceleration_y_Amp = Oscillate_y_Amp / Oscillate_y_Period / Oscillate_y_Period;
		_Acceleration_y_Period = Oscillate_y_Period;
	}
		
	public void SetSawtooth(float Velocity_x0, float Acceleration_x, float Oscillate_y_Amp, float Oscillate_y_Period, float UpTimeRatio){
		_waveform = WaveForm.Sawtooth;

		_timeSinceSpawn = 0.0f;

		_Vx = Velocity_x0;
		_Acceleration_x = Acceleration_x;

		_minY = Mathf.Max (transform.position.y, _minY);
		_maxY = Mathf.Min (transform.position.y + Oscillate_y_Amp, _maxY);

		Oscillate_y_Amp = _maxY - _minY;

		_Vy_Up = Oscillate_y_Amp / (Oscillate_y_Period * UpTimeRatio);
		_Vy_Down = - Oscillate_y_Amp / (Oscillate_y_Period * (1-UpTimeRatio));

		_Vy = _Vy_Up;
	}

	public void SetSquare(float Velocity_x0, float Acceleration_x, float Oscillate_y_Amp, float Oscillate_y_Period){
		_waveform = WaveForm.Square;

		_timeSinceSpawn = 0.0f;

		_Vx = Velocity_x0;
		_Acceleration_x = Acceleration_x;

		_Acceleration_y_Amp = Oscillate_y_Amp / Oscillate_y_Period / Oscillate_y_Period;
		_Acceleration_y_Period = Oscillate_y_Period;

		_minY = Mathf.Max (transform.position.y, _minY);
		_maxY = Mathf.Min (transform.position.y + Oscillate_y_Amp, _maxY);

		_Acceleration_y_Amp = 1000.0f / Oscillate_y_Period / Oscillate_y_Period;
		_Acceleration_y_Period = Oscillate_y_Period;
	}

	void OnTriggerEnter2D (Collider2D other){
		if (other.gameObject.tag == "Shield") {
			_parentSpawner.OneEnemyDestroyed (false);
			if (shieldExplosion)
			{
				Instantiate (shieldExplosion, new Vector3 (transform.position.x, transform.position.y, 0), transform.rotation, transform.parent);
			}
			Destroy (gameObject);
		} else if (other.gameObject.tag == "House") {
			_parentSpawner.OneEnemyDestroyed (true);
			if (houseExplosion)
			{
				Instantiate (houseExplosion, new Vector3 (transform.position.x, transform.position.y, 0), transform.rotation, transform.parent);
			}
			Destroy (gameObject);
		}
	}
}
