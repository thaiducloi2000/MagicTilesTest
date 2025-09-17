﻿using System;
using UnityEngine;
using UnityEngine.Rendering;

public class EnvSystem : MonoBehaviour
{
    public bool applyOnEnable = true;
    public EnvFog fogInfo;
    public EnvLight lightInfo;
    public EnvSkybox skyboxInfo;


    [ContextMenu("Read")]
    private void Read()
    {
        fogInfo.Read();
        lightInfo.Read();
        skyboxInfo.Read();
    }

    [ContextMenu("Write")]
    public void Write()
    {
        // Debug.Log("Write Env System");
        fogInfo.Write();
        lightInfo.Write();
        skyboxInfo.Write();
    }

    private void OnEnable()
    {
        if (applyOnEnable) Write();
    }
}

[Serializable]
public class EnvFog
{
    public bool enable;
    public Color fogColor;
    public float fogDensity;
    public float fogEndDistance;
    public int fogMode;
    public float fogStartDistance;

    public void Read()
    {
        enable = RenderSettings.fog;
        fogDensity = RenderSettings.fogDensity;
        fogColor = RenderSettings.fogColor;
        fogMode = (int) RenderSettings.fogMode;
        fogEndDistance = RenderSettings.fogEndDistance;
        fogStartDistance = RenderSettings.fogStartDistance;
    }

    public void Write()
    {
        RenderSettings.fog = enable;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = (FogMode) fogMode;
        RenderSettings.fogEndDistance = fogEndDistance;
        RenderSettings.fogStartDistance = fogStartDistance;
    }
}

[Serializable]
public class EnvLight
{
    [ColorUsage(true, true)] public Color ambientEquatorColor;
    [ColorUsage(true, true)] public Color ambientGroundColor;

    public float ambientIntensity;
    [ColorUsage(true, true)] public Color ambientLight;
    public AmbientMode ambientMode; // AmbientMode: Skybox = 0, Trilight = 1, Flat = 3, Custom = 4
    [ColorUsage(true, true)] public Color ambientSkyColor;

    public void Read()
    {
        ambientSkyColor = RenderSettings.ambientSkyColor;
        ambientEquatorColor = RenderSettings.ambientEquatorColor;
        ambientGroundColor = RenderSettings.ambientGroundColor;
        ambientIntensity = RenderSettings.ambientIntensity;
        ambientLight = RenderSettings.ambientLight;
        ambientMode = RenderSettings.ambientMode;
    }

    public void Write()
    {
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.ambientMode = ambientMode;
    }
}

[Serializable]
public class EnvSkybox
{
    public Material matSkybox;

    public void Read()
    {
        matSkybox = RenderSettings.skybox;
    }

    public void Write()
    {
        RenderSettings.skybox = matSkybox;
    }
}