using TankSummoner.Content.Projectiles.Minions;
using Terraria;
// using Terraria.ID;
using Terraria.ModLoader;

namespace TankSummoner.Content.Buffs.Minions
{
    public class LittleTankBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            int NumbersOfTank = player.ownedProjectileCounts[ModContent.ProjectileType<LittleTankMinion>()];
            player.statDefense += (4 + player.statDefense/100) * NumbersOfTank;
            
            if (NumbersOfTank >= 1)
                player.buffTime[buffIndex] = 4;
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}