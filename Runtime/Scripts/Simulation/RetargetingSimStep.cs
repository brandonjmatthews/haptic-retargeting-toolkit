/*
 * HRTK: RetargetingSimStep.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.Simulation {

    public class SetHandPose : RetargetingSimStep {
        public Chirality hand;
        public string poseTrigger;
        Animator animator;

        public override void Start(RetargetingManager manager) {
            base.Start(manager);
            animator = manager.GetHand(hand).GetComponentInChildren<Animator>();
            animator.SetTrigger(poseTrigger);
        }
        public override void Update() {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0)) {
                OnComplete();
            }
        }
    }

    [System.Serializable]
    public abstract class RetargetingSimStep {
        protected RetargetingManager manager;
        public delegate void StepCompleteCallback();
        public StepCompleteCallback onStepCompleteCallback;

        public RetargetingSimStep() {

        }

        public virtual void Start(RetargetingManager manager) {
            this.manager = manager;
        }

        public virtual void Update() {}
        protected virtual void OnComplete() {
            onStepCompleteCallback.Invoke();
        }
    }

    [System.Serializable]
    public class SetRetargetingEnabled : RetargetingSimStep {
        public Chirality hand;
        public bool enabled;
        public bool onTheFly;
       
        public override void Start(RetargetingManager manager)
        {
            base.Start(manager);
            manager.GetHand(hand).RetargetingEnabled = enabled;
            manager.GetHand(hand).UseOnTheFlyOrigin = onTheFly;
            OnComplete();
        }
    }

    [System.Serializable]
    public class Delay : RetargetingSimStep {

        public float delay;
        float startTime;
      
        public override void Start(RetargetingManager manager)
        {
            base.Start(manager);
            startTime = Time.time;
        }

        public override void Update()
        {
            if (Time.time - startTime >= delay) {
                OnComplete();
            }
        }
    }

    [System.Serializable]
    public class ToggleGameObjectsActive : RetargetingSimStep {

        public GameObject[] gameObjects;
        public bool active;
      
        public override void Start(RetargetingManager manager)
        {
            base.Start(manager);
            foreach (GameObject g in gameObjects) {
                g.SetActive(active);
            }
            OnComplete();
        }
    }

    [System.Serializable]
    public class MoveHandToTransform : RetargetingSimStep {
        public Chirality hand;
        public Transform targetTransform;
        public float speed;
        public float tolerance = 0.0001f;
        
        float t = 0.0f;
        Transform handTransform;
        Vector3 startingPosition;
        Quaternion startingRotation;

        public override void Start(RetargetingManager manager)
        {
            base.Start(manager);
            if (targetTransform == null || speed <= 0.0f) {
                OnComplete();
                return;
            }

            handTransform = manager.GetHand(hand).TrackedHand.transform;
            startingPosition = handTransform.position;
            startingRotation = handTransform.rotation;

        }

        public override void Update()
        {
            if (targetTransform != null) {
                t += speed * Time.deltaTime;
                Vector3 newPosition = new Vector3(
                  Mathf.SmoothStep(startingPosition.x, targetTransform.position.x, t),
                  Mathf.SmoothStep(startingPosition.y, targetTransform.position.y, t),
                  Mathf.SmoothStep(startingPosition.z, targetTransform.position.z, t));

                Quaternion newRotation = Quaternion.Lerp(startingRotation, targetTransform.rotation, t);
 
                handTransform.position = newPosition;
                handTransform.rotation = newRotation;

                if (Vector3.Distance(handTransform.position, targetTransform.position) < tolerance) {
                    OnComplete();
                }
            }
        }
    }

    [System.Serializable]
    public class MoveTransformToTransform : RetargetingSimStep {
        public Transform moveTransform;
        public Transform targetTransform;
        public float speed;
        public float tolerance = 0.0001f;
        
        float t = 0.0f;
        Vector3 startingPosition;
        Quaternion startingRotation;

        public override void Start(RetargetingManager manager)
        {
            base.Start(manager);
            if (moveTransform == null ||targetTransform == null || speed <= 0.0f) {
                OnComplete();
                return;
            }
   
            startingPosition = moveTransform.position;
            startingRotation = moveTransform.rotation;

        }

        public override void Update()
        {
            if (targetTransform != null) {
                t += speed * Time.deltaTime;
                Vector3 newPosition = new Vector3(
                  Mathf.SmoothStep(startingPosition.x, targetTransform.position.x, t),
                  Mathf.SmoothStep(startingPosition.y, targetTransform.position.y, t),
                  Mathf.SmoothStep(startingPosition.z, targetTransform.position.z, t));

                Quaternion newRotation = Quaternion.Lerp(startingRotation, targetTransform.rotation, t);
 
                moveTransform.position = newPosition;
                moveTransform.rotation = newRotation;

                if (Vector3.Distance(moveTransform.position, targetTransform.position) < tolerance) {
                    OnComplete();
                }
            }
        }
    }

    [System.Serializable]
    public class SetTargets : RetargetingSimStep {
        public Chirality hand;
        public TrackedTarget trackedTarget;
        public VirtualTarget virtualTarget;

        public override void Start(RetargetingManager manager)
        {
            base.Start(manager);
            RetargetingController handController = manager.GetHand(hand);
            if (trackedTarget != null) {
                handController.SetTarget(trackedTarget);
            }
            if (virtualTarget != null) {
                handController.SetTarget(virtualTarget);
            }
            OnComplete();
        }
    }
}
