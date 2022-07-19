using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeChecker : MonoBehaviour {
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CharacterController _charController;
    [SerializeField] private PlayerAnimations _playerAnimations;
    [SerializeField] private Transform _ledgeSnapToTransform;
    [SerializeField] private bool _movePlayerTowardsSnapTo;
    [SerializeField] private float _snapToMoveSpeed;
    

    private void Start() {
        _playerTransform = transform.parent;
        _playerController = _playerTransform.GetComponent<PlayerController>();
        _charController = _playerTransform.GetComponent<CharacterController>();
        _playerAnimations = _playerTransform.GetComponent<PlayerAnimations>();
        DoNullChecks();
    }

    private void FixedUpdate() { if (_movePlayerTowardsSnapTo) { MovePlayerTowardsSnapTo(); } }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("LedgeTrigger")) {
            foreach (Transform tran in other.GetComponentInChildren<Transform>()) {
                if (tran.CompareTag("LedgeSnapTo")) { _ledgeSnapToTransform = tran; }
            }

            if (_ledgeSnapToTransform != null) { _movePlayerTowardsSnapTo = true; }

            _playerController.DisableMovement();
            DisableCharController();
            _playerController.SetPlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.jumpToHanging);
            StartCoroutine(HangDelay());
        }
    }

    private IEnumerator HangDelay() {
        yield return new WaitForSeconds(0.5f);
        _playerController.SetHangingInput(true);
    }

    private void MovePlayerTowardsSnapTo() {
        if (_playerTransform.position != _ledgeSnapToTransform.position) {
            _playerTransform.position = Vector3.MoveTowards(_playerTransform.position, _ledgeSnapToTransform.position, _snapToMoveSpeed * Time.deltaTime);
        } else { _movePlayerTowardsSnapTo = false; }
    }

    private void DisableCharController() { _charController.enabled = false; }

    private void DoNullChecks() {
        if (_playerTransform == null) { Debug.LogError("LedgeChecker::DoNullChecks() _playerTransform is NULL!"); }
        if (_playerController == null) { Debug.LogError("LedgeChecker::DoNullChecks() _playerController is NULL!"); }
        if (_charController == null) { Debug.LogError("LedgeChecker::DoNullChecks() _charController is NULL!"); }
        if (_playerAnimations == null) { Debug.LogError("LedgeChecker::DoNullChecks() _playerAnimations is NULL!"); }
        if (_snapToMoveSpeed == 1f) { Debug.Log("LedgeChecker::DoNullChecks() _snapToMoveSpeed = 0! Set to 1."); }
    }
}

