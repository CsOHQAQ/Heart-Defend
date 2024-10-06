using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public abstract class CustomVolumeComponent : VolumeComponent, IPostProcessComponent, IDisposable
{
    /// <summary>
    /// 在同一插入点，渲染执行的顺序
    /// </summary>
    public virtual int OrderInPass => 0;
    /// <summary>
    /// 插入点
    /// </summary>
    public virtual CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterOpaqueAndSky;
    /// <summary>
    /// 渲染的准备工作，比如准备材质等
    /// </summary>
    public abstract void SetUp();
    /// <summary>
    /// 执行渲染。
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="renderingData"></param>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public abstract void Render(CommandBuffer cmd, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier destination);
    /// <summary>
    /// 是否开启此渲染。如果一个后处理没有效果，请让这个方法返回false以节省性能
    /// </summary>
    /// <returns></returns>
    public abstract bool IsActive();

    public virtual bool IsTileCompatible() => false;
    /// <summary>
    /// 回收资源。防止材质漏的到处都是
    /// </summary>
    /// <param name="disposing"></param>
    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
public enum CustomPostProcessInjectionPoint
{
    AfterOpaqueAndSky,
    BeforePostProcess,
    AfterPostProcess,
}
