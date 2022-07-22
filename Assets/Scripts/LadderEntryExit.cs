using System;
using System.Collections;
using UnityEngine;

public class LadderEntryExit : MonoBehaviour {
    public enum EntryExitLocation { Bottom, Top }

    // [SerializeField] private EntryExitLocation _entryExitLocation;
    [SerializeField] private LadderEntryExit _bottomLadderEntryExit;
    [SerializeField] private LadderEntryExit _topLadderEntryExit;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _snapToTransform;
    [SerializeField] private Transform _reachedTopLocationTransform;
    [SerializeField] private bool _movePlayerTowardsSnapTo;
    [SerializeField] private float _snapToMoveSpeed;
    [SerializeField] private bool _movePlayerUpLadder;
    [SerializeField] private float _movePlayerUpLadderSpeed;
    [SerializeField] private PlayerAnimations _playerAnimations;


    // public EntryExitLocation GetEntryExitLocation() { return _entryExitLocation; }
    public void SetMovePlayerTowardsSnapTo(bool movePlayerTowardsSnapTo) { _movePlayerTowardsSnapTo = movePlayerTowardsSnapTo; }
    public bool GetMovePlayerTowardsSnapTo() { return _movePlayerTowardsSnapTo; }
    public void SetMovePlayerUpLadder(bool movePlayerUpLadder) { _movePlayerUpLadder = movePlayerUpLadder; }
    public bool GetMovePlayerUpLadder() { return _movePlayerUpLadder; }

    private void Start() { DoNullChecks(); }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // PlayerController playerController; // = other.GetComponent<PlayerController>();

            _playerTransform = other.transform;
            _characterController = _playerTransform.GetComponent<CharacterController>();
            _playerController = _playerTransform.GetComponent<PlayerController>();
            _playerAnimations = _playerTransform.GetComponent<PlayerAnimations>();
            
            // if (_entryExitLocation == EntryExitLocation.Bottom) {

                if (_snapToTransform != null) {

                    _playerController.DisableMovement();
                    _characterController.enabled = false;
                    _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderClimbing);
                    _movePlayerTowardsSnapTo = true;
                    StartCoroutine(ResetPlayerSnapTo());
                    _movePlayerUpLadder = true;
                } else { Debug.LogError("LadderEntryExit::OnTriggerEnter() _snapToTransform is NULL!");}
            // }
            // else if (_entryExitLocation == EntryExitLocation.Top) {
                //Trigger climb to top animation
                // _bottomLadderEntryExit.SetMovePlayerUpLadder(false);
                // _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderTopClimb);
                // Debug.Log("fired5");
            // }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("LedgeChecker")) {
            _movePlayerUpLadder = false;
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderTopClimb);
            StartCoroutine(_playerController.ClimbDelaySetPlayerGOPosition(3.5f));
        }
    }

    private IEnumerator ResetPlayerSnapTo() {
        yield return new WaitForSeconds(0.5f);
        _movePlayerTowardsSnapTo = false;
    }

    private void FixedUpdate() {
        if (_movePlayerTowardsSnapTo) { MovePlayerTowardsSnapTo(); }
        if (_movePlayerUpLadder) { MovePlayerUpLadder(); }

        // if (_playerTransform != null) {
        //     Debug.Log("Player position = " + _playerTransform.position + " _snapToTransform position = " + _snapToTransform.position + "_reachedTopLocationTransform position = " + _reachedTopLocationTransform.position);
        // }
    }
    
    private void MovePlayerTowardsSnapTo() {
        if (_playerTransform.position != _snapToTransform.position) {
            _playerTransform.position = Vector3.MoveTowards(_playerTransform.position, _snapToTransform.position, _snapToMoveSpeed * Time.deltaTime);
        } else { _movePlayerTowardsSnapTo = false; }
    }

    private void MovePlayerUpLadder() {
        if (_playerTransform.position != _reachedTopLocationTransform.position) {
            _playerTransform.position = Vector3.MoveTowards(_playerTransform.position,
                _reachedTopLocationTransform.position, _movePlayerUpLadderSpeed * Time.deltaTime);
        } else {
            _movePlayerUpLadder = false;
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderTopClimb);
        }
    }

    private void DoNullChecks() {
        if (_snapToTransform == null) { Debug.LogError("LadderEntryExit::DoNullChecks() _snapToTransform is NULL!"); }
        if (_reachedTopLocationTransform == null) { Debug.LogError("LadderEntryExit::DoNullChecks() _reachedTopLocationTransform is NULL!"); }
        if (_snapToMoveSpeed == 0) { _snapToMoveSpeed = 5f; Debug.Log("LadderEntryExit::DoNullChecks() _snapToMoveSpeed = 0! Set to 5."); } 
        if (_movePlayerUpLadderSpeed == 0f) { _movePlayerUpLadderSpeed = 3f; Debug.Log("LadderEntryExit::DoNullChecks() _movePlayerUpLadderSpeed = 0! Set to 3."); }
    }
}


//reenable charController when drops from ladder or end of climb up top animation

