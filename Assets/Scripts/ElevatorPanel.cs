using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPanel : MonoBehaviour
{
    [SerializeField] private Transform _panelLightTransform;
    [SerializeField] private Elevator _elevatorScript;


    private void Start() { DoNullChecks(); }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && (other.GetComponent<PlayerInventory>().GetNumOfCoins() > 7)) {
            _panelLightTransform.GetComponent<MeshRenderer>().material.color = Color.green;
            _elevatorScript.MoveDown();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) { _panelLightTransform.GetComponent<MeshRenderer>().material.color = Color.red; }
    }

    private void DoNullChecks() {
        if (_panelLightTransform == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _panelLightTransform is NULL!");}
        if (_elevatorScript == null) { Debug.LogError("ElevatorPanel::DoNullChecks() _elevatorScript is NULL!");}
    }
}
