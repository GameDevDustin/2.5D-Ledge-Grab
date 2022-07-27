using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    private enum MovePlayerDirectionOnLadder {up, down}
    
    [Header("Player Character State")]
    [Tooltip("Determines whether player input for character movement is enabled.")]
    [SerializeField] private bool _movementDisabled;
    [Space] [Tooltip("Player character's current velocity.")]
    [SerializeField] private Vector3 _playerVelocity;
    [Space] [Tooltip("Linked to Character Controller component; determines if character is considered to be on ground.")]
    [SerializeField] private bool _isGrounded;
    [Space] [Tooltip("Character is climbing.")] 
    [SerializeField] private bool _isClimbingLadder;
    [Tooltip("Character is being snapped to a ladder.")]
    [SerializeField] private bool _moveTowardsLadderSnapTo;
    [Space] [Tooltip("Character is climbing a ledge.")]
    [SerializeField] private bool _isClimbingLedge;
    [Tooltip("Character is hanging from a ledge.")]
    [SerializeField] private bool _isHangingFromLedge;
    [Space] [Tooltip("Character is wall jumping.")] 
    [SerializeField] private bool _isWallJumping;
    [Tooltip("Character has jumped and made contact with a jumpable wall, thus can jump off of wall.")]
    [SerializeField] private bool _canWallJump;
    [Tooltip("Jump Action is also used for dropping from ledges. This differentiates between dropping and also represents the first jump when grounded.")]
    [SerializeField] private bool _startJump;
    [Space][Header("Default Values")]
    [Tooltip("Character's default run speed in relation to physics, not animation.")]
    [SerializeField] private float _playerRunSpeed;
    [Tooltip("Character's default walk speed in relation to physics, not animation.")]
    [SerializeField] private float _playerWalkSpeed;
    [Tooltip("Character's default jump height for physics interactions.")]
    [SerializeField] private float _jumpHeight;
    [Tooltip("Default gravity value for physics interactions.")]
    [SerializeField] private float _gravityValue;
    [Tooltip("Default move speed in relation to physics when snapping character to objects.")]
    [SerializeField] private float _snapToMoveSpeed;
    [Tooltip("Default move speed in relation to physics when moving character up/down a ladder.")]
    [SerializeField] private float _movePlayerOnLadderSpeed;
    [Space][Header("Current Values")]
    [Tooltip("Tracks number of jumps since character was grounded to cap jumping to only two jumps.")]
    [SerializeField] private int _currNumOfJumps;
    [Tooltip("Determines what direction a character is moving on a ladder.")]
    [SerializeField] private MovePlayerDirectionOnLadder _movePlayerDirectionOnLadder;
    [Tooltip("To determine which direction the character should be facing, current ladder angle must be known.")]
    [SerializeField] private Ladder.LadderAngle _currentLadderAngle;
    [Tooltip("The current position on the ladder the character will snap to before climbing.")]
    [SerializeField] private Vector3 _ladderSnapToPosition;
    [Tooltip("The position on the ladder when the character should transition to an exit sequence.")]
    [SerializeField] private Vector3 _ladderReachedEndPosition;
    [Space][Header("References")]
    [Tooltip("The PlayerAnimations class handles character animations. Reference needed for physics interactions.")]
    [SerializeField] private PlayerAnimations _playerAnimations;
    [Tooltip("The child game object positioned above the character model whose collisions with ledge triggers initiates ledge hanging mechanic.")]
    [SerializeField] private LedgeChecker _ledgeChecker;
    private CharacterController _controller;
    private InputActions _inputActions;
    private PlayerAnimations.PlayerCharAnimState _animStatePriorToFirstJump;
    private float _timeSinceWalkStarted;
    private Vector3 _wallCollisionNormal;
    
    
    //Public Methods
    public bool GetMovementDisabled() { return _movementDisabled; }
    public void EnableMovement() { _movementDisabled = false; _controller.enabled = true; }
    public void EnableMovement(float delay) { StartCoroutine(DelayEnableMovement(delay)); }
    private IEnumerator DelayEnableMovement(float delay) { yield return new WaitForSeconds(delay); _movementDisabled = false; _controller.enabled = true; }
    public void DisableMovement() { _movementDisabled = true; _controller.enabled = false; }
    public void DisableMovement(float delay) { StartCoroutine(DelayDisableMovement(delay)); }
    private IEnumerator DelayDisableMovement(float delay) { yield return new WaitForSeconds(delay); _movementDisabled = true; _controller.enabled = false; }
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
        _movementDisabled = _canWallJump = _isWallJumping = _moveTowardsLadderSnapTo = _isClimbingLadder = _isClimbingLedge = false;
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
            EnableMovement();
            _playerVelocity = Vector3.zero;
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle, .25f);
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
        
        yield return new WaitForSeconds(climbDelay - 0.4f); //Must trigger before animation completes or player animation screws up
        finalPlayerGOPosition = _playerAnimations.GetAnimator().bodyPosition;
        transform.SetPositionAndRotation(finalPlayerGOPosition, quaternion.identity);
        _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle);
        _playerVelocity = Vector3.zero;
        _isHangingFromLedge = false;
        EnableMovement();
    }
    
    //Physics
    private void FixedUpdate() {
        DeterminePlayerVelocity();
        if (_isClimbingLadder) { MovePlayerOnLadder(); }
        if (_moveTowardsLadderSnapTo) { MovePlayerTowardsLadderSnapTo(); }
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
            //cycle between jump and double jump animation for wall jumping
            if (_playerAnimations.GetPlayerCharAnimState() == PlayerAnimations.PlayerCharAnimState.jumping) {
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.doubleJumping);
            } else { _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.jumping); }
            
            //Apply velocities appropriate for wall jumping
            _playerVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravityValue);
            _playerVelocity.x *= 1.5f;
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

    private void MovePlayer(Vector3 moveVelocity, float delay) { StartCoroutine(DelayMovePlayer(moveVelocity, delay)); }

    private IEnumerator DelayMovePlayer(Vector3 moveVelocity, float delay) {
        yield return new WaitForSeconds(delay);
        _controller.Move(moveVelocity * Time.deltaTime);
    }

    private void MovePlayerTowardsLadderSnapTo()
    {
        if (transform.position != _ladderSnapToPosition) {
            transform.position = Vector3.MoveTowards(transform.position, _ladderSnapToPosition, _snapToMoveSpeed * Time.deltaTime);
        } else { _moveTowardsLadderSnapTo = false; }
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
        } else if (transform.position.y > reachedTopPosition.position.y) { //player char at top
            ladderSnapToLocation = Ladder.LadderSnapToLocations.top;
        } else if (transform.position.y > reachedBottomPosition.position.y) { //player char in between reached positions
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderDropping); //Slide char down ladder
            ladderSnapToLocation = Ladder.LadderSnapToLocations.bottom;
        } else { ladderSnapToLocation = Ladder.LadderSnapToLocations.bottom; } //Must have else statement to avoid compile error

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

        if (_movePlayerDirectionOnLadder == MovePlayerDirectionOnLadder.up) { //Climb up the ladder
            _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderClimbingUp);
        } else { _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderClimbingDown); } //Climb down ladder
        
        _moveTowardsLadderSnapTo = true;
        StartCoroutine(ResetPlayerSnapTo());
    }

    private IEnumerator ResetPlayerSnapTo() {
        yield return new WaitForSeconds(0.5f);
        _moveTowardsLadderSnapTo = false;
        _isClimbingLadder = true;
    }
    
    private void MovePlayerOnLadder() {
        if (transform.position != _ladderReachedEndPosition) { //Player has not reached the end of climbing on the ladder
            transform.position = Vector3.MoveTowards(transform.position, _ladderReachedEndPosition, _movePlayerOnLadderSpeed * Time.deltaTime);
        } else { //Player has reached the end of climbing on the ladder
            _isClimbingLadder = false;

            if (_movePlayerDirectionOnLadder == MovePlayerDirectionOnLadder.up) { //Player is climbing up the ledge at top of ladder
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderTopClimb);
                StartCoroutine(ClimbDelaySetPlayerGOPosition(3f, false, true)); //Delay should be ~ animation duration
            } else { //Player is dropping down from a ladder
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.ladderDropping);
                _playerAnimations.UpdatePlayerCharAnimState(PlayerAnimations.PlayerCharAnimState.idle, 0.25f);
            }

            if (_movePlayerDirectionOnLadder == MovePlayerDirectionOnLadder.down) { //Execute after idle anim plays from above
                EnableMovement(0.27f); 
                if (_currentLadderAngle == Ladder.LadderAngle.topLeftToBottomRight) { _playerAnimations.CharFaceRight(); //face char away from ladder
                } else { _playerAnimations.CharFaceLeft(); } //face char away from ladder
            }
        }
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

