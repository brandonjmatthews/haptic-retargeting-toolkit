using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{
    public class RayDwellSelector : TargetSelector
    {
        [Header("Ray Dwell Selection")]
        public Reticle reticle;
        [SerializeField]
        protected bool ShowReticle;
        [SerializeField]
        protected bool ReticleCollide;
        public LineRenderer lineRenderer;
        [SerializeField]
        protected bool ShowLine;
        [SerializeField]
        protected float LineLength = 1.0f;

        float dwellStart;
        public float dwellTime = 3.0f;
        bool dwelling;
        public bool Dwelling => dwelling;
        protected VirtualTarget currentDwellTarget;

        protected void Start() {
            SetReticleVisibility(ShowReticle && SelectionEnabled);
            SetLineVisibility(ShowLine && SelectionEnabled);
            if (reticle) reticle.ToggleInvalidIndicator(false);
        }

        public void StartDwell() {
            dwelling = true;
            dwellStart = Time.time;
            if (reticle) reticle.SetFillAmount(0.0f);
        }

        public void StopDwell() {
            dwelling = false;
            dwellStart = 0.0f;
            if (reticle) reticle.SetFillAmount(0.0f);
        }

        protected void UpdateRayTarget(Ray ray)
        {
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200.0f, targetLayerMask.value))
            {
                reticle.gameObject.SetActive(true);
                if (ReticleCollide) SetReticlePosition(hit.point, hit.normal);

                // Update Reticle Position
                VirtualTarget target = hit.collider.GetComponent<VirtualTarget>();

                if (target == null) target = hit.collider.GetComponentInParent<VirtualTarget>();
                
                if (target == null) {
                    // Debug.Log("Target is null...");
                    // Is not a virtual target
                    if (reticle) reticle.ToggleInvalidIndicator(true);
                    StopDwell();
                } else if (!target.Selectable) {
                    // Debug.Log("Target not selectable...");
                    // Is a virtual target but is not selectable
                    if (reticle) reticle.ToggleInvalidIndicator(true);
                    StopDwell();
                } else {                
                    // Is a virtual target and is selectable
                    if (!dwelling) {
                        // Start new dwell target and dont start new dwell on current interaction target/pending reset target
                        if (target != _manager.GetHand(hand).VirtualTarget && target != _tempResetVirtualTarget)
                        { 
                            // Debug.Log("Target already selected...");
                            currentDwellTarget = target;
                            if (reticle) reticle.ToggleInvalidIndicator(false);
                            StartDwell();
                        }
                    } else {
                        // If already dwelling and the target is the same
                        if (target != currentDwellTarget) {
                            // This is a new target, reset the reticle and restart dwelling
                            StopDwell();
                            currentDwellTarget = target;
                            StartDwell();
                        } else {
                            // Do nothing
                        }
                    }
                }
                
            } else {
                reticle.gameObject.SetActive(false);
            }


            if (dwelling) {
                float elapsed = Time.time - dwellStart;

                if (reticle) reticle.SetFillAmount(Mathf.Clamp01(elapsed / dwellTime));

                if (elapsed > dwellTime)
                {
                    OnDwellComplete();
                }
            }
        }

        protected void OnDwellComplete() {
            Debug.Log("Dwell Completed");
            TargetSelected(currentDwellTarget);
            StopDwell();
        }

        public void SetReticlePosition(Vector3 position, Vector3 normal)
        {
            if (reticle != null) {
                position = position + normal * 0.01f;
                reticle.transform.position = position;

                reticle.transform.rotation = Quaternion.LookRotation(normal, Vector3.Slerp(normal, -normal, 0.5f));
            }
        }

        public void UpdateLine(Vector3 start, Vector3 direction) {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start + (direction.normalized * LineLength));
        }

        public void SetReticleVisibility(bool visible)
        {
            if (reticle != null) {
                reticle.gameObject.SetActive(visible);
            }
        }

        public void SetLineVisibility(bool visible)
        {
            if (lineRenderer != null) {
                lineRenderer.enabled = visible;
            }
        }

        public override void SetSelectorEnabled(bool enabled)
        {
            base.SetSelectorEnabled(enabled);
        }

        protected override void SetSelectionEnabled(bool enabled)
        {
            SetReticleVisibility(enabled && ShowReticle);
            SetLineVisibility(enabled && ShowLine);
            base.SetSelectionEnabled(enabled);
        }
    }
}
