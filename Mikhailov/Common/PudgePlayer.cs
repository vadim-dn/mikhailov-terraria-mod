using Microsoft.Xna.Framework;
using Mikhailov.Content.Buffs;
using Mikhailov.Content.Items.Armor;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;

namespace Mikhailov.Common;

public enum PudgeAbility : byte { Hook, Rot, DismemberStart, DismemberStop }

public sealed class PudgePlayer : ModPlayer
{
    public bool RotActive;
    public int HookCooldown;
    public int DismemberCooldown;
    public int DismemberTarget = -1;
    public int DismemberTime;
    public int DismemberHealing;
    public int ProtectedNpc = -1;
    public int ProtectedTime;

    private bool hookWasDown;
    private bool rotWasDown;
    private bool dismemberWasDown;

    private bool Enabled => ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent;

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        bool hookDown = Mikhailov.HookKeybind.Current || Main.keyState.IsKeyDown(Keys.Z);
        bool rotDown = Mikhailov.RotKeybind.Current || Main.keyState.IsKeyDown(Keys.X);
        bool dismemberDown = Mikhailov.DismemberKeybind.Current || Main.keyState.IsKeyDown(Keys.C);

        if (Enabled && HasPudgeSet() && !Player.dead)
        {
            if (hookDown && !hookWasDown) Request(PudgeAbility.Hook);
            if (rotDown && !rotWasDown) Request(PudgeAbility.Rot);
            if (dismemberDown && !dismemberWasDown) Request(PudgeAbility.DismemberStart);
            if (!dismemberDown && dismemberWasDown) Request(PudgeAbility.DismemberStop);
        }

        hookWasDown = hookDown;
        rotWasDown = rotDown;
        dismemberWasDown = dismemberDown;
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        if (!Enabled || !HasPudgeSet()) return;
        PlayerDrawLayer pudgeLayer = ModContent.GetInstance<PudgePlayerDrawLayer>();
        foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
            if (layer != pudgeLayer) layer.Hide();
    }

    private void Request(PudgeAbility ability)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) Mikhailov.SendAbility(ability, Main.MouseWorld);
        else HandleAbility(ability, Main.MouseWorld);
    }

    internal void HandleAbility(PudgeAbility ability, Vector2 cursor)
    {
        if (!Enabled || !HasPudgeSet() || Player.dead) return;
        switch (ability)
        {
            case PudgeAbility.Hook: StartHook(cursor); break;
            case PudgeAbility.Rot: RotActive = !RotActive; break;
            case PudgeAbility.DismemberStart: StartDismember(cursor); break;
            case PudgeAbility.DismemberStop: StopDismember(); break;
        }
    }

    public bool HasPudgeSet() => Player.armor[0].type == ModContent.ItemType<PudgeMask>()
        && Player.armor[1].type == ModContent.ItemType<PudgeBody>()
        && Player.armor[2].type == ModContent.ItemType<PudgeLegs>();

    private void StartHook(Vector2 cursor)
    {
        if (HookCooldown > 0 || Player.ownedProjectileCounts[ModContent.ProjectileType<PudgeHook>()] > 0) return;
        Vector2 velocity = Player.DirectionTo(cursor);
        if (velocity.LengthSquared() < 0.01f) velocity = Vector2.UnitX * Player.direction;
        Projectile.NewProjectile(Player.GetSource_Misc("PudgeHook"), Player.MountedCenter, velocity * 20f,
            ModContent.ProjectileType<PudgeHook>(), 90, 5f, Player.whoAmI);
        HookCooldown = 600;
    }

    private void StartDismember(Vector2 cursor)
    {
        if (DismemberCooldown > 0 || Vector2.Distance(Player.Center, cursor) > 90f) return;
        NPC best = null;
        float bestDistance = 46f;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy()) continue;
            float distance = Vector2.Distance(cursor, npc.Center);
            if (distance < bestDistance && Vector2.Distance(Player.Center, npc.Center) <= 100f)
            {
                best = npc;
                bestDistance = distance;
            }
        }
        if (best == null) return;
        DismemberTarget = best.whoAmI;
        DismemberTime = 180;
        DismemberHealing = 0;
        DismemberCooldown = 1200;
        best.GetGlobalNPC<PudgeGlobalNPC>().StunTime = 2;
        SoundEngine.PlaySound(PudgeSounds.Dismember, Player.Center);
    }

    private void StopDismember()
    {
        DismemberTarget = -1;
        DismemberTime = 0;
    }

    public override void PostUpdate()
    {
        if (HookCooldown > 0) HookCooldown--;
        if (DismemberCooldown > 0) DismemberCooldown--;
        if (ProtectedTime > 0) ProtectedTime--; else ProtectedNpc = -1;

        if (!Enabled || !HasPudgeSet() || Player.dead)
        {
            RotActive = false;
            StopDismember();
            KillHooks();
            return;
        }

        if (HookCooldown > 0) Player.AddBuff(ModContent.BuffType<HookCooldownBuff>(), HookCooldown);
        if (DismemberCooldown > 0) Player.AddBuff(ModContent.BuffType<DismemberCooldownBuff>(), DismemberCooldown);
        if (RotActive) UpdateRot();
        if (DismemberTime > 0) UpdateDismember();
    }

    private void UpdateRot()
    {
        Player.AddBuff(ModContent.BuffType<RotActiveBuff>(), 2);
        EnsureVisual(ModContent.ProjectileType<PudgeRotAura>());
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 15 == 0)
            foreach (NPC npc in Main.ActiveNPCs)
                if (npc.CanBeChasedBy() && Vector2.Distance(Player.Center, npc.Center) <= 120f)
                {
                    Player.ApplyDamageToNPC(npc, 10, 0f, npc.Center.X >= Player.Center.X ? 1 : -1, false, DamageClass.Generic);
                    npc.AddBuff(ModContent.BuffType<RotSlowBuff>(), 30);
                }
        if (Main.GameUpdateCount % 18 == 0) SoundEngine.PlaySound(PudgeSounds.Rot with { MaxInstances = 1 }, Player.Center);
    }

    private void UpdateDismember()
    {
        if (DismemberTarget < 0 || DismemberTarget >= Main.maxNPCs) { StopDismember(); return; }
        NPC target = Main.npc[DismemberTarget];
        if (!target.active || target.friendly || Vector2.Distance(Player.Center, target.Center) > 110f)
        {
            StopDismember();
            return;
        }

        DismemberTime--;
        EnsureVisual(ModContent.ProjectileType<PudgeDismemberVisual>(), DismemberTarget);
        Player.immune = true;
        Player.immuneTime = 2;
        Player.velocity = Vector2.Zero;
        target.GetGlobalNPC<PudgeGlobalNPC>().StunTime = 2;
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 30 == 0)
        {
            int before = target.life;
            Player.ApplyDamageToNPC(target, 30, 0f, target.Center.X >= Player.Center.X ? 1 : -1, false, DamageClass.Generic);
            int dealt = System.Math.Max(0, before - target.life);
            int heal = System.Math.Min((dealt + 1) / 2, 90 - DismemberHealing);
            if (heal > 0)
            {
                DismemberHealing += heal;
                Player.statLife = System.Math.Min(Player.statLifeMax2, Player.statLife + heal);
                Player.HealEffect(heal, true);
            }
        }
        if (DismemberTime <= 0) StopDismember();
    }

    private void EnsureVisual(int type, float target = 0f)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && Player.ownedProjectileCounts[type] == 0)
            Projectile.NewProjectile(Player.GetSource_Misc("PudgeVisual"), Player.Center, Vector2.Zero, type, 0, 0f, Player.whoAmI, target);
    }

    private void KillHooks()
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.owner == Player.whoAmI && projectile.type == ModContent.ProjectileType<PudgeHook>()) projectile.Kill();
    }

    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (ProtectedTime > 0 && ProtectedNpc == npc.whoAmI) modifiers.FinalDamage *= 0f;
    }

    public override bool FreeDodge(Player.HurtInfo info) => DismemberTime > 0 && Enabled && HasPudgeSet();
}
