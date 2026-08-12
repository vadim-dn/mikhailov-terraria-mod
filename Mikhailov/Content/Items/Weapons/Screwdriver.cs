using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class Screwdriver : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-10f, 1f);
    public override bool AltFunctionUse(Player player) => true;
    public override bool CanUseItem(Player player) => player.altFunctionUse != 2 || player.ownedProjectileCounts[ModContent.ProjectileType<ScrewTurretHoldout>()] == 0;
    public override void SetDefaults()
    {
        Item.damage = 39; Item.DamageType = DamageClass.Ranged; Item.width = 34; Item.height = 36; Item.noMelee = true;
        Item.useTime = 6; Item.useAnimation = 6; Item.useStyle = ItemUseStyleID.Shoot; Item.knockBack = 2f;
        Item.shoot = ModContent.ProjectileType<ScrewProjectile>(); Item.shootSpeed = 13f; Item.UseSound = new Terraria.Audio.SoundStyle("Mikhailov/Assets/Sounds/Drill") { Volume = .38f, PitchVariance = .05f, MaxInstances = 2 }; Item.autoReuse = true; Item.channel = true; Item.rare = ItemRarityID.LightRed; Item.value = Item.sellPrice(gold: 7);
    }
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool alt = player.altFunctionUse == 2;
        Vector2 aim = Aim(player, alt ? 16f : 13f); position = Muzzle(player, aim, 31f);
        if(alt) Projectile.NewProjectile(source, position, aim, ModContent.ProjectileType<ScrewTurretHoldout>(), (int)(damage*.9f), knockback, player.whoAmI);
        else Projectile.NewProjectile(source, position, Spread(aim,.08f), type, damage, knockback, player.whoAmI, 0f, Main.GameUpdateCount % 12);
        return false;
    }
}
