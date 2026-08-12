using Microsoft.Xna.Framework;
using Mikhailov.Common;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class MikhailovskyAK : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-12f, 2f);
    public override bool AltFunctionUse(Player player) => true;
    public override void SetDefaults()
    {
        Item.damage = 34; Item.DamageType = DamageClass.Ranged; Item.width = 58; Item.height = 38;
        Item.useTime = 7; Item.useAnimation = 7; Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true; Item.knockBack = 2.2f; Item.shoot = ModContent.ProjectileType<AkBullet>();
        Item.shootSpeed = 14f; Item.UseSound = new Terraria.Audio.SoundStyle("Mikhailov/Assets/Sounds/AkShot") { Volume = .55f, PitchVariance = .08f, MaxInstances = 4 }; Item.autoReuse = true; Item.rare = ItemRarityID.Orange; Item.value = Item.sellPrice(gold: 3);
    }
    public override bool CanUseItem(Player player)
    {
        bool alt = player.altFunctionUse == 2;
        Item.useTime = Item.useAnimation = alt ? 24 : 7;
        return !alt || player.GetModPlayer<MikhailovPlayer>().AkAltCooldown <= 0;
    }
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool alt = player.altFunctionUse == 2;
        Vector2 aim = Aim(player, alt ? 17f : 14f);
        position = Muzzle(player, aim, 39f, -2f);
        if (alt) Projectile.NewProjectile(source, position, aim, ModContent.ProjectileType<MagicBurstController>(), damage, knockback, player.whoAmI);
        else Projectile.NewProjectile(source, position, Spread(aim, .10f), type, damage, knockback, player.whoAmI, Main.GameUpdateCount % 3);
        if (alt) player.GetModPlayer<MikhailovPlayer>().AkAltCooldown = 45;
        MuzzleFlash(position, alt ? Color.OrangeRed : Color.Gold);
        return false;
    }
}
