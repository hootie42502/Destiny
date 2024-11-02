using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
namespace Destiny.Content.Items
{
    public class TheLastWord : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 20; // Set base damage
        Item.DamageType = DamageClass.Ranged; // Make it a ranged weapon
        Item.width = 40; // Weapon size
        Item.height = 20;
        Item.useTime = 20; // How fast the weapon is used
        Item.useAnimation = 20; 
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 2;
        Item.value = 10000;
        Item.rare = ItemRarityID.Yellow;
        Item.UseSound = SoundID.Item11; // Pistol sound
        Item.autoReuse = true; // Prevent auto-fire
        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 16f; // Bullet speed
        Item.useAmmo = AmmoID.Bullet;
        Item.scale = 0.05f;
    }

    public override bool AltFunctionUse(Player player) // Enable right-click functionality
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2) // Right-click
        {
            Item.useTime = 8; // Faster fire rate for barrage
            Item.useAnimation = 64;
            Item.reuseDelay = 68; // Small delay between bursts
            Item.shootSpeed = 10f; // Lower speed for more spread
            Item.UseSound = SoundID.Item40; // Different sound for barrage
            Item.autoReuse = false;
        }
        else // Left-click (normal shot)
        {
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.shootSpeed = 16f;
            Item.reuseDelay = 20;
            Item.UseSound = SoundID.Item11; 
            Item.autoReuse = true;
        }
        return base.CanUseItem(player);
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(-10, 0);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack)
    {
        if (player.altFunctionUse == 2) // Right-click (barrage)
        {
            float numberProjectiles = 1; // 3, 4, or 5 shots
			

			position += Vector2.Normalize(velocity) * 45f;

			for (int i = 0; i < numberProjectiles; i++) {
				Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(18)); // Watch out for dividing by 0 if there is only 1 projectile.
				Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockBack, player.whoAmI);
			}
            return false; // Don't shoot a single bullet
        }
        return true; // Normal shot on left click
    }
}
}
