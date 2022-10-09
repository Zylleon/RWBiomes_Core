﻿using Verse;
using RimWorld;
using HarmonyLib;

namespace BiomesCore.Patches
{
    [HarmonyPatch(typeof(Plant), "Graphic", MethodType.Getter)]
    internal static class Plant_Graphic
    {
        internal static Graphic Postfix(Graphic originalResult, Plant __instance)
        {
            var extension = __instance.def.GetModExtension<DefModExtensions.Plant_GraphicPerBiome>();
            if (extension != null)
            {
                if (__instance.LifeStage == PlantLifeStage.Sowing)
                {
                    return extension.SowingGraphicPerBiome(__instance.Map.Biome);
                }
                if (__instance.def.plant.leaflessGraphic != null && __instance.LeaflessNow && (!__instance.sown || !__instance.HarvestableNow))
                {
                    return extension.LeaflessGraphicPerBiome(__instance.Map.Biome);
                }
                if (__instance.def.plant.immatureGraphic != null && !__instance.HarvestableNow)
                {
                    return extension.ImmatureGraphicPerBiome(__instance.Map.Biome);
                }
                return extension.GraphicForBiome(__instance.Map.Biome);
            }
            return originalResult;
        }
    }
}