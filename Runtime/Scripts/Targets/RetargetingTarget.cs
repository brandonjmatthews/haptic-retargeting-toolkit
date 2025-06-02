/*
 * HRTK: RetargetingTarget.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{
    public abstract class RetargetingTarget : MonoBehaviour
    {
        [SerializeField][ReadOnly("Selected")] 
        private bool _selected;
        /// <summary>
        /// Gets if the target is currently selected.
        /// </summary>
        public bool Selected => _selected;


        [SerializeField]
        private bool _selectable;
        /// <summary>
        /// Gets or sets if the target can be selected.
        /// </summary>
        public bool Selectable
        {
            get => _selectable && !_selected;
            set {
                if (_lockSelectable) return;
                _selectable = value;
            }
        }

        [SerializeField]
        private bool _lockSelectable;
        /// <summary>
        /// Locks the current selectable state to override selectability control.
        /// </summary>
        public bool LockSelectable
        {
            get => _lockSelectable;
            set => _lockSelectable = value;
        }

        [Header("[Optional]")]
        [SerializeField]
        private Transform _targetOverride;
        /// <summary>
        /// Gets the transform that specifies the target position.
        /// </summary>
        public Transform Target
        {
            get
            {
                if (_targetOverride != null) return _targetOverride;
                return transform;
            }
        }

        [Header("Selection Events")]
        /// <summary>
        /// Invoked when the target is selected.
        /// </summary>
        public TargetEvent OnSelected;
        /// <summary>
        /// Invoked when the target is deselected.
        /// </summary>
        public TargetEvent OnDeselected;


        protected virtual void Awake()
        {
            if (Selected) Select();
            else Deselect();
        }

        /// <summary>
        /// Selects the target if it is selectable and not already selected.
        /// </summary>
        public virtual void Select()
        {
            if (Selectable && !Selected)
            {
                _selected = true;
                OnSelected.Invoke(this);
            }
        }

        /// <summary>
        /// Deselects the target if it is current selected.
        /// </summary>
        public virtual void Deselect()
        {
            if (Selected)
            {
                _selected = false;
                OnDeselected.Invoke(this);
            }
        }
    }
}