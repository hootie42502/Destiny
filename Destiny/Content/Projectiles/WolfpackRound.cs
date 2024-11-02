
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace Destiny.Content.Projectiles
{
	// This Example show how to implement simple homing projectile
	// Can be tested with ExampleCustomAmmoGun
	public class WolfpackRound : ModProjectile
	{

        private int homingDelay = 15;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.Explosive[Type] = true;
            ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
		}

		// Setting the default parameters of the projectile
		// You can check most of Fields and Properties here https://github.com/tModLoader/tModLoader/wiki/Projectile-Class-Documentation
		public override void SetDefaults() {
			Projectile.width = 1; // The width of projectile hitbox
			Projectile.height = 1; // The height of projectile hitbox

			Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
			Projectile.DamageType = DamageClass.Ranged; // What type of damage does this projectile affect?
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.light = 1f; // How much light emit around the projectile
			Projectile.tileCollide = true; // Can the projectile collide with tiles?
			Projectile.timeLeft = 120; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
		
            // Set random velocity when spawned
            float speedX = Main.rand.NextFloat(-3f, 3f); // Random value between -3 and 3 for X direction
            float speedY = Main.rand.NextFloat(-3f, 3f); // Random value between -3 and 3 for Y direction
            Projectile.velocity = new Vector2(speedX, speedY); // Apply random velocity
        }

		// Custom AI
		public override void AI() {
			float maxDetectRadius = 400f; // The maximum radius at which a projectile can detect a target
			float projSpeed = 30f; // The speed at which the projectile moves towards the target
            // Countdown delay before homing starts
			if (homingDelay > 0) {
				homingDelay--;
				return; // Exit AI until the delay is over
			}
            // Apply a slight drift to make the trajectory curve
        float curveFactor = 0.1f; // Adjust this value to increase or decrease the curve
        Projectile.velocity = Projectile.velocity.RotatedBy(curveFactor);

        // Homing logic after delay
        NPC closestNPC = FindClosestNPC(maxDetectRadius);
        if (closestNPC == null)
            return;

        // Calculate direction to target
        Vector2 direction = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

        // Gradually adjust the velocity toward the target to create a more curved approach
        float turnResistance = 7f; // The higher this value, the slower it turns toward the target
        Projectile.velocity = ((Projectile.velocity * (turnResistance - 1) + direction * projSpeed) / turnResistance).RotatedByRandom(MathHelper.ToRadians(30));

        // Update the projectile's rotation to match its velocity
        Projectile.rotation = Projectile.velocity.ToRotation();

       for (int i = 0; i < 2; i++) {
						float posOffsetX = 0f;
						float posOffsetY = 0f;
						if (i == 1) {
							posOffsetX = Projectile.velocity.X * 0.5f;
							posOffsetY = Projectile.velocity.Y * 0.5f;
						}

						// Spawn fire dusts at the back of the rocket.
						Dust fireDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + posOffsetX, Projectile.position.Y + 3f + posOffsetY) - Projectile.velocity * 0.5f,
							Projectile.width - 8, Projectile.height - 8, DustID.BlueFairy, 0f, 0f, 100);
						fireDust.scale *= 2f + Main.rand.Next(10) * 0.1f;
						fireDust.velocity *= 0.2f;
						fireDust.noGravity = true;

						// Used by the liquid rockets which leave trails of their liquid instead of fire.
						// if (fireDust.type == Dust.dustWater()) {
						//	fireDust.scale *= 0.65f;
						//	fireDust.velocity += Projectile.velocity * 0.1f;
						// }

						// Spawn smoke dusts at the back of the rocket.
						Dust smokeDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + posOffsetX, Projectile.position.Y + 3f + posOffsetY) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.BlueTorch, 0f, 0f, 100, default, 0.5f);
						smokeDust.fadeIn = 1f + Main.rand.Next(5) * 0.1f;
						smokeDust.velocity *= 0.05f;

       }
			/*// Trying to find NPC closest to the projectile
			NPC closestNPC = FindClosestNPC(maxDetectRadius);
            
			if (closestNPC == null)
				return;

			// If found, change the velocity of the projectile and turn it in the direction of the target
			// Use the SafeNormalize extension method to avoid NaNs returned by Vector2.Normalize when the vector is zero
			Projectile.velocity =  (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * projSpeed;
			Projectile.rotation = Projectile.velocity.ToRotation();*/
		}

		// Finding the closest NPC to attack within maxDetectDistance range
		// If not found then returns null
		public NPC FindClosestNPC(float maxDetectDistance) {
			NPC closestNPC = null;

			// Using squared values in distance checks will let us skip square root calculations, drastically improving this method's speed.
			float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

			// Loop through all NPCs(max always 200)
			for (int k = 0; k < Main.maxNPCs; k++) {
				NPC target = Main.npc[k];
				// Check if NPC able to be targeted. It means that NPC is
				// 1. active (alive)
				// 2. chaseable (e.g. not a cultist archer)
				// 3. max life bigger than 5 (e.g. not a critter)
				// 4. can take damage (e.g. moonlord core after all it's parts are downed)
				// 5. hostile (!friendly)
				// 6. not immortal (e.g. not a target dummy)
				if (target.CanBeChasedBy()) {
					// The DistanceSquared function returns a squared distance between 2 points, skipping relatively expensive square root calculations
					float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);

					// Check if it is within the radius
					if (sqrDistanceToTarget < sqrMaxDetectDistance) {
						sqrMaxDetectDistance = sqrDistanceToTarget;
						closestNPC = target;
					}
				}
			}

			return closestNPC;
		}

        public override void OnKill(int timeLeft) {
			// Vanilla code takes care ensuring that in For the Worthy or Get Fixed Boi worlds the blast can damage other players because
			// this projectile is ProjectileID.Sets.Explosive[Type] = true;. It also takes care of hurting the owner. The Projectile.PrepareBombToBlow
			// and Projectile.HurtPlayer methods can be used directly if needed for a projectile not using ProjectileID.Sets.Explosive

			// Play an exploding sound.
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

			// Resize the projectile again so the explosion dust and gore spawn from the middle.
			// Rocket I: 22, Rocket III: 80, Mini Nuke Rocket: 50
			Projectile.Resize(22, 22);

			// Spawn a bunch of smoke dusts.
			for (int i = 0; i < 3; i++) {
				Dust smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
				smokeDust.velocity *= 1.4f;
			}

			// Spawn a bunch of fire dusts.
			for (int j = 0; j < 2; j++) {
				Dust fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
				fireDust.noGravity = true;
				fireDust.velocity *= 7f;
				fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
				fireDust.velocity *= 3f;
			}

			// Spawn a bunch of smoke gores.
			for (int k = 0; k < 1; k++) {
				float speedMulti = 0.4f;
				if (k == 1) {
					speedMulti = 0.8f;
				}

				Gore smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity += Vector2.One;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity.X -= 1f;
				smokeGore.velocity.Y += 1f;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity.X += 1f;
				smokeGore.velocity.Y -= 1f;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity -= Vector2.One;
			}
        }
	}
}