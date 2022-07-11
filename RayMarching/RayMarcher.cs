using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarcher : MonoBehaviour
{
    public Shader shader;
    private Material mat;

    private void Start()
    {
        mat = new Material(shader);
    }

    private void Update()
    {
        https://forum.unity.com/threads/calculating-inverse-view-projection-for-a-shader.991097/
        // get GPU projection matrix
        Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);

        // get GPU view projection matrix
        Matrix4x4 viewProjMatrix = projMatrix * Camera.main.worldToCameraMatrix;

        // get inverse VP matrix
        Matrix4x4 inverseViewProjMatrix = viewProjMatrix.inverse;

        mat.SetMatrix("_InverseViewProjection", inverseViewProjMatrix);
    }

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    Graphics.Blit(source, destination, mat);
    //}
}