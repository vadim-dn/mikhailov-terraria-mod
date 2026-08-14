using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class ManholeCover : MikhailovWeapon
{
    public override bool AltFunctionUse(Player player) => true;

    public override void SetDefaults()
    {
        Item.damage = 5;
        Item.DamageType = DamageClass.Melee;
        Item.width = 52;
        Item.height = 52;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 24;
        Item.knockBack = 9f;
        Item.shoot = ModContent.ProjectileType<ManholeCoverProjectile>();
        Item.shootSpeed = 17.5f;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 6);
        Item.UseSound = ManholeCoverSounds.Throw;
    }

    public override bool CanUseItem(Player player)
    {
        bool charged = player.altFunctionUse == 2;
        Item.channel = charged;
        Item.useTime = Item.useAnimation = charged ? 12 : 24;
        Item.shoot = charged
            ? ModContent.ProjectileType<ManholeCoverChargeHoldout>()
            : ModContent.ProjectileType<ManholeCoverProjectile>();
        Item.shootSpeed = charged ? 1f : 17.5f;
        Item.UseSound = charged ? null : ManholeCoverSounds.Throw;
        return !charged || player.ownedProjectileCounts[ModContent.ProjectileType<ManholeCoverChargeHoldout>()] == 0;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Vector2 direction = Main.MouseWorld - player.MountedCenter;
        if (direction.LengthSquared() < 0.01f) direction = Vector2.UnitX * player.direction;
        Vector2 aim = direction.SafeNormalize(Vector2.UnitX * player.direction) * (player.altFunctionUse == 2 ? 1f : 17.5f);
        Vector2 spawn = player.MountedCenter + aim.SafeNormalize(Vector2.UnitX * player.direction) * 42f;
        Projectile.NewProjectile(source, spawn, aim, type, damage, knockback, player.whoAmI);
        return false;
    }
}

internal static class ManholeCoverSounds
{
    internal static readonly SoundStyle Throw = new("Mikhailov/Assets/Sounds/ManholeThrow") { Volume = .46f, PitchVariance = .06f, MaxInstances = 3 };
    internal static readonly SoundStyle Impact = new("Mikhailov/Assets/Sounds/ManholeImpact") { Volume = .52f, PitchVariance = .06f, MaxInstances = 4 };
    internal static readonly SoundStyle Bounce = new("Mikhailov/Assets/Sounds/ManholeBounce") { Volume = .45f, PitchVariance = .08f, MaxInstances = 4 };
}
