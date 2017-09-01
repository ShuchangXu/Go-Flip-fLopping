using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour {

	public float maxY = 4.5f;
	public float minY = -3.0f;
	public float maxX = 4.3f;
	public float minX = -10.0f;

	public int multiThreshold = 5;

	public AudioClip shieldCollision;
	public AudioClip bombExplosion;

	public GameObject bullet;
	public PlayerController player;

//	int _defeatedByPlayer = 0;
//	int _destroyOnCastle = 0;
	int _HP = 0;

	int _bulletAccumulator = 0;
	int _currBulletCount = 0;
	int _maxBulletNum = 1;

	float _posXThreshold;

	AudioSource _audio;

	GameObject _latestBullet;

	void Awake(){
		_audio = GetComponent<AudioSource> ();
		if (_audio == null) { // if AudioSource is missing
			Debug.LogWarning ("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource> ();
		}
		_posXThreshold = (minX + 2 * maxX) / 3;
	}
		
	void Update () {
		if (_currBulletCount < _maxBulletNum) {
			if (_latestBullet) {
				if (_latestBullet.transform.position.x < _posXThreshold) {
					return;
				}
			}
			StartCoroutine (spawnBullet ());
		}
	}

	IEnumerator spawnBullet(){
		int num = 1;

		bool isBurst = (Random.Range (0.0f, 1.0f) < 0.1f);

		if (isBurst) {
			num = Random.Range (4, 7);
		}

		ParameterList tempParas = RandomParameterGenerator.GenerateRandomParameters ();

		Vector3 position = new Vector3 (minX, Random.Range (minY, maxY), 0);

		Quaternion rotation = new Quaternion ();

		GameObject newBullet = null;

		for (int i = 0; i < num; i++) {
			newBullet = Instantiate (bullet, position, rotation, gameObject.transform);
			newBullet.GetComponent<EnemyFiring> ().SetParameters (tempParas);

			_currBulletCount++;
			_bulletAccumulator++;
			_latestBullet = newBullet;

			yield return new WaitForSeconds (0.2f);
		}

		if (_bulletAccumulator > multiThreshold) {
			_maxBulletNum = 2;
		}
	}

	public void InitHP(int gold, int silver, int copper){
		_HP = gold * 3 + silver * 2 + copper * 1;
	}

	public int getHP(){
		return _HP;
	}

	public void OneEnemyDestroyed(bool enemyWin){
		_currBulletCount--;
		//To Do
		if (enemyWin) {
			PlaySound (bombExplosion,1.0f);
			_HP = _HP - 10;
			if (_HP < 0) {
				player.Die ();
			}
			//_destroyOnCastle++;
		} else {
			PlaySound (shieldCollision,1.0f);
			_HP++;
			//_defeatedByPlayer++;
		}
	}
		
//	public int getPerformance(bool playerWin){
//		if (playerWin) {
//			return _defeatedByPlayer;
//		} else {
//			return _destroyOnCastle;
//		}
//	}

	void PlaySound(AudioClip clip,float volume)
	{
		_audio.PlayOneShot (clip, volume);
	}
}
