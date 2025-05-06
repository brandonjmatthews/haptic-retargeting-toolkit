using UnityEngine;

namespace HRTK {
    public class TargetCalibrationHandler : MonoBehaviour {

        [SerializeField]
        RetargetingManager _manager;

        [SerializeField]
        bool _enableSeeThrough;
        public bool EnableSeeThrough => _enableSeeThrough;
        
        bool _calibrating = false;
        public bool IsCalibration => _calibrating;

        private void Start() {
            _manager = GameObject.FindObjectOfType<RetargetingManager>();
        }


        public virtual void StartCalibration() {
            _calibrating = true;

            if (EnableSeeThrough) {
                PassthroughHandler pt = _manager.RenderCamera.Passthrough;

                if (pt != null) {
                    pt.SetPassthroughEnabled(true);
                }
            }
        }

        public virtual void CalbrationComplete() {
            _calibrating = false;

            if (EnableSeeThrough) {
                PassthroughHandler pt = _manager.RenderCamera.Passthrough;

                if (pt != null) {
                    pt.SetPassthroughEnabled(false);
                }
            }
        }
    }
}