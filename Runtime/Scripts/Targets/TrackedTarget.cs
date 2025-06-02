/*
 * HRTK: TrackedTarget.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;

namespace HRTK {
    public class TrackedTarget : RetargetingTarget {
        [Header("Distance Source")]
        [SerializeField]
        private RetargetingShape _shape;
        /// <summary>
        /// Gets the retargeting shape of the target.
        /// </summary>
        public RetargetingShape Shape
        {
            get
            {
                return _shape;
            }
        }

        private void OnValidate()
        {
            if (_shape == null)
            {
                _shape = GetComponent<RetargetingShape>();
            }
        }
    }
}