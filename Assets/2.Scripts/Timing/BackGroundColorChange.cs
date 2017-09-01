using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundColorChange : MonoBehaviour {

	public Color[] colors;
	public float changeInterval = 10.0f;

	int _colorNum;
	int _currentColorIndex = 0;
	bool _canChangeIndex = false;
	[SerializeField]
	Color _currentColor;

	List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer> ();

	void Start(){
		foreach (Transform child in gameObject.transform) {
			_spriteRenderers.Add (child.gameObject.GetComponent<SpriteRenderer> ());
		}
		_colorNum = colors.Length;
	}

	// Update is called once per frame
	void Update () {
		float periodCount = Time.time / changeInterval;
		float lerpValue = periodCount - Mathf.FloorToInt (periodCount);

		if (!_canChangeIndex) {
			if (lerpValue > 0.9) {
				_canChangeIndex = true;
			}
		} else {
			if (lerpValue < 0.1) {
				_currentColorIndex = (_currentColorIndex + 1) % _colorNum;
				_canChangeIndex = false;
			}
		}
		_currentColor = Color.Lerp (colors [_currentColorIndex], colors [(_currentColorIndex + 1) % _colorNum], (Mathf.Cos ((lerpValue - 1) * Mathf.PI)+1)/2);
		foreach (SpriteRenderer sR in _spriteRenderers) {
			sR.color = _currentColor;
		}
	}
}
