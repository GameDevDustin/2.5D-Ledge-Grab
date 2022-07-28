using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [SerializeField] private float _timeRemaining;
    [SerializeField] private Vector3 _playerStartPosition;
    [SerializeField] private Vector3 _playerStartRotation;
    [SerializeField] private Vector3 _playerCharModelStartRotation;
    [SerializeField] private Vector3 _playerFinishPosition;
    [SerializeField] private UI_Manager _uiManager;
    [SerializeField] private PlayerAnimations _playerAnimations;

    public float GetTimeRemaining() { return _timeRemaining; }
    public void AddToTimeRemaining(float addTime) { _timeRemaining += addTime; }
    public Vector3 GetPlayerStartPosition() { return _playerStartPosition; }
    public Vector3 GetPlayerFinishPosition() { return _playerFinishPosition; }
    public Vector3 GetPlayerStartRotation() { return _playerStartRotation; }
    public Vector3 GetPlayerCharModelStartRotation() { return _playerCharModelStartRotation; }
    
    private void Start() { SetPlayerCharFacingDirection(); DoNullChecks(); }

    private void Update() { CalculateTimeRemaining(); }
    
    private void SetPlayerCharFacingDirection() {
        if (_playerCharModelStartRotation.y == -90) { //facing left
            _playerAnimations.SetPlayerCharFacingDirection(PlayerAnimations.PlayerCharFacingDirection.left);
        } else if (_playerCharModelStartRotation.y == 90) { //facing right
            _playerAnimations.SetPlayerCharFacingDirection(PlayerAnimations.PlayerCharFacingDirection.right);
        } else { Debug.LogError("LevelManager::SetPlayerCharFacingDirection() _playerCharModelStartRotation.y has an invalid value!"); }
    }

    private void CalculateTimeRemaining() {
        _timeRemaining -= Time.deltaTime; //decrement time
        _uiManager.UpdateTimeRemaining(_timeRemaining); //update UI
        
        if (_timeRemaining <= 0f) { TriggerGameOver(); _timeRemaining = 0f; } //trigger game over when time runs out
    }

    private void TriggerGameOver() { _uiManager.DisplayGameOver(); }

    private void DoNullChecks() {
        if (_timeRemaining == 0) { _timeRemaining = 120f; Debug.LogError("LevelManager::DoNullChecks() _timeRemaining = 0! Set to 120.");}
        if (_playerStartPosition == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks() _playerStartPosition = Vector3.zero!"); }
        if (_playerStartRotation == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks() _playerStartRotation = Vector3.zero!"); }
        if (_playerCharModelStartRotation == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks() _playerCharModelRotation = Vector3.zero!"); }
        if (_playerFinishPosition == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks _playerFinishPosition = Vector3.zero!"); }
        if (_uiManager == null) { Debug.LogError("LevelManager::DoNullChecks() _uiManager is NULL!"); }
        if (_playerAnimations == null) { Debug.LogError("LevelManager::DoNullChecks() _playerAnimations is NULL!"); }
    }
}

