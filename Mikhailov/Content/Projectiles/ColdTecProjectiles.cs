using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Projectiles;

internal static class ColdTecVisuals
{
    internal const string Path = "Mikhailov/Assets/Effects/ColdTec/Blue";
    internal static Texture2D Get(int number) => ModContent.Request<Texture2D>($"{Path}{number:00}", AssetRequestMode.ImmediateLoad).Value;
    internal static void Draw(Projectile p, int number, Color color, float scale = 1f, float rotationOffset = 0f)
    {
        Texture2D texture = Get(number);
        SpriteEffects effects = p.velocity.X < 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Main.EntitySpriteDraw(texture, p.Center - Main.screenPosition, null, color, p.rotation + rotationOffset, texture.Size() / 2f, p.scale * scale, effects);
    }
}

internal static class ColdTecSounds
{
    internal static readonly SoundStyle Pulse = new("Mikhailov/Assets/Sounds/ColdPulse") { Volume = .34f, PitchVariance = .07f, MaxInstances = 4 };
    internal static readonly SoundStyle PrismBurst = new("Mikhailov/Assets/Sounds/ColdPrismBurst") { Volume = .58f, PitchVariance = .04f, MaxInstances = 2 };
}

public sealed class ColdTecPulse : MikhailovProjectile
{
    public override string Texture => ColdTecVisuals.Path + "25";
    protected override Color Glow => new(40, 190, 255);
    private int Bounces { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
    private bool Refracted { get => Projectile.ai[1] == 1f; set => Projectile.ai[1] = value ? 1f : 0f; }

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
    }

    public override void AI()
    {
        base.AI();
        Projectile.spriteDirection = Projectile.velocity.X < 0f ? -1 : 1;
        Projectile.scale = .14f + Bounces * .012f + (Refracted ? .018f : 0f);
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, -Projectile.velocity * .04f, 90, Color.Cyan, .6f + Bounces * .06f);
            dust.noGravity = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Bounces >= 4) return true;
        bool bounced = false;
        if (System.Math.Abs(Projectile.velocity.X - oldVelocity.X) > .01f) { Projectile.velocity.X = -oldVelocity.X; bounced = true; }
        if (System.Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > .01f) { Projectile.velocity.Y = -oldVelocity.Y; bounced = true; }
        if (!bounced) Projectile.velocity = -oldVelocity;
        Bounces++;
        Projectile.damage = (int)System.MathF.Ceiling(Projectile.damage * 1.2f);
        Projectile.velocity *= 1.08f;
        Projectile.netUpdate = true;
        SpawnEffect(1f + Bounces % 2);
        SoundEngine.PlaySound(ColdTecSounds.Pulse with { Pitch = .12f + Bounces * .06f, Volume = .22f }, Projectile.Center);
        return false;
    }

    internal void Refract(Vector2 prismCenter)
    {
        if (Refracted) return;
        NPC target = FindTarget(700f);
        Vector2 direction = target != null ? Projectile.DirectionTo(target.Center) : Projectile.velocity.SafeNormalize(Vector2.UnitX);
        float speed = Projectile.velocity.Length();
        Projectile.Center = prismCenter + direction * 18f;
        Projectile.velocity = direction * speed;
        Projectile.damage = (int)System.MathF.Ceiling(Projectile.damage * 1.3f);
        Refracted = true;
        Projectile.netUpdate = true;
        SpawnEffect(3f);
        SoundEngine.PlaySound(ColdTecSounds.PrismBurst with { Volume = .32f, Pitch = .2f }, prismCenter);
    }

    private void SpawnEffect(float kind)
    {
        if (Projectile.owner == Main.myPlayer)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ColdTecImpact>(), 0, 0f, Projectile.owner, kind);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Slow, 90);
    public override void OnKill(int timeLeft) => SpawnEffect(Bounces >= 3 || Refracted ? 3f : 0f);

    public override bool PreDraw(ref Color lightColor)
    {
        int body = Bounces < 2 ? 25 : Bounces < 4 ? 26 : 27;
        int ring = Bounces < 2 ? 24 : Bounces < 4 ? 28 : 29;
        if (Bounces > 0) ColdTecVisuals.Draw(Projectile, Bounces < 3 ? 32 : 33, Color.White * .28f, .62f);
        ColdTecVisuals.Draw(Projectile, body, Color.White);
        ColdTecVisuals.Draw(Projectile, ring, Color.White * .6f, .32f, -Projectile.rotation);
        return false;
    }
}

public sealed class ColdTecPrism : ModProjectile
{
    public override string Texture => ColdTecVisuals.Path + "36";

    public override void SetDefaults()
    {
        Projectile.width = 46;
        Projectile.height = 46;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += .025f;
        Projectile.scale = .19f + (float)System.Math.Sin(Main.GameUpdateCount * .12f) * .012f;
        Lighting.AddLight(Projectile.Center, .08f, .48f, .8f);
        if (Projectile.owner != Main.myPlayer) return;

        foreach (Projectile other in Main.ActiveProjectiles)
        {
            if (other.owner != Projectile.owner || other.type != ModContent.ProjectileType<ColdTecPulse>() || other.ai[1] == 1f) continue;
            Vector2 offset = Projectile.Center - other.Center;
            float distance = offset.Length();
            if (distance > 180f || distance < .01f) continue;
            if (distance <= 22f)
            {
                ((ColdTecPulse)other.ModProjectile).Refract(Projectile.Center);
                continue;
            }
            float pull = MathHelper.Lerp(.12f, .55f, 1f - distance / 180f);
            other.velocity = Vector2.Lerp(other.velocity, offset.SafeNormalize(Vector2.Zero) * System.Math.Max(7f, other.velocity.Length()), pull);
            if (Main.GameUpdateCount % 6 == 0) other.netUpdate = true;
        }
    }

    public override bool? CanDamage() => false;

    public override void OnKill(int timeLeft)
    {
        if (Projectile.owner == Main.myPlayer)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ColdTecImpact>(), 0, 0f, Projectile.owner, 3f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        ColdTecVisuals.Draw(Projectile, 37, new Color(80, 190, 255, 150), .75f, -Projectile.rotation * 2f);
        ColdTecVisuals.Draw(Projectile, 36, Color.White * Projectile.Opacity);
        return false;
    }
}

public sealed class ColdTecImpact : ModProjectile
{
    public override string Texture => ColdTecVisuals.Path + "40";
    public override void SetDefaults() { Projectile.width = 4; Projectile.height = 4; Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.timeLeft = 12; }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        Projectile.rotation += .08f;
        Projectile.scale = .1f + (12 - Projectile.timeLeft) * .012f;
        Projectile.alpha = Projectile.timeLeft < 5 ? (5 - Projectile.timeLeft) * 50 : 0;
        Lighting.AddLight(Projectile.Center, .04f, .28f, .55f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        int kind = (int)Projectile.ai[0];
        int elapsed = 12 - Projectile.timeLeft;
        int texture = kind switch { 0 => elapsed < 6 ? 30 : 31, 1 => 38, 2 => 35, _ => elapsed < 4 ? 34 : elapsed < 8 ? 39 : 40 };
        ColdTecVisuals.Draw(Projectile, texture, Color.White * Projectile.Opacity, kind == 3 ? .75f : .55f);
        return false;
    }
}
