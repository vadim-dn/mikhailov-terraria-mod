using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Mikhailov.Common;

public sealed class MikhailovPlayer : ModPlayer
{
    public int AkAltCooldown;
    public int RailCooldown;
    public float ScreenShake;

    public override void ModifyScreenPosition()
    {
        if (ScreenShake > 0.1f)
        {
            Main.screenPosition += Main.rand.NextVector2Circular(ScreenShake, ScreenShake);
            ScreenShake *= 0.86f;
        }
    }

    public override void OnEnterWorld()
    {
        if (Player.whoAmI == Main.myPlayer)
            Main.NewText($"Михайлов v{Mod.Version} загружен.", new Color(255, 180, 60));
    }

    public override void PostUpdate()
    {
        if (AkAltCooldown > 0) AkAltCooldown--;
        if (RailCooldown > 0) RailCooldown--;
        if (ScreenShake < 0.1f) ScreenShake = 0f;
    }
}
