using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mikhailov.Common;
using Mikhailov.Content.Items.Weapons;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Projectiles;

public abstract class ManholeCoverBase : ModProjectile
{
    public override string Texture => "Mikhailov/Assets/Effects/Luke/CoverFront";
    protected virtual bool BrightTrail => false;
    public override void SetDefaults()
    {
        // The sprite is intentionally much larger than the physical hitbox. A large hitbox
        // spawned at hand height immediately collides with the floor on horizontal throws.
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 420;
        Projectile.tileCollide = true;
        Projectile.netImportant = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 14;
    }

    protected bool ValidateOwner(out Player player)
    {
        player = Main.player[Projectile.owner];
        if (player.active && !player.dead) return true;
        Projectile.Kill();
        return false;
    }

    protected void SpinAndTrail(Color color)
    {
        Projectile.rotation += .34f * Math.Sign(Projectile.velocity.X == 0f ? 1f : Projectile.velocity.X);
        Lighting.AddLight(Projectile.Center, color.ToVector3() * .18f);
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), DustID.Iron, -Projectile.velocity * .08f, 90, color, 1.05f);
            dust.noGravity = true;
        }
    }

    protected static void MetalBurst(Vector2 center, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            Dust dust = Dust.NewDustPerfect(center, i % 3 == 0 ? DustID.Torch : DustID.Iron, Main.rand.NextVector2Circular(speed, speed), 70, new Color(174, 128, 91), Main.rand.NextFloat(.9f, 1.45f));
            dust.noGravity = i % 2 == 0;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        string trailPath = BrightTrail ? "Mikhailov/Assets/Effects/Luke/SlashBright" : "Mikhailov/Assets/Effects/Luke/SlashRust";
        Texture2D trail = ModContent.Request<Texture2D>(trailPath, AssetRequestMode.ImmediateLoad).Value;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        SpriteEffects effects = direction.X < 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Main.EntitySpriteDraw(trail, Projectile.Center - direction * 24f - Main.screenPosition, null, Color.White * .52f, direction.ToRotation(), trail.Size() / 2f, .34f, effects);

        int frame = (int)(Projectile.localAI[0] / 5f) % 6 + 1;
        Texture2D texture = ModContent.Request<Texture2D>($"Mikhailov/Assets/Effects/Luke/Spin{frame:00}", AssetRequestMode.ImmediateLoad).Value;
        float normalizedScale = 58f / Math.Max(texture.Width, texture.Height);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, 0f, texture.Size() / 2f, normalizedScale, effects);
        return false;
    }
}

public sealed class ManholeCoverProjectile : ManholeCoverBase
{
    private int Bounces { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.extraUpdates = 1;
        Projectile.timeLeft = 240;
    }

    public override void AI()
    {
        if (!ValidateOwner(out _)) return;
        SpinAndTrail(new Color(170, 130, 95));
        Projectile.localAI[0]++;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Bounces++;
        if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
        Projectile.velocity = Projectile.velocity.SafeNormalize(oldVelocity.SafeNormalize(Vector2.UnitX)) * 17.5f;
        SoundEngine.PlaySound(ManholeCoverSounds.Bounce, Projectile.Center);
        MetalBurst(Projectile.Center, 12, 4.5f);
        if (Bounces >= 5) { Projectile.Kill(); return false; }
        Projectile.netUpdate = true;
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(ManholeCoverSounds.Impact, target.Center);
        MetalBurst(target.Center, 9, 3.5f);
    }
}

public sealed class ChargedManholeCoverProjectile : ManholeCoverBase
{
    private readonly HashSet<int> hitTargets = new();
    private bool finishing;
    private bool ricochetMode;
    private byte ricochetBounces;
    private int MaxTargets => Math.Clamp((int)Projectile.ai[1], 1, 5);
    protected override bool BrightTrail => FullCharge;
    private bool FullCharge => MaxTargets == 5;
    private int Target { get => (int)Projectile.ai[2] - 1; set => Projectile.ai[2] = value + 1; }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = 180;
    }

    public override void AI()
    {
        if (!ValidateOwner(out _)) return;
        Color trail = FullCharge ? new Color(245, 225, 195) : new Color(178, 132, 94);
        SpinAndTrail(trail);
        Projectile.localAI[0]++;

        if (Projectile.localAI[0] == 1f && Projectile.owner == Main.myPlayer) ChooseNextTarget();
        if (Target >= 0)
        {
            NPC target = Main.npc[Target];
            if (!target.active || !target.CanBeChasedBy()) { ChooseNextTarget(); return; }
            Projectile.tileCollide = false;
            float stepSpeed = MathHelper.Lerp(35f, 52f, Projectile.ai[0]) / (Projectile.extraUpdates + 1f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * stepSpeed, .42f);
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(ManholeCoverSounds.Bounce, Projectile.Center);
        MetalBurst(Projectile.Center, 16, 5.5f);
        if (!ricochetMode) { FinishWithBurst(); return false; }

        ricochetBounces++;
        if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
        float stepSpeed = MathHelper.Lerp(35f, 52f, Projectile.ai[0]) / (Projectile.extraUpdates + 1f);
        Projectile.velocity = Projectile.velocity.SafeNormalize(oldVelocity.SafeNormalize(Vector2.UnitX)) * stepSpeed;
        if (ricochetBounces >= 5) FinishWithBurst(); else Projectile.netUpdate = true;
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (finishing) return;
        if (!hitTargets.Add(target.whoAmI)) return;
        SoundEngine.PlaySound(ManholeCoverSounds.Impact, target.Center);
        MetalBurst(target.Center, FullCharge ? 18 : 11, FullCharge ? 6f : 4f);
        PullEnemies(target.Center, target.whoAmI);
        if (hitTargets.Count < MaxTargets) ChooseNextTarget(); else FinishWithBurst();
    }

    private void ChooseNextTarget()
    {
        if (Projectile.owner != Main.myPlayer) return;
        NPC next = null;
        float best = 900f;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy() || hitTargets.Contains(npc.whoAmI)) continue;
            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance < best) { best = distance; next = npc; }
        }
        if (next != null)
        {
            Target = next.whoAmI;
            ricochetMode = false;
            Projectile.netUpdate = true;
            return;
        }

        if (hitTargets.Count == 0) EnterRicochetMode(); else FinishWithBurst();
    }

    private void EnterRicochetMode()
    {
        ricochetMode = true;
        Target = -1;
        Projectile.tileCollide = true;
        Projectile.netUpdate = true;
    }

    private static void PullEnemies(Vector2 center, int struckTarget)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.whoAmI == struckTarget || npc.boss || !npc.CanBeChasedBy()) continue;
            Vector2 pull = center - npc.Center;
            if (pull.LengthSquared() < 1f || pull.LengthSquared() > 260f * 260f) continue;
            float strength = MathHelper.Lerp(2.5f, 7f, npc.knockBackResist);
            npc.velocity += pull.SafeNormalize(Vector2.Zero) * strength;
            if (!float.IsFinite(npc.velocity.X) || !float.IsFinite(npc.velocity.Y)) npc.velocity = Vector2.Zero;
            npc.netUpdate = true;
        }
    }

    private void FinishWithBurst()
    {
        if (!Projectile.active || finishing) return;
        finishing = true;
        MetalBurst(Projectile.Center, 24, 7f);
        SoundEngine.PlaySound(ManholeCoverSounds.Impact with { Volume = .6f, Pitch = -.12f }, Projectile.Center);
        if (Projectile.owner == Main.myPlayer)
        {
            Vector2 center = Projectile.Center;
            Projectile.position = center - new Vector2(52f);
            Projectile.width = Projectile.height = 104;
            Projectile.damage = Math.Max(1, Projectile.damage / 2);
            Projectile.Damage();
        }
        Projectile.Kill();
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(ricochetMode);
        writer.Write(ricochetBounces);
        writer.Write((byte)hitTargets.Count);
        foreach (int target in hitTargets) writer.Write((short)target);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        ricochetMode = reader.ReadBoolean();
        ricochetBounces = reader.ReadByte();
        hitTargets.Clear();
        int count = reader.ReadByte();
        for (int i = 0; i < count; i++) hitTargets.Add(reader.ReadInt16());
    }
}

public sealed class ManholeCoverChargeHoldout : HoldoutController
{
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead) { Projectile.Kill(); return; }
        Vector2 aim = UpdateAim(player, 1f);
        if (Held(player, ModContent.ItemType<ManholeCover>()))
        {
            Projectile.localAI[0] = Math.Min(60f, Projectile.localAI[0] + 1f);
            float charge = Projectile.localAI[0] / 60f;
            Color color = charge >= 1f ? Color.White : charge >= .5f ? new Color(205, 155, 105) : new Color(110, 100, 90);
            int count = charge >= 1f ? 3 : charge >= .5f ? 2 : 1;
            for (int i = 0; i < count; i++) if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(28f, 28f), DustID.Iron, Projectile.DirectionTo(player.MountedCenter) * 2f, 70, color, 1f + charge * .55f);
                dust.noGravity = true;
            }
            if (Projectile.localAI[0] is 30f or 60f) SoundEngine.PlaySound(ManholeCoverSounds.Bounce with { Pitch = Projectile.localAI[0] == 60f ? .35f : .05f, Volume = .42f }, Projectile.Center);
            return;
        }

        float power = MathHelper.Clamp(Projectile.localAI[0] / 60f, .15f, 1f);
        if (Projectile.owner == Main.myPlayer)
        {
            int damage = (int)(Projectile.damage * MathHelper.Lerp(1.05f, 1.85f, power));
            int maxTargets = 1 + (int)Math.Floor(power * 4f);
            float stepSpeed = MathHelper.Lerp(35f, 52f, power) / 3f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + aim * 42f, aim * stepSpeed, ModContent.ProjectileType<ChargedManholeCoverProjectile>(), damage, Projectile.knockBack * MathHelper.Lerp(1f, 1.35f, power), Projectile.owner, power, maxTargets);
        }
        SoundEngine.PlaySound(ManholeCoverSounds.Throw with { Pitch = -.18f + power * .2f }, Projectile.Center);
        player.velocity -= aim * MathHelper.Lerp(.6f, 2.2f, power);
        player.GetModPlayer<MikhailovPlayer>().ScreenShake = MathHelper.Lerp(1.5f, 5f, power);
        Projectile.Kill();
    }
}
