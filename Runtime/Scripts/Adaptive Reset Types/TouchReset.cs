using System.Collections.Generic;
using UnityEngine;

namespace HRTK
{
    public class TouchReset : RetargetingReset
    {
        public List<VirtualTarget> CoLocatedVirtualTargets;

        protected override bool CheckResetComplete() {
            throw new System.NotImplementedException();
        }

        public override void ConfigureStaticReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked, VirtualTarget nextVirtual)
        {
            throw new System.NotImplementedException();
        }
        
        public override void ConfigureAdaptiveReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked, VirtualTarget nextVirtual)
        {
            throw new System.NotImplementedException();
        }

        public override void ConfigureInitialReset()
        {
            throw new System.NotImplementedException();
        }
    }
}
