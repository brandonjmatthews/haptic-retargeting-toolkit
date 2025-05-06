using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.Simulation {
    public class RetargetingSimulator : MonoBehaviour
    {
        public RetargetingManager retargetingManager;
    
        [SerializeReference]
        public List<RetargetingSimStep> Procedure;
        int currentStepIndex;
        public bool loop = false;
        public bool IsStarted => isStarted;
        bool isStarted = false;
        public bool IsRunning => isRunning;
        bool isRunning = false;

        public string currentStep = "None";

        public void StartProcedure() {
            if (Procedure.Count != 0) {
                currentStepIndex = 0;
                isStarted = true;
                isRunning = true;
                StartCurrentProcedureStep();
            }
        }

        private void StartCurrentProcedureStep() {
            if (Procedure[currentStepIndex] != null) {
                currentStep = currentStepIndex + ": " + Procedure[currentStepIndex].GetType().Name;
                Procedure[currentStepIndex].onStepCompleteCallback += OnProcedureStepComplete;
                Procedure[currentStepIndex].Start(retargetingManager);
            }
        }

        private void Update() {
            if (isStarted && isRunning && Procedure[currentStepIndex] != null) Procedure[currentStepIndex].Update();
        }

        private void OnProcedureStepComplete() {
            Procedure[currentStepIndex].onStepCompleteCallback -= OnProcedureStepComplete;

            if (currentStepIndex < Procedure.Count - 1) {
                currentStepIndex += 1;
                StartCurrentProcedureStep();
            } else {
                if (loop) {
                    currentStepIndex = 0;
                    StartCurrentProcedureStep();
                } else {
                    isRunning = false;
                    isStarted = false;
                }
            }
        }

        public void ToggleProcedurePaused() {
            if (isStarted) isRunning = !isRunning;
        }
    }

}
