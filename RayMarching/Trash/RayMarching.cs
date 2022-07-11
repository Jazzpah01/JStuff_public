//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.HighDefinition;
//using System;

//[Serializable, VolumeComponentMenu("Post-processing/Custom/RayMarching")]
//public sealed class RayMarching : CustomPostProcessVolumeComponent, IPostProcessComponent
//{
//    [Tooltip("Controls the intensity of the effect.")]
//    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

//    Material m_Material;

//    public bool IsActive() => m_Material != null && intensity.value > 0f;

//    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Settings).
//    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

//    const string kShaderName = "Hidden/Shader/RayMarching";

//    public override void Setup()
//    {
//        if (Shader.Find(kShaderName) != null)
//            m_Material = new Material(Shader.Find(kShaderName));
//        else
//            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume RayMarching is unable to load.");
//    }

//    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
//    {
//        if (m_Material == null)
//            return;

//        m_Material.SetFloat("_Intensity", intensity.value);
//        cmd.Blit(source, destination, m_Material, 0);
//    }

//    public override void Cleanup()
//    {
//        CoreUtils.Destroy(m_Material);
//    }
//}
