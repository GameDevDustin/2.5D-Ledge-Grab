using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimations : MonoBehaviour {
    public enum PlayerCharAnimState { idle, walking, running, jumping, doubleJumping }
    [SerializeField] private PlayerCharAnimState _playerCharAnimState;
    [SerializeField] private Transform _charModelTransform;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerController _playerController;


    private void OnEnable() {
        if (_charModelTransform == null) {
            Transform[] childTransforms = transform.GetComponentsInChildren<Transform>();
            
            foreach(Transform tran in childTransforms) {
                if (tran.CompareTag("PlayerModel")) { _charModelTransform = tran; }
            }
        }
        
        if (_animator == null && _charModelTransform != null) { _animator = _charModelTransform.GetComponent<Animator>(); }
        _animator.SetBool("isIdle", true);
    }
    
    private void Start() {
        if (_playerController == null) { _playerController = transform.GetComponent<PlayerController>(); }
        DoNullChecks();
    }

    public void UpdatePlayerCharAnimState(PlayerCharAnimState playerCharAnimState) {
        //Reset all Animator parameters to false
        _animator.SetBool("isIdle", false);
        _animator.SetBool("isWalking", false);
        _animator.SetBool("isRunning", false);
        _animator.SetBool("isJumping", false);
        _animator.SetBool("isDoubleJumping", false);
        
        //Update _playerCharAnimState
        _playerCharAnimState = playerCharAnimState;
        
        //Set true new Animator parameter
        switch (playerCharAnimState) {
            case PlayerCharAnimState.idle: _animator.SetBool("isIdle", true); break;
            case PlayerCharAnimState.walking: _animator.SetBool("isWalking", true); break;
            case PlayerCharAnimState.running: _animator.SetBool("isRunning", true); break;
            case PlayerCharAnimState.jumping: _animator.SetBool("isJumping", true); break;
            case PlayerCharAnimState.doubleJumping: _animator.SetBool("isDoubleJumping", true); break;
        }
    }

    public void CharFaceRight() {
        _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(_charModelTransform.rotation.x, 90f, _charModelTransform.rotation.z));
    }

    public void CharFaceLeft() {
        _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(_charModelTransform.rotation.x, -90f, _charModelTransform.rotation.z));
    }
    
    private void DoNullChecks() {
        if (_charModelTransform == null) { Debug.LogError("PlayerAnimations::DoNullChecks() _charModelTransform is NULL!"); }
        if (_animator == null) { Debug.LogError("PlayerAnimations::DoNullChecks() _animator is NULL!"); }
        if (_playerController == null) { Debug.LogError("PlayerAnimations::DoNullChecks() _playerController is NULL!"); }
    }
}

