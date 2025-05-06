using UnityEngine.Events;

namespace HRTK {
    [System.Serializable]
    public class ChiralityEvent : UnityEvent<Chirality> {}
    [System.Serializable]
    public class VisibilityEvent : UnityEvent<Visibility> {}
    [System.Serializable]
    public class TargetEvent : UnityEvent<RetargetingTarget> {}
    [System.Serializable]
    public class TrackedTargetEvent : UnityEvent<TrackedTarget> {}
    [System.Serializable]
    public class VirtualTargetEvent : UnityEvent<VirtualTarget> {}
    [System.Serializable]
    public class TargetPairEvent : UnityEvent<TrackedTarget, VirtualTarget> {}
    [System.Serializable]
    public class TargetPairsEvent : UnityEvent<TrackedTarget, TrackedTarget, VirtualTarget, VirtualTarget> {}
}