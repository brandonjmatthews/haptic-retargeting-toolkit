using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{

    public abstract class TargetSelector : MonoBehaviour
    {
        public enum SelectionFilterType
        {
            None,
            RequireOrigin,
            RequireReset,
            DynamicThreshold,
            StaticThreshold,
            LiveFingertipAngle,
            LiveFingertipTranslation,
            LiveFingertipCombined,
        }

        [Header("Target Selection")]
        [SerializeField]
        protected bool _selectorEnabled;
        [SerializeField]
        protected bool _selectionEnabled;
        public bool SelectorEnabled => _selectorEnabled;
        public bool SelectionEnabled => _selectorEnabled && _selectionEnabled;
        // public List<string> tagFilter;
        public LayerMask targetLayerMask;
        public RetargetingManager _manager;
        public Chirality hand;

        [SerializeField]
        private TargetPairEvent onTargetsSelected;
        public TargetPairEvent OnTargetsSelected => onTargetsSelected;

        [SerializeField]
        private TargetPairEvent onTargetsDeselected;
        public TargetPairEvent OnTargetsDeselected => onTargetsDeselected;

        [Header("Optimal Mapping Method")]
        public TargetMapping.Method MappingMethod;
        public TargetMapping.Source OptimisationSource;
        public TargetMapping.Factor OptimisationFactor;

        [Header("Target Filtering")]
        public bool UpdateTargetSelectability;
        public SelectionFilterType FilterType;
        [Header("Dynamic Filtering")]
        public float MinimumDistanceCutoff;
        public float DistanceCutoff;
        public float AngleCutoff;
        public float TranslationCutoff;
        [Header("Intervening Reset")]
        public RetargetingReset ResetMethod;
        [SerializeField] private UnityEvent onAwaitReset;
        public UnityEvent OnAwaitReset => onAwaitReset;

        public Color SelectionColor;

        protected TrackedTarget _tempResetTrackedTarget;
        protected VirtualTarget _tempResetVirtualTarget;

        private void OnValidate() {
            if (_manager == null) {
                RetargetingManager[] managers = GameObject.FindObjectsOfType<RetargetingManager>();

                if (managers.Length > 0) {
                    _manager = managers[0];
                
                    if (managers.Length > 1) {
                        Debug.LogWarning("Multiple instances of RetargetingManager found, please ensure the correct manager is selected.");
                    }
                }
            }
        }


        protected virtual void Update() {
            if (SelectorEnabled && FilterType == SelectionFilterType.RequireOrigin) {
                SetSelectionEnabled(_manager.GetHand(hand).Status.AtOrigin);
            }

            if (SelectorEnabled && SelectionEnabled) {
                if (UpdateTargetSelectability) UpdateSelectableTargets();
            }
        }

        public virtual void TargetSelected(VirtualTarget virtualTarget)
        {
            if (SelectionEnabled && _selectionEnabled) {
                if (virtualTarget.Selectable) {
                    TrackedTarget trackedTarget = GetTrackedTarget(virtualTarget);

                    if (trackedTarget == null) Debug.LogWarning("No Suitable Tracked Target to Select");

                    if (trackedTarget != null && virtualTarget != null) {

                        if (FilterType == SelectionFilterType.RequireReset) {
                            if (ResetMethod == null) {
                                Debug.LogError("Selection Requires Reset but no Reset method is provided. This may cause unexpected behaviour.");
                            } else {
                                _tempResetTrackedTarget = trackedTarget;
                                _tempResetVirtualTarget = virtualTarget;

                                ResetMethod.AwaitReset();
                                ResetMethod.OnResetComplete.AddListener(OnResetComplete);  

                                onAwaitReset.Invoke();
                            }
                        } else {
                            if (IsTargetSelectable(virtualTarget, trackedTarget)) {
                                DeselectTargets();
                                SelectTargets(trackedTarget, virtualTarget);
                            }
                        }
                    }
                }
            }
        }

        void OnResetComplete() {
            ResetMethod.OnResetComplete.RemoveListener(OnResetComplete);

            SelectTargets(_tempResetTrackedTarget, _tempResetVirtualTarget);
            _tempResetTrackedTarget = null;
            _tempResetVirtualTarget = null;
        }

        public void DeselectTargets() {
            TrackedTarget trackedTarget = _manager.GetHand(hand).TrackedTarget;
            VirtualTarget virtualTarget = _manager.GetHand(hand).VirtualTarget;
            
            _manager.GetHand(hand).SetTargets(null, null);
            Debug.Log("[HRTK] Targets Deselected: Tracked = " + 
                            (trackedTarget == null ? "None" : trackedTarget.name) + 
                            ", Virtual = " +  
                            (virtualTarget == null ? "None" : virtualTarget.name));
            onTargetsDeselected.Invoke(trackedTarget, virtualTarget);
        }

        protected void SelectTargets(TrackedTarget trackedTarget, VirtualTarget virtualTarget) {
            Debug.Log("[HRTK] Targets Selected: Tracked = " + trackedTarget.name + ", Virtual = " + virtualTarget.name);

            _manager.GetHand(hand).SetTargets(trackedTarget, virtualTarget);

            onTargetsSelected.Invoke(trackedTarget, virtualTarget);
        }

        public TrackedTarget GetTrackedTarget(VirtualTarget virtualTarget)
        {
            if (virtualTarget == null) return null;

            if (MappingMethod == TargetMapping.Method.Optimized)
            {
                if (OptimisationSource == TargetMapping.Source.Hand)
                {
                    return TargetMapping.GetOptimalTrackedTarget(virtualTarget, virtualTarget.TargetMapping, OptimisationFactor,
                                                                _manager.GetHand(hand).VirtualHand.transform.position, _manager.GetHand(hand).TrackedHand.transform.position);
                }
                else
                {
                    return TargetMapping.GetOptimalTrackedTarget(virtualTarget, virtualTarget.TargetMapping, OptimisationFactor,
                                                                _manager.GetHand(hand).Origin.VirtualPosition, _manager.GetHand(hand).Origin.Position);
                }
            }
            else
            {
                return TargetMapping.GetNearestTarget(virtualTarget, virtualTarget.TargetMapping);
            }
        }

        public virtual void SetSelectorEnabled(bool enabled)
        {
            _selectorEnabled = enabled;
            SetSelectionEnabled(enabled);
        }

        protected virtual void SetSelectionEnabled(bool enabled) {
            _selectionEnabled = enabled;
        }

        protected bool IsTargetSelectable(VirtualTarget vt, TrackedTarget tt) {
            bool selectable = true;

            if (tt != null)
            {
                switch (FilterType)
                {
                    case TargetSelector.SelectionFilterType.None:
                        selectable = true;
                        break;
                    case TargetSelector.SelectionFilterType.RequireReset:
                        selectable = true;
                        break;
                    case TargetSelector.SelectionFilterType.RequireOrigin:
                        if (_manager.GetHand(hand).Status.AtOrigin) {
                            selectable = true;
                        } else {
                            selectable = false;
                        }
                        break;
                    case SelectionFilterType.StaticThreshold:
                        selectable = DynamicSelectionHelpers.BeyondStaticSelectableThreshold(_manager.GetHand(hand), tt, vt, GetDirectionNormal(vt), DistanceCutoff);
                        break;
                    case SelectionFilterType.DynamicThreshold:
                        if (_manager.GetHand(hand).TrackedTarget != null && _manager.GetHand(hand).VirtualTarget != null)
                        {
                            selectable = DynamicSelectionHelpers.BeyondDynamicSelectableThreshold(_manager.GetHand(hand), 
                                                                                                  _manager.GetHand(hand).TrackedTarget, 
                                                                                                  _manager.GetHand(hand).VirtualTarget, 
                                                                                                  tt, vt, GetDirectionNormal(vt), AngleCutoff, TranslationCutoff);
                        }
                        break;
                    case SelectionFilterType.LiveFingertipAngle:
                        selectable = DynamicSelectionHelpers.WithinSelectableAngle(_manager.GetHand(hand), tt, vt, AngleCutoff);
                        break;
                    case SelectionFilterType.LiveFingertipTranslation:
                        selectable = DynamicSelectionHelpers.WithinSelectableTranslation(_manager.GetHand(hand), tt, vt, TranslationCutoff);
                        break;
                    case SelectionFilterType.LiveFingertipCombined:
                        selectable = DynamicSelectionHelpers.WithinSelectableAngleAndTranslation(_manager.GetHand(hand), tt, vt, AngleCutoff, TranslationCutoff);
                        break;
                }

                selectable = selectable && DynamicSelectionHelpers.BeyondStaticSelectableThreshold(_manager.GetHand(hand), tt, vt, GetDirectionNormal(vt), MinimumDistanceCutoff);
            
                return selectable;
            }
            else
            {
                return false;
            }
        }

        public Vector3 GetDirectionNormal(RetargetingTarget target)
        {
            return (_manager.Head.position - target.Target.position).normalized;
        }

        public void UpdateSelectableTargets()
        {
            foreach (VirtualTarget vt in _manager.VirtualTargets)
            {
                if (vt == null) continue;
                
                TrackedTarget pt = GetTrackedTarget(vt);

                bool selectable = true;


                if (pt != null)
                {
                    switch (FilterType)
                    {
                        case TargetSelector.SelectionFilterType.None:
                            selectable = true;
                            break;
                        case TargetSelector.SelectionFilterType.RequireReset:
                            selectable = true;
                            break;
                        case TargetSelector.SelectionFilterType.RequireOrigin:
                            if (_manager.GetHand(hand).Status.AtOrigin) {
                                selectable = true;
                            } else {
                                selectable = false;
                            }
                            break;
                        case TargetSelector.SelectionFilterType.StaticThreshold:
                            selectable = DynamicSelectionHelpers.BeyondStaticSelectableThreshold(_manager.GetHand(hand), pt, vt, GetDirectionNormal(vt), DistanceCutoff);
                            // Debug.DrawRay(vt.transform.position,  GetDirectionNormal(vt));
                            break;
                        case TargetSelector.SelectionFilterType.DynamicThreshold:
                            if (_manager.GetHand(hand).TrackedTarget != null && _manager.GetHand(hand).VirtualTarget != null)
                            {
                                selectable = DynamicSelectionHelpers.BeyondDynamicSelectableThreshold(_manager.GetHand(hand), _manager.GetHand(hand).TrackedTarget, _manager.GetHand(hand).VirtualTarget, pt, vt, GetDirectionNormal(vt), AngleCutoff, TranslationCutoff);
                            }
                            break;
                        case TargetSelector.SelectionFilterType.LiveFingertipAngle:
                            selectable = DynamicSelectionHelpers.WithinSelectableAngle(_manager.GetHand(hand), pt, vt, AngleCutoff);
                            break;
                        case TargetSelector.SelectionFilterType.LiveFingertipTranslation:
                            selectable = DynamicSelectionHelpers.WithinSelectableTranslation(_manager.GetHand(hand), pt, vt, TranslationCutoff);
                            break;
                        case TargetSelector.SelectionFilterType.LiveFingertipCombined:
                            selectable = DynamicSelectionHelpers.WithinSelectableAngleAndTranslation(_manager.GetHand(hand), pt, vt, AngleCutoff, TranslationCutoff);
                            break;
                    }

                    selectable = selectable && DynamicSelectionHelpers.BeyondStaticSelectableThreshold(_manager.GetHand(hand), pt, vt, GetDirectionNormal(vt), MinimumDistanceCutoff);
                }
                else
                {
                    selectable = false;
                }

                if (!vt.LockSelectable) {
                    vt.Selectable = selectable;
                }
            }
        }

    }
}
