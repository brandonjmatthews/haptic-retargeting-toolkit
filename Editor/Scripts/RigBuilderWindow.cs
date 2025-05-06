using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace HRTK {
    public class RigBuilderWindow : EditorWindow
    {

        RetargetingManager manager;
        Camera camera;
        RetargetingCamera retargetingCamera;
        Transform leftHandParent;
        GameObject leftHandPrefab;
        RetargetingController leftHandController;
        RetargetingHand leftTrackedHand;
        RetargetingHand leftVirtualHand;

        Transform rightHandParent;
        GameObject rightHandPrefab;
        RetargetingController rightHandController;
        RetargetingHand rightTrackedHand;
        RetargetingHand rightVirtualHand;
        bool placeVirtualHandsInWorld = false;
        
        [MenuItem ("HRTK/Build Retargeting Rig")]
        public static void  ShowWindow () {
            EditorWindow.GetWindow(typeof(RigBuilderWindow));
        }
        
        void OnGUI () {

            // The actual window code goes here
            // Handle Retargeting Manager
            EditorGUILayout.LabelField("Select or Create the Retargeting Manager:", EditorStyles.boldLabel);
            manager = EditorGUILayout.ObjectField("Manager", manager, typeof(RetargetingManager), true) as RetargetingManager;


            if (manager != null) {
                if (GUILayout.Button("Load Retargeting Manager")) {
                    LoadManager();
                }
            } else {
                if (GUILayout.Button("Create Retargeting Manager")) {
                    this.manager = CreateManager();
                }
            }

            EditorGUI.BeginDisabledGroup(true);
            retargetingCamera = EditorGUILayout.ObjectField("Retargeting Camera", retargetingCamera, typeof(RetargetingCamera), true) as RetargetingCamera;
             leftHandController = EditorGUILayout.ObjectField("Left Hand Controller", leftHandController, typeof(RetargetingController), true) as RetargetingController;
                         rightHandController = EditorGUILayout.ObjectField("Right Hand Controller", rightHandController, typeof(RetargetingController), true) as RetargetingController;
            EditorGUI.EndDisabledGroup();


            DrawUILine(Color.grey);
            EditorGUI.BeginDisabledGroup(this.manager == null);
            EditorGUILayout.LabelField("Select Camera to render retargeting elements:", EditorStyles.boldLabel);
            camera = EditorGUILayout.ObjectField("Main Camera", camera, typeof(Camera), true) as Camera;
            if (camera != null) {
                if (retargetingCamera == null) {
                    if (GUILayout.Button("Setup Camera")) {
                        retargetingCamera = CreateCamera(camera);
                        manager.RenderCamera = retargetingCamera;
                        manager.Head = retargetingCamera.transform;
                    }
                } else {
                    if (GUILayout.Button("Clear Camera")) {
                        DestroyImmediate(retargetingCamera);
                        retargetingCamera = null;
                    }
                }
            }

            EditorGUILayout.LabelField("Configure the Tracked and Virtual Hands:", EditorStyles.boldLabel);
            EditorGUI.indentLevel ++;
            placeVirtualHandsInWorld = EditorGUILayout.Toggle("Place Virtual Hand in World Space", placeVirtualHandsInWorld);

            EditorGUILayout.LabelField("Left Hand");
            EditorGUI.indentLevel ++;
            leftHandParent = EditorGUILayout.ObjectField("Left Hand Parent", leftHandParent, typeof(Transform), true) as Transform;
            leftHandPrefab = EditorGUILayout.ObjectField("Left Hand Prefab", leftHandPrefab, typeof(GameObject), false) as GameObject;

            if (leftHandPrefab != null) {
                if (GUILayout.Button("Create Left Retargeting Hand")) {
                    leftHandController = CreateHand(leftHandParent, leftHandPrefab, Chirality.Left);
                    manager.LeftHandController = leftHandController;
                    
                }
            }

            EditorGUI.indentLevel --;
            EditorGUILayout.LabelField("Right Hand");
            EditorGUI.indentLevel ++;

            rightHandParent = EditorGUILayout.ObjectField("Right Hand Parent", rightHandParent, typeof(Transform), true) as Transform;
            rightHandPrefab = EditorGUILayout.ObjectField("Right Hand Prefab", rightHandPrefab, typeof(GameObject), false) as GameObject;

            if (rightHandPrefab != null) {
                if (GUILayout.Button("Create Right Retargeting Hand")) {
                    rightHandController = CreateHand(rightHandParent, rightHandPrefab, Chirality.Right);
                    manager.RightHandController = rightHandController;
                }
            }


            EditorGUI.indentLevel --;
            EditorGUI.indentLevel --;

            EditorGUILayout.LabelField("Configure interaction targets:", EditorStyles.boldLabel);
            EditorGUI.indentLevel ++;
            TrackedTarget[] trackedTargets = FindObjectsOfType<TrackedTarget>(true);
            VirtualTarget[] virtualTargets = FindObjectsOfType<VirtualTarget>(true);
            EditorGUILayout.LabelField(trackedTargets.Length + " tracked targets detected.");
            EditorGUILayout.LabelField(virtualTargets.Length + " virtual targets detected.");

            if (manager != null) {
                List<TrackedTarget> trackedTargetList = new List<TrackedTarget>(trackedTargets);
                List<VirtualTarget> virtualTargetList = new List<VirtualTarget>(virtualTargets);
                if (manager.TrackedTargets != null) trackedTargets = trackedTargetList.Except(manager.TrackedTargets).ToArray<TrackedTarget>();
                if (manager.VirtualTargets != null) virtualTargets = virtualTargetList.Except(manager.VirtualTargets).ToArray<VirtualTarget>();
            }

            EditorGUILayout.LabelField(trackedTargets.Length + " tracked targets not registered with Manager.");
            EditorGUILayout.LabelField(virtualTargets.Length + " virtual targets not registered with Manager.");
            
            if (trackedTargets.Length != 0 || virtualTargets.Length != 0) {
                if (GUILayout.Button("Add Missing Targets to Manager")) { 
                    manager.AddTrackedTargets(trackedTargets);
                    manager.AddVirtualTargets(virtualTargets);
                }      
            }
            
            EditorGUI.indentLevel --;

            EditorGUI.EndDisabledGroup();

            /*
            TODO:
                - Add selection of retargeting offset handler
                - Add selection of target selector

            */
        }


        void LoadManager() {
            retargetingCamera = manager.RenderCamera;
            if (retargetingCamera != null) camera = retargetingCamera.GetComponent<Camera>();
            leftHandController = manager.LeftHandController;
            rightHandController = manager.RightHandController;
        }

        RetargetingManager CreateManager() {
            GameObject managerObject = new GameObject("Retargeting Manager");
            RetargetingManager newManager = managerObject.AddComponent<RetargetingManager>();
            return newManager;
        }

        RetargetingController CreateHand(Transform parent, GameObject prefab, Chirality whichHand) {
           GameObject handRoot = new GameObject();
            handRoot.transform.parent = manager.transform;
            handRoot.transform.localPosition = Vector3.zero;
            handRoot.transform.localRotation = Quaternion.identity;
            RetargetingController controller = handRoot.AddComponent<RetargetingController>();
            controller.name = (whichHand == Chirality.Left ? "Left" : "Right") + " Hand Controller";
            controller.whichHand = whichHand;

            GameObject trackedHandObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            trackedHandObject.transform.parent = parent;

            RetargetingHand trackedHand = trackedHandObject.GetComponent<RetargetingHand>();

            if (trackedHand != null)
            {
                trackedHand.HandType = HandType.Tracked;
                trackedHand.gameObject.name = prefab.name + " (Tracked)";
                trackedHand.transform.localPosition = Vector3.zero;
                trackedHand.transform.localRotation = Quaternion.identity;
                trackedHand.gameObject.layer = LayerMask.NameToLayer(Constants.TrackedEnvironmentLayer);
            }


            GameObject virtualHandObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (!placeVirtualHandsInWorld) {
                virtualHandObject.transform.parent = parent;
            }

            RetargetingHand virtualHand = virtualHandObject.GetComponent<RetargetingHand>();

            if (virtualHand != null)
            {
                virtualHand.HandType = HandType.Virtual;
                virtualHand.gameObject.name = prefab.name + " (Virtual)";
                virtualHand.transform.localPosition = Vector3.zero;
                virtualHand.transform.localRotation = Quaternion.identity;
                virtualHand.gameObject.layer = LayerMask.NameToLayer(Constants.VirtualEnvironmentLayer);
            }

            controller.TrackedHand = trackedHand;
            controller.VirtualHand = virtualHand;


            return controller;
        }

        RetargetingCamera CreateCamera(Camera baseCamera) {
            return baseCamera.gameObject.AddComponent<RetargetingCamera>();
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            r.height = thickness;
            r.y+=padding/2;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);
        }

        void DetectTargets() {
            if (manager != null) {
               

            }
        }

    }
}