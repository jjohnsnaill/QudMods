using Genkit;
using HarmonyLib;
using XRL;
using XRL.Rules;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(SoundManager))]
	public class SwapSound
	{
		private static long lastPlayed;

		[HarmonyPatch("PlayWorldSound")]
		static void Prefix(ref string Clip, int Distance, bool Occluded, ref float VolumeIntensity, Location2D Cell, ref float PitchVariance, ref float Pitch)
		{
			if (Clip == null)
			{
				return;
			}

			if (Clip[0] == '!')
			{
				Pitch /= 2;
				PitchVariance /= 2;
				Clip = Clip.Remove(0);
			}

			if (The.Player != null)
			{
				ConfusionSounds.GetConfusion(The.Player, out bool confused, out bool furious);
				if (furious)
				{
					Pitch /= 2;
					PitchVariance /= 2;
				}
				else if (confused)
				{
					Clip = "Sounds/StatusEffects/confused";
					return;
				}
			}

			if (Clip == "Sounds/Damage/sfx_damage_critical")
			{
				switch (Stat.Rnd2.Next(5))
				{
					case 0:
						Clip = "critical1";
						break;
					case 1:
						Clip = "critical2";
						break;
					case 2:
						Clip = "critical3";
						break;
					case 3:
						Clip = "critical4";
						break;
					case 4:
						Clip = "critical5";
						break;
				}
			}
			else if (Clip.EndsWith("_hurt"))
			{
				switch (Stat.Rnd2.Next(5))
				{
					case 0:
						Clip = "hit1";
						break;
					case 1:
						Clip = "hit2";
						break;
					case 2:
						Clip = "hit3";
						break;
					case 3:
						Clip = "hit4";
						break;
					case 4:
						Clip = "hit5";
						break;
				}
			}
			else if (Clip == "Sounds/Creatures/VO/sfx_humanoid_generic_vo_die")
			{
				Clip = Stat.Rnd2.Next(2) == 0 ? "humanDie1" : "humanDie2";
			}
			else if (Clip == "Sounds/Creatures/VO/sfx_robot_generic_vo_die")
			{
				Clip = Stat.Rnd2.Next(2) == 0 ? "botDie1" : "botDie2";
			}
			else if (Clip == "Sounds/Creatures/VO/sfx_ooze_generic_vo_die")
			{
				Clip = Stat.Rnd2.Next(2) == 0 ? "oozeDie1" : "oozeDie2";
			}
			else if (Clip.EndsWith("_die") || Clip.StartsWith("death")) //TODO: remove last condition
			{
				switch (Stat.Rnd2.Next(4))
				{
					case 0:
						Clip = "die1";
						break;
					case 1:
						Clip = "die2";
						break;
					case 2:
						Clip = "die3";
						break;
					case 3:
						Clip = "die4";
						break;
				}
			}
			else if (Clip == "Sounds/Foley/fly_tileMove_water_wade" || Clip == "Sounds/Foley/fly_tileMove_water_swim")
			{
				Clip = "splash";
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_physicalRupture" || Clip == "Sounds/StatusEffects/sfx_statusEffect_mechanicalRupture")
			{
				Clip = "breakage";
			}
			else if (Clip == "Clink1" || Clip == "Clink2" || Clip == "Clink3")
			{
				switch (Stat.Rnd2.Next(3))
				{
					case 0:
						Clip = "tinker1";
						break;
					case 1:
						Clip = "tinker2";
						break;
					case 2:
						Clip = "tinker3";
						break;
				}
			}
			else if (Clip == "Sounds/Enhancements/sfx_enhancement_electric_conductiveJump")
			{
				Clip = Stat.Rnd2.Next(2) == 0 ? "spark1" : "spark2";
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_robotBeep")
			{
				Clip = Stat.Rnd2.Next(2) == 0 ? "turret1" : "turret2";
			}
			else if (Clip == "Sounds/Missile/Fires/Bows/sfx_missile_bow_fire" || Clip == "Sounds/Missile/Fires/Bows/sfx_missile_electroBow_fire" || Clip == "Sounds/Missile/Fires/Bows/sfx_missile_turbow_fire")
			{
				Clip = "bow";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_flamingRay_attack" || Clip == "Sounds/Abilities/sfx_ability_pyrokinesis_active")
			{
				if (lastPlayed == The.Game.Turns)
				{
					return;
				}
				lastPlayed = The.Game.Turns;

				Clip = Stat.Rnd2.Next(2) == 0 ? "Sounds/Abilities/burn1" : "Sounds/Abilities/burn2";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_cryokinesis_active" || Clip == "hiss_high") //TODO: remove last condition
			{
				Clip = "hiss_low";
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_frozen")
			{
				Clip = "Sounds/StatusEffects/frozen";
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_poisoned")
			{
				Clip = "Sounds/StatusEffects/poison";
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_fungal")
			{
				Clip = "Sounds/StatusEffects/bruh";
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_rusted")
			{
				Clip = "Sounds/StatusEffects/rust";
				VolumeIntensity = 1;
			}
			else if (Clip == "Sounds/StatusEffects/sfx_statusEffect_charm")
			{
				Clip = "Sounds/StatusEffects/charm";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_lightManipulation_laser_fire")
			{
				Clip = "Sounds/Abilities/lightManipulation";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_disintegration_disintegrate")
			{
				Clip = "Sounds/Abilities/disintegrate";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_burgeoning_plantGrow")
			{
				Clip = "Sounds/Abilities/burgeoning";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_forcefield_create")
			{
				if (lastPlayed == The.Game.Turns)
				{
					return;
				}
				lastPlayed = The.Game.Turns;

				switch (Stat.Rnd2.Next(4))
				{
					case 0:
						Clip = "Sounds/Abilities/force1";
						break;
					case 1:
						Clip = "Sounds/Abilities/force2";
						break;
					case 2:
						Clip = "Sounds/Abilities/force3";
						break;
					case 3:
						Clip = "Sounds/Abilities/force4";
						break;
				}
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_psychometry_activate")
			{
				Clip = "startup";
				VolumeIntensity = 0.75f;
			}
			else if (Clip == "startup" || Clip == "whine_up" || Clip == "whine_down")
			{
				VolumeIntensity = 0.75f;
			}
			else if (Clip == "completion")
			{
				Clip = "whine_up";
				VolumeIntensity = 0.75f;
			}
			else if (Clip == "shutdown")
			{
				Clip = "whine_down";
				VolumeIntensity = 0.75f;
			}
			else if (Clip == "compartment_close_whine_up")
			{
				Clip = "compartment_close";
				SoundManager.PlayWorldSound("whine_up", Distance, Occluded, VolumeIntensity, Cell, PitchVariance, 0, Pitch);
			}
			else if (Clip == "compartment_open_whine_down")
			{
				Clip = "compartment_open";
				SoundManager.PlayWorldSound("whine_down", Distance, Occluded, VolumeIntensity, Cell, PitchVariance, 0, Pitch);
			}
			else if (Clip.StartsWith("Sounds/Abilities/sfx_ability_teleport"))
			{
				Clip = "Sounds/Abilities/teleport";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_timeDilation_activate")
			{
				Clip = "Sounds/Abilities/timeDilation";
				VolumeIntensity = 1;
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_mutation_timeDilation_deactivate")
			{
				Clip = "Sounds/Abilities/timeDilationOff";
				VolumeIntensity = 1;
			}
			else if (Clip.StartsWith("Sounds/Abilities/sfx_ability_spacetimeVortex"))
			{
				Clip = "Sounds/Abilities/spacetimeVortex";
			}
			else if (Clip == "Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_windup")
			{
				Clip = "Sounds/Abilities/irisdualBeamWindup";
			}
			else if (Clip == "Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_attack")
			{
				Clip = "Sounds/Abilities/irisdualBeam";
			}
			else if (Clip == "Sounds/Abilities/sfx_ability_spitSlime_spit")
			{
				switch (Stat.Rnd2.Next(3))
				{
					case 0:
						Clip = "Sounds/Abilities/spit1";
						break;
					case 1:
						Clip = "Sounds/Abilities/spit2";
						break;
					case 2:
						Clip = "Sounds/Abilities/spit3";
						break;
				}
			}
		}
	}
}