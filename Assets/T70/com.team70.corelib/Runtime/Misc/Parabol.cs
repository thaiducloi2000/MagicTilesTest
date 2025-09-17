﻿using System;
using UnityEngine;

[Serializable]
public class Parabol
{
    public float g;
    public float v0;
    public float y0;

    public float GetY(float t)
    {
        return y0 + v0 * t + g * t * t / 2f;
    }

	public float GetVY(float t)
	{
		return v0 + g*t;
	}

    public static Parabol Calculate(float hA, float hB, float hJump)
    {
        var result = new Parabol();
        var hMax = Mathf.Max(hA, hB) + hJump;

        var h1 = hMax - hA;
        var h2 = hB - hA;

        result.y0 = hA;
        result.v0 = 2 * h1 + 2 * Mathf.Sqrt(h1 * (h1 - h2));
        result.g = 2 * (h2 - result.v0);

        return result;
    }
}