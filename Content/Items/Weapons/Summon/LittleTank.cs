using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Items;
using Terraria.ID;
using Terraria.ModLoader;

namespace TankSummoner.Content.Items.Weapons.Summon
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class LittleTank : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.TankSummoner.hjson' file.
		public override void SetDefaults()
		{
			Item.damage = 4;
			Item.DamageType = DamageClass.Summon;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.knockBack = 8;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item10;
			Item.autoReuse = true;
			Item.mana = 1;
			Item.noMelee = true;
			Item.buffType = ModContent.BuffType<Buffs.Minions.LittleTankBuff>();
			Item.shoot = ModContent.ProjectileType<Projectiles.Minions.LittleTankMinion>();
		}

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.position;
			// position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			player.AddBuff(Item.buffType,4);
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            // Projectile.NewProjectile()返回int;
			// Projectile.NewProjectileDirect()返回Projectile;
			projectile.originalDamage = Item.damage;
			return false;
        }

        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Wood, 10);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 6);
			recipe.AddIngredient(ItemID.FallenStar, 1);
			recipe.AddIngredient(ItemID.Shackle, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
