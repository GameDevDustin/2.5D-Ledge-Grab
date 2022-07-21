using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPanel : MonoBehaviour {
    [SerializeField] private Transform _panelLightTransform;
    // [SerializeField] private Material _redLight;
    // [SerializeField] private Material _greenLight;
    [SerializeField] private Elevator _elevatorScript;
    [SerializeField] private bool _moveElevatorDownOnCall;


    private void Start() { DoNullChecks(); }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !_elevatorScript.GetMovingUp() && !_elevatorScript.GetMovingDown()) {
            // _panelLightTransform.GetComponent<MeshRenderer>().material = _greenLight;
            MeshRenderer meshRenderer = _panelLightTransform.GetComponent<MeshRenderer>();
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.green);

            if (_moveElevatorDownOnCall) { _elevatorScript.MoveDown();
            } else { _elevatorScript.MoveUp(); }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            // _panelLightTransform.GetComponent<MeshRenderer>().material = _redLight;
            MeshRenderer meshRenderer = _panelLightTransform.GetComponent<MeshRenderer>();
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.red);
        }
    }

    private void DoNullChecks() {
        if (_panelLightTransform == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _panelLightTransform is NULL!");}
        // if (_redLight == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _redLight is NULL!"); }
        // if (_greenLight == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _greenLight is NULL!"); }
        if (_elevatorScript == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _elevatorScript is NULL!");}
    }
}
