using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [SerializeField] private Vector3 _playerStartPosition;
    [SerializeField] private Vector3 _playerStartRotation;
    [SerializeField] private Vector3 _playerCharModelStartRotation;
    [SerializeField] private Vector3 _playerFinishPosition;
    [SerializeField] private PlayerAnimations _playerAnimations;

    public Vector3 GetPlayerStartPosition() { return _playerStartPosition; }
    public Vector3 GetPlayerFinishPosition() { return _playerFinishPosition; }
    public Vector3 GetPlayerStartRotation() { return _playerStartRotation; }
    public Vector3 GetPlayerCharModelStartRotation() { return _playerCharModelStartRotation; }
    
    private void Start() { SetPlayerCharFacingDirection(); DoNullChecks(); }

    private void SetPlayerCharFacingDirection() {
        if (_playerCharModelStartRotation.y == -90) { //facing left
            _playerAnimations.SetPlayerCharFacingDirection(PlayerAnimations.PlayerCharFacingDirection.left);
        } else if (_playerCharModelStartRotation.y == 90) { //facing right
            _playerAnimations.SetPlayerCharFacingDirection(PlayerAnimations.PlayerCharFacingDirection.right);
        } else { Debug.LogError("LevelManager::SetPlayerCharFacingDirection() _playerCharModelStartRotation.y has an invalid value!"); }
    }
    
    private void DoNullChecks() {
       if (_playerStartPosition == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks() _playerStartPosition = Vector3.zero!"); }
       if (_playerStartRotation == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks() _playerStartRotation = Vector3.zero!");}
       if (_playerCharModelStartRotation == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks() _playerCharModelRotation = Vector3.zero!");}
       if (_playerFinishPosition == Vector3.zero) { Debug.Log("LevelManager::DoNullChecks _playerFinishPosition = Vector3.zero!"); }
       if (_playerAnimations == null) { Debug.LogError("LevelManager::DoNullChecks() _playerAnimations is NULL!");}
   }
}

