using System;
using System.Collections;
using UnityEngine;

public class TimeCollectable : MonoBehaviour {
    [Tooltip("addTime holds the amount of time player will gain when collecting")] 
    [SerializeField] private float _addTime;
    [SerializeField] private LevelManager _levelManager;

    public float GetAddTime() { return _addTime; }

    private void Start() { DoNullChecks(); SetScaleBasedOnTimeReward(); }

    private void SetScaleBasedOnTimeReward() {
        float newScale = _addTime / 5;
        
        transform.localScale = new Vector3(newScale, newScale, newScale); //Resize according to time value
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            _levelManager.AddToTimeRemaining(_addTime); //Add to time remaining
            transform.gameObject.SetActive(false);
        }
    }

    private void DoNullChecks() {
        if (_addTime == 0) { _addTime = 5; Debug.Log("TimeCollectable::DoNullChecks() _addTime = 0! Set to 5."); }
    }
}

