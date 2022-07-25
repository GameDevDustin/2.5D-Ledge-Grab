using System;
using System.Collections;
using UnityEngine;

public class Ladder : MonoBehaviour {
    public enum LadderSnapToLocations {bottom, top}
    public enum LadderAngle {topLeftToBottomRight, topRightToBottomLeft}

    [SerializeField] private LadderAngle _ladderAngle;
    [SerializeField] private Transform _snapToBottomPosition;
    [SerializeField] private Transform _snapToTopPosition;
    [SerializeField] private Transform _reachedBottomPosition;
    [SerializeField] private Transform _reachedTopPosition;
    [Space] [Space] [SerializeField] private Transform _playerTransform;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CharacterController _characterController;
    [Space] [SerializeField] private bool _otherIsPlayer;


    public LadderAngle GetLadderAngle() { return _ladderAngle; }
    public Transform GetReachedBottomPosition() { return _reachedBottomPosition; }
    public Transform GetReachedTopPosition() { return _reachedTopPosition; }
    public Transform GetSnapToBottomPosition() { return _snapToBottomPosition; }
    public Transform GetSnapToTopPosition() { return _snapToTopPosition; }
    
    private void OnEnable() {
        if (_snapToBottomPosition == null || _snapToTopPosition == null) {
            foreach (Transform tran in transform.GetComponentsInChildren<Transform>()) {
                if (tran.name == "SnapToBottomPosition") {
                    _snapToBottomPosition = tran;
                }
                if (tran.name == "SnapToTopPosition") {
                    _snapToTopPosition = tran;
                }
            }
        }
        _otherIsPlayer = false;
        DoNullChecks();
    }

    private void DoNullChecks() {
        if (_snapToBottomPosition == null) { Debug.LogError("Ladder::DoNullChecks() _snapToBottomPosition is NULL!"); }
        if (_snapToTopPosition == null) { Debug.LogError("Ladder::DoNullChecks() _snapToTopPosition is NULL!"); }
    }
}

