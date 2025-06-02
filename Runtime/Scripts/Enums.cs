/*
 * HRTK: Enums.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;

namespace HRTK
{
    public enum Chirality
    {
        Left,
        Right
    }

    public enum HandType
    {
        Tracked,
        Virtual,
    }

    public enum Visibility
    {
        Both,
        None,
        Tracked,
        Virtual
    }

    public enum FilterFunction
    {
        Linear,
        SmoothStep,
        SmootherStep,
        SmoothestStep,
    }

    public enum Environment {
        Tracked,
        Virtual
    }
}