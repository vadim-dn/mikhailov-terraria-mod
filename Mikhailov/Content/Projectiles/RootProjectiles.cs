using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Projectiles;

internal static class RootVisuals
{
    internal const string Path = "Mikhailov/Assets/Effects/Root/Green";
    internal static Texture2D Get(int number) => ModContent.Request<Texture2D>($"{Path}{number:00}", AssetRequestMode.ImmediateLoad).Value;
    internal static void Draw(Projectile p, int number, Color color, float scale = 1f)
    {
        Texture2D texture = Get(number);
        SpriteEffects effects = p.spriteDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Main.EntitySpriteDraw(texture, p.Center - Main.screenPosition, null, color, p.rotation, texture.Size() / 2f, p.scale * scale, effects);
    }
    internal static void DustBurst(Vector2 center, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            Dust dust = Dust.NewDustPerfect(center, DustID.GreenTorch, Main.rand.NextVector2CircularEdge(speed, speed), 60, Color.GreenYellow, Main.rand.NextFloat(.8f, 1.45f));
            dust.noGravity = true;
        }
    }
}

public sealed class RootBolt : MikhailovProjectile
{
    public override string Texture => RootVisuals.Path + "03";
    protected override Color Glow => Color.GreenYellow;
    private int Step => (int)Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.width = 24; Projectile.height = 14; Projectile.friendly = true; Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1; Projectile.timeLeft = 90; Projectile.extraUpdates = 1;
    }
    public override void AI()
    {
        base.AI(); Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
        Projectile.scale = Step switch { 0 => .28f, 1 => .38f, _ => .48f };
        if (Step == 2) { Projectile.penetrate = 3; Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 12; }
        Trail(DustID.GreenTorch, Color.Lime, Step == 2 ? 1.15f : .8f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        int texture = Step switch { 0 => 4, 1 => 3, _ => 5 };
        RootVisuals.Draw(Projectile, texture, Color.White);
        if (Step > 0) RootVisuals.Draw(Projectile, 6 + (int)(Main.GameUpdateCount / 5 % 5), Color.White * .7f, .55f);
        return false;
    }
    public override void OnKill(int timeLeft)
    {
        RootVisuals.DustBurst(Projectile.Center, Step == 2 ? 16 : 7, Step == 2 ? 4.5f : 2.5f);
        if (Step == 2 && Projectile.owner == Main.myPlayer)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RootGrowth>(), Projectile.damage / 2, 2f, Projectile.owner);
    }
}

public sealed class RootCrescent : MikhailovProjectile
{
    public override string Texture => RootVisuals.Path + "17";
    protected override Color Glow => Color.LimeGreen;
    public override void SetDefaults()
    {
        Projectile.width = 92; Projectile.height = 64; Projectile.friendly = true; Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 5; Projectile.timeLeft = 22; Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 18;
    }
    public override void AI()
    {
        base.AI(); Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1; Projectile.velocity *= .94f;
        Projectile.scale = .38f + (22 - Projectile.timeLeft) * .018f; Projectile.alpha += 7;
        Trail(DustID.GreenTorch, Color.GreenYellow, 1.25f);
    }
    public override bool PreDraw(ref Color lightColor) { RootVisuals.Draw(Projectile, 17, Color.White * Projectile.Opacity); return false; }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3.5f;
        if (Projectile.localAI[0] != 0 || Projectile.owner != Main.myPlayer) return;
        Projectile.localAI[0] = 1;
        for (int i = -2; i <= 2; i++)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(i * .22f) * 8f, ModContent.ProjectileType<RootLeaf>(), Projectile.damage / 2, 1f, Projectile.owner, i + 2);
    }
}

public sealed class RootLeaf : MikhailovProjectile
{
    public override string Texture => RootVisuals.Path + "07";
    protected override Color Glow => Color.Lime;
    public override void SetDefaults()
    {
        Projectile.width = 12; Projectile.height = 10; Projectile.friendly = true; Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1; Projectile.timeLeft = 75; Projectile.tileCollide = false;
    }
    public override void AI()
    {
        NPC target = FindTarget(420f);
        if (target != null) Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * 11f, .075f);
        base.AI(); Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1; Projectile.scale = .25f;
        Trail(DustID.GreenTorch, Color.Lime, .7f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        RootVisuals.Draw(Projectile, 7 + ((int)Projectile.ai[0] + (int)(Main.GameUpdateCount / 6)) % 10, Color.White);
        return false;
    }
}

public sealed class RootGrowth : ModProjectile
{
    public override string Texture => RootVisuals.Path + "18";
    public override void SetDefaults()
    {
        Projectile.width = 68; Projectile.height = 68; Projectile.friendly = true; Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1; Projectile.timeLeft = 28; Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 28;
    }
    public override void AI()
    {
        Projectile.velocity = Vector2.Zero; Projectile.scale = .22f + (28 - Projectile.timeLeft) * .018f;
        Projectile.alpha = Projectile.timeLeft < 9 ? (9 - Projectile.timeLeft) * 24 : 0;
        Lighting.AddLight(Projectile.Center, .15f, .85f, .12f);
        if (Projectile.timeLeft == 27)
        {
            RootVisuals.DustBurst(Projectile.Center, 22, 5f);
            SoundEngine.PlaySound(SoundID.Dig with { Volume = .34f, Pitch = .35f, PitchVariance = .12f, MaxInstances = 3 }, Projectile.Center);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        int elapsed = 28 - Projectile.timeLeft;
        int texture = elapsed < 9 ? 18 : elapsed < 14 ? 19 : elapsed < 19 ? 20 : elapsed < 24 ? 21 : 22;
        RootVisuals.Draw(Projectile, texture, Color.White * Projectile.Opacity);
        return false;
    }
}
