/*
 * HRTK: SelectionIndicator.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

using System;
using HRTK;
using UnityEngine;

namespace HRTK {
    public abstract class SelectionIndicator : MonoBehaviour {

        protected bool currentlySelected = false;
        protected Color fallbackSelectionColor = Color.white;
        
        public abstract void Initalize();
        public abstract void OnSelected();
        public abstract void OnSelected(Color selectedColor);
        public abstract void OnDeselected();
    }
}
