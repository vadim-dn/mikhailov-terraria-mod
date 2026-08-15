using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mikhailov.Common;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Projectiles;

public static class PudgeSounds
{
    public static readonly SoundStyle HookLaunch = new("Mikhailov/Assets/Sounds/PudgeHookLaunch") { Volume = 0.8f };
    public static readonly SoundStyle HookHit = new("Mikhailov/Assets/Sounds/PudgeHookHit") { Volume = 0.85f };
    public static readonly SoundStyle Rot = new("Mikhailov/Assets/Sounds/PudgeRot") { Volume = 0.35f, PitchVariance = 0.08f };
    public static readonly SoundStyle Dismember = new("Mikhailov/Assets/Sounds/PudgeDismember") { Volume = 0.8f };
}

public sealed class PudgeHook : ModProjectile
{
    public override string Texture => "Mikhailov/Assets/Effects/Pudge/HookHead";

    public override void SetDefaults()
    {
        Projectile.width = 38;
        Projectile.height = 38;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 45;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
        Projectile.extraUpdates = 1;
    }

    public override void OnSpawn(Terraria.DataStructures.IEntitySource source) =>
        SoundEngine.PlaySound(PudgeSounds.HookLaunch, Projectile.Center);

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.45f, 0.08f, 0.04f);
        if (Projectile.Distance(Main.player[Projectile.owner].Center) >= 800f) Projectile.Kill();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            PudgeGlobalNPC state = target.GetGlobalNPC<PudgeGlobalNPC>();
            state.HookOwner = Projectile.owner;
            state.HookTime = 28;
            PudgePlayer player = Main.player[Projectile.owner].GetModPlayer<PudgePlayer>();
            player.ProtectedNpc = target.whoAmI;
            player.ProtectedTime = 120;
            target.netUpdate = true;
        }
        SoundEngine.PlaySound(PudgeSounds.HookHit, target.Center);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Player owner = Main.player[Projectile.owner];
        Texture2D chain = ModContent.Request<Texture2D>("Mikhailov/Assets/Effects/Pudge/HookChain", AssetRequestMode.ImmediateLoad).Value;
        Vector2 start = owner.MountedCenter;
        Vector2 delta = Projectile.Center - start;
        float length = delta.Length();
        if (length > 1f)
        {
            Vector2 direction = delta / length;
            float rotation = direction.ToRotation() + MathHelper.PiOver2;
            for (float distance = 12f; distance < length; distance += 14f)
                Main.EntitySpriteDraw(chain, start + direction * distance - Main.screenPosition, null, lightColor,
                    rotation, chain.Size() / 2f, 1f, SpriteEffects.None);
        }
        return true;
    }
}

public sealed class PudgeRotAura : ModProjectile
{
    public override string Texture => "Mikhailov/Assets/Effects/Invisible";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 120;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.netImportant = true;
        Projectile.timeLeft = 2;
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        PudgePlayer state = owner.GetModPlayer<PudgePlayer>();
        if (!owner.active || owner.dead || !ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent ||
            (Main.netMode != NetmodeID.MultiplayerClient && (!state.RotActive || !state.HasPudgeSet())))
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = owner.Center;
        Projectile.timeLeft = 2;
        Lighting.AddLight(owner.Center, 0.38f, 0.42f, 0.04f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        int frame = 9 + (int)(Main.GameUpdateCount / 6 % 7);
        Texture2D texture = ModContent.Request<Texture2D>($"Mikhailov/Assets/Effects/Pudge/Rot{frame:00}", AssetRequestMode.ImmediateLoad).Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * 0.72f,
            0f, texture.Size() / 2f, 0.72f, SpriteEffects.None);
        return false;
    }
}

public sealed class PudgeDismemberVisual : ModProjectile
{
    public override string Texture => "Mikhailov/Assets/Effects/Invisible";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 80;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.netImportant = true;
        Projectile.timeLeft = 2;
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        PudgePlayer state = owner.GetModPlayer<PudgePlayer>();
        int targetIndex = Main.netMode == NetmodeID.MultiplayerClient ? (int)Projectile.ai[0] : state.DismemberTarget;
        if (!owner.active || owner.dead || targetIndex < 0 ||
            !ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent ||
            (Main.netMode != NetmodeID.MultiplayerClient && state.DismemberTime <= 0))
        {
            Projectile.Kill();
            return;
        }
        NPC target = Main.npc[targetIndex];
        if (!target.active) { Projectile.Kill(); return; }
        Projectile.Center = Vector2.Lerp(owner.Center, target.Center, 0.5f);
        Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        int frame = 16 + (int)(Main.GameUpdateCount / 5 % 15);
        Texture2D texture = ModContent.Request<Texture2D>($"Mikhailov/Assets/Effects/Pudge/Dismember{frame:00}", AssetRequestMode.ImmediateLoad).Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White,
            0f, texture.Size() / 2f, 0.72f, SpriteEffects.None);
        return false;
    }
}
