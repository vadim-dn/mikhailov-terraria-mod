using System.IO;
using Microsoft.Xna.Framework;
using Mikhailov.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov;

public sealed class Mikhailov : Mod
{
    internal static ModKeybind HookKeybind { get; private set; }
    internal static ModKeybind RotKeybind { get; private set; }
    internal static ModKeybind DismemberKeybind { get; private set; }

    public override void Load()
    {
        HookKeybind = KeybindLoader.RegisterKeybind(this, "PudgeHook", "Z");
        RotKeybind = KeybindLoader.RegisterKeybind(this, "PudgeRot", "X");
        DismemberKeybind = KeybindLoader.RegisterKeybind(this, "PudgeDismember", "C");
    }

    public override void Unload()
    {
        HookKeybind = RotKeybind = DismemberKeybind = null;
    }

    internal static void SendAbility(PudgeAbility ability, Vector2 cursor)
    {
        ModPacket packet = ModContent.GetInstance<Mikhailov>().GetPacket();
        packet.Write((byte)ability);
        packet.WriteVector2(cursor);
        packet.Send();
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        PudgeAbility ability = (PudgeAbility)reader.ReadByte();
        Vector2 cursor = reader.ReadVector2();
        if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
            Main.player[whoAmI].GetModPlayer<PudgePlayer>().HandleAbility(ability, cursor);
    }
}
