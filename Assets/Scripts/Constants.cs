using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    #region Zone Color Difference
    public static float maxDepth = 1000;
    public static float maxFogDensity = 0.1f;
    public static float minFogDensity = 0.03f;
    public static (float depth, Color color)[] depthColors =
    {
        (0, new Color(50, 130, 200)), // Beninging
        (40, new Color(50, 130, 200)), // Sunlight Zone ends
        (100, new Color(100, 40, 145)), // Twilight Zone begins
        (220, new Color(100, 40, 145)), // Twilight Zone ends
        (400, new Color(10, 60, 30)), // Midnight Zone begins
        (1000, new Color(10, 60, 30)) // Midnight Zone end
    };
    public static ZoneID GetCurZoneID(float curDepth)
    {
        // Sunlight Zone
        if (0 < curDepth && curDepth < 40) return ZoneID.Sunlight;
        // Twilight Zone
        if (100 < curDepth && curDepth < 220) return ZoneID.Twilight;
        // Midnight Zone
        if (400 < curDepth && curDepth < 1000) return ZoneID.Midnight;

        return ZoneID.None;
    }
    #endregion

    #region Layermasks
    public static LayerMask LAYER_BOID = 1 << LayerMask.NameToLayer("Boid");
    public static LayerMask LAYER_CREATURE = 1 << LayerMask.NameToLayer("Creature");
    public static LayerMask LAYER_TERRAIN = 1 << LayerMask.NameToLayer("Terrain");
    public static LayerMask LAYER_PLAYER = 1 << LayerMask.NameToLayer("Player");
    public static LayerMask LAYER_SUBMARINE = 1 << LayerMask.NameToLayer("Submarine");
    #endregion

    #region Settings

    public static float mouseSensitivity = 5;

    #endregion
}
