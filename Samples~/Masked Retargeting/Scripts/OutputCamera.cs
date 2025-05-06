using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OutputCamera : MonoBehaviour {
    Camera _cam;

    public Camera Camera => _cam;
    public Action<Camera> onPreRender;
    private void Start() {
        _cam = GetComponent<Camera>();
    }

    private void OnPreRender() {
        onPreRender(_cam);
    }
}