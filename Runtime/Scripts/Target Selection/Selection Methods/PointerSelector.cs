using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{
    public class PointerSelector : RayDwellSelector
    {
        public float shoulderOriginWidthOffset = 0.2f;
        public float shoulderOriginHeightOffset = 0.2f;
        public Transform originOverride;
        public Transform controlOverride;

        protected override void Update()
        {
            base.Update();

            if (SelectorEnabled && SelectionEnabled)
            {
                Vector3 shoulderOffset = hand == Chirality.Left ? _manager.Head.right  * -1.0f : _manager.Head.right;
                Vector3 originPosition = _manager.Head.position + (_manager.Head.up * -shoulderOriginHeightOffset) + (shoulderOffset * shoulderOriginWidthOffset);

                if (originOverride != null)
                {
                    originPosition = originOverride.position;
                }

                Vector3 controlPosition = (_manager.GetHand(hand).VirtualHand.indexTip.position + _manager.GetHand(hand).VirtualHand.thumbTip.position) / 2.0f;


                if (controlOverride != null)
                {
                    controlPosition = controlOverride.position;
                }

                Vector3 direction = controlPosition - originPosition;

                UpdateLine(controlPosition, direction);

                Ray pointerRay = new Ray(controlPosition, direction);
                UpdateRayTarget(pointerRay);
            }
        }
    }
}
