using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPanel : MonoBehaviour {
    [SerializeField] private Transform _panelLightTransform;
    [SerializeField] private Elevator _elevatorScript;
    [SerializeField] private bool _moveElevatorDownOnCall;


    private void Start() { DoNullChecks(); }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !_elevatorScript.GetMovingUp() && !_elevatorScript.GetMovingDown()) {
            //Only turn panel light green if the elevator is not on the same level as the elevator panel already
            if ((_moveElevatorDownOnCall && _elevatorScript.GetCurrentElevatorPosition() == Elevator.ElevatorPosition.top) || 
                !_moveElevatorDownOnCall && _elevatorScript.GetCurrentElevatorPosition() == Elevator.ElevatorPosition.bottom) {
                SetPanelLight(Color.green);
            } 

            if (_moveElevatorDownOnCall) { _elevatorScript.MoveDown(); }
            else { _elevatorScript.MoveUp(); }
        }
    }

    private void SetPanelLight(Color color) {
        MeshRenderer meshRenderer = _panelLightTransform.GetComponent<MeshRenderer>();
        meshRenderer.material.EnableKeyword("_EMISSION");
        meshRenderer.material.SetColor("_EmissionColor", color);
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) { SetPanelLight(Color.red); }
    }

    private void DoNullChecks() {
        if (_panelLightTransform == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _panelLightTransform is NULL!");}
        if (_elevatorScript == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _elevatorScript is NULL!");}
    }
}
