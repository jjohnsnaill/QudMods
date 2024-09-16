using Genkit;
using HarmonyLib;
using System.Collections.Generic;
using XRL;
using XRL.Rules;

namespace Mods.MemesOfQud
{
	[HarmonyPatch(typeof(SoundManager))]
	public class SwapSound
	{
		public static Dictionary<string, SoundReplacement> swaps;
		private static long lastPlayed;

		[HarmonyPrefix]
		[HarmonyPatch("PlayWorldSound")]
		static void WorldSound(ref string Clip, int Distance, bool Occluded, ref float VolumeIntensity, Location2D Cell, ref float PitchVariance, ref float Pitch)
		{
			if (Clip != null)
			{
				string extra = Swap(ref Clip, ref VolumeIntensity, ref PitchVariance, ref Pitch);
				if (extra != null)
				{
					SoundManager.PlayWorldSound(extra, Distance, Occluded, VolumeIntensity, Cell, PitchVariance, 0, Pitch);
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("PlayUISound")]
		static void UISound(ref string Clip, ref float Volume, bool Combat, bool Interface, SoundRequest.SoundEffectType Effect)
		{
			if (Clip != null)
			{
				float dummy = 0;
				string extra = Swap(ref Clip, ref Volume, ref dummy, ref dummy);
				if (extra != null)
				{
					SoundManager.PlayUISound(Clip, Volume, Combat, Interface, Effect);
				}
			}
		}

		private static string Swap(ref string clip, ref float volume, ref float pitchVariance, ref float pitch)
		{
			if (clip[0] == '!')
			{
				pitch /= 2;
				pitchVariance /= 2;
				clip = clip.Remove(0);
			}

			if (The.Player != null)
			{
				ConfusionSounds.GetConfusion(The.Player, out bool confused, out bool furious);
				if (furious)
				{
					pitch /= 2;
					pitchVariance /= 2;
				}
				else if (confused)
				{
					clip = "Sounds/StatusEffects/confused";
					return null;
				}
			}

			if (swaps == null)
			{
				swaps = new Dictionary<string, SoundReplacement>()
				{
					["Sounds/Damage/sfx_damage_critical"] = new SoundReplacement("critical1", "critical2", "critical3", "critical4", "critical5"),

					["Sounds/Creatures/VO/sfx_humanoid_generic_vo_die"] = new SoundReplacement("humanDie1", "humanDie2"),
					["Sounds/Creatures/VO/sfx_robot_generic_vo_die"] = new SoundReplacement("botDie1", "botDie2"),
					["Sounds/Creatures/VO/sfx_ooze_generic_vo_die"] = new SoundReplacement("oozeDie1", "oozeDie2"),

					["Sounds/Abilities/sfx_ability_charge"] = new SoundReplacement("charge1", "charge2", "charge3"),

					["Sounds/Foley/fly_tileMove_water_wade"] = new SoundReplacement("splash"),
					["Sounds/Foley/fly_tileMove_water_swim"] = new SoundReplacement("splash"),

					["Sounds/StatusEffects/sfx_statusEffect_physicalRupture"] = new SoundReplacement("breakage"),
					["Sounds/StatusEffects/sfx_statusEffect_mechanicalRupture"] = new SoundReplacement("breakage"),

					["Sounds/Missile/Fires/Bows/sfx_missile_bow_fire"] = new SoundReplacement("bow"),
					["Sounds/Missile/Fires/Bows/sfx_missile_electroBow_fire"] = new SoundReplacement("bow"),
					["Sounds/Missile/Fires/Bows/sfx_missile_turbow_fire"] = new SoundReplacement("bow"),

					["hiss_high"] = new SoundReplacement("hiss_low"),
					["Sounds/Abilities/sfx_ability_cryokinesis_active"] = new SoundReplacement("hiss_low"),

					["Sounds/StatusEffects/sfx_statusEffect_frozen"] = new SoundReplacement("Sounds/StatusEffects/frozen"),
					["Sounds/StatusEffects/sfx_statusEffect_poisoned"] = new SoundReplacement("Sounds/StatusEffects/poison"),
					["Sounds/StatusEffects/sfx_statusEffect_fungal"] = new SoundReplacement("Sounds/StatusEffects/bruh"),
					["Sounds/StatusEffects/sfx_statusEffect_rusted"] = new SoundReplacement("Sounds/StatusEffects/rust", 1),
					["Sounds/StatusEffects/sfx_statusEffect_charm"] = new SoundReplacement("Sounds/StatusEffects/charm"),
					["Sounds/StatusEffects/sfx_statusEffect_robotBeep"] = new SoundReplacement("turret1", "turret2"),

					["Sounds/Enhancements/sfx_enhancement_electric_conductiveJump"] = new SoundReplacement("spark1", "spark2"),

					["Sounds/Interact/sfx_interact_door_wood_open"] = new SoundReplacement("doorOpen"),
					["Sounds/Interact/sfx_interact_door_wood_close"] = new SoundReplacement("doorClose"),
					["Sounds/Interact/sfx_interact_door_metal_open"] = new SoundReplacement("metalDoorOpen"),
					["Sounds/Interact/sfx_interact_door_metal_close"] = new SoundReplacement("metalDoorClose"),

					["Sounds/Interact/sfx_interact_liquidContainer_pourout"] = new SoundReplacement("pour"),
					["Sounds/Abilities/sfx_ability_generic_waterPour"] = new SoundReplacement("pour"),

					["Sounds/Abilities/sfx_ability_jump"] = new SoundReplacement("Sounds/Abilities/jump"),

					["Sounds/Abilities/sfx_ability_cudgel_slam"] = new SoundReplacement("Sounds/Abilities/slam"),

					["Sounds/Abilities/sfx_ability_gasMutation_passiveRelease"] = new SoundReplacement("Sounds/Abilities/gas"),

					["Sounds/Abilities/sfx_ability_mutation_evilTwin_spawn"] = new SoundReplacement("imposter", 0.75f),

					["Sounds/Abilities/sfx_ability_spitSlime_spit"] = new SoundReplacement("Sounds/Abilities/spit1", "Sounds/Abilities/spit2", "Sounds/Abilities/spit3"),
					["Sounds/Abilities/sfx_ability_mutation_flamingRay_attack"] = new SoundReplacement("Sounds/Abilities/burn1", "Sounds/Abilities/burn2"),
					["Sounds/Abilities/sfx_ability_mutation_lightManipulation_laser_fire"] = new SoundReplacement("Sounds/Abilities/lightManipulation"),
					["Sounds/Abilities/sfx_ability_mutation_disintegration_disintegrate"] = new SoundReplacement("Sounds/Abilities/disintegrate"),
					["Sounds/Abilities/sfx_ability_mutation_stunning_force"] = new SoundReplacement("Sounds/Abilities/stunningForce"),
					["Sounds/Abilities/sfx_ability_mutation_burgeoning_plantGrow"] = new SoundReplacement("Sounds/Abilities/burgeoning"),

					["Sounds/Abilities/sfx_ability_mutation_timeDilation_activate"] = new SoundReplacement("Sounds/Abilities/timeDilation", 1),
					["Sounds/Abilities/sfx_ability_mutation_timeDilation_deactivate"] = new SoundReplacement("Sounds/Abilities/timeDilationOff", 1),

					["Sounds/Abilities/sfx_ability_sunderMind_attack"] = new SoundReplacement("Sounds/Abilities/sunderMindStart", 1),
					["Sounds/Abilities/sfx_ability_sunderMind_dig"] = new SoundReplacement("Sounds/Abilities/sunderMind", 1),
					["Sounds/Abilities/sfx_ability_sunderMind_final"] = new SoundReplacement("Sounds/Abilities/sunderMindEnd", 1),

					["Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_windup"] = new SoundReplacement("Sounds/Abilities/irisdualBeamWindup"),
					["Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_attack"] = new SoundReplacement("Sounds/Abilities/irisdualBeam"),

					["Sounds/Misc/sfx_quest_gain"] = new SoundReplacement(""),
					["Sounds/Misc/sfx_quest_total_fail"] = new SoundReplacement(""),

					["Sounds/UI/ui_notification_death"] = new SoundReplacement(""),

					["Sounds/Abilities/sfx_ability_mutation_psychometry_activate"] = new SoundReplacement("startup", 0.75f),
					["startup"] = new SoundReplacement("startup", 0.75f),

					["whine_up"] = new SoundReplacement("whine_up", 0.75f),
					["completion"] = new SoundReplacement("whine_up", 0.75f),

					["whine_down"] = new SoundReplacement("whine_down", 0.75f),
					["shutdown"] = new SoundReplacement("whine_down", 0.75f),

					["Clink1"] = new SoundReplacement("tinker1", "tinker2", "tinker3"),
					["Clink2"] = new SoundReplacement("tinker1", "tinker2", "tinker3"),
					["Clink3"] = new SoundReplacement("tinker1", "tinker2", "tinker3"),
				};
			}

			if (swaps.TryGetValue(clip, out var swap))
			{
				string[] paths = swap.paths;
				clip = paths.Length > 1 ? paths[Stat.Rnd2.Next(paths.Length)] : paths[0];
				if (swap.volume > 0)
				{
					volume = swap.volume;
				}
				return null;
			}

			if (clip.EndsWith("_hurt"))
			{
				switch (Stat.Rnd2.Next(5))
				{
					case 0:
						clip = "hit1";
						break;
					case 1:
						clip = "hit2";
						break;
					case 2:
						clip = "hit3";
						break;
					case 3:
						clip = "hit4";
						break;
					case 4:
						clip = "hit5";
						break;
				}
			}
			else if (clip.EndsWith("_die"))
			{
				switch (Stat.Rnd2.Next(4))
				{
					case 0:
						clip = "die1";
						break;
					case 1:
						clip = "die2";
						break;
					case 2:
						clip = "die3";
						break;
					case 3:
						clip = "die4";
						break;
				}
			}
			else if (clip == "Sounds/Abilities/sfx_ability_pyrokinesis_active")
			{
				if (lastPlayed == The.Game.Turns)
				{
					return null;
				}
				lastPlayed = The.Game.Turns;

				clip = Stat.Rnd2.Next(2) == 0 ? "Sounds/Abilities/burn1" : "Sounds/Abilities/burn2";
			}
			else if (clip == "Sounds/Abilities/sfx_ability_forcefield_create")
			{
				if (lastPlayed == The.Game.Turns)
				{
					return null;
				}
				lastPlayed = The.Game.Turns;

				switch (Stat.Rnd2.Next(4))
				{
					case 0:
						clip = "Sounds/Abilities/force1";
						break;
					case 1:
						clip = "Sounds/Abilities/force2";
						break;
					case 2:
						clip = "Sounds/Abilities/force3";
						break;
					case 3:
						clip = "Sounds/Abilities/force4";
						break;
				}
			}
			else if (clip.StartsWith("Sounds/Abilities/sfx_ability_teleport"))
			{
				clip = "Sounds/Abilities/teleport";
			}
			else if (clip.StartsWith("Sounds/Abilities/sfx_ability_spacetimeVortex"))
			{
				clip = "Sounds/Abilities/spacetimeVortex";
			}
			else if (clip == "compartment_close_whine_up")
			{
				clip = "compartment_close";
				return "whine_up";
			}
			else if (clip == "compartment_open_whine_down")
			{
				clip = "compartment_open";
				return "whine_down";
			}

			return null;
		}
	}

	public struct SoundReplacement
	{
		public string[] paths;
		public float volume;

		public SoundReplacement(string path)
		{
			paths = new string[] { path };
			volume = 0;
		}

		public SoundReplacement(params string[] paths)
		{
			this.paths = paths;
			volume = 0;
		}

		public SoundReplacement(string path, float volume)
		{
			paths = new string[] { path };
			this.volume = volume;
		}

		public SoundReplacement(float volume, params string[] paths)
		{
			this.paths = paths;
			this.volume = volume;
		}
	}
}