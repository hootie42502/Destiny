using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Destiny.Content.Items
{
    public class OrbOfLight : ModItem
    {


        public override void SetDefaults() {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
            Item.consumable = true;
        }

        public override bool OnPickup(Player player) {
            // Apply a temporary buff when the player picks up the orb
            player.AddBuff(BuffID.Shine, 600); // Example: Shine buff for 10 seconds (600 ticks)
            return false; // Returning false ensures the item is not added to the player's inventory
        }

        public override void PostUpdate() {
            Lighting.AddLight(Item.Center, 0.8f, 0.8f, 0.2f); // Add light to the item when it's on the ground
        }
    }
}