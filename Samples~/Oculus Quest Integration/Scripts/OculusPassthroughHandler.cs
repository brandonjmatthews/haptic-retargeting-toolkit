using UnityEngine;

namespace HRTK.OculusQuest {
    public class OculusPassthroughHandler : PassthroughHandler
    {
        public override void SetPassthroughEnabled(bool enable)
        {
            OVRManager.instance.enableMixedReality = enable;
        }

        private void Update() {
            if (OVRInput.GetDown(OVRInput.Button.One)) {
                SetPassthroughEnabled(!OVRManager.instance.enableMixedReality);
            }
        }
    }
}