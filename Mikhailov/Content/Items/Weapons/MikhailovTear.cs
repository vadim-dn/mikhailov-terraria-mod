using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class MikhailovTear : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-8f, 4f);
    public override bool AltFunctionUse(Player player) => true;
    public override void SetDefaults()
    {
        Item.damage = 42; Item.DamageType = DamageClass.Magic; Item.width = 39; Item.height = 48; Item.noMelee = true;
        Item.useTime = 30; Item.useAnimation = 30; Item.useStyle = ItemUseStyleID.Swing; Item.knockBack = 4f;
        Item.shoot = ModContent.ProjectileType<LivingFireBottle>(); Item.shootSpeed = 11f; Item.UseSound = new Terraria.Audio.SoundStyle("Mikhailov/Assets/Sounds/BottleThrow") { Volume = .6f, PitchVariance = .12f }; Item.autoReuse = true; Item.rare = ItemRarityID.Orange; Item.value = Item.sellPrice(gold: 3);
    }
    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool alt = player.altFunctionUse == 2;
        Vector2 aim=Aim(player,alt?9f:11f);
        Projectile.NewProjectile(source,position,Spread(aim,alt?.06f:.03f),type,alt?(int)(damage*1.15f):damage,knockback,player.whoAmI,alt?1f:0f);
        return false;
    }
}
