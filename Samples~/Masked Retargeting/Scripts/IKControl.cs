using UnityEngine;
using System;
using System.Collections;
namespace HRTK.MaskedRetargeting
{
    [RequireComponent(typeof(Animator))]

    public class IKControl : MonoBehaviour
    {

        protected Animator animator;
        public bool active = false;
        public Transform lookTarget = null;
        public Transform rightHandTarget = null;
        public Transform leftHandTarget = null;
        public Transform rightFootTarget = null;
        public Transform leftFootTarget = null;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        //a callback for calculating IK
        void OnAnimatorIK()
        {
            if (animator)
            {
                //if the IK is active, set the position and rotation directly to the goal.
                if (active)
                {

                    // Set the look target position, if one has been assigned
                    if (lookTarget != null)
                    {
                        animator.SetLookAtWeight(1);
                        animator.SetLookAtPosition(lookTarget.position);
                    }

                    // Set the right hand target position and rotation, if one has been assigned
                    if (rightHandTarget != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                    }

                    // Set the left hand target position and rotation, if one has been assigned
                    if (leftHandTarget != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                    }

                    // Set the right foot target position and rotation, if one has been assigned
                    if (rightFootTarget != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);
                    }

                    // Set the left foot target position and rotation, if one has been assigned
                    if (leftFootTarget != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootTarget.rotation);
                    }
                }

                //if the IK is not active, set the position and rotation of the hand and head back to the original position
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                    animator.SetLookAtWeight(0);
                    animator.SetLookAtWeight(0);
                }
            }
        }
    }
}