using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Mikhailov.Content.Projectiles;

public abstract class MikhailovProjectile : ModProjectile
{
    protected virtual Color Glow => Color.White;
    protected void FaceVelocity() => Projectile.rotation = Projectile.velocity.ToRotation();
    protected NPC FindTarget(float range)
    {
        NPC result=null; float best=range;
        foreach(NPC npc in Main.ActiveNPCs) if(npc.CanBeChasedBy()) { float d=Vector2.Distance(Projectile.Center,npc.Center); if(d<best){best=d;result=npc;} }
        return result;
    }
    public override void AI(){ FaceVelocity(); Lighting.AddLight(Projectile.Center,Glow.ToVector3()*.55f); }
    protected void Trail(int dust,Color color,float scale=.9f){ if(Main.rand.NextBool(2)){Dust d=Dust.NewDustPerfect(Projectile.Center,dust,-Projectile.velocity*.08f,80,color,scale);d.noGravity=true;} }
}

public sealed class AkBullet : MikhailovProjectile
{
    public override string Texture => "Mikhailov/Assets/Effects/AkTracer";
    protected override Color Glow => Projectile.ai[0] switch {1f=>Color.Cyan,2f=>Color.Magenta,_=>Color.Orange};
    public override void SetDefaults(){Projectile.width=14;Projectile.height=6;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=Projectile.ai[0]==2f?3:1;Projectile.timeLeft=90;Projectile.extraUpdates=1;}
    public override void AI(){base.AI();Trail(DustID.RainbowTorch,Glow,1f);}
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int done){if(Projectile.ai[0]>0)target.AddBuff(BuffID.OnFire3,180);}
    public override void OnKill(int timeLeft){if(Projectile.ai[0]==2f)Effects.Explode(Projectile,64,DustID.RainbowTorch);}
    public override bool PreDraw(ref Color lightColor){if(Projectile.ai[0]==0f)return true;Texture2D t=ModContent.Request<Texture2D>("Mikhailov/Assets/Effects/MagicBullet",AssetRequestMode.ImmediateLoad).Value;Main.EntitySpriteDraw(t,Projectile.Center-Main.screenPosition,null,Glow,Projectile.rotation,t.Size()/2,Projectile.scale,SpriteEffects.None);return false;}
}

public sealed class WaterSlash : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/WaterSlash"; protected override Color Glow=>Color.DeepSkyBlue;
    public override void SetDefaults(){Projectile.width=46;Projectile.height=26;Projectile.friendly=true;Projectile.DamageType=DamageClass.Melee;Projectile.penetrate=2;Projectile.timeLeft=20;Projectile.tileCollide=false;}
    public override void AI(){base.AI();Projectile.scale+=.025f;Projectile.alpha+=7;Trail(DustID.Water,Color.Cyan,1.1f);}
}

public sealed class PipeWaterJet : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/WaterBolt"; protected override Color Glow=>Color.Cyan;
    public override void SetDefaults(){Projectile.width=28;Projectile.height=18;Projectile.friendly=true;Projectile.DamageType=DamageClass.Melee;Projectile.penetrate=3;Projectile.timeLeft=35;Projectile.extraUpdates=1;Projectile.usesLocalNPCImmunity=true;Projectile.localNPCHitCooldown=12;}
    public override void AI(){base.AI();Projectile.velocity*=.99f;Projectile.scale+=.01f;Projectile.alpha+=3;Trail(DustID.Water,Color.Cyan,1.2f);}
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int done)=>target.velocity+=Vector2.Normalize(Projectile.velocity)*2.2f;
}

public sealed class WaterBurst : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/WaterSlash"; protected override Color Glow=>Color.LightCyan;
    public override void SetDefaults(){Projectile.width=60;Projectile.height=40;Projectile.friendly=true;Projectile.DamageType=DamageClass.Melee;Projectile.penetrate=5;Projectile.timeLeft=25;Projectile.tileCollide=false;Projectile.usesLocalNPCImmunity=true;Projectile.localNPCHitCooldown=18;}
    public override void AI(){base.AI();Projectile.scale+=.065f;Projectile.alpha+=6;for(int i=0;i<2;i++)Trail(DustID.Water,Color.LightCyan,1.5f);}
}

public sealed class GopArrowProjectile : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/GopArrowBolt"; protected override Color Glow=>Projectile.ai[0]==1f?Color.Lime:Color.GreenYellow;
    public override void SetDefaults(){Projectile.width=22;Projectile.height=12;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=3;Projectile.timeLeft=150;Projectile.extraUpdates=1;}
    public override void AI(){base.AI();Trail(DustID.GreenTorch,Color.LimeGreen,1.15f);}
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int done)=>target.AddBuff(BuffID.Poisoned,240);
    public override void OnKill(int timeLeft)
    {
        if(Projectile.ai[0]==1f){if(Projectile.owner==Main.myPlayer)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Vector2.Zero,ModContent.ProjectileType<GreenTornado>(),Projectile.damage/2,2f,Projectile.owner);return;}
        if(Projectile.owner==Main.myPlayer)for(int i=0;i<4;i++)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Main.rand.NextVector2CircularEdge(6,6),ModContent.ProjectileType<PoisonShard>(),Projectile.damage/3,1f,Projectile.owner);
    }
}

public sealed class GreenTornado : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/GreenTornado"; protected override Color Glow=>Color.LimeGreen;
    public override void SetDefaults(){Projectile.width=58;Projectile.height=82;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=-1;Projectile.timeLeft=240;Projectile.tileCollide=false;Projectile.usesLocalNPCImmunity=true;Projectile.localNPCHitCooldown=24;}
    public override void AI(){Projectile.rotation+=.08f;Projectile.scale=.85f+.08f*(float)System.Math.Sin(Main.GameUpdateCount*.12f);Lighting.AddLight(Projectile.Center,0.1f,.8f,.12f);foreach(NPC n in Main.ActiveNPCs)if(n.CanBeChasedBy()&&Vector2.Distance(n.Center,Projectile.Center)<230f&&!n.boss)n.velocity+=n.DirectionTo(Projectile.Center)*.22f;Trail(DustID.GreenTorch,Color.Lime,1.4f);}
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int done)=>target.AddBuff(BuffID.Venom,180);
    public override void OnKill(int timeLeft)=>Effects.Explode(Projectile,100,DustID.GreenTorch);
}

public sealed class PoisonShard : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/PoisonShard";protected override Color Glow=>Color.Lime;
    public override void SetDefaults(){Projectile.width=10;Projectile.height=8;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=1;Projectile.timeLeft=40;}
    public override void AI(){base.AI();Trail(DustID.GreenTorch,Color.Lime);}
}

public sealed class LivingFireBottle : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/FireBottle";protected override Color Glow=>Color.OrangeRed;
    public override void SetDefaults(){Projectile.width=18;Projectile.height=24;Projectile.friendly=true;Projectile.DamageType=DamageClass.Magic;Projectile.penetrate=1;Projectile.timeLeft=150;}
    public override void AI(){Projectile.rotation+=Projectile.velocity.X*.06f;Projectile.velocity.Y+=.25f;Trail(DustID.Torch,Color.Orange,1.1f);}
    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Shatter,Projectile.Center);Effects.Explode(Projectile,48,DustID.Torch);
        if(Projectile.owner!=Main.myPlayer)return;
        if(Projectile.ai[0]==0f)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Projectile.velocity.SafeNormalize(Vector2.UnitX)*9f,ModContent.ProjectileType<FireSnake>(),Projectile.damage,Projectile.knockBack,Projectile.owner);
        else {int spirit=FindOwnedSpirit();if(spirit>=0){Main.projectile[spirit].ai[0]=System.Math.Min(3f,Main.projectile[spirit].ai[0]+1f);Main.projectile[spirit].netUpdate=true;Projectile.NewProjectile(Projectile.GetSource_FromThis(),Main.projectile[spirit].Center,Vector2.Zero,ModContent.ProjectileType<FireNova>(),Projectile.damage,3f,Projectile.owner);}else Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Vector2.Zero,ModContent.ProjectileType<FireSpirit>(),Projectile.damage,Projectile.knockBack,Projectile.owner);}
    }
    private int FindOwnedSpirit(){foreach(Projectile p in Main.ActiveProjectiles)if(p.owner==Projectile.owner&&p.type==ModContent.ProjectileType<FireSpirit>())return p.whoAmI;return-1;}
}

public sealed class FireSnake : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/FireSnake";protected override Color Glow=>Color.OrangeRed;
    public override void SetDefaults(){Projectile.width=42;Projectile.height=22;Projectile.friendly=true;Projectile.DamageType=DamageClass.Magic;Projectile.penetrate=4;Projectile.timeLeft=150;Projectile.tileCollide=false;Projectile.usesLocalNPCImmunity=true;Projectile.localNPCHitCooldown=15;}
    public override void AI(){base.AI();NPC n=FindTarget(500f);if(n!=null)Projectile.velocity=Vector2.Lerp(Projectile.velocity,Projectile.DirectionTo(n.Center)*11f,.055f);Projectile.velocity=Projectile.velocity.RotatedBy((float)System.Math.Sin(Main.GameUpdateCount*.22f)*.018f);Trail(DustID.Torch,Color.OrangeRed,1.3f);}
    public override void OnHitNPC(NPC t,NPC.HitInfo h,int d)=>t.AddBuff(BuffID.OnFire3,240);
}

public sealed class FireSpirit : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/FireSpirit";protected override Color Glow=>Color.OrangeRed;
    public override void SetDefaults(){Projectile.width=42;Projectile.height=42;Projectile.friendly=false;Projectile.timeLeft=600;Projectile.tileCollide=false;Projectile.netImportant=true;}
    public override void AI()
    {
        Player p=Main.player[Projectile.owner];if(!p.active||p.dead){Projectile.Kill();return;}Projectile.localAI[0]+=.045f;Vector2 desired=p.Center+new Vector2(70,0).RotatedBy(Projectile.localAI[0])-Projectile.Center;Projectile.velocity=Vector2.Lerp(Projectile.velocity,desired*.08f,.1f);Projectile.rotation+=.04f;Lighting.AddLight(Projectile.Center,1f,.3f,.05f);
        if(++Projectile.localAI[1]>=System.Math.Max(16,34-(int)Projectile.ai[0]*5)){Projectile.localAI[1]=0;NPC n=FindTarget(600f);if(n!=null&&Projectile.owner==Main.myPlayer)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Projectile.DirectionTo(n.Center)*12f,ModContent.ProjectileType<FireArc>(),Projectile.damage+(int)Projectile.ai[0]*6,2f,Projectile.owner);}
    }
    public override void OnKill(int timeLeft){NPC n=FindTarget(700f);if(n!=null&&Projectile.owner==Main.myPlayer)Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Projectile.DirectionTo(n.Center)*14f,ModContent.ProjectileType<FireSnake>(),Projectile.damage*2,4f,Projectile.owner);}
}

public sealed class FireArc : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/FireArc";protected override Color Glow=>Color.Gold;
    public override void SetDefaults(){Projectile.width=30;Projectile.height=18;Projectile.friendly=true;Projectile.DamageType=DamageClass.Magic;Projectile.penetrate=2;Projectile.timeLeft=70;Projectile.tileCollide=false;}
    public override void AI(){base.AI();Trail(DustID.Torch,Color.Gold,1.2f);}
    public override void OnHitNPC(NPC t,NPC.HitInfo h,int d)=>t.AddBuff(BuffID.OnFire3,180);
}

public sealed class FireNova : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/MagicExplosion";
    public override void SetDefaults(){Projectile.width=60;Projectile.height=60;Projectile.friendly=true;Projectile.DamageType=DamageClass.Magic;Projectile.penetrate=-1;Projectile.timeLeft=20;Projectile.tileCollide=false;Projectile.usesLocalNPCImmunity=true;Projectile.localNPCHitCooldown=20;}
    public override void AI(){Projectile.scale+=.08f;Projectile.alpha+=8;Lighting.AddLight(Projectile.Center,1f,.25f,.05f);}
}

public sealed class RailSlug : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/RailSlug";protected override Color Glow=>Projectile.ai[0]==1f?Color.Cyan:Color.Orange;
    public override void SetDefaults(){Projectile.width=30;Projectile.height=12;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=6;Projectile.timeLeft=100;Projectile.extraUpdates=2;}
    public override void AI(){base.AI();Trail(Projectile.ai[0]==1?DustID.BlueTorch:DustID.Smoke,Glow,1.2f);}
    public override void OnKill(int timeLeft){if(Projectile.ai[0]==1f)Effects.Explode(Projectile,112,DustID.BlueTorch);}
    public override bool PreDraw(ref Color lightColor){if(Projectile.ai[0]!=1f)return true;Texture2D t=ModContent.Request<Texture2D>("Mikhailov/Assets/Effects/RailBeam",AssetRequestMode.ImmediateLoad).Value;Main.EntitySpriteDraw(t,Projectile.Center-Main.screenPosition,null,Color.White,Projectile.rotation,t.Size()/2,Projectile.scale,SpriteEffects.None);return false;}
}

public sealed class ScrewProjectile : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/ScrewBolt";protected override Color Glow=>Projectile.ai[0]==1f?Color.Violet:Color.Gold;
    public override void SetDefaults(){Projectile.width=14;Projectile.height=8;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=1;Projectile.timeLeft=130;Projectile.extraUpdates=1;}
    public override void AI(){base.AI();Projectile.rotation+=.35f;Projectile.velocity*=1.002f;NPC n=FindTarget(Projectile.ai[0]==1?700:420);if(n!=null)Projectile.velocity=Vector2.Lerp(Projectile.velocity,Projectile.DirectionTo(n.Center)*Projectile.velocity.Length(),Projectile.ai[0]==1?.08f:.04f);Trail(DustID.RainbowTorch,Glow,.9f);}
    public override void OnHitNPC(NPC target,NPC.HitInfo hit,int done){if(Projectile.ai[0]!=1||Projectile.owner!=Main.myPlayer)return;NPC next=null;float best=260;foreach(NPC n in Main.ActiveNPCs)if(n.whoAmI!=target.whoAmI&&n.CanBeChasedBy()){float d=Vector2.Distance(n.Center,target.Center);if(d<best){best=d;next=n;}}if(next!=null)Projectile.NewProjectile(Projectile.GetSource_FromThis(),target.Center,target.DirectionTo(next.Center)*18f,ModContent.ProjectileType<ElectricArc>(),Projectile.damage/2,0,Projectile.owner,next.whoAmI);}
}

public sealed class ElectricArc : MikhailovProjectile
{
    public override string Texture=>"Mikhailov/Assets/Effects/ElectricArc";protected override Color Glow=>Color.Violet;
    public override void SetDefaults(){Projectile.width=24;Projectile.height=10;Projectile.friendly=true;Projectile.DamageType=DamageClass.Ranged;Projectile.penetrate=1;Projectile.timeLeft=24;Projectile.tileCollide=false;Projectile.extraUpdates=2;}
    public override void AI(){base.AI();int i=(int)Projectile.ai[0];if(i>=0&&i<Main.maxNPCs&&Main.npc[i].active)Projectile.velocity=Projectile.DirectionTo(Main.npc[i].Center)*18f;Trail(DustID.RainbowTorch,Color.Violet);}
}

internal static class Effects
{
    internal static void Explode(Projectile p,int size,int dust){SoundEngine.PlaySound(SoundID.Item14,p.Center);Vector2 c=p.Center;p.position=c-new Vector2(size/2f);p.width=p.height=size;p.penetrate=-1;p.Damage();for(int i=0;i<24;i++){Dust d=Dust.NewDustDirect(p.position,size,size,dust,Scale:1.5f);d.velocity*=2.5f;d.noGravity=true;}}
}
