using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FFTimingGameManager : MonoBehaviour {

	public GameObject gameController;
	public PlayerController player;
	public BulletSpawner bulletManager;
	public Animator fadeInOut;
	public EnemyChasing enemy;
	public MovingMap movingMap;

	public Text bronzeCoinCount;
	public Text silverCoinCount;
	public Text goldCoinCount;

	public Text holdHP;
	public Text holdTimeLeft;

	public GameObject setupUI;
	public GameObject holdUI;
	public GameObject gameOverUI;
	public GameObject winUI;

	public GameObject setupMap;
	public GameObject setupEnemy;

	public GameObject holdMap;
	public GameObject holdCastle;
	public GameObject holdEnemySpawner;
	public GameObject holdWing;
	public GameObject holdShield;

	public float maxHoldTime = 30.0f;
	AudioSource _bgmSource;

	bool _isFirstSetup = true;
	bool _lastFrameStateIsSetup = false;

	bool _holdIsOn = false;
	float _holdTimeLeft = 0.0f;

	// Use this for initialization
	void Start () {
		_bgmSource = gameObject.GetComponent<AudioSource> ();
		_holdTimeLeft = maxHoldTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (player.checkState ("dead")) {
			gameController.SetActive (false);
			gameOverUI.SetActive (true);
			if (_bgmSource.pitch > 0.0f) {
				_bgmSource.pitch = _bgmSource.pitch - 0.005f;
			}
		} else if (player.checkState ("setup")) {
			bronzeCoinCount.text = player.getCollectionCount ("bronze").ToString ();
			silverCoinCount.text = player.getCollectionCount ("silver").ToString ();
			goldCoinCount.text = player.getCollectionCount ("gold").ToString ();
			if (_lastFrameStateIsSetup == false) {
				StartCoroutine (Setup ());
			}
			_lastFrameStateIsSetup = true;
		} else {
			holdHP.text = bulletManager.getHP ().ToString ();
			_holdTimeLeft = _holdTimeLeft - Time.deltaTime;
			holdTimeLeft.text = Mathf.RoundToInt (_holdTimeLeft).ToString();
			if (player.getSetupFinishCount() > 2) {
				winUI.SetActive (true);
			}
			else if (_lastFrameStateIsSetup == true) {
				StartCoroutine (Hold ());
			}
			if (_holdIsOn && (_holdTimeLeft <= 0)) {
				player.HoldWin ();
				_holdIsOn = false;
			}

			_lastFrameStateIsSetup = false;
		}
	}

	IEnumerator Setup(){
		if (_isFirstSetup) {
			_isFirstSetup = false;
		} else {
			holdEnemySpawner.SetActive (false);
			fadeInOut.SetTrigger ("FadeOut");
			yield return new WaitForSecondsRealtime (2.0f);
		}
		setupUI.SetActive (true);
		holdUI.SetActive (false);

		//Debug.Log ("Here!");
		setupMap.SetActive (true);
		setupEnemy.SetActive (true);

		enemy.resetPos ();

		player.SetSetupTransform ();
		holdMap.SetActive (false);
		holdCastle.SetActive (false);
		holdWing.SetActive (false);
		holdShield.SetActive (false);

		fadeInOut.SetTrigger ("FadeIn");
		yield return new WaitForSecondsRealtime (1.0f);
	}
	IEnumerator Hold(){
		setupEnemy.SetActive (false);

		fadeInOut.SetTrigger ("FadeOut");
		yield return new WaitForSecondsRealtime (2.0f);

		setupUI.SetActive (false);
		holdUI.SetActive (true);

		setupMap.SetActive (false);

		player.SetHoldTransform ();
		holdMap.SetActive (true);
		holdCastle.SetActive (true);
		holdEnemySpawner.SetActive (true);
		holdWing.SetActive (true);
		holdShield.SetActive (true);

		bulletManager.InitHP (player.getCollectionCount ("gold"), player.getCollectionCount ("silver"), player.getCollectionCount ("bronze"));
		player.resetCollectionCount ();

		_holdTimeLeft = maxHoldTime;

		fadeInOut.SetTrigger ("FadeIn");
		yield return new WaitForSecondsRealtime (1.0f);

		_holdIsOn = true;
	}

	public void Restart(){
		SceneManager.LoadScene ("TimingLevelCombined");
	}
}
