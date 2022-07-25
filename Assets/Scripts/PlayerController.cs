using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    private enum MovePlayerDirectionOnLadder {up, down}
    
    [Header("Player State")]
    [SerializeField] private bool _movementDisabled;
    [Space] [SerializeField] private Vector3 _playerVelocity;
    [Space] [SerializeField] private bool _isGrounded;
    [Space] [SerializeField] private bool _isClimbingLadder;
    [SerializeField] private bool _movePlayerTowardsLadderSnapTo;
    [Space] [SerializeField] private bool _isClimbingLedge;
    [SerializeField] private bool _isHangingFromLedge;
    [Space] [SerializeField] private bool _isWallJumping;
    [SerializeField] private bool _canWallJump;
    [SerializeField] private bool _startJump;
    [Space][Header("Default Values")]
    [SerializeField] private float _playerRunSpeed;
    [SerializeField] private float _playerWalkSpeed;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _gravityValue;
    [SerializeField] private float _snapToMoveSpeed;
    [SerializeField] private float _movePlayerOnLadderSpeed;
    [Space][Header("Current Values")]
    [SerializeField] private int _currNumOfJumps;
    [SerializeField] private MovePlayerDirectionOnLadder _movePlayerDirectionOnLadder;
    [SerializeField] private Ladder.LadderAngle _currentLadderAngle;
    [SerializeField] private Vector3 _ladderSnapToPosition;
    [SerializeField] private Vector3 _ladderReachedEndPosition;
    [Space][Header("References")]
    [SerializeField] private PlayerAnimations _playerAnimations;
    [SerializeField] private LedgeChecker _ledgeChecker;
    private CharacterController _controller;
    private InputActions _inputActions;
    private PlayerAnimations.PlayerCharAnimState _animStatePriorToFirstJump;
    private float _timeSinceWalkStarted;
    private Vector3 _wallCollisionNormal;
    
    
    //Public Methods
    public bool GetMovementDisabled() { return _movementDisabled; }
    public void EnableMovement() { _movementDisabled = false;}
    public void DisableMovement() { _movementDisabled = true; }
    public bool GetHangingInputEnabled() { return _isHangingFromLedge; }
    public void SetHangingInput(bool hangingInputEnabled) { _isHangingFromLedge = hangingInputEnabled; }

    //Init
    private void OnEnable() {
        _inputActions = new InputActions();
        _inputActions.Player.Enable();
        _inputActions.Player.Jump.started += JumpOnStarted;
        _inputActions.Player.Jump.canceled += JumpOnCancelled;
        _inputActions.Player.Use.performed += UseOnPeformed;
        _currNumOfJumps = 0;
        _movementDisabled = _canWallJump = _isWallJumping = _movePlayerTowardsLadderSnapTo = _isClimbingLadder = _isClimbingLedge = false;
    }

    private void Start() {
        _controller = GetComponent<CharacterController>();
        _playerAnimations = transform.GetComponent<PlayerAnimations>();
        
        foreach (Transform tran in transform.GetComponentInChildren<Transform>()) {
            if (tran.CompareTag("LedgeChecker")) { _ledgeChecker = tran.GetComponent<LedgeChecker>(); }
        }
        DoNullChecks();
    }
    
    //Decommission
    private void OnDisable() {
        _inputActions.Player.Jump.started -= JumpOnStarted;
        _inputActions.Player.Jump.canceled -= JumpOnCancelled;
        _inputActions.Player.Use.performed -= UseOnPeformed;
    }
    
    //Event Subscribers
    private void JumpOnStarted(InputAction.CallbackContext context) {
        if (_canWallJump) { //wall jump, reverse animation state between jumping and double jumping
            _canWallJump = false; 
            _isWallJumping = true;
            
            if (_playerAnimations.GetPlayerCharFacingDirection() == PlayerAnimations.PlayerCharFacingDirection.left) {
                _playerAnimations.SetPlayerCharFacingDirection(PlayerAnimations.PlayerCharFacingDirection.right);
            } else { _playerAnimations.SetPlayerCharFacingDirection(PlayerAnimations.PlayerCharFacingDirection.left); }
        } else { //start normal jump
            if (!_isHangingFromLedge) { _startJump = true; transform.SetParent(null); }
        } 
    }
    
    private void JumpOnCancelled(InputAction.CallbackContext context) {
        _currNumOfJumps += 1;
 
        if (_isHangingFromLedge) {
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.hangingDropping);
            _controller.enabled = true;
            _playerVelocity = Vector3.zero;
            _movementDisabled = false;
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle, 2f);
            _isHangingFromLedge = false;
        }
    }

    private void UseOnPeformed(InputAction.CallbackContext context) {
        if (_isHangingFromLedge) {
            float climbDelay = 3.51f;

            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.hangingClimbing);
            StartCoroutine(ClimbDelaySetPlayerGOPosition(climbDelay, true, false));
        }
    }

    private IEnumerator ClimbDelaySetPlayerGOPosition(float climbDelay, bool hangClimbing, bool ladderClimbing) {
        Vector3 finalPlayerGOPosition;
        
        yield return new WaitForSeconds(climbDelay);
        finalPlayerGOPosition = _playerAnimations.GetAnimator().bodyPosition;
        if (hangClimbing) { finalPlayerGOPosition.y += 0.39254f; }
        transform.SetPositionAndRotation(finalPlayerGOPosition, quaternion.identity);
        _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle);
        _isHangingFromLedge = false;
        _playerVelocity = Vector3.zero;
        _controller.enabled = true;
        _movementDisabled = false;
    }
    
    //Physics
    private void FixedUpdate() {
        DeterminePlayerVelocity();
        if (_isClimbingLadder) { MovePlayerOnLadder(); }
        if (_movePlayerTowardsLadderSnapTo) { MovePlayerTowardsLadderSnapTo(); }
    }
    
    private void OnAnimatorIK(int layerIndex) { transform.position = _playerAnimations.GetAnimator().bodyPosition; }
    
    private void OnTriggerEnter(Collider other) {
        if (other.tag.ToLower().StartsWith("moving")) { //Collided with a moving platform or elevator
            if (other.tag.ToLower() == "movingelevator") { transform.parent = other.transform.GetChild(0);
            } else { transform.parent = other.transform; }
        } else if (other.CompareTag("Ladder") && !_isClimbingLedge) { ClimbLadder(other); } //Collided with a ladder
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag.ToLower().StartsWith("moving")) { transform.parent = null; } //Moving platform or elevator
        else if (other.CompareTag("Ladder")) { _isClimbingLedge = false; }  //Reset playerIsClimbing flag
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

    private IEnumerator EndIsWallJumping() { yield return new WaitForSeconds(0.15f); _isWallJumping = false; }
    
    //Game Logic - Movement
    private void DeterminePlayerVelocity() {
        Vector3 moveDirection = _inputActions.Player.Movement.ReadValue<Vector2>();
        bool isMoving;
        
        _isGrounded = _controller.isGrounded;
        
        if (moveDirection.x != 0) { isMoving = true; } 
        else { isMoving = false; }
        
        if (_isGrounded) {
            _currNumOfJumps = 0;
            _canWallJump = _isWallJumping = false;
            _playerVelocity.x = moveDirection.x;

            if (moveDirection.x > 0 && !_movementDisabled) { _playerAnimations.CharFaceRight(); //going right
            } else if (moveDirection.x < 0 && !_movementDisabled) { _playerAnimations.CharFaceLeft(); } //going left
            
            switch (_playerAnimations.GetPlayerCharAnimState()) {
                case PlayerAnimations.PlayerCharAnimState.idle:
                    if (isMoving) { //Move from idle to walking
                        _timeSinceWalkStarted = Time.time;
                        _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.walking);
                        _playerVelocity.x *= _playerWalkSpeed;
                    }
                    break;
                case PlayerAnimations.PlayerCharAnimState.walking:
                    if (isMoving) {
                        if (Time.time - _timeSinceWalkStarted > 0.25f) { //Move from walking to running
                            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.running);
                            _playerVelocity.x *= _playerRunSpeed;
                        } else { _playerVelocity.x *= _playerWalkSpeed; } //keep walking
                    } else { 
                        _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle); //Transition to idle
                    }
                    break;
                case PlayerAnimations.PlayerCharAnimState.running:
                    if (isMoving) { _playerVelocity.x *= _playerRunSpeed; } //keep running
                    else { //slow to walking
                        _timeSinceWalkStarted = Time.time;
                        _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.walking);
                        _playerVelocity.x *= _playerWalkSpeed;
                    }
                    break;
                case PlayerAnimations.PlayerCharAnimState.jumping:
                    _playerAnimations.UpdatePlayerCharAnimState(_animStatePriorToFirstJump);
                    break;
                case PlayerAnimations.PlayerCharAnimState.doubleJumping:
                    _playerAnimations.UpdatePlayerCharAnimState(_animStatePriorToFirstJump);
                    break;
            }
        } else { //is jumping
            if (_isWallJumping) { _playerVelocity.x = _wallCollisionNormal.x * 4; } //horizontal bounce when wall jumping
        } 
        
        if (_isGrounded && _playerVelocity.y < -2) { _playerVelocity.y = 0; } //_playerVelocity.y should never be < -2

        if (_isWallJumping && !_canWallJump) { //wall jump
            if (_playerAnimations.GetPlayerCharAnimState() == PlayerAnimations.PlayerCharAnimState.jumping) {
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.doubleJumping);
            } else { _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.jumping); }
            
            _playerVelocity.y = Mathf.Sqrt(_jumpHeight * -1.25f * _gravityValue); 
        } else if (_startJump && _isGrounded) { //first jump
            _animStatePriorToFirstJump = _playerAnimations.GetPlayerCharAnimState(); //track prior anim state for exitting jump
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.jumping);
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue); 
        } else if (_startJump && !_isGrounded && _currNumOfJumps < 2) { //double jump
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.doubleJumping);
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -2.5f * _gravityValue);
        } 
        
        if (_startJump) { _startJump = false; } //reset _startJump flag
        if (!_isGrounded && _wallCollisionNormal.y == -1f) { _playerVelocity.y = -20f; } //bounce off ceilings
        if (!_isGrounded) { _playerVelocity.y += _gravityValue * Time.deltaTime; } //apply gravity
        if (!_movementDisabled) { MovePlayer(_playerVelocity); } //if movement enabled, move player
    }
    
    private void MovePlayer(Vector3 moveVelocity) { _controller.Move(moveVelocity * Time.deltaTime); }

    private void MovePlayerTowardsLadderSnapTo()
    {
        if (transform.position != _ladderSnapToPosition) {
            transform.position = Vector3.MoveTowards(transform.position, _ladderSnapToPosition, _snapToMoveSpeed * Time.deltaTime);
        } else { _movePlayerTowardsLadderSnapTo = false; }
    }
    
    private void ClimbLadder(Collider other) {
        Ladder ladder = other.GetComponent<Ladder>();
        Transform snapToBottomPosition = ladder.GetSnapToBottomPosition();
        Transform snapToTopPosition = ladder.GetSnapToTopPosition();
        Transform reachedBottomPosition = ladder.GetReachedBottomPosition();
        Transform reachedTopPosition = ladder.GetReachedTopPosition();
        Ladder.LadderSnapToLocations ladderSnapToLocation;

        _currentLadderAngle = ladder.GetLadderAngle();
        _isClimbingLedge = true;
        
        //Determine if at top or bottom
        if ((transform.position.y - _controller.height) < other.transform.position.y) { //player char at bottom
            ladderSnapToLocation = Ladder.LadderSnapToLocations.bottom;
        } else if (transform.position.y > other.transform.position.y) { //player char at top
            ladderSnapToLocation = Ladder.LadderSnapToLocations.top;
        } else {
            ladderSnapToLocation = Ladder.LadderSnapToLocations.bottom;
            Debug.LogError("PlayerController:OnTriggerEnter() Player position relative to ladder unknown! Assumed to be bottom of ladder.");
        }

        //Make player character face ladder correctly
        if (_currentLadderAngle == Ladder.LadderAngle.topLeftToBottomRight) { _playerAnimations.CharFaceLeft(); } //ladder goes from top left to bottom right
        else if (_currentLadderAngle == Ladder.LadderAngle.topRightToBottomLeft) { _playerAnimations.CharFaceRight(); } //ladder goes from top right to bottom left

        //Determine ladder snap to position & reached end position
        if (ladderSnapToLocation == Ladder.LadderSnapToLocations.bottom) { //Snap to bottom of ladder
            _ladderSnapToPosition = snapToBottomPosition.position;
            _ladderReachedEndPosition = reachedTopPosition.position;
            _movePlayerDirectionOnLadder = MovePlayerDirectionOnLadder.up;
        } else if (ladderSnapToLocation == Ladder.LadderSnapToLocations.top) { //Snap to top of ladder
            _ladderSnapToPosition = snapToTopPosition.position;
            _ladderReachedEndPosition = reachedBottomPosition.position;
            _movePlayerDirectionOnLadder = MovePlayerDirectionOnLadder.down;
        }

        DisableMovement();
        _controller.enabled = false;
        
        if (_movePlayerDirectionOnLadder == MovePlayerDirectionOnLadder.up) { //Climb up the ladder
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderClimbingUp);
        } else { _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderClimbingDown); } //Climb down ladder
        
        _movePlayerTowardsLadderSnapTo = true;
        StartCoroutine(ResetPlayerSnapTo());
    }

    private IEnumerator ResetPlayerSnapTo() {
        yield return new WaitForSeconds(0.5f);
        _movePlayerTowardsLadderSnapTo = false;
        _isClimbingLadder = true;
    }
    
    private void MovePlayerOnLadder() {
        if (transform.position != _ladderReachedEndPosition) { //Player has not reached the end of climbing on the ladder
            transform.position = Vector3.MoveTowards(transform.position, _ladderReachedEndPosition, _movePlayerOnLadderSpeed * Time.deltaTime);
        } else { //Player has reached the end of climbing on the ladder
            _isClimbingLadder = false;

            if (_movePlayerDirectionOnLadder == MovePlayerDirectionOnLadder.up) { //Player is climbing up a ladder
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderTopClimb);
                ClimbDelaySetPlayerGOPosition(3.5f, false, true);
                _controller.enabled = true;
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle, 2f);
            } else { //Player is climbing down a ladder
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderDropping);
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle, 0.75f);
                _controller.enabled = true;
                if (_currentLadderAngle == Ladder.LadderAngle.topLeftToBottomRight) { //move player to the right of ladder
                    _playerAnimations.CharFaceRight();
                    MovePlayer(new Vector3(2,0,0));
                } else { //move player to the left of ladder
                    _playerAnimations.CharFaceLeft();
                    MovePlayer(new Vector3(-2,0,0));
                }
            }

            if (_movePlayerDirectionOnLadder == MovePlayerDirectionOnLadder.down) {
                _movementDisabled = false;
            } else { StartCoroutine(DelayEnableMovement(4f)); }
        }
    }

    private IEnumerator DelayEnableMovement(float delay) {
        yield return new WaitForSeconds(delay);
        transform.position = _playerAnimations.GetAnimator().bodyPosition;
        _movementDisabled = false;
    }
    
    private void DoNullChecks() {
        if (_playerRunSpeed <= 0) { _playerRunSpeed = 11; Debug.Log("PlayerController::DoNullChecks() _playerSpeed <= 0! Set to 11."); }
        if (_playerWalkSpeed <= 0) { _playerWalkSpeed = 5; Debug.Log("PlayerController::DoNullChecks() _playerWalkSpeed <= 0! Set to 5."); }
        if (_jumpHeight <= 0) { _jumpHeight = 3; Debug.Log("PlayerController::DoNullChecks() _jumpHeight <= 0! Set to 3."); }
        if (_gravityValue == 0) { _gravityValue = -100; Debug.Log("PlayerController::DoNullChecks() _gravityValue = 0! Set to -100."); }
        if (_snapToMoveSpeed <= 0) { _snapToMoveSpeed = 10; Debug.Log("PlayerController::DoNullChecks() _snapToMoveSpeed <= 0! Set to 10."); }
        if (_movePlayerOnLadderSpeed <= 0) { _movePlayerOnLadderSpeed = 1; Debug.Log("PlayerController::DoNullChecks() _movePlayerOnLadderSpeed is <= 0! Set to 1."); }
        if (_playerAnimations == null) { Debug.LogError("PlayerController::DoNullChecks() _playerAnimations is NULL!");}
        if (_ledgeChecker == null) { Debug.LogError("PlayerController::DoNullChecks() _ledgeChecker is NULL!"); }
    }
}

