using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CutGrassGrowsFlowers
{

    [BepInPlugin("jas.Dinkum.CutGrassGrowsFlowers", "Cut Grass Grows Flowers", "1.1.0")]
    public class CutGrassGrowsFlowers : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Harmony harmony = new Harmony("jas.Dinkum.CutGrassGrowsFlowers");
        internal static ConfigEntry<int> numberOfRolls;
        internal static ConfigEntry<bool> spawnMushrooms;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            numberOfRolls = Config.Bind<int>("config", "numberOfRolls", 2, "The number of times that a wildflower will attempt to be generated, per tile. Higher numbers increase the odds.");
            spawnMushrooms = Config.Bind<bool>("config", "spawnMushrooms", true, "Controls whether or not mushrooms will spawn on mowed grass.");
            Logger.LogInfo("Plugin jas.Dinkum.CutGrassGrowsFlowers is loaded!");
            harmony.PatchAll();
            if (spawnMushrooms.Value)
            {
                WorldManagerPatch.wildFlowers.UnionWith(new int[] { 783, 784, 785, 786, 787 });
            }
        }
    }


    [HarmonyPatch("WorldManager", "nextDayChanges")]
    public class WorldManagerPatch
    {
        internal static readonly HashSet<int> wildFlowers = new HashSet<int> { 201, 202, 203, 204, 205, 498, 499, 500, 516, 517, 518, 521, 522, 523, 573, 574, 575 };

        public static void Postfix(WorldManager __instance, bool raining, int mineSeed)
        {
            int MowedBushGrass = 23;
            int PineGrassMowed = 24;
            int TropicalGrassMowed = 25;
            var mapSize = 1000;
            for (int chunkY = 0; chunkY < mapSize / 10; chunkY++)
            {
                for (int chunkX = 0; chunkX < mapSize / 10; chunkX++)
                {
                    if (!__instance.chunkChangedMap[chunkX, chunkY])
                    {
                        continue;
                    }
                    UnityEngine.Random.InitState(mineSeed + chunkX * chunkY);
                    for (int i = chunkY * 10; i < chunkY * 10 + 10; i++)
                    {
                        for (int j = chunkX * 10; j < chunkX * 10 + 10; j++)
                        {
                            // only trigger on "empty" tiles
                            if (__instance.onTileMap[j, i] == -1)
                            {
                                // where the tile type is one of the three mowed grass types
                                if (__instance.tileTypeMap[j, i] == MowedBushGrass || __instance.tileTypeMap[j, i] == PineGrassMowed || __instance.tileTypeMap[j, i] == TropicalGrassMowed)
                                {
                                    if (__instance.tileTypeMap[j, i] == MowedBushGrass)
                                    {
                                        // the new function either returns a wildflower ID or it returns -1, both of which are safe to assign without further checking
                                        __instance.onTileMap[j, i] = RollBiomeSpawnTableForFlower(GenerateMap.generate.bushLandGrowBack, CutGrassGrowsFlowers.numberOfRolls.Value);
                                    }
                                    else if (__instance.tileTypeMap[j, i] == PineGrassMowed)
                                    {
                                        __instance.onTileMap[j, i] = RollBiomeSpawnTableForFlower(GenerateMap.generate.coldLandGrowBack, CutGrassGrowsFlowers.numberOfRolls.Value);
                                    }
                                    else if (__instance.tileTypeMap[j, i] == TropicalGrassMowed)
                                    {
                                        __instance.onTileMap[j, i] = RollBiomeSpawnTableForFlower(GenerateMap.generate.tropicalGrowBack, CutGrassGrowsFlowers.numberOfRolls.Value);
                                    }
                                    if (__instance.onTileMap[j, i] > -1)
                                    {
                                        if ((bool)__instance.allObjects[__instance.onTileMap[j, i]].tileObjectGrowthStages)
                                        {
                                            __instance.onTileStatusMap[j, i] = 0;
                                        }
                                        __instance.chunkHasChangedToday[chunkX, chunkY] = true;
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private static int RollBiomeSpawnTableForFlower(BiomSpawnTable table, int attempts = 1)
        {
            if (table == null) throw new NullReferenceException("BiomSpawnTable must not be null");
            // potentially expensive check against spawn tables that lack flowers
            // if (!table.objectsInBiom.Select(to => to.tileObjectId).Intersect(wildFlowers).Any()) return -1;
            int tileObjectId = table.getBiomObject();
            // chance to reroll an arbitrary number of times, skipped if there isn't more than 1 roll or if it's already a wildflower
            // potentially an issue if a BiomSpawnTable without *any* wild flowers is supplied
            if (attempts > 1 && !wildFlowers.Contains(tileObjectId))
            {
                while (!wildFlowers.Contains(tileObjectId) && attempts > 1)
                {
                    tileObjectId = table.getBiomObject();
                    attempts--;
                }
            }
            // only return the tile object ID if it's a flower since the while loop will produce a non-flower value if it's not lucky
            if (wildFlowers.Contains(tileObjectId)) return tileObjectId;
            // otherwise return an "empty" tile
            // since we're only operating on empty tiles to begin with, this should be safe to skip a check for -1 and assign it over itself
            else return -1;
        }
    }
}