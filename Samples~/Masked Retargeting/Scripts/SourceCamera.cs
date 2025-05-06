using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SourceCamera : MonoBehaviour {
    Camera _cam;

    public Camera Camera => _cam;
    public Action<RenderTexture, RenderTexture> onRenderImage;
    private void Start() {
        _cam = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest);
        if (onRenderImage != null) onRenderImage(src, dest);
    }
}