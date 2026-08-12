using System.IO;
using Mikhailov.Content.Items.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Mikhailov.Common;

public sealed class StarterChestSystem : ModSystem
{
    private bool generated;
    private int modChestX = -1;
    private int modChestY = -1;
    private int placementDelay;
    private bool surfacePrepared;

    public override void ClearWorld()
    {
        generated = false;
        modChestX = modChestY = -1;
        placementDelay = 30;
        surfacePrepared = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (!generated) return;
        tag["StarterChestsGenerated"] = true;
        tag["StarterChestX"] = modChestX;
        tag["StarterChestY"] = modChestY;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        modChestX = tag.GetInt("StarterChestX");
        modChestY = tag.GetInt("StarterChestY");
        // Старые миры содержали только флаг. Для них сундуки создаются повторно корректным способом.
        generated = tag.ContainsKey("StarterChestsGenerated") && modChestX > 0 && modChestY > 0;
        placementDelay = 30;
        surfacePrepared = false;
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(generated);
        writer.Write(modChestX);
        writer.Write(modChestY);
    }

    public override void NetReceive(BinaryReader reader)
    {
        generated = reader.ReadBoolean();
        modChestX = reader.ReadInt32();
        modChestY = reader.ReadInt32();
    }

    public override void PostWorldGen() => TryPlaceStarterChests();

    public override void PostUpdateWorld()
    {
        if (generated || Main.netMode == NetmodeID.MultiplayerClient || --placementDelay > 0) return;
        TryPlaceStarterChests();
        placementDelay = 300;
    }

    private void TryPlaceStarterChests()
    {
        if (generated || Main.netMode == NetmodeID.MultiplayerClient) return;

        int left = FindSurfaceSide();
        int floorY = FindSurfaceY(left + 8);
        if (!surfacePrepared)
        {
            PrepareSurface(left, floorY);
            surfacePrepared = true;
        }

        int firstX = left + 3;
        int secondX = left + 11;
        // WorldGen.PlaceChest принимает координату нижнего левого тайла сундука.
        // Опорные блоки должны располагаться непосредственно строкой ниже.
        int chestY = floorY - 1;
        int modChest = WorldGen.PlaceChest(firstX, chestY, TileID.Containers, false, 0);
        int vanillaChest = WorldGen.PlaceChest(secondX, chestY, TileID.Containers, false, 0);

        if (modChest < 0 || vanillaChest < 0) return;

        FillModChest(Main.chest[modChest]);
        FillVanillaChest(Main.chest[vanillaChest]);
        modChestX = firstX;
        modChestY = chestY;
        generated = true;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendTileSquare(-1, left, floorY - 5, 20, 6);
    }

    private static int FindSurfaceSide()
    {
        int preferred = Main.spawnTileX + 14;
        return Utils.Clamp(preferred, 20, Main.maxTilesX - 40);
    }

    private static int FindSurfaceY(int x)
    {
        int start = 40;
        int end = Utils.Clamp((int)Main.worldSurface + 80, 100, Main.maxTilesY - 100);
        for (int y = start; y < end; y++)
            if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
                return y;

        return Utils.Clamp(Main.spawnTileY + 3, 50, Main.maxTilesY - 100);
    }

    private static void PrepareSurface(int left, int floorY)
    {
        for (int x = left; x < left + 20; x++)
        {
            for (int y = floorY - 5; y < floorY; y++)
            {
                WorldGen.KillTile(x, y, false, false, true);
                WorldGen.KillWall(x, y);
            }

            WorldGen.KillTile(x, floorY, false, false, true);
            WorldGen.PlaceTile(x, floorY, TileID.WoodBlock, true, true);
        }
    }

    private static void FillModChest(Chest chest)
    {
        chest.name = "Арсенал Михайлова";
        int[] items = { ModContent.ItemType<MikhailovskyAK>(), ModContent.ItemType<HeatingPipe>(), ModContent.ItemType<GopArrow>(), ModContent.ItemType<MikhailovTear>(), ModContent.ItemType<RailBorer>(), ModContent.ItemType<Screwdriver>(), ModContent.ItemType<MikhailovRoot>() };
        for (int i = 0; i < items.Length; i++) chest.item[i].SetDefaults(items[i]);
    }

    private static void FillVanillaChest(Chest chest)
    {
        chest.name = "Стартовый набор";
        (int type, int stack)[] items = {
            (ItemID.BetsyWings, 1), (ItemID.SolarFlarePickaxe, 1), (ItemID.SolarFlareHelmet, 1),
            (ItemID.SolarFlareBreastplate, 1), (ItemID.SolarFlareLeggings, 1), (ItemID.TerrasparkBoots, 1),
            (ItemID.CelestialShell, 1), (ItemID.AnkhShield, 1), (ItemID.LunarHook, 1), (ItemID.MagicMirror, 1),
            (ItemID.SuperHealingPotion, 30), (ItemID.IronskinPotion, 10), (ItemID.RegenerationPotion, 10),
            (ItemID.SwiftnessPotion, 10), (ItemID.EndurancePotion, 10), (ItemID.LifeforcePotion, 10),
            (ItemID.WrathPotion, 10), (ItemID.RagePotion, 10), (ItemID.SeafoodDinner, 10)
        };
        for (int i = 0; i < items.Length; i++) { chest.item[i].SetDefaults(items[i].type); chest.item[i].stack = items[i].stack; }
    }
}
