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

    public class Helper
    {


        static readonly Dictionary<OperandType, int> _operandSizes = new Dictionary<OperandType, int> {
            { OperandType.InlineNone,        0 },
            { OperandType.ShortInlineBrTarget, 1 },
            { OperandType.InlineBrTarget,      4 },
            { OperandType.ShortInlineI,        1 },
            { OperandType.InlineI,             4 },
            { OperandType.InlineI8,            8 },
            { OperandType.ShortInlineR,        4 },
            { OperandType.InlineR,             8 },
            { OperandType.ShortInlineVar,      1 }, // local or arg index
            { OperandType.InlineVar,           2 },
            { OperandType.InlineSwitch,        -1 }, // special-case below
            { OperandType.InlineString,        4 },
            { OperandType.InlineSig,           4 },
            { OperandType.InlineField,         4 },
            { OperandType.InlineType,          4 },
            { OperandType.InlineTok,           4 },
            { OperandType.InlineMethod,        4 }
          };

        public static int GetOpSize(OpCode op, object operand = null)
        {
            // 1) opcode.Size is 1 or 2
            var size = op.Size;

            // 2) operand size
            var operandType = op.OperandType;
            if (operandType == OperandType.InlineSwitch)
            {
                // operand for switch is an int[] of labels: first Int32 count, then that many Int32 targets
                var targets = operand as int[] ?? Array.Empty<int>();
                size += 4 + (4 * targets.Length);
            }
            else
            {
                size += _operandSizes.TryGetValue(operandType, out var opSize)
                  ? opSize
                  : throw new InvalidOperationException($"Unknown operand type {operandType}");
            }

            return size;
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
            FieldInfo tropicalGrowBackField = AccessTools.Field(typeof(GenerateMap), "tropicalGrowBack");
            FieldInfo coldLandGrowBackField = AccessTools.Field(typeof(GenerateMap), "coldLandGrowBack");
            FieldInfo tileObjectGrowthStagesField = AccessTools.Field(typeof(TileObject), "tileObjectGrowthStages");

            MethodInfo arrayGetMethod = typeof(int[,]).GetMethod("Get", new[] { typeof(int), typeof(int) });
            MethodInfo arraySetMethod = typeof(int[,]).GetMethod("Set", new[] { typeof(int), typeof(int), typeof(int) });
            MethodInfo getBiomObjectMethod = typeof(BiomSpawnTable).GetMethod("getBiomObject", new Type[] { typeof(MapRand) });
            MethodInfo op_ImplicitMethod = typeof(UnityEngine.Object).GetMethod("op_Implicit");
            MethodInfo getRandomObjectAndPlaceWithGrowthMethod = typeof(BiomSpawnTable).GetMethod("getRandomObjectAndPlaceWithGrowth", new Type[] { typeof(int), typeof(int) });


            CodeMatcher matcher = new CodeMatcher(instructions, generator);

            CodeMatch[] bushLandGrassOuterTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, grassTypeField),
                new CodeMatch(OpCodes.Beq) // grab label from operand
            };
            matcher.MatchStartForward(bushLandGrassOuterTarget);

            // grab label for start of inner if statement
            var IL_03a1 = matcher.InstructionAt(2).operand;
            List<CodeInstruction> cutBushLandGrassOuterInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 23),
                new CodeInstruction(OpCodes.Beq, IL_03a1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.InsertAndAdvance(cutBushLandGrassOuterInsertion);

            CodeMatch[] tropicalGrassOuterTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, tropicalGrassTypeField),
                new CodeMatch(OpCodes.Beq) // label is the same as previous jump, can ignore
            };
            matcher.MatchStartForward(tropicalGrassOuterTarget);

            List<CodeInstruction> cutTropicalGrassOuterInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 25),
                new CodeInstruction(OpCodes.Beq, IL_03a1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.InsertAndAdvance(cutTropicalGrassOuterInsertion);

            CodeMatch[] firGrassOuterTarget = new CodeMatch[]
            {
                // insert new code here
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, pineGrassTypeField),
                new CodeMatch(OpCodes.Bne_Un)  // grab label from operand
            };
            matcher.MatchStartForward(firGrassOuterTarget);

            List<CodeInstruction> cutFirGrassOuterInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 24),
                new CodeInstruction(OpCodes.Beq, IL_03a1),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.InsertAndAdvance(cutFirGrassOuterInsertion);

            CodeMatch[] bushLandGrassInnerTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, grassTypeField),
                new CodeMatch(OpCodes.Bne_Un),
                new CodeMatch(OpCodes.Ldsfld)
            };
            matcher.MatchStartForward(bushLandGrassInnerTarget);

            // grab label for inside if block
            var IL_03b6 = generator.DefineLabel();
            matcher.InstructionAt(3).labels.Add(IL_03b6);
            List<CodeInstruction> cutBushLandGrassInnerInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 23),
                new CodeInstruction(OpCodes.Beq, IL_03b6),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.InsertAndAdvance(cutBushLandGrassInnerInsertion);


            // cursed bullshit to inline a function call
            Label inlineJumpLabel = generator.DefineLabel();
            Label inlinePopLabel = generator.DefineLabel();
            Label inlinePlaceAllowedLabel = generator.DefineLabel();
            CodeMatch[] callvirtTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Callvirt, getRandomObjectAndPlaceWithGrowthMethod)
            };
            matcher.MatchStartForward(callvirtTarget);

            Label inlineEndLabel = generator.DefineLabel();
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

                new CodeInstruction(OpCodes.Nop).WithLabels(inlineJumpLabel),
                new CodeInstruction(OpCodes.Br, inlineEndLabel), // jump target that needs label

                new CodeInstruction(OpCodes.Pop).WithLabels(inlinePopLabel)
            };
            // insert our custom version of the function
            matcher.InsertAndAdvance(inlineGetRandomObjectAndPlaceWithGrowth);
            // remove the callvirt to getRandomObjectAndPlaceWithGrowth
            matcher.RemoveInstructions(callvirtTarget.Length);
            // give the inlineEndLabel to the br jump that'r right after the now-removed callvirt
            CodeInstruction brokenBranch = matcher.Instruction;
            brokenBranch.labels.Add(inlineEndLabel);

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
            List<CodeInstruction> cutTropicalGrassInnerInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 25),
                new CodeInstruction(OpCodes.Beq, IL_03de),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.InsertAndAdvance(cutTropicalGrassInnerInsertion);

            Label tropicalGrowBackCallLabel = generator.DefineLabel();
            CodeMatch[] tropicalGrowBackTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldfld, tropicalGrowBackField),
                new CodeMatch(OpCodes.Ldnull),
                new CodeMatch(OpCodes.Callvirt, getBiomObjectMethod),
                new CodeMatch(OpCodes.Call, arraySetMethod)
            };
            matcher.MatchEndForward(tropicalGrowBackTarget);
            // label the call instruction that's after CallVirt
            // matcher.InstructionAt(1).labels.Add(tropicalGrowBackCallLabel);
            // move forward one so that inserted code comes after CallVirt
            Label tropicalGrowBackBreakLabel = generator.DefineLabel();
            Label tropicalGrowBackPopLabel = generator.DefineLabel();
            LocalBuilder itemIDLocal = generator.DeclareLocal(typeof(int));
            List<CodeInstruction> tropicalGrowBackInsertion = new List<CodeInstruction>()
            {
                // stash our item ID
                new CodeInstruction(OpCodes.Stloc, itemIDLocal),
                // check tile type for cut grass
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod),
                new CodeInstruction(OpCodes.Ldc_I4, 25),
                new CodeInstruction(OpCodes.Bne_Un, tropicalGrowBackCallLabel), // jump straight to the set call if it's regular tropical grass

                new CodeInstruction(OpCodes.Ldloc, itemIDLocal),
                new CodeInstruction(OpCodes.Ldc_I4, 4),
                new CodeInstruction(OpCodes.Beq, tropicalGrowBackBreakLabel),

                new CodeInstruction(OpCodes.Ldloc, itemIDLocal),
                new CodeInstruction(OpCodes.Ldc_I4, 5),
                new CodeInstruction(OpCodes.Beq, tropicalGrowBackBreakLabel),

                new CodeInstruction(OpCodes.Ldloc, itemIDLocal),
                new CodeInstruction(OpCodes.Ldc_I4, 135),
                new CodeInstruction(OpCodes.Beq, tropicalGrowBackBreakLabel),

                new CodeInstruction(OpCodes.Ldloc, itemIDLocal),
                new CodeInstruction(OpCodes.Ldc_I4, 296),
                new CodeInstruction(OpCodes.Beq, tropicalGrowBackBreakLabel),

                new CodeInstruction(OpCodes.Ldloc, itemIDLocal),
                new CodeInstruction(OpCodes.Ldc_I4, 297),
                new CodeInstruction(OpCodes.Beq, tropicalGrowBackBreakLabel), // jump to the Set call if it's not any of the 5 values
                
                
                new CodeInstruction(OpCodes.Nop).WithLabels(tropicalGrowBackCallLabel),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, onTileMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc, itemIDLocal)

            };
            // matcher.InsertAndAdvance(tropicalGrowBackInsertion);
            // give a label to the br jump instruction that's after the call
            matcher.InstructionAt(1).labels.Add(tropicalGrowBackBreakLabel);
            // move past the call now that we've restored the stack
            // matcher.Advance(1);

            CodeMatch[] firGrassInnerTarget = new CodeMatch[]
            {
                // insert new code here
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, pineGrassTypeField),
                new CodeMatch(OpCodes.Bne_Un)
            };
            matcher.MatchStartForward(firGrassInnerTarget);

            // grab label for inside if block
            var IL_0412 = generator.DefineLabel();
            matcher.InstructionAt(3).labels.Add(IL_0412);
            List<CodeInstruction> cutFirGrassInnerInsertion = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, 24),
                new CodeInstruction(OpCodes.Beq, IL_0412),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, arrayGetMethod)
            };
            matcher.Insert(cutFirGrassInnerInsertion);

            CodeMatch[] coldLandGrowBackTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldfld, coldLandGrowBackField),
                new CodeMatch(OpCodes.Ldnull),
                new CodeMatch(OpCodes.Callvirt, getBiomObjectMethod),
                new CodeMatch(OpCodes.Call, arraySetMethod)
            };
            matcher.MatchEndForward(coldLandGrowBackTarget);

            Label coldLandGrowBackPopLabel = generator.DefineLabel();
            Label coldLandGrowBackCallLabel = generator.DefineLabel();
            Label coldLandGrowBackBreakLabel = generator.DefineLabel();
            List<CodeInstruction> coldLandGrowBackInsertion = new List<CodeInstruction>()
            {
                // check tile type for cut grass
                // grab this
                new CodeInstruction(OpCodes.Ldloc_1),
                // go from this to this.tileTypeMap, discarding this
                new CodeInstruction(OpCodes.Ldfld, tileTypeMapField),
                // grab X and Y (unknown order)
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_2),
                // grab the tile ID, consuming X, Y, and this.tileTypeMap
                new CodeInstruction(OpCodes.Call, arrayGetMethod),
                new CodeInstruction(OpCodes.Ldc_I4, 24),
                new CodeInstruction(OpCodes.Bne_Un, coldLandGrowBackCallLabel), // jump straight to the set call if it's regular fir grass

                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4, 137),
                new CodeInstruction(OpCodes.Bne_Un, coldLandGrowBackCallLabel), // jump to the Set call if it's not cold grass
                
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Br, coldLandGrowBackBreakLabel)
            };
            matcher.InsertAndAdvance(coldLandGrowBackInsertion);

            // label the Call instruction for int[,] = coldLandGrowBack.getBiomObject(null)
            matcher.Instruction.labels.Add(coldLandGrowBackCallLabel);
            // label the br instruction
            matcher.InstructionAt(1).labels.Add(coldLandGrowBackBreakLabel);

            // find the code block after coldLandGrowBack spawning that gets jumped to by a few different br's
            CodeMatch[] brokenBranchTarget = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldfld, onTileMapField),
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Call, arrayGetMethod),
                new CodeMatch(OpCodes.Ldc_I4_M1),
                new CodeMatch(OpCodes.Ble)
            };
            matcher.MatchStartForward(brokenBranchTarget);
            // create and assign a new label
            Label brokenBranchTargetLabel = generator.DefineLabel();
            matcher.Instruction.labels.Add(brokenBranchTargetLabel);
            brokenBranch.operand = brokenBranchTargetLabel;


            //CutGrassGrowsFlowers.Logger.LogMessage(String.Join("\r\n", matcher.Instructions().Select((inst, index) =>
            //{
            //    return index.ToString() + ": " + inst.opcode.ToString() + " " + inst.operand?.ToString();
            //})));
            int offset = 0;
            foreach (CodeInstruction inst in matcher.Instructions())
            {
                Console.WriteLine($"IL_{offset:X4}: {inst.opcode} {inst.operand ?? ""}");
                offset += Helper.GetOpSize(inst.opcode, inst.operand);
            }

            return matcher.Instructions();
        }
    }
}