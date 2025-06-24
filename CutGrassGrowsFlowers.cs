using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CutGrassGrowsFlowers
{

    [BepInPlugin("jas.CutGrassGrowsFlowers", "Cut Grass Grows Flowers", "1.0.0")]
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
            // harmony.Patch(nextDayChangesNestedType.GetMethod("MoveNext"), transpiler: new HarmonyMethod(typeof(WorldManagerPatch).GetMethod("nextDayChanges_Transpiler")));
        }

        private void Start()
        {

        }
    }

    [HarmonyPatch("WorldManager+<nextDayChanges>d__135")]
    public class WorldManagerPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("WorldManager+<nextDayChanges>d__135", "MoveNext")]
        public static IEnumerable<CodeInstruction> nextDayChanges_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo grassTypeField = typeof(WorldManager).GetNestedType("<nextDayChanges>d__135", BindingFlags.NonPublic).GetField("<grassType>5__4", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo tropicalGrassTypeField = typeof(WorldManager).GetNestedType("<nextDayChanges>d__135", BindingFlags.NonPublic).GetField("<tropicalGrassType>5__5", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo pineGrassTypeField = typeof(WorldManager).GetNestedType("<nextDayChanges>d__135", BindingFlags.NonPublic).GetField("<pineGrassType>5__6", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo tileTypeMapField = AccessTools.Field(typeof(WorldManager), "tileTypeMap");

            FieldInfo onTileMapField = AccessTools.Field(typeof(WorldManager), "onTileMap");
            FieldInfo onTileStatusMapField = AccessTools.Field(typeof(WorldManager), "onTileStatusMap");
            FieldInfo allObjectsField = AccessTools.Field(typeof(WorldManager), "allObjects");
            FieldInfo generateField = AccessTools.Field(typeof(GenerateMap), "generate");
            FieldInfo bushLandGrowBackField = AccessTools.Field(typeof(GenerateMap), "bushLandGrowBack");
            FieldInfo tileObjectGrowthStagesField = AccessTools.Field(typeof(TileObject), "tileObjectGrowthStages");

            MethodInfo arrayGetMethod = typeof(int[,]).GetMethod("Get", new[] { typeof(int), typeof(int) });
            MethodInfo arraySetMethod = typeof(int[,]).GetMethod("Set", new[] { typeof(int), typeof(int), typeof(int) });
            MethodInfo getBiomObjectMethod = typeof(BiomSpawnTable).GetMethod("getBiomObject", new Type[] { typeof(MapRand) });
            MethodInfo op_ImplicitMethod = typeof(UnityEngine.Object).GetMethod("op_Implicit");
            MethodInfo getRandomObjectAndPlaceWithGrowthMethod = typeof(BiomSpawnTable).GetMethod("getRandomObjectAndPlaceWithGrowth", new Type[] { typeof(int), typeof(int) });


            CodeMatcher matcher = new CodeMatcher(instructions, generator);

            CodeMatch[] firstTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, grassTypeField),
                new CodeMatch(OpCodes.Beq) // grab label from operand
            };
            matcher.MatchStartForward(firstTarget);

            // grab label for start of inner if statement
            var IL_03a1 = matcher.InstructionAt(2).operand;
            List<CodeInstruction> firstInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 23),
                new CodeInstruction(OpCodes.Beq, IL_03a1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(firstInsertion);

            CodeMatch[] secondTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, tropicalGrassTypeField),
                new CodeMatch(OpCodes.Beq) // label is the same as previous jump, can ignore
            };
            matcher.MatchStartForward(secondTarget);

            List<CodeInstruction> secondInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 25),
                new CodeInstruction(OpCodes.Beq, IL_03a1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(secondInsertion);

            CodeMatch[] thirdTarget = new CodeMatch[]
            {
                // insert new code here
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, pineGrassTypeField),
                new CodeMatch(OpCodes.Bne_Un)  // grab label from operand
            };

            matcher.MatchStartForward(thirdTarget);

            List<CodeInstruction> thirdInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 24),
                new CodeInstruction(OpCodes.Beq, IL_03a1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(thirdInsertion);

            CodeMatch[] fourthTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, grassTypeField),
                new CodeMatch(OpCodes.Bne_Un),
                new CodeMatch(OpCodes.Ldsfld)
            };
            matcher.MatchStartForward(fourthTarget);

            // grab label for inside if block
            var IL_03b6 = generator.DefineLabel();
            matcher.InstructionAt(3).labels.Add(IL_03b6);
            List<CodeInstruction> fourthInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 23),
                new CodeInstruction(OpCodes.Beq, IL_03b6),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(fourthInsertion);


            // cursed bullshit to inline a function call
            Label inlineJumpLabel = generator.DefineLabel();
            Label inlineEndLabel = generator.DefineLabel();
            Label inlinePopLabel = generator.DefineLabel();
            Label inlinePlaceAllowedLabel = generator.DefineLabel();
            List<CodeInstruction> inlineGetRandomObjectAndPlaceWithGrowth = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, onTileMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldsfld, generateField),
                new CodeInstruction(OpCodes.Ldfld, bushLandGrowBackField),
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Call, getBiomObjectMethod),

                // check if we're trying to spawn on cut grass, skipping the next check if it's regular grass
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod),
                new CodeInstruction(OpCodes.Ldc_I4, 23),
                new CodeInstruction(OpCodes.Bne_Un, inlinePlaceAllowedLabel),

                // Check against grass and desert spinifex IDs here
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Beq, inlinePopLabel),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4, 13),
                new CodeInstruction(OpCodes.Beq, inlinePopLabel),

                // if it's acceptable, place it in the world
                new CodeInstruction(OpCodes.Call, arraySetMethod).WithLabels(inlinePlaceAllowedLabel),

                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, onTileMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod),
                new CodeInstruction(OpCodes.Ldc_I4_M1),
                new CodeInstruction(OpCodes.Beq, inlineJumpLabel), // need label for br

                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, allObjectsField),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, onTileMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod),
                new CodeInstruction(OpCodes.Ldelem_Ref),
                new CodeInstruction(OpCodes.Ldfld, tileObjectGrowthStagesField),
                new CodeInstruction(OpCodes.Call, op_ImplicitMethod),
                new CodeInstruction(OpCodes.Brfalse, inlineJumpLabel), // same label as beq

                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, onTileStatusMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, arraySetMethod),

                new CodeInstruction(OpCodes.Br, inlineEndLabel).WithLabels(inlineJumpLabel), // jump target that needs label

                new CodeInstruction(OpCodes.Pop).WithLabels(inlinePopLabel),
                new CodeInstruction(OpCodes.Nop).WithLabels(inlineEndLabel) // jump target that needs label
            };
            CodeMatch[] callvirtTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Callvirt, getRandomObjectAndPlaceWithGrowthMethod)
            };
            matcher.MatchEndForward(callvirtTarget);
            // remove the callvirt to getRandomObjectAndPlaceWithGrowth
            matcher.RemoveInstruction();
            // insert our custom version of the function
            matcher.InsertAndAdvance(inlineGetRandomObjectAndPlaceWithGrowth);

            CodeMatch[] fifthTarget = new CodeMatch[]
            {
                // insert new code here
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, tropicalGrassTypeField),
                new CodeMatch(OpCodes.Bne_Un) // grab label from operand
            };
            matcher.MatchStartForward(fifthTarget);

            // grab label for inside if block
            var IL_03de = generator.DefineLabel();
            matcher.InstructionAt(3).labels.Add(IL_03de);
            List<CodeInstruction> fifthInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 25),
                new CodeInstruction(OpCodes.Beq, IL_03de),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(fifthInsertion);

            CodeMatch[] sixthTarget = new CodeMatch[]
            {
                // insert new code here
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, pineGrassTypeField),
                new CodeMatch(OpCodes.Bne_Un) // grab label from operand
            };
            matcher.MatchStartForward(sixthTarget);

            // grab label for inside if block
            var IL_0412 = generator.DefineLabel();
            matcher.InstructionAt(3).labels.Add(IL_0412);
            List<CodeInstruction> sixthInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 24),
                new CodeInstruction(OpCodes.Beq, IL_0412),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(sixthInsertion);

            return matcher.Instructions();
        }
    }
}
