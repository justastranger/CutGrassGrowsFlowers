using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;

namespace CutGrassGrowsFlowers
{

    [BepInPlugin("jas.CutGrassGrowsFlowers", "Cut Grass Grows Flowers", "1.0.1")]
    public class CutGrassGrowsFlowers : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Harmony harmony = new Harmony("jas.CutGrassGrowsFlowers");

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo("Plugin jas.CutGrassGrowsFlowers is loaded!");
            harmony.PatchAll();
        }
    }


    [HarmonyPatch("WorldManager", "nextDayChanges")]
    public class WorldManagerPatch
    {
        public static void Postfix(WorldManager __instance, bool raining, int mineSeed)
        {
            List<int> bushLandNonFlowers = new List<int> { 1, 13, 403 };
            int bushLandCutGrass = 23;
            List<int> tropicalNonFlowers = new List<int> { 1, 4, 5, 135, 296, 297, 403, 407 };
            int tropicalCutGrass = 24;
            List<int> coldLandsNonFlowers = new List<int> { 137, 402 };
            int coldLandsCutGrass = 25;
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
                            if (__instance.tileTypeMap[j, i] == bushLandCutGrass || __instance.tileTypeMap[j, i] == coldLandsCutGrass || __instance.tileTypeMap[j, i] == tropicalCutGrass)
                            {
                                if (__instance.tileTypeMap[j, i] == bushLandCutGrass)
                                {
                                    // GenerateMap.generate.bushLandGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
                                    int objectToSpawn = GenerateMap.generate.bushLandGrowBack.getBiomObject();
                                    if (!bushLandNonFlowers.Contains(objectToSpawn))
                                    {
                                        __instance.onTileMap[j, i] = objectToSpawn;
                                        if (objectToSpawn != -1 && (bool)__instance.allObjects[objectToSpawn].tileObjectGrowthStages)
                                        {
                                            __instance.onTileStatusMap[j, i] = 0;
                                        }
                                    }
                                }
                                else if (__instance.tileTypeMap[j, i] == coldLandsCutGrass)
                                {
                                    int objectToSpawn = GenerateMap.generate.tropicalGrowBack.getBiomObject();
                                    if (!tropicalNonFlowers.Contains(objectToSpawn))
                                        __instance.onTileMap[j, i] = objectToSpawn;
                                }
                                else if (__instance.tileTypeMap[j, i] == tropicalCutGrass)
                                {
                                    int objectToSpawn = GenerateMap.generate.coldLandGrowBack.getBiomObject();
                                    if (!coldLandsNonFlowers.Contains(objectToSpawn))
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