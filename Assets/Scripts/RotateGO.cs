using System;
using System.Collections;
using UnityEngine;

public class RotateGO : MonoBehaviour {
    [SerializeField] private Vector3 _rotationVector;
    [SerializeField] private bool _rotateOnXAxis;
    [SerializeField] private bool _rotateOnYAxis;
    [SerializeField] private bool _rotateOnZAxis;
    [SerializeField] private float _speedOnXAxis;
    [SerializeField] private float _speedOnYAxis;
    [SerializeField] private float _speedOnZAxis;

    private void Start() {
        DoNullChecks();
        if (_rotateOnXAxis) { _rotationVector.x = _speedOnXAxis * Time.deltaTime; }
        if (_rotateOnYAxis) { _rotationVector.y = _speedOnYAxis * Time.deltaTime; }
        if (_rotateOnZAxis) { _rotationVector.z = _speedOnZAxis * Time.deltaTime; }
    }

    private void Update() { transform.Rotate(_rotationVector); }

    private void DoNullChecks() {
        // if (_rotationVector == Vector3.zero) { Debug.Log("RotateGO::DoNullChecks _rotationVector = Vector3.zero(0,0,0)!"); }
    }
}

