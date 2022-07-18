using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour {
    private CharacterController _controller;
    private InputActions _inputActions;
    [SerializeField] private PlayerAnimations.PlayerCharAnimState _playerCharAnimState;
    [SerializeField] private Vector3 _playerVelocity;
    [SerializeField] private bool _isGrounded;
    [Space]
    [SerializeField] private float _playerRunSpeed;
    [SerializeField] private float _playerWalkSpeed;
    [SerializeField] private float _jumpHeight;
    [Space]
    [SerializeField] private bool _startJump;
    [SerializeField] private int _currNumOfJumps;
    [SerializeField] private bool _canWallJump;
    [SerializeField] bool _isWallJumping;
    [Space]
    [SerializeField] private float _gravityValue;
    [Space]
    [SerializeField] private bool _movementDisabled;
    [Space] [SerializeField] private PlayerAnimations _playerAnimations;
    private float _timeSinceWalkStarted;
    private PlayerAnimations.PlayerCharAnimState _animStatePriorToFirstJump;
    private Vector3 _wallCollisionNormal;
    

    private void OnEnable() {
        _inputActions = new InputActions();
        _inputActions.Player.Enable();
        _inputActions.Player.Movement.performed += MovementOnPerformed;
        _inputActions.Player.Jump.started += JumpOnStarted;
        _inputActions.Player.Jump.canceled += JumpOnCancelled;
        _currNumOfJumps = 0;
        _movementDisabled = false;
        _canWallJump = false;
        _isWallJumping = false;
        _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.idle;
    }

    private void OnDisable() {
        _inputActions.Player.Movement.performed -= MovementOnPerformed;
        _inputActions.Player.Jump.canceled -= JumpOnCancelled;
    }

    private void MovementOnPerformed(InputAction.CallbackContext context) { }

    private void JumpOnStarted(InputAction.CallbackContext context) {
        if (_canWallJump) { //wall jump, reverse animation state between jumping and double jumping
            _canWallJump = false; 
            _isWallJumping = true; 
        } else { //start normal jump
            _startJump = true;
            transform.SetParent(null);
        } 
    }
    
    private void JumpOnCancelled(InputAction.CallbackContext context) {
        _currNumOfJumps += 1;
    }
    
    void Start() {
        _controller = GetComponent<CharacterController>();
        _playerAnimations = transform.GetComponent<PlayerAnimations>();
        DoNullChecks();
    }
    
    void FixedUpdate() { DeterminePlayerVelocity(); UpdatePlayerAnimationState(); }

    private void DeterminePlayerVelocity() {
        Vector3 moveDirection = _inputActions.Player.Movement.ReadValue<Vector2>();
        bool isMoving;
        
        _isGrounded = _controller.isGrounded;
        
        if (moveDirection.x != 0) { isMoving = true; } 
        else { isMoving = false; }
        
        if (_isGrounded) {
            _currNumOfJumps = 0;
            _canWallJump = false;
            _isWallJumping = false;
            _playerVelocity.x = moveDirection.x;

            if (moveDirection.x > 0) { //going right
                _playerAnimations.CharFaceRight();
            } else if (moveDirection.x < 0) { //going left
                _playerAnimations.CharFaceLeft();
            }
            
            switch (_playerCharAnimState) {
                case PlayerAnimations.PlayerCharAnimState.idle:
                    if (isMoving) { //Move from idle to walking
                        _timeSinceWalkStarted = Time.time;
                        _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.walking;
                        _playerVelocity.x *= _playerWalkSpeed;
                    }
                    break;
                case PlayerAnimations.PlayerCharAnimState.walking:
                    if (isMoving) {
                        if (Time.time - _timeSinceWalkStarted > 0.25f) { //Move from walking to running
                            _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.running;
                            _playerVelocity.x *= _playerRunSpeed;
                        } else { _playerVelocity.x *= _playerWalkSpeed; } //keep walking
                    } else { 
                        _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.idle; //Transition to idle
                    }
                    break;
                case PlayerAnimations.PlayerCharAnimState.running:
                    if (isMoving) { _playerVelocity.x *= _playerRunSpeed; } //keep running
                    else { //slow to walking
                        _timeSinceWalkStarted = Time.time;
                        _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.walking;
                        _playerVelocity.x *= _playerWalkSpeed;
                    }
                    break;
                case PlayerAnimations.PlayerCharAnimState.jumping:
                    _playerCharAnimState = _animStatePriorToFirstJump;
                    break;
                case PlayerAnimations.PlayerCharAnimState.doubleJumping:
                    _playerCharAnimState = _animStatePriorToFirstJump;
                    break;
            }
        } else {
            if (_isWallJumping) { _playerVelocity.x = _wallCollisionNormal.x * 4; } //horizontal bounce when wall jumping
        } 
        
        if (_isGrounded && _playerVelocity.y < -2) { _playerVelocity.y = 0; } //_playerVelocity.y should never be < -2

        if (_isWallJumping && !_canWallJump) { //wall jump
            if (_playerCharAnimState == PlayerAnimations.PlayerCharAnimState.jumping) {
                _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.doubleJumping;
            } else { _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.jumping; }
            _playerVelocity.y = Mathf.Sqrt(_jumpHeight * -1.25f * _gravityValue); 
        } else if (_startJump && _isGrounded) { //first jump
            _animStatePriorToFirstJump = _playerCharAnimState; //track prior anim state for exit or use x.velocity to determine landing idle/walk/run ?
            _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.jumping;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue); 
        } else if (_startJump && !_isGrounded && _currNumOfJumps < 2) { //double jump
            _playerCharAnimState = PlayerAnimations.PlayerCharAnimState.doubleJumping;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -2.5f * _gravityValue);
        } 
        
        if (_startJump) { _startJump = false; } //reset _startJump flag
        if (!_isGrounded && _wallCollisionNormal.y == -1f) { _playerVelocity.y = -20f; } //bounce off ceilings
        if (!_isGrounded) { _playerVelocity.y += _gravityValue * Time.deltaTime; } //apply gravity
        if (!_movementDisabled) { MovePlayer(_playerVelocity); }
    }

    private void MovePlayer(Vector3 moveVelocity) { _controller.Move(moveVelocity * Time.deltaTime); }

    public void EnableMovement() { _movementDisabled = false;}
    public void DisableMovement() { _movementDisabled = true; }

    private void OnTriggerEnter(Collider other) {
        if (other.tag.ToLower().StartsWith("moving")) { transform.parent = other.transform; }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag.ToLower().StartsWith("moving")) { transform.parent = null; }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody rigidBody = hit.collider.attachedRigidbody;
        
        if (!_isGrounded) { _wallCollisionNormal = hit.normal; }
        
        if (!_isGrounded && hit.transform.CompareTag("JumpableWall")) {
            _canWallJump = true;
            StartCoroutine(EndIsWallJumping());
            return;
        }

        if (rigidBody != null && !rigidBody.isKinematic && hit.normal.y == 0f) {
            Vector3 pushDirection = new Vector3(hit.moveDirection.x * (_playerVelocity.x / 2), 0, 0);
            
            rigidBody.velocity = pushDirection;
        }
    }

    private IEnumerator EndIsWallJumping() {
        yield return new WaitForSeconds(0.15f);
        _isWallJumping = false;
    }

    private void UpdatePlayerAnimationState() { _playerAnimations.UpdatePlayerCharAnimState(_playerCharAnimState); }
    
    private void DoNullChecks() {
        if (_playerRunSpeed <= 0) { _playerRunSpeed = 1; Debug.Log("PlayerController::DoNullChecks() _playerSpeed <= 0! Set to 1."); }
        if (_jumpHeight <= 0) { _jumpHeight = 1; Debug.Log("PlayerController::DoNullChecks() _jumpHeight <= 0! Set to 1."); }
        if (_gravityValue == 0) { _gravityValue = -100; Debug.Log("PlayerController::DoNullChecks() _gravityValue <= 0! Set to -100."); }
        if (_playerAnimations == null) {Debug.LogError("PlayerController::DoNullChecks() _playerAnimations is NULL!");}
    }
}