using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public abstract class MikhailovWeapon : ModItem
{
    protected virtual Vector2 GripOffset => Vector2.Zero;
    public override Vector2? HoldoutOffset() => GripOffset;

    protected static Vector2 Aim(Player player, float speed)
    {
        Vector2 direction = Main.MouseWorld - player.MountedCenter;
        if (direction.LengthSquared() < 0.01f) direction = Vector2.UnitX * player.direction;
        return Vector2.Normalize(direction) * speed;
    }

    protected static Vector2 Spread(Vector2 velocity, float radians) => velocity.RotatedBy(Main.rand.NextFloat(-radians, radians));

    protected static void MuzzleFlash(Vector2 position, Color color)
    {
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustPerfect(position, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 80, color, 1.3f);
            dust.noGravity = true;
        }
        SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.65f }, position);
    }

    protected static Vector2 Muzzle(Player player, Vector2 aim, float distance, float vertical = 0f)
        => player.MountedCenter + Vector2.Normalize(aim) * distance + new Vector2(0f, vertical * player.gravDir);
}
