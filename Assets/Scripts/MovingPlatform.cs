using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Vector3[] _waypoints;
    [SerializeField] private int _currMoveToWP;
    [Range(0,10)]
    [SerializeField] private float _platformSpeed;

    private void Start() {
        DoNulLChecks();
        _currMoveToWP = 0;
    }

    private void FixedUpdate() {
        MovePlatform();
    }

    private void MovePlatform() {
        if (transform.position == _waypoints[_currMoveToWP]) { ReachedWaypoint(); }
        else {
            transform.position = Vector3.MoveTowards(transform.position, _waypoints[_currMoveToWP], _platformSpeed * Time.deltaTime);
        }
    }

    private void ReachedWaypoint() {
        if (_currMoveToWP + 1 == _waypoints.Length) { _currMoveToWP -= 1;
        } else { _currMoveToWP += 1; }
    }

    private void DoNulLChecks() {
        if (_waypoints.Length == 0 || _waypoints[0] == Vector3.zero) { Debug.LogError("MovingPlatforms::DoNullChecks() _waypoints.Length = 0 or _waypoints[0] = Vector3.zero!"); }
        if (_waypoints.Length == 1) { Debug.LogError("MovingPlatforms::DoNullChecks() _waypoints.Length = 1! Must be > 1!");}
    }
}
