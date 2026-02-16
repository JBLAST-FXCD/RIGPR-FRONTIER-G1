using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Anthony - 8/2/2026

/// <summary>
/// Decor tags used for city aesthetics morale.
/// This is a [Flags] enum so a single decoration can belong to multiple categories,
/// e.g. Nature + Cute.
/// </summary>
[Flags]
public enum DecorTag
{
    None = 0,
    Nature = 1 << 0,
    Cosy = 1 << 1,
    Luxury = 1 << 2,
    Rustic = 1 << 3,
    Colourful = 1 << 4,
    Festive = 1 << 5,
    Industrial = 1 << 6,
    Cute = 1 << 7
}

