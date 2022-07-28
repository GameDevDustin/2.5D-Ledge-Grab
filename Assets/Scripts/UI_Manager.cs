using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour {
    [SerializeField] private TMP_Text _txtTimeRemaining;
    [SerializeField] private TMP_Text _txtLivesCount;
    [SerializeField] private TMP_Text _txtGameOver;
    private InputActions _inputActions;
    

    public void UpdateTimeRemaining(float timeRemaining) { _txtTimeRemaining.text = Mathf.Round(timeRemaining) + " seconds"; }
    public void UpdateLivesCount(int numOfLives) { _txtLivesCount.text = numOfLives.ToString(); }
    public void DisplayGameOver() {
        _txtGameOver.text = "Game Over";
        _txtGameOver.gameObject.SetActive(true);
        StartCoroutine(ReloadScene());
    }
    
    private IEnumerator ReloadScene() {
        yield return new WaitForSeconds(10f);
        SceneManager.LoadScene(0);
    }

    private void OnEnable() {
        _inputActions = new InputActions();
        _inputActions.UI_Controls.Enable();
        _inputActions.UI_Controls.ExitGame.performed += ExitGameOnPerformed;
    }

    private void ExitGameOnPerformed(InputAction.CallbackContext obj) { Application.Quit(); }

    private void Start() { DoNullChecks(); }

    private void OnDisable() { _inputActions.UI_Controls.ExitGame.performed -= ExitGameOnPerformed; }

    private void DoNullChecks() {
        if (_txtTimeRemaining == null) { Debug.LogError("UI_Manager::DoNullChecks() _txtTimeRemaining is NULL!");}
        if (_txtLivesCount == null) { Debug.LogError("UI_Manager::DoNullChecks() _txtLivesCount is NULL!"); }
        if (_txtGameOver == null) { Debug.LogError("UI_Manager::DoNullChecks() _txtGameOver is NULL!");}
    }
}

