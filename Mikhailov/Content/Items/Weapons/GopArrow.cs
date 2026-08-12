using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class GopArrow : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-20f, 1f);
    public override bool AltFunctionUse(Player player) => true;
    public override void SetDefaults()
    {
        Item.damage = 58; Item.DamageType = DamageClass.Ranged; Item.width = 51; Item.height = 45; Item.noMelee = true;
        Item.useTime = 24; Item.useAnimation = 24; Item.useStyle = ItemUseStyleID.Shoot; Item.knockBack = 4f;
        Item.shoot = ModContent.ProjectileType<GopArrowProjectile>(); Item.shootSpeed = 15f; Item.UseSound = new Terraria.Audio.SoundStyle("Mikhailov/Assets/Sounds/MagicShot") { Volume = .65f, PitchVariance = .08f }; Item.autoReuse = true; Item.channel = true; Item.rare = ItemRarityID.LightRed; Item.value = Item.sellPrice(gold: 6);
    }
    public override bool CanUseItem(Player player) { bool alt=player.altFunctionUse==2;Item.useTime=Item.useAnimation=alt?12:24;return !alt||player.ownedProjectileCounts[ModContent.ProjectileType<ChargedWeaponHoldout>()]==0; }
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool alt = player.altFunctionUse == 2;
        Vector2 aim = Aim(player, alt ? 19f : 15f); position = Muzzle(player, aim, 42f);
        if (alt) Projectile.NewProjectile(source, position, aim, ModContent.ProjectileType<ChargedWeaponHoldout>(), damage, knockback, player.whoAmI, 0f);
        else Projectile.NewProjectile(source, position, aim, type, damage, knockback, player.whoAmI, 0f);
        return false;
    }
}
