﻿using BiomesCore.DefModExtensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace BiomesCore.MapGeneration
{
    /// <summary>
    /// Allows rock chunks to be generated on maps that are below sea level
    /// </summary>
    public class GenStep_IslandRockChunks : GenStep
    {
        private ModuleBase freqFactorNoise;

        private const float ThreshLooseRock = 0.55f;

        private const float PlaceProbabilityPerCell = 0.006f;

        private const float RubbleProbability = 0.5f;

        public override int SeedPart
        {
            get
            {
                return 1898758716;
            }
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            if (!map.Biome.HasModExtension<BiomesMap>())
            {
                return;
            }
            if (!map.Biome.GetModExtension<BiomesMap>().isIsland)
            {
                return;
            }


            this.freqFactorNoise = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
            this.freqFactorNoise = new ScaleBias(1.0, 1.0, this.freqFactorNoise);
            NoiseDebugUI.StoreNoiseRender(this.freqFactorNoise, "rock_chunks_freq_factor");
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            foreach (IntVec3 current in map.AllCells)
            {
                float num = 0.006f * this.freqFactorNoise.GetValue(current);
                if (elevation[current] < 0.55f && Rand.Value < num)
                {
                    this.GrowLowRockFormationFrom(current, map);
                }
            }
            this.freqFactorNoise = null;
        }

        private void GrowLowRockFormationFrom(IntVec3 root, Map map)
        {
            ThingDef filth_RubbleRock = ThingDefOf.Filth_RubbleRock;
            ThingDef mineableThing = Find.World.NaturalRockTypesIn(map.Tile).RandomElement<ThingDef>().building.mineableThing;
            Rot4 random = Rot4.Random;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            IntVec3 intVec = root;
            while (true)
            {
                Rot4 random2 = Rot4.Random;
                if (!(random2 == random))
                {
                    intVec += random2.FacingCell;
                    if (!intVec.InBounds(map) || intVec.GetEdifice(map) != null || intVec.GetFirstItem(map) != null)
                    {
                        break;
                    }
                    if (elevation[intVec] > 0.55f)
                    {
                        return;
                    }
                    if (!map.terrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                    {
                        return;
                    }
                    GenSpawn.Spawn(mineableThing, intVec, map, WipeMode.Vanish);
                    IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
                    for (int i = 0; i < adjacentCellsAndInside.Length; i++)
                    {
                        IntVec3 b = adjacentCellsAndInside[i];
                        if (Rand.Value < 0.5f)
                        {
                            IntVec3 c = intVec + b;
                            if (c.InBounds(map))
                            {
                                bool flag = false;
                                List<Thing> thingList = c.GetThingList(map);
                                for (int j = 0; j < thingList.Count; j++)
                                {
                                    Thing thing = thingList[j];
                                    if (thing.def.category != ThingCategory.Plant && thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Pawn)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    FilthMaker.TryMakeFilth(c, map, filth_RubbleRock, 1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}