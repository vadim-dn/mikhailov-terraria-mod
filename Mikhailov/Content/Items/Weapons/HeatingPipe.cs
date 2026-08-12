using Microsoft.Xna.Framework;
using Mikhailov.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Weapons;

public sealed class HeatingPipe : MikhailovWeapon
{
    protected override Vector2 GripOffset => new(-18f, 3f);
    public override bool AltFunctionUse(Player player) => true;
    public override bool CanUseItem(Player player) { bool alt=player.altFunctionUse==2;Item.useTime=Item.useAnimation=alt?8:18;Item.channel=alt;return !alt||player.ownedProjectileCounts[ModContent.ProjectileType<PipeHoldout>()]==0; }
    public override void SetDefaults()
    {
        Item.damage = 52; Item.DamageType = DamageClass.Melee; Item.width = 51; Item.height = 38;
        Item.useTime = 18; Item.useAnimation = 18; Item.useStyle = ItemUseStyleID.Swing; Item.knockBack = 8f;
        Item.UseSound = new Terraria.Audio.SoundStyle("Mikhailov/Assets/Sounds/PipeSwing") { Volume = .7f, PitchVariance = .1f }; Item.autoReuse = true; Item.rare = ItemRarityID.Orange; Item.value = Item.sellPrice(gold: 2);
    }
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            Vector2 aim = Aim(player, 10f);
            if (player.altFunctionUse == 2)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, aim, ModContent.ProjectileType<PipeHoldout>(), Item.damage / 3, Item.knockBack, player.whoAmI);
            else
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), Muzzle(player, aim, 30f), aim, ModContent.ProjectileType<WaterSlash>(), Item.damage / 2, Item.knockBack, player.whoAmI, player.itemAnimation % 2);
        }
        return true;
    }
    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Main.rand.NextBool(3)) target.AddBuff(BuffID.Bleeding, 240);
    }
}
