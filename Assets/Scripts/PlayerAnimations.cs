using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour {
    public enum PlayerCharAnimState { idle, walking, running, jumping, doubleJumping, jumpToHanging, hangingIdle, hangingDropping, hangingClimbing, ladderClimbingUp, ladderClimbingDown, ladderDropping, ladderTopClimb }
    public enum PlayerCharFacingDirection { left, right }

    [SerializeField] private PlayerCharFacingDirection _playerCharFacingDirection;
    [SerializeField] private PlayerCharAnimState _playerCharAnimState;
    [SerializeField] private Transform _charModelTransform;
    [SerializeField] private Animator _animator;


    public void CharFaceRight() {
        _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(_charModelTransform.rotation.x, 90f, _charModelTransform.rotation.z));
        _playerCharFacingDirection = PlayerCharFacingDirection.right;
    }
    public void CharFaceLeft() {
        _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(_charModelTransform.rotation.x, -90f, _charModelTransform.rotation.z));
        _playerCharFacingDirection = PlayerCharFacingDirection.left;
    }
    public PlayerCharAnimState GetPlayerCharAnimState() { return _playerCharAnimState; }
    public PlayerCharFacingDirection GetPlayerCharFacingDirection() { return _playerCharFacingDirection; }

    public void SetPlayerCharFacingDirection(PlayerCharFacingDirection playerCharFacingDirection) {
        Vector3 newCharModelRotation;
        _playerCharFacingDirection = playerCharFacingDirection;
        
        if (_playerCharFacingDirection == PlayerCharFacingDirection.left) {
            newCharModelRotation = new Vector3(0f, -90, 0f);
            _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(newCharModelRotation));
        } else {
            newCharModelRotation = new Vector3(0f, 90, 0f);
            _charModelTransform.SetPositionAndRotation(_charModelTransform.position, Quaternion.Euler(newCharModelRotation));
        }
    }

    public Animator GetAnimator() { return _animator; }
    
    public void UpdatePlayerCharAnimState(PlayerCharAnimState playerCharAnimState) {
        UpdateCharAnimState(playerCharAnimState);
    }
    
    public void UpdatePlayerCharAnimState(PlayerCharAnimState playerCharAnimState, float delayUpdateExecution) {
        StartCoroutine(DelayUpdatePlayerAnimState(playerCharAnimState,delayUpdateExecution));
    }

    private void UpdateCharAnimState(PlayerCharAnimState playerCharAnimState) {
        ResetAnimatorParameters();
        
        //Update _playerCharAnimState
        _playerCharAnimState = playerCharAnimState;
        
        //Set true new Animator parameter
        switch (playerCharAnimState) {
            case PlayerCharAnimState.idle: _animator.SetBool("isIdle", true); break;
            case PlayerCharAnimState.walking: _animator.SetBool("isWalking", true); break;
            case PlayerCharAnimState.running: _animator.SetBool("isRunning", true); break;
            case PlayerCharAnimState.jumping: _animator.SetBool("isJumping", true); break;
            case PlayerCharAnimState.doubleJumping: _animator.SetBool("isDoubleJumping", true); break;
            case PlayerCharAnimState.jumpToHanging: _animator.SetBool("isJumpToHanging", true); break;
            case PlayerCharAnimState.hangingIdle: _animator.SetBool("isHangingIdle", true); break;
            case PlayerCharAnimState.hangingDropping: _animator.SetBool("isHangingDropping", true); break;
            case PlayerCharAnimState.hangingClimbing: _animator.SetBool("isHangingClimbing", true); break;
            case PlayerCharAnimState.ladderClimbingUp: _animator.SetBool("isClimbingUpLadder", true); break;
            case PlayerCharAnimState.ladderClimbingDown: _animator.SetBool("isClimbingDownLadder", true); break;
            case PlayerCharAnimState.ladderDropping: _animator.SetBool("isLadderDropping", true); break; 
            case PlayerCharAnimState.ladderTopClimb: _animator.SetBool("isClimbingToTop", true); break;
        }
    }

    private IEnumerator DelayUpdatePlayerAnimState(PlayerCharAnimState playerCharAnimState, float delay) {
        yield return new WaitForSeconds(delay);
        UpdateCharAnimState(playerCharAnimState);
    }
    
    private void OnEnable() {
        if (_charModelTransform == null) {
            Transform[] childTransforms = transform.GetComponentsInChildren<Transform>();
            
            foreach(Transform tran in childTransforms) {
                if (tran.CompareTag("PlayerModel")) { _charModelTransform = tran; }
            }
        }
        
        if (_animator == null && _charModelTransform != null) { _animator = _charModelTransform.GetComponent<Animator>(); }
        
        ResetAnimatorParameters();
        _animator.SetBool("isIdle", true);
    }
    
    private void Start() { DoNullChecks(); }

    private void ResetAnimatorParameters() {
        //Reset all Animator parameters to false
        _animator.SetBool("isIdle", false);
        _animator.SetBool("isWalking", false);
        _animator.SetBool("isRunning", false);
        _animator.SetBool("isJumping", false);
        _animator.SetBool("isDoubleJumping", false);
        _animator.SetBool("isJumpToHanging", false);
        _animator.SetBool("isHangingIdle", false);
        _animator.SetBool("isHangingDropping", false);
        _animator.SetBool("isHangingClimbing", false);
        _animator.SetBool("isClimbingUpLadder", false);
        _animator.SetBool("isClimbingDownLadder", false);
        _animator.SetBool("isLadderDropping", false);
        _animator.SetBool("isClimbingToTop", false);
    }
    
    private void DoNullChecks() {
        if (_charModelTransform == null) { Debug.LogError("PlayerAnimations::DoNullChecks() _charModelTransform is NULL!"); }
        if (_animator == null) { Debug.LogError("PlayerAnimations::DoNullChecks() _animator is NULL!"); }
    }
}

