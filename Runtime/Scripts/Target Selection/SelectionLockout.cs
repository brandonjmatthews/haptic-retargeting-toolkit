using UnityEngine;

namespace HRTK
{
    public class SelectionLockout : MonoBehaviour
    {
        [SerializeField]
        RetargetingManager _manager;

        [SerializeField]
        TargetSelector _selector;

        private void Update()
        {
            UpdateSelectableTargets();
        }

        public void UpdateSelectableTargets()
        {
            foreach (VirtualTarget vt in _manager.VirtualTargets)
            {
                if (vt == null) continue;
                
                TrackedTarget pt = _selector.GetTrackedTarget(vt);

                bool selectable = true;

                if (pt != null)
                {
                    switch (_selector.FilterType)
                    {
                        case TargetSelector.SelectionFilterType.None:
                            selectable = true;
                            break;
                        case TargetSelector.SelectionFilterType.RequireOrigin:
                            if (_manager.GetHand(_selector.hand).Status.AtOrigin) {
                                selectable = true;
                            } else {
                                selectable = false;
                            }
                            break;
                        case TargetSelector.SelectionFilterType.StaticThreshold:
                            selectable = DynamicSelectionHelpers.BeyondStaticSelectableThreshold(_manager.GetHand(_selector.hand), pt, vt, GetDirectionNormal(vt), _selector.DistanceCutoff);
                            // Debug.DrawRay(vt.transform.position,  GetDirectionNormal(vt));
                            break;
                        case TargetSelector.SelectionFilterType.DynamicThreshold:
                            if (_manager.GetHand(_selector.hand).TrackedTarget != null && _manager.GetHand(_selector.hand).VirtualTarget != null)
                            {
                                selectable = DynamicSelectionHelpers.BeyondDynamicSelectableThreshold(_manager.GetHand(_selector.hand), _manager.GetHand(_selector.hand).TrackedTarget, _manager.GetHand(_selector.hand).VirtualTarget, pt, vt, GetDirectionNormal(vt), _selector.AngleCutoff, _selector.TranslationCutoff);
                            }
                            break;
                        case TargetSelector.SelectionFilterType.LiveFingertipAngle:
                            selectable = DynamicSelectionHelpers.WithinSelectableAngle(_manager.GetHand(_selector.hand), pt, vt, _selector.AngleCutoff);
                            break;
                        case TargetSelector.SelectionFilterType.LiveFingertipTranslation:
                            selectable = DynamicSelectionHelpers.WithinSelectableTranslation(_manager.GetHand(_selector.hand), pt, vt, _selector.TranslationCutoff);
                            break;
                        case TargetSelector.SelectionFilterType.LiveFingertipCombined:
                            selectable = DynamicSelectionHelpers.WithinSelectableAngleAndTranslation(_manager.GetHand(_selector.hand), pt, vt, _selector.AngleCutoff, _selector.TranslationCutoff);
                            break;
                    }

                    selectable = selectable && DynamicSelectionHelpers.BeyondStaticSelectableThreshold(_manager.GetHand(_selector.hand), pt, vt, GetDirectionNormal(vt), _selector.MinimumDistanceCutoff);
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


        public Vector3 GetDirectionNormal(RetargetingTarget target)
        {
            return (_manager.Head.position - target.Target.position).normalized;
        }
    }
}