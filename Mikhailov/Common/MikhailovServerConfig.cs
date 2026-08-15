using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Mikhailov.Common;

public sealed class MikhailovServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(true)]
    [ReloadRequired]
    public bool EnablePudgeContent { get; set; } = true;
}
