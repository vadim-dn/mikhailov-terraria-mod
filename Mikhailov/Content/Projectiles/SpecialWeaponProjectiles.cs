using Microsoft.Xna.Framework;
using Mikhailov.Common;
using Mikhailov.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Projectiles;

public abstract class HoldoutController : ModProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/Invisible";
    public override void SetDefaults(){Projectile.width=2;Projectile.height=2;Projectile.timeLeft=3600;Projectile.tileCollide=false;Projectile.netImportant=true;}
    protected Vector2 UpdateAim(Player p,float speed)
    {
        if(Projectile.owner==Main.myPlayer){Vector2 v=Main.MouseWorld-p.MountedCenter;if(v.LengthSquared()<1)v=Vector2.UnitX*p.direction;Projectile.velocity=Vector2.Normalize(v)*speed;Projectile.netUpdate=true;}
        Vector2 aim=Projectile.velocity.SafeNormalize(Vector2.UnitX*p.direction);p.ChangeDir(aim.X>=0?1:-1);p.itemTime=2;p.itemAnimation=2;p.itemRotation=aim.ToRotation()*p.direction;Projectile.Center=p.MountedCenter+aim*38f;return aim;
    }
    protected bool Held(Player p,int itemType)=>p.active&&!p.dead&&p.HeldItem.type==itemType&&p.controlUseTile;
}

public sealed class MagicBurstController : HoldoutController
{
    public override void SetDefaults(){base.SetDefaults();Projectile.timeLeft=18;}
    public override void AI(){Player p=Main.player[Projectile.owner];Projectile.Center=p.MountedCenter;if(Projectile.timeLeft is 18 or 12 or 6&&Projectile.owner==Main.myPlayer){int shot=(18-Projectile.timeLeft)/6;Vector2 v=Projectile.velocity.RotatedBy(Main.rand.NextFloat(-.025f,.025f));Projectile.NewProjectile(Projectile.GetSource_FromThis(),p.MountedCenter+v.SafeNormalize(Vector2.UnitX)*38,v,ModContent.ProjectileType<AkBullet>(),(int)(Projectile.damage*.78f),Projectile.knockBack,Projectile.owner,shot);SoundEngine.PlaySound(new SoundStyle("Mikhailov/Assets/Sounds/MagicShot"){Volume=.5f,Pitch=shot*.08f},Projectile.Center);}}
}

public sealed class ChargedWeaponHoldout : HoldoutController
{
    private bool Rail=>Projectile.ai[0]==1f;
    public override void AI()
    {
        Player p=Main.player[Projectile.owner];Vector2 aim=UpdateAim(p,Rail?20:19);int item=Rail?ModContent.ItemType<RailBorer>():ModContent.ItemType<GopArrow>();
        if(Held(p,item)){Projectile.localAI[0]=System.Math.Min(90,Projectile.localAI[0]+1);Color c=Main.hslToRgb((Projectile.localAI[0]*.018f)%1,.9f,.6f);if(Main.rand.NextBool(2)){Dust d=Dust.NewDustPerfect(Projectile.Center+Main.rand.NextVector2Circular(22,22),DustID.RainbowTorch,Projectile.DirectionTo(p.MountedCenter)*2,0,c,1.2f);d.noGravity=true;}return;}
        Fire(p,aim,Projectile.localAI[0]>=70); 
    }
    private void Fire(Player p,Vector2 aim,bool full)
    {
        if(Projectile.owner==Main.myPlayer){int type=Rail?ModContent.ProjectileType<RailSlug>():ModContent.ProjectileType<GopArrowProjectile>();Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,aim*(Rail?22:19),type,(int)(Projectile.damage*(full?1.85f:1.1f)),Projectile.knockBack,Projectile.owner,full?1:0);}
        SoundEngine.PlaySound(new SoundStyle(Rail?"Mikhailov/Assets/Sounds/RailShot":"Mikhailov/Assets/Sounds/MagicShot"){Volume=.8f},Projectile.Center);p.velocity-=aim*(Rail?(full?4:2):.8f);p.GetModPlayer<MikhailovPlayer>().ScreenShake=full?(Rail?8:5):2;Projectile.Kill();
    }
}

public sealed class PipeHoldout : HoldoutController
{
    public override void AI(){Player p=Main.player[Projectile.owner];Vector2 aim=UpdateAim(p,11);if(Held(p,ModContent.ItemType<HeatingPipe>())){if(++Projectile.localAI[0]%5==0&&Projectile.owner==Main.myPlayer)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,aim.RotatedBy(Main.rand.NextFloat(-.035f,.035f))*11,ModContent.ProjectileType<PipeWaterJet>(),Projectile.damage,Projectile.knockBack,Projectile.owner);p.velocity-=aim*.035f;return;}if(Projectile.owner==Main.myPlayer)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,aim*9,ModContent.ProjectileType<WaterBurst>(),Projectile.damage*2,Projectile.knockBack,Projectile.owner);Projectile.Kill();}
}

public sealed class ScrewTurretHoldout : HoldoutController
{
    public override void AI(){Player p=Main.player[Projectile.owner];Vector2 aim=UpdateAim(p,16);if(!Held(p,ModContent.ItemType<Screwdriver>())){Projectile.Kill();return;}if(++Projectile.localAI[0]%6==0&&Projectile.owner==Main.myPlayer)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,aim.RotatedBy(Main.rand.NextFloat(-.025f,.025f))*16,ModContent.ProjectileType<ScrewProjectile>(),Projectile.damage,Projectile.knockBack,Projectile.owner,1,Projectile.localAI[0]);}
}
