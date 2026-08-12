using Microsoft.Xna.Framework;
using Mikhailov.Common;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class RailBorer : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-18f, 3f);
    public override bool AltFunctionUse(Player player) => true;
    public override void SetDefaults()
    {
        Item.damage = 112; Item.DamageType = DamageClass.Ranged; Item.width = 50; Item.height = 32; Item.noMelee = true;
        Item.useTime = 52; Item.useAnimation = 52; Item.useStyle = ItemUseStyleID.Shoot; Item.knockBack = 9f;
        Item.shoot = ModContent.ProjectileType<RailSlug>(); Item.shootSpeed = 20f; Item.UseSound = new Terraria.Audio.SoundStyle("Mikhailov/Assets/Sounds/RailShot") { Volume = .75f, PitchVariance = .04f }; Item.channel = true; Item.rare = ItemRarityID.LightRed; Item.value = Item.sellPrice(gold: 8);
    }
    public override bool CanUseItem(Player player)
    {
        Item.useTime = Item.useAnimation = player.altFunctionUse == 2 ? 90 : 52;
        return player.GetModPlayer<MikhailovPlayer>().RailCooldown <= 0 && (player.altFunctionUse != 2 || player.ownedProjectileCounts[ModContent.ProjectileType<ChargedWeaponHoldout>()] == 0);
    }
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool alt = player.altFunctionUse == 2;
        Vector2 aim = Aim(player, alt ? 16f : 22f); position = Muzzle(player, aim, 43f);
        if (alt) Projectile.NewProjectile(source, position, aim, ModContent.ProjectileType<ChargedWeaponHoldout>(), damage, knockback, player.whoAmI, 1f);
        else Projectile.NewProjectile(source, position, aim, type, damage, knockback, player.whoAmI, 0f);
        player.velocity -= Vector2.Normalize(aim) * (alt ? 0f : 2.2f);
        player.GetModPlayer<MikhailovPlayer>().RailCooldown = alt ? 100 : 55;
        MuzzleFlash(position, alt ? Color.OrangeRed : Color.SandyBrown);
        player.GetModPlayer<MikhailovPlayer>().ScreenShake = alt ? 2f : 5f;
        return false;
    }
}
