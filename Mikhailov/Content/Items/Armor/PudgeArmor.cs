using System.Collections.Generic;
using Mikhailov.Common;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Mikhailov.Content.Items.Armor;

public abstract class PudgeArmorItem : ModItem
{
    protected abstract int Defense { get; }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.defense = Defense;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent) return;
        tooltips.Add(new TooltipLine(Mod, "PudgeDisabled", Language.GetTextValue("Mods.Mikhailov.Common.PudgeDisabled"))
        {
            OverrideColor = Microsoft.Xna.Framework.Color.Gray
        });
    }

    public override void UpdateEquip(Player player)
    {
        if (!ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent)
            player.statDefense -= Defense;
    }
}

[AutoloadEquip(EquipType.Head)]
public sealed class PudgeMask : PudgeArmorItem
{
    protected override int Defense => 10;

    public override bool IsArmorSet(Item head, Item body, Item legs) =>
        body.type == ModContent.ItemType<PudgeBody>() && legs.type == ModContent.ItemType<PudgeLegs>();

    public override void UpdateArmorSet(Player player)
    {
        if (!ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent) return;
        player.setBonus = Language.GetTextValue("Mods.Mikhailov.Items.PudgeMask.SetBonus");
    }
}

[AutoloadEquip(EquipType.Body)]
public sealed class PudgeBody : PudgeArmorItem
{
    protected override int Defense => 18;
}

[AutoloadEquip(EquipType.Legs)]
public sealed class PudgeLegs : PudgeArmorItem
{
    protected override int Defense => 14;
}
