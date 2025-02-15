using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using TankSummoner.Content.Buffs.Minions;

namespace TankSummoner.Content.Projectiles.Minions
{
    public class LittleTankMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //Main.projFrames[Type] = 1;
			Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionShot[Type] = false;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 32;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.DamageType = DamageClass.Summon;
            // Projectile.aiStyle = -1;    // 0 为静止不动
            Projectile.light = 0.1f;
            Projectile.penetrate = -1;   // 穿透数
            // Projectile.tileCollide = true;
            // Projectile.friendly = true;
            // Projectile.timeLeft = 3600;
            base.SetDefaults();
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public override bool? CanCutTiles()
        {
            return true;
        }
 
        // readonly float baseAcceleration = 1f;

        // readonly float maxSpeed = 10f;
		
        // NPC WaitForTarget(Player owner)
        // {
        //     float idealSpeed;
        //     float idealAcceration;
        //     var idealDisplacement = Projectile.position - owner.Center;
        //     if (idealDisplacement.Length() <= 480f)
        //     {
        //         idealSpeed = 0;
        //         idealAcceration = baseAcceleration;
        //     }
        //     else
        //     {
        //         idealSpeed = maxSpeed;
        //         idealAcceration = 2 * baseAcceleration;
        //     }
        //     float nowSpeed = Projectile.velocity.Length();
        //     idealDisplacement.Normalize();
        //     Projectile.velocity = idealDisplacement * nowSpeed * (1 + idealAcceration / (idealSpeed - nowSpeed));

        //     NPC target = null;
        //     int targetID = Projectile.FindTargetWithLineOfSight();
        //     if (targetID != -1)
        //         target = Main.npc[targetID];
        //     return target;
        // }

        // void Shoot(Player owner, NPC target)
        // {
        //     if (target != null)
        //     {
        //         var direction = target.position - Projectile.position;
        //         //……………………………………………………
        //     }
        // }

        // void Move(Player owner, NPC target)
        // {
        //     if (target != null)
        //     {
        //         if ((target.position - owner.position).Length() <= 1800)
        //         {
        //             float idealSpeed = maxSpeed;
        //             float idealAcceration;
        //             if ((target.position - Projectile.position).Length() <= 600)
        //             {
        //                 idealAcceration = baseAcceleration;
        //             }
        //             else
        //             {
        //                 idealAcceration = 3 * baseAcceleration;
        //             }
        //         }
        //     }
        // }

        // void Visuals()
        // {
        //     if (Projectile.velocity.X > 0)
        //         Projectile.reflected = true;
        //     else
        //         Projectile.reflected = false;
        // }

        // public override void AI()
        // {
        //     Player owner = Main.player[Projectile.owner];

        //     // if (owner.HasBuff<LittleTankBuff>())
        //     // {
        //     //     Projectile.timeLeft = 3600;
        //     // }
        //     // else
        //     //     Projectile.timeLeft = 0;

        //     NPC target = WaitForTarget(owner);
        //     Shoot(owner, target);
        //     Move(owner, target);
        //     // Visuals();
        // }

        // The AI of this minion is split into multiple methods to avoid bloat. This method just passes values between calls actual parts of the AI.
		public override void AI() {
			Player owner = Main.player[Projectile.owner];

			if (!CheckActive(owner)) {
				return;
			}

			GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
			Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
			Visuals();
		}

		// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
		private bool CheckActive(Player owner) {
			if (owner.dead || !owner.active) {
				owner.ClearBuff(ModContent.BuffType<LittleTankBuff>());

				return false;
			}

			if (owner.HasBuff(ModContent.BuffType<LittleTankBuff>())) {
				Projectile.timeLeft = 3600;
			}

			return true;
		}

		private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition) {
			Vector2 idlePosition = owner.Center;
			idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos
			float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -owner.direction;
			idlePosition.X += minionPositionOffsetX; // Go behind the player

			// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

			// Teleport to player if distance is too big
			vectorToIdlePosition = idlePosition - Projectile.Center;
			distanceToIdlePosition = vectorToIdlePosition.Length();

			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;

			// Fix overlap with other minions
			foreach (var other in Main.ActiveProjectiles) {
				if (other.whoAmI != Projectile.whoAmI && other.owner == Projectile.owner && System.Math.Abs(Projectile.position.X - other.position.X) + System.Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) {
						Projectile.velocity.X -= overlapVelocity;
					}
					else {
						Projectile.velocity.X += overlapVelocity;
					}

					if (Projectile.position.Y < other.position.Y) {
						Projectile.velocity.Y -= overlapVelocity;
					}
					else {
						Projectile.velocity.Y += overlapVelocity;
					}
				}
			}
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter) {
			// Starting search distance
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = false;

			// This code is required if your minion weapon has the targeting feature
			if (owner.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);

				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 2000f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					foundTarget = true;
				}
			}

			if (!foundTarget) {
				// This code is required either way, used for finding a target
				foreach (var npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;

						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			Projectile.friendly = foundTarget;
		}

		private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition) {
			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 20f;

			if (foundTarget) {
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f) {
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - Projectile.Center;
					direction.Normalize();
					direction *= speed;

					Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				}
			}
			else {
				// Minion doesn't have a target: return to player and idle
				if (distanceToIdlePosition > 600f) {
					// Speed up the minion if it's away from the player
					speed = 12f;
					inertia = 60f;
				}
				else {
					// Slow down the minion if closer to the player
					speed = 4f;
					inertia = 80f;
				}

				if (distanceToIdlePosition > 20f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				}
				else if (Projectile.velocity == Vector2.Zero) {
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity.X = -0.15f;
					Projectile.velocity.Y = -0.05f;
				}
			}
		}

		private void Visuals() {
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;

            // 按速度方向翻转
            Projectile.spriteDirection = Projectile.direction;

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);

		}
    }
}