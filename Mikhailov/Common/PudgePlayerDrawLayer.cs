using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mikhailov.Content.Items.Armor;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Mikhailov.Common;

public sealed class PudgePlayerDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        return ModContent.GetInstance<MikhailovServerConfig>().EnablePudgeContent
            && player.armor[0].type == ModContent.ItemType<PudgeMask>()
            && player.armor[1].type == ModContent.ItemType<PudgeBody>()
            && player.armor[2].type == ModContent.ItemType<PudgeLegs>();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        Texture2D texture = ModContent.Request<Texture2D>("Mikhailov/Assets/Effects/Pudge/PudgePlayer", AssetRequestMode.ImmediateLoad).Value;
        const int frameWidth = 90;
        const int frameHeight = 130;
        int frame = 0;
        Rectangle source = new(0, frame * frameHeight, frameWidth, frameHeight);
        Vector2 position = player.Bottom - Main.screenPosition + new Vector2(0f, 5f);
        SpriteEffects effects = player.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        drawInfo.DrawDataCache.Add(new DrawData(texture, position, source, drawInfo.colorArmorBody,
            0f, new Vector2(frameWidth / 2f, frameHeight), 0.72f, effects, 0));
    }
}
