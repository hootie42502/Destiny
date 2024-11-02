using Destiny.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Destiny.Content.Projectiles
{
	public class KhvostovBullet2 : ModProjectile
	{
		private int ricochetCount = 0; // Counter for ricochets
		private const int MaxRicochets = 6; // Maximum number of ricochets

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; 
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0; 
		}

		public override void SetDefaults() {
			Projectile.width = 1; 
			Projectile.height = 1; 
			Projectile.aiStyle = 1; 
			Projectile.friendly = true; 
			Projectile.hostile = false; 
			Projectile.DamageType = DamageClass.Ranged; 
			Projectile.penetrate = MaxRicochets + 1; // The number of penetrations is based on max ricochets
			Projectile.timeLeft = 600; 
			Projectile.alpha = 255; 
			Projectile.light = 0.5f; 
			Projectile.ignoreWater = true; 
			Projectile.tileCollide = false; 
			Projectile.extraUpdates = 1; 
			AIType = ProjectileID.Bullet; 
		}
/*
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}

			return true;
		}
*/
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
           
		}

		// This method is called when the projectile hits an NPC
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (ricochetCount < MaxRicochets) {
				ricochetCount++;
				RicochetToNextTarget(target);
			} else {
				Projectile.Kill(); // Destroy the projectile after the max ricochets
			}
		}

		private void RicochetToNextTarget(NPC currentTarget) {
			// Find the nearest NPC to the current target, excluding the one we just hit
			NPC closestNPC = Main.npc
				.Where(n => n.active && !n.friendly && n != currentTarget && n.Distance(Projectile.Center) < 500f)
				.OrderBy(n => n.Distance(Projectile.Center))
				.FirstOrDefault();

			// If we found an NPC to ricochet to, change the projectile's direction
			if (closestNPC != null) {
				Vector2 direction = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
				Projectile.velocity = direction * Projectile.velocity.Length(); // Maintain the same speed but change direction
				Projectile.netUpdate = true; // Sync with multiplayer
			}
			else {
				Projectile.Kill(); // If no valid NPC is found, destroy the projectile
			}
		}
	}
}