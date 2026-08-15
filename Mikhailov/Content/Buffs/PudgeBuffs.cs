using Mikhailov.Common;
using Terraria;
using Terraria.ModLoader;

namespace Mikhailov.Content.Buffs;

public abstract class PudgeStatusBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = true;
    }
}

public sealed class HookCooldownBuff : PudgeStatusBuff { }
public sealed class DismemberCooldownBuff : PudgeStatusBuff { }
public sealed class RotActiveBuff : PudgeStatusBuff { }

public sealed class RotSlowBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.pvpBuff[Type] = false;
    }

    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<PudgeGlobalNPC>().RotSlowed = true;
}
