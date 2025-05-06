using System.Collections.Generic;
using UnityEngine;

namespace HRTK {
    public class VirtualTarget : RetargetingTarget {

        [Header("Virtual Target Options")]
        [SerializeField]
        private bool _showMappingInEditor;

        [SerializeField]
        private List<TrackedTarget> _targetMapping;
        public List<TrackedTarget> TargetMapping
        {
            get => _targetMapping;
            set
            {
                if (value == null) return;
                _targetMapping = value;
            }
        }

        [Header("Selection Options")]
        [SerializeField] 
        private SelectionIndicator _selectionIndicator;
        /// <summary>
        /// Gets or sets the current selection indicator for the target.
        /// </summary>
        public SelectionIndicator SelectionIndicator
        {
            get => _selectionIndicator;
            set => _selectionIndicator = value;
        }

        protected override void Awake()
        {
            if (_selectionIndicator == null) _selectionIndicator = GetComponentInChildren<SelectionIndicator>();
            if (Selected) Select();
            else Deselect();
            Debug.Log(_targetMapping);
            if (_targetMapping == null) _targetMapping = new List<TrackedTarget>();

            if (SelectionIndicator) SelectionIndicator.Initalize();

            base.Awake();
        }

        public override void Select() {
            base.Select();
            if (SelectionIndicator) _selectionIndicator.OnSelected();
        }

        public override void Deselect() {
            base.Deselect();
            if (SelectionIndicator) _selectionIndicator.OnDeselected();
        }

        private void OnDrawGizmos() {
            if (_targetMapping == null) return;

            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            foreach(TrackedTarget trackedTarget in _targetMapping) {
                if (trackedTarget != null) Gizmos.DrawLine(Target.position, trackedTarget.Target.position);
            }

            Gizmos.color = oldColor;
        }
    }
}