﻿using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class ControllerParameters2D {

    public enum JumpBehaviour
    {
        CanJumpOnGround,
        CanJumpAnywhere,
        CantJump
    }

    public Vector2 MaxVelocity = new Vector2(float.MaxValue, float.MaxValue);

    [Range(0, 90)]
    public float SlopeLimit = 30;

    public float Gravity = -25;

    public JumpBehaviour JumpRestrictions;

    public float JumpFrequency = 0.25f;

    public float JumpMagnitude = 12;
}
