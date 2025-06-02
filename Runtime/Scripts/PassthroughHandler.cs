/*
 * HRTK: PassthroughHandler.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;

namespace HRTK {
    public abstract class PassthroughHandler : MonoBehaviour {
        public abstract void SetPassthroughEnabled(bool enable);
    }
}