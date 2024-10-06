using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/ScreenBreak", typeof(UniversalRenderPipeline))]
public class ScreenBreak : CustomVolumeComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0, 0, 0.1f);
    public ClampedFloatParameter alpha = new ClampedFloatParameter(0, 0, 10);
    public Texture2DParameter breakTex = new Texture2DParameter(null);
    public Texture2DParameter maskTex = new Texture2DParameter(null);

    Material material;
    string shaderName = "Shader Graphs/ScreenBreak";

    public override int OrderInPass => 1;

    public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

    public override bool IsActive() => material != null && (intensity.value > 0 || alpha.value > 0);

    public override void SetUp()
    {
        if (material == null)
        {
            material = CoreUtils.CreateEngineMaterial(shaderName);
        }
    }

    public override void Render(CommandBuffer cmd, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier destination)
    {
        if (material == null)
        {
            return;
        }

        material.SetTexture("_BreakTex1", breakTex.value);
        material.SetTexture("_BreakTex2", maskTex.value);
        material.SetFloat("_Intensity", intensity.value);
        material.SetFloat("_Alpha", alpha.value);
        cmd.Blit(source, destination, material);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
    }
}
