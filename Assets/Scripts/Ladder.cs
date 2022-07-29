using System;
using System.Collections;
using UnityEngine;

public class Ladder : MonoBehaviour {
    public enum LadderSnapToLocations {bottom, top}
    public enum LadderAngle {topLeftToBottomRight, topRightToBottomLeft}

    [SerializeField] private LadderAngle _ladderAngle;
    [SerializeField] private Transform _snapToBottomPosition;
    [SerializeField] private Transform _snapToTopPosition;
    [SerializeField] private Transform _snapToBottomExitPosition;
    [SerializeField] private Transform _reachedBottomPosition;
    [SerializeField] private Transform _reachedTopPosition;


    public LadderAngle GetLadderAngle() { return _ladderAngle; }
    public Transform GetReachedBottomPosition() { return _reachedBottomPosition; }
    public Transform GetReachedTopPosition() { return _reachedTopPosition; }
    public Transform GetSnapToBottomPosition() { return _snapToBottomPosition; }
    public Transform GetSnapToBottomExitPosition() { return _snapToBottomExitPosition; }
    public Transform GetSnapToTopPosition() { return _snapToTopPosition; }
    
    private void OnEnable() {
        if (_snapToBottomPosition == null || _snapToTopPosition == null || _snapToBottomExitPosition == null) {
            foreach (Transform tran in transform.GetComponentsInChildren<Transform>()) {
                if (tran.name == "SnapToBottomPosition") { _snapToBottomPosition = tran; }
                if (tran.name == "SnapToTopPosition") { _snapToTopPosition = tran; }
                if (tran.name == "SnapToBottomExitPosition") { _snapToBottomExitPosition = tran; }
                if (tran.name == "ReachedBottomPosition") { _reachedBottomPosition = tran; }
                if (tran.name == "ReachedTopPosition") { _reachedTopPosition = tran; }
            }
        }
        DoNullChecks();
    }

    private void DoNullChecks() {
        if (_snapToBottomPosition == null) { Debug.LogError("Ladder::DoNullChecks() _snapToBottomPosition is NULL!"); }
        if (_snapToBottomExitPosition == null) { Debug.LogError("Ladder:DoNullChecks() _snapToBottomExitPosition is NULL!"); }
        if (_snapToTopPosition == null) { Debug.LogError("Ladder::DoNullChecks() _snapToTopPosition is NULL!"); }
        if (_reachedBottomPosition == null) { Debug.LogError("Ladder::DoNullChecks() _reachedBottomPosition is NULL!"); }
        if (_reachedTopPosition == null) { Debug.LogError("Ladder::DoNullChecks() _reachedTopPosition is NULL!"); }
    }
}

