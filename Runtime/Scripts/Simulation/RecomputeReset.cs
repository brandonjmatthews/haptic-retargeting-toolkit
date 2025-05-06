using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecomputeReset : MonoBehaviour
{
    public HRTK.RetargetingReset reset;
    public HRTK.RetargetingController controller;

    public HRTK.TrackedTarget nextTrackedTarget;
    public HRTK.VirtualTarget nextVirtualTarget;

    // Start is called before the first frame update
    void Start()
    {
        if (reset == null) this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        reset.ConfigureReset(controller.TrackedTarget, controller.VirtualTarget, nextTrackedTarget, nextVirtualTarget);
    }
}
