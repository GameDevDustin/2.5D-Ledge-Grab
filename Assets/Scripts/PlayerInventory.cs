using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    [SerializeField] private int _numOfLives;
    [SerializeField] private int _defaultNumOfLives;
    [SerializeField] private int _numOfCoins;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private UI_Manager _uiManager;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private Transform _charModelTransform;
    [Space] [SerializeField] private PlayerAnimations _playerAnimations;


    private void Start() {
        if (_charModelTransform == null) {
            Transform[] childTransforms = transform.GetComponentsInChildren<Transform>();
            
            foreach(Transform tran in childTransforms) {
                if (tran.CompareTag("PlayerModel")) { _charModelTransform = tran; }
            }
        }

        _playerAnimations = transform.GetComponent<PlayerAnimations>();
        DoNullChecks();
        _uiManager.UpdateLivesCount(_numOfLives);
        _uiManager.UpdateCoinCount(_numOfCoins);
        
        if (transform.position != _levelManager.GetPlayerStartPosition() //check if Player GO at start position
            || transform.eulerAngles != _levelManager.GetPlayerStartRotation() //check if PlayerGO at start rotation
            || _charModelTransform.eulerAngles != _levelManager.GetPlayerCharModelStartRotation())  //check if Player CharModel at Start Rotation
        { StartCoroutine(RespawnPlayer()); }
    }

    private void FixedUpdate() { if (transform.position.y < -50f) { OnDeath(); } }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Collectable") && other.name.ToLower().StartsWith("coin")) {
            Coin coinScript = other.GetComponent<Coin>();
        
            _numOfCoins += coinScript.GetNumOfCoins();
            _uiManager.UpdateCoinCount(_numOfCoins);
            coinScript.HideCollectable();
        }
    }

    private void OnDeath() {
        _numOfLives -= 1;
        _uiManager.UpdateLivesCount(_numOfLives);

        if (_numOfLives <= 0) { //Show Game Over
            transform.GetComponent<MeshRenderer>().enabled = false;
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            transform.position = _levelManager.GetPlayerStartPosition();
            _playerController.DisableMovement();
            _uiManager.DisplayGameOver();
        }else { //Respawn player at start of level
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle);
            _charModelTransform.gameObject.SetActive(false);
            StartCoroutine(RespawnPlayer());
        }
    }

    private IEnumerator RespawnPlayer() {
        _playerController.DisableMovement();
        transform.SetPositionAndRotation(_levelManager.GetPlayerStartPosition(), Quaternion.Euler(_levelManager.GetPlayerStartRotation()));
        _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(_levelManager.GetPlayerCharModelStartRotation()));
        yield return new WaitForSeconds(3f);
        _charModelTransform.gameObject.SetActive(true);
        _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle);
        _playerController.EnableMovement();
    }

    public int GetNumOfCoins() { return _numOfCoins; }
    
    private void DoNullChecks() {
        if (_playerController == null) { Debug.LogError("PlayerInventory::DoNullChecks() _playerController is NULL!"); }
        if (_uiManager == null) { Debug.LogError("PlayerInventory::DoNullChecks() _uiManager is NULL!"); }
        if (_levelManager == null) { Debug.LogError("PlayerInventory::DoNullChecks() _levelManager is NULL!"); }
        if (_defaultNumOfLives == 0) { _defaultNumOfLives = 1; Debug.LogError("_defaultNumOfLives = 0! Set to 1."); }
        if (_numOfLives == 0) { _numOfLives = _defaultNumOfLives; Debug.Log("PlayerInventory::DoNullChecks() _numOfLives = 0! Set to _defaultNumOfLives.");}
    }
}
