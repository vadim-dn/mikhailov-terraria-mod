using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class ColdTecLight : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-18f, 5f);
    public override bool AltFunctionUse(Player player) => true;

    public override void SetDefaults()
    {
        Item.damage = 24; Item.DamageType = DamageClass.Magic; Item.width = 54; Item.height = 54; Item.mana = 0;
        Item.noMelee = true; Item.useStyle = ItemUseStyleID.Shoot; Item.useTime = Item.useAnimation = 15;
        Item.knockBack = 3f; Item.shoot = ModContent.ProjectileType<ColdTecPulse>(); Item.shootSpeed = 15f;
        Item.autoReuse = true; Item.UseSound = ColdTecSounds.Pulse; Item.rare = ItemRarityID.LightRed; Item.value = Item.sellPrice(gold: 6);
    }

    public override bool CanUseItem(Player player)
    {
        bool alt = player.altFunctionUse == 2;
        Item.mana = 0; Item.useTime = Item.useAnimation = alt ? 24 : 15;
        Item.shoot = alt ? ModContent.ProjectileType<ColdTecPrism>() : ModContent.ProjectileType<ColdTecPulse>();
        Item.shootSpeed = alt ? 0f : 15f; Item.UseSound = alt ? ColdTecSounds.PrismBurst : ColdTecSounds.Pulse;
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse != 2)
        {
            Vector2 aim = Aim(player, 15f);
            Projectile.NewProjectile(source, Muzzle(player, aim, 36f), aim, type, damage, knockback, player.whoAmI);
            return false;
        }

        for (int i = 0; i < Main.maxProjectiles; i++)
            if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == type)
                Main.projectile[i].Kill();
        Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, 0, 0f, player.whoAmI);
        player.GetModPlayer<Common.MikhailovPlayer>().ScreenShake = 2.5f;
        return false;
    }
}
