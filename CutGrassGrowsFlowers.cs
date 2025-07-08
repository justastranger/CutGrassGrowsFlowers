using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;

namespace CutGrassGrowsFlowers
{

    [BepInPlugin("jas.Dinkum.CutGrassGrowsFlowers", "Cut Grass Grows Flowers", "1.0.4")]
    public class CutGrassGrowsFlowers : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Harmony harmony = new Harmony("jas.Dinkum.CutGrassGrowsFlowers");

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo("Plugin jas.Dinkum.CutGrassGrowsFlowers is loaded!");
            harmony.PatchAll();
        }
    }


    [HarmonyPatch("WorldManager", "nextDayChanges")]
    public class WorldManagerPatch
    {
        private static List<int> wildFlowers = new List<int> { 201, 202, 203, 204, 205, 498, 499, 500, 516, 517, 518, 521, 522, 523, 573, 574, 575, 783, 784, 785, 786, 787 };

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
                            if (__instance.onTileMap[j, i] == -1)
                            {
                                if (__instance.tileTypeMap[j, i] == MowedBushGrass || __instance.tileTypeMap[j, i] == PineGrassMowed || __instance.tileTypeMap[j, i] == TropicalGrassMowed)
                                {
                                    if (__instance.tileTypeMap[j, i] == MowedBushGrass)
                                    {
                                        // GenerateMap.generate.bushLandGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
                                        int objectToSpawn = GenerateMap.generate.bushLandGrowBack.getBiomObject();
                                        // one chance at a reroll
                                        if (!wildFlowers.Contains(objectToSpawn))
                                        {
                                            objectToSpawn = GenerateMap.generate.bushLandGrowBack.getBiomObject();
                                        }

                                        if (wildFlowers.Contains(objectToSpawn))
                                        {
                                            __instance.onTileMap[j, i] = objectToSpawn;
                                            if (objectToSpawn != -1 && (bool)__instance.allObjects[objectToSpawn].tileObjectGrowthStages)
                                            {
                                                __instance.onTileStatusMap[j, i] = 0;
                                            }
                                        }
                                    }
                                    else if (__instance.tileTypeMap[j, i] == PineGrassMowed)
                                    {
                                        int objectToSpawn = GenerateMap.generate.coldLandGrowBack.getBiomObject();
                                        // one chance at a reroll
                                        if (!wildFlowers.Contains(objectToSpawn))
                                        {
                                            objectToSpawn = GenerateMap.generate.coldLandGrowBack.getBiomObject();
                                        }
                                        if (wildFlowers.Contains(objectToSpawn))
                                            __instance.onTileMap[j, i] = objectToSpawn;
                                    }
                                    else if (__instance.tileTypeMap[j, i] == TropicalGrassMowed)
                                    {
                                        int objectToSpawn = GenerateMap.generate.tropicalGrowBack.getBiomObject();
                                        // one chance at a reroll
                                        if (!wildFlowers.Contains(objectToSpawn))
                                        {
                                            objectToSpawn = GenerateMap.generate.tropicalGrowBack.getBiomObject();
                                        }
                                        if (wildFlowers.Contains(objectToSpawn))
                                            __instance.onTileMap[j, i] = objectToSpawn;
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
    }
}