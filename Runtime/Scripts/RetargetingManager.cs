/*
 * HRTK: RetargetingManager.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections.Generic;
using UnityEngine;

namespace HRTK
{
    public class RetargetingManager : MonoBehaviour
    {   
        [Header("Hand Controllers")]
        public Chirality DominantHand;
        public RetargetingController LeftHandController;
        public RetargetingController RightHandController;
        
        [Header("Head and Camera")]
        public Transform Head;
        public RetargetingCamera RenderCamera;

        [Header("Available Targets")]
        public List<TrackedTarget> TrackedTargets;
        public List<VirtualTarget> VirtualTargets;

        public TrackedTargetEvent OnTrackedTargetAdded;
        public VirtualTargetEvent OnVirtualTargetAdded;
        public TrackedTargetEvent OnTrackedTargetRemoved;
        public VirtualTargetEvent OnVirtualTargetRemoved;

        public RetargetingController DominantRetargetingHand {
            get {
                if (DominantHand == Chirality.Left) return LeftHandController;
                else return RightHandController;
            }
        }

        public RetargetingController GetHand(Chirality hand) {
            return hand == Chirality.Left ? LeftHandController : RightHandController;
        }
        
        public void SetDominantHand(Chirality hand) {
            DominantHand = hand;
        }

        public void SetDominantHand(int hand) {
            SetDominantHand((Chirality) hand);
        }

        public void AddTrackedTarget(TrackedTarget targetToAdd)
        {
            if (TrackedTargets == null) TrackedTargets = new List<TrackedTarget>();
            if (!TrackedTargets.Contains(targetToAdd))
            {
                TrackedTargets.Add(targetToAdd);
                OnTrackedTargetAdded.Invoke(targetToAdd);
            }
        }

        public void AddVirtualTarget(VirtualTarget targetToAdd)
        {
            if (VirtualTargets == null) VirtualTargets = new List<VirtualTarget>();
            if (!VirtualTargets.Contains(targetToAdd))
            {
                VirtualTargets.Add(targetToAdd);
                OnVirtualTargetAdded.Invoke(targetToAdd);
            }
        }


        public void AddTrackedTargets(IEnumerable<GameObject> targetsToAdd)
        {
            foreach (GameObject g in targetsToAdd)
            {
                TrackedTarget pt = g.GetComponent<TrackedTarget>();
                if (pt != null)
                {
                    AddTrackedTarget(pt);
                }
            }
        }

        public void AddVirtualTargets(IEnumerable<GameObject> targetsToAdd)
        {
            foreach (GameObject g in targetsToAdd)
            {
                VirtualTarget vt = g.GetComponent<VirtualTarget>();
                if (vt != null)
                {
                    AddVirtualTarget(vt);
                }
            }
        }

        public void AddTrackedTargets(IEnumerable<TrackedTarget> targetsToAdd)
        {
            foreach (TrackedTarget pt in targetsToAdd)
            {
                if (pt != null)
                {
                    AddTrackedTarget(pt);
                }
            }
        }

        public void AddVirtualTargets(IEnumerable<VirtualTarget> targetsToAdd)
        {
            foreach (VirtualTarget vt in targetsToAdd)
            {
                if (vt != null)
                {
                    AddVirtualTarget(vt);
                }
            }
        }


        public void RemoveTrackedTarget(TrackedTarget targetToRemove)
        {
            if (TrackedTargets.Contains(targetToRemove))
            {
                TrackedTargets.Remove(targetToRemove);
                OnTrackedTargetRemoved.Invoke(targetToRemove);
            }
        }

        public void RemoveVirtualTarget(VirtualTarget targetToRemove)
        {
            if (VirtualTargets.Contains(targetToRemove))
            {
                VirtualTargets.Remove(targetToRemove);
                OnVirtualTargetRemoved.Invoke(targetToRemove);
            }
        }

        public void RemoveTrackedTargets(List<GameObject> targetsToRemove)
        {
            foreach (GameObject g in targetsToRemove)
            {
                TrackedTarget pt = g.GetComponent<TrackedTarget>();
                if (pt != null)
                {
                    RemoveTrackedTarget(pt);
                }
            }
        }

        public void RemoveVirtualTargets(List<GameObject> targetsToRemove)
        {
            foreach (GameObject g in targetsToRemove)
            {
                VirtualTarget vt = g.GetComponent<VirtualTarget>();
                if (vt != null)
                {
                    RemoveVirtualTarget(vt);
                }
            }
        }

        public void RemoveTrackedTargets(List<TrackedTarget> targetsToRemove)
        {
            foreach (TrackedTarget pt in targetsToRemove)
            {
                if (pt != null)
                {
                    RemoveTrackedTarget(pt);
                }
            }
        }

        public void RemoveVirtualTargets(List<VirtualTarget> targetsToRemove)
        {
            foreach (VirtualTarget vt in targetsToRemove)
            {
                if (vt != null)
                {
                    RemoveVirtualTarget(vt);
                }
            }
        }
    }
}
