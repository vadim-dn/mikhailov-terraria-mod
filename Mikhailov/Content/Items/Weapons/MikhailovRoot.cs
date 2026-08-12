using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class MikhailovRoot : MikhailovWeapon
{
    private int combo;

    protected override Vector2 GripOffset => new(-18f, 5f);
    public override bool AltFunctionUse(Player player) => true;

    public override void SetDefaults()
    {
        Item.damage = 25;
        Item.DamageType = DamageClass.Magic;
        Item.width = 54;
        Item.height = 54;
        Item.mana = 0;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 9;
        Item.knockBack = 3.5f;
        Item.shoot = ModContent.ProjectileType<RootBolt>();
        Item.shootSpeed = 15f;
        Item.UseSound = null;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 5);
    }

    public override bool CanUseItem(Player player)
    {
        bool alt = player.altFunctionUse == 2;
        Item.useTime = Item.useAnimation = alt ? 24 : 9;
        Item.shoot = alt ? ModContent.ProjectileType<RootCrescent>() : ModContent.ProjectileType<RootBolt>();
        Item.shootSpeed = alt ? 11f : 15f;
        Item.knockBack = alt ? 7f : 3.5f;
        Item.UseSound = null;
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool alt = player.altFunctionUse == 2;
        Vector2 aim = Aim(player, alt ? 11f : 15f);
        position = Muzzle(player, aim, 38f);
        if (alt)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item60 with { Volume = .28f, Pitch = .25f, PitchVariance = .08f, MaxInstances = 2 }, player.Center);
            Projectile.NewProjectile(source, position, aim, type, damage, knockback, player.whoAmI);
        }
        else
        {
            int step = combo++ % 3;
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item8 with { Volume = step == 2 ? .3f : .18f, Pitch = .45f + step * .08f, PitchVariance = .06f, MaxInstances = 3 }, player.Center);
            float spread = step == 2 ? .015f : .035f;
            Projectile.NewProjectile(source, position, Spread(aim, spread), type, step == 2 ? (int)(damage * 1.35f) : damage, knockback, player.whoAmI, step);
        }
        return false;
    }
}
