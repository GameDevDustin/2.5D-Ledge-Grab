using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Elevator : MonoBehaviour {
    private enum ElevatorPosition { top, bottom }
    [Range(0,10)]
    [SerializeField] private float _elevatorSpeed;
    [SerializeField] private ElevatorPosition _elevatorCurrentPosition;
    [SerializeField] private Vector3 _elevatorTopPosition;
    [SerializeField] private Vector3 _elevatorBottomPosition;
    [SerializeField] private bool _movingUp;
    [SerializeField] private bool _movingdown;


    private void Start() { DoNullChecks(); }

    private void FixedUpdate() {
        if (_movingUp) { MovingUp(); }
        if (_movingdown) { MovingDown(); }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (!_movingUp && !_movingdown) {
                if (_elevatorCurrentPosition == ElevatorPosition.top) { MoveDown(); } else { MoveUp(); }
            }
        }
    }

    public void MoveUp() {
        if (_elevatorCurrentPosition != ElevatorPosition.top) { StartCoroutine(MoveElevatorUp()); }
    }

    private IEnumerator MoveElevatorUp() {
        yield return new WaitForSeconds(2f);
        _movingUp = true;
        _elevatorCurrentPosition = ElevatorPosition.top;
    }

    private void MovingUp() {
        if (transform.position == _elevatorTopPosition) { _movingUp = false; }
        transform.position = Vector3.MoveTowards(transform.position, _elevatorTopPosition, _elevatorSpeed * Time.deltaTime);
    }

    public void MoveDown() {
        if (_elevatorCurrentPosition != ElevatorPosition.bottom) { StartCoroutine(MoveElevatorDown()); }
    }

    private IEnumerator MoveElevatorDown() {
        yield return new WaitForSeconds(2f);
        _movingdown = true;
        _elevatorCurrentPosition = ElevatorPosition.bottom;
    }

    private void MovingDown() {
        if (transform.position == _elevatorBottomPosition) { _movingdown = false;}
        transform.position = Vector3.MoveTowards(transform.position, _elevatorBottomPosition, _elevatorSpeed * Time.deltaTime);
    }
    
    private void DoNullChecks() {
        if (_elevatorSpeed == 0) { _elevatorSpeed = 1; Debug.Log("Elevator::DoNullChecks() _elevatorSpeed is 0! Set to 1."); } 
        if (_elevatorTopPosition == Vector3.zero) { Debug.LogError("Elevator::DoNullChecks() _elevatorTopPosition = Vector3.zero!"); }
        if (_elevatorBottomPosition == Vector3.zero) { Debug.LogError("Elevator::DoNullChecks() _elevatorBottomPosition = Vector3.zero!"); }
    }
}
