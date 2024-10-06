using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System;

public class CustomRenderPassFeature : ScriptableRendererFeature
{    
    class CustomRenderPass : ScriptableRenderPass
    {
        List<CustomVolumeComponent> volumeComponents;
        List<int> activeComponents;

        string profilerTag;
        List<ProfilingSampler> profilingSamplers;

        RenderTargetHandle source;
        RenderTargetHandle destination;
        RenderTargetHandle tempRT0;
        RenderTargetHandle tempRT1;

        public CustomRenderPass(string profilerTag, List<CustomVolumeComponent> volumeComponents)
        {
            this.profilerTag = profilerTag;
            this.volumeComponents = volumeComponents;
            activeComponents = new List<int>(volumeComponents.Count);
            profilingSamplers = new List<ProfilingSampler>();
            foreach (var com in volumeComponents)
            {
                profilingSamplers.Add(new ProfilingSampler(com.ToString()));
            }
            tempRT0.Init("_TemporaryRenderTexture0");
            tempRT1.Init("_TemporaryRenderTexture1");
        }


        public bool SetupComponents()
        {
            activeComponents.Clear();
            for (int i = 0; i < volumeComponents.Count; i++)
            {
                volumeComponents[i].SetUp();
                if (volumeComponents[i].IsActive())
                {
                    activeComponents.Add(i);
                }
            }
            return activeComponents.Count != 0;
        }

        public void Setup(RenderTargetHandle source, RenderTargetHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(profilerTag);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // 获取Descriptor
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;

            // 初始化临时RT
            RenderTargetIdentifier buff0, buff1;
            bool rt1Used = false;
            cmd.GetTemporaryRT(tempRT0.id, descriptor);
            buff0 = tempRT0.id;
            // 如果destination没有初始化，则需要获取RT，主要是destinaton为_AfterPostProcessTexture的情况
            if (destination != RenderTargetHandle.CameraTarget && !destination.HasInternalRenderTargetId())
            {
                cmd.GetTemporaryRT(destination.id, descriptor);
            }

            // 执行每个组件的Render方法
            // 如果只有一个组件，则直接source -> buff0
            if (activeComponents.Count == 1)
            {
                int index = activeComponents[0];
                using (new ProfilingScope(cmd, profilingSamplers[index]))
                {
                    volumeComponents[index].Render(cmd, ref renderingData, source.Identifier(), buff0);
                }
            }
            else
            {
                // 如果有多个组件，则在两个RT上左右横跳
                cmd.GetTemporaryRT(tempRT1.id, descriptor);
                buff1 = tempRT1.id;
                rt1Used = true;
                Blit(cmd, source.Identifier(), buff0);
                for (int i = 0; i < activeComponents.Count; i++)
                {
                    int index = activeComponents[i];
                    var component = volumeComponents[index];
                    using (new ProfilingScope(cmd, profilingSamplers[index]))
                    {
                        component.Render(cmd, ref renderingData, buff0, buff1);
                    }
                    CoreUtils.Swap(ref buff0, ref buff1);
                }
            }

            // 最后blit到destination
            Blit(cmd, buff0, destination.Identifier());

            // 释放
            cmd.ReleaseTemporaryRT(tempRT0.id);
            if (rt1Used)
                cmd.ReleaseTemporaryRT(tempRT1.id);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass afterOpaqueAndSky;
    CustomRenderPass beforePostProcess;
    CustomRenderPass afterPostProcess;

    List<CustomVolumeComponent> components;

    RenderTargetHandle afterPostProcessTexture;

    public override void Create()
    {
        var stack = VolumeManager.instance.stack;
        components = new List<CustomVolumeComponent>();
        foreach (var type in VolumeManager.instance.baseComponentTypeArray)
        {
            if (type.IsSubclassOf(typeof(CustomVolumeComponent)) && stack.GetComponent(type) != null)
            {
                components.Add(stack.GetComponent(type) as CustomVolumeComponent);
            }
        }
        List<CustomVolumeComponent> tempCom = new List<CustomVolumeComponent>();
        foreach (var com in components)
        {
            if (com.InjectionPoint == CustomPostProcessInjectionPoint.AfterOpaqueAndSky)
            {
                tempCom.Add(com);
            }
        }
        tempCom.Sort((a, b) => a.OrderInPass.CompareTo(b.OrderInPass));
        afterOpaqueAndSky = new CustomRenderPass("Custom PostProcess after Opaque and Sky", tempCom);
        afterOpaqueAndSky.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        
        tempCom = new List<CustomVolumeComponent>();
        foreach (var com in components)
        {
            if (com.InjectionPoint == CustomPostProcessInjectionPoint.BeforePostProcess)
            {
                tempCom.Add(com);
            }
        }
        tempCom.Sort((a, b) => a.OrderInPass.CompareTo(b.OrderInPass));
        beforePostProcess = new CustomRenderPass("Custom PostProcess before PostProcess", tempCom);
        beforePostProcess.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        tempCom = new List<CustomVolumeComponent>();
        foreach (var com in components)
        {
            if (com.InjectionPoint == CustomPostProcessInjectionPoint.AfterPostProcess)
            {
                tempCom.Add(com);
            }
        }
        tempCom.Sort((a, b) => a.OrderInPass.CompareTo(b.OrderInPass));
        afterPostProcess = new CustomRenderPass("Custom PostProcess after PostProcess", tempCom);
        afterPostProcess.renderPassEvent = RenderPassEvent.AfterRendering;

        afterPostProcessTexture.Init("_AfterPostProcessTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled)
        {
            var source = new RenderTargetHandle(renderer.cameraColorTarget);
            if (afterOpaqueAndSky.SetupComponents())
            {
                afterOpaqueAndSky.Setup(source, source);
                renderer.EnqueuePass(afterOpaqueAndSky);
            }
            if (beforePostProcess.SetupComponents())
            {
                beforePostProcess.Setup(source, source);
                renderer.EnqueuePass(beforePostProcess);
            }
            if (afterPostProcess.SetupComponents())
            {
                source = renderingData.cameraData.resolveFinalTarget ? afterPostProcessTexture : source;
                afterPostProcess.Setup(source, source);
                renderer.EnqueuePass(afterPostProcess);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && components != null)
        {
            foreach (var com in components)
            {
                com.Dispose();
            }
        }
    }
}


