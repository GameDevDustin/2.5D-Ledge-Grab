using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour {
    [SerializeField] private TMP_Text _txtTimeRemaining;
    [SerializeField] private TMP_Text _txtLivesCount;
    [SerializeField] private TMP_Text _txtGameOver;
    [SerializeField] private TMP_Text _txtVictory;
    [SerializeField] private TMP_Text _txtEscToExit;
    private InputActions _inputActions;
    

    public void UpdateTimeRemaining(float timeRemaining) { _txtTimeRemaining.text = Mathf.Round(timeRemaining) + " seconds"; }
    public void UpdateLivesCount(int numOfLives) { _txtLivesCount.text = numOfLives.ToString(); }
    public void DisplayGameOver() {
        // _txtGameOver.text = "Game Over";
        // _txtEscToExit.text = "Press the escape key to exit";
        _txtGameOver.gameObject.SetActive(true);
        _txtEscToExit.gameObject.SetActive(true);
        StartCoroutine(ReloadScene());
    }

    public void DisplayVictory() {
        // _txtVictory.text = "You Win!";
        // _txtEscToExit.text = "Press the escape key to exit";
        _txtVictory.gameObject.SetActive(true);
        _txtEscToExit.gameObject.SetActive(true);
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

