using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int _numOfCoins;

    
    private void Start() { _numOfCoins = Random.Range(1, 5); }
    public int GetNumOfCoins() { return _numOfCoins; }
    public void HideCollectable() { gameObject.SetActive(false); }
}
