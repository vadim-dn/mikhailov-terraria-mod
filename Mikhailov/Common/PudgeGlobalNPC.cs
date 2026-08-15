using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Mikhailov.Common;

public sealed class PudgeGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public int StunTime;
    public int HookOwner = -1;
    public int HookTime;
    public bool RotSlowed;

    public override void ResetEffects(NPC npc) => RotSlowed = false;

    public override void PostAI(NPC npc)
    {
        if (RotSlowed) npc.velocity.X *= 0.7f;
    }

    public override bool PreAI(NPC npc)
    {
        if (!ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent)
        {
            StunTime = HookTime = 0;
            HookOwner = -1;
            return true;
        }

        if (HookTime > 0 && HookOwner >= 0 && HookOwner < Main.maxPlayers)
        {
            Player owner = Main.player[HookOwner];
            if (!owner.active || owner.dead)
            {
                HookTime = 0;
                HookOwner = -1;
            }
            else
            {
                Vector2 destination = owner.Center + new Vector2(owner.direction * 54f, -8f);
                npc.Center = Vector2.Lerp(npc.Center, destination, 0.28f);
                npc.velocity = npc.DirectionTo(destination) * System.Math.Min(18f, npc.Distance(destination) * 0.3f);
                HookTime--;
                npc.netUpdate = true;
                return false;
            }
        }

        if (StunTime > 0)
        {
            StunTime--;
            npc.velocity = Vector2.Zero;
            return false;
        }

        return true;
    }
}
