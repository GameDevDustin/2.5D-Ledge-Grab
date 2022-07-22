using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeTrigger : MonoBehaviour {
    [SerializeField] private PlayerAnimations.PlayerCharFacingDirection _ledgeCharFacingDirection;


    public PlayerAnimations.PlayerCharFacingDirection GetLedgeCharFacingDirection() { return _ledgeCharFacingDirection; }
}
