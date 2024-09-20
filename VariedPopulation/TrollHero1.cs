using Qud.API;
using System;
using System.Collections.Generic;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ObjectBuilders
{
	public class TrollHero1 : IObjectBuilder
	{
		public override void Apply(GameObject Object, string Context)
		{
			string title = NameMaker.MakeTitle(Object, null, null, null, null, null, null, null, null, null, "Hero", null, SpecialFaildown: true);
			Object.GiveProperName(null, Force: false, "Hero", SpecialFaildown: true);
			Object.RequirePart<HeroIconColor>().Configure("&M", "&M", null, null, null, HeroMaker.ICON_COLOR_PRIORITY, HeroMaker.ICON_COLOR_PRIORITY, HeroMaker.ICON_COLOR_PRIORITY);
			Object.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
			Object.RequirePart<Titles>().AddTitle(title, -40);

			int tier = ZoneManager.zoneGenerationContextTier;

			switch (title)
			{
				case "Who Duets with Flame":
					Object.Render.Tile = "Creatures/elementaltroll.png";
					Object.Render.DetailColor = "o";

					Object.GetStat("Strength").BoostStat(1);
					Object.GetStat("Agility").BoostStat(1);

					Object.GetStat("HeatResistance").BaseValue = 200;

					switch (Stat.Rnd.Next(6))
					{
						case 0:
							Object.RequirePart<Mutations>().AddMutation(new FlamingRay(), 10);
							break;
						case 1:
							Object.RequirePart<Mutations>().AddMutation(new Pyrokinesis(), 10);
							break;
						case 2:
							Object.RequirePart<Mutations>().AddMutation(new FireBreather(), 10);
							break;
						case 3:
							Object.RequirePart<Mutations>().AddMutation(new FlamingRay(), 5);
							Object.RequirePart<Mutations>().AddMutation(new Pyrokinesis(), 5);
							Object.RequirePart<Mutations>().AddMutation(new FireBreather(), 5);
							break;
						case 4:
						case 5:
							Object.AddSkill("Acrobatics");
							Object.AddSkill("Tactics_Kickback");
							Object.AddSkill("HeavyWeapons_Tank");
							Object.AddSkill("HeavyWeapons_Sweep");

							GameObject flamethrower = GameObject.Create("Flamethrower", 50);
							flamethrower.IsGiganticEquipment = true;
							Object.ReceiveObject(flamethrower);
							Object.ReceiveObject("Oilskin");
							break;
					}

					GetTwoHandedWeapon(Object, 0);
					break;

				case "Who Leads the Way of Frost":
					Object.Render.Tile = "Creatures/elementaltroll.png";
					Object.Render.DetailColor = "C";

					Object.GetStat("Strength").BoostStat(1);
					Object.GetStat("Agility").BoostStat(1);

					Object.GetStat("ColdResistance").BaseValue = 200;

					switch (Stat.Rnd.Next(4))
					{
						case 0:
							Object.RequirePart<Mutations>().AddMutation(new FreezingRay(), 10);
							break;
						case 1:
							Object.RequirePart<Mutations>().AddMutation(new Cryokinesis(), 10);
							break;
						case 2:
							Object.RequirePart<Mutations>().AddMutation(new IceBreather(), 10);
							break;
						case 3:
							Object.RequirePart<Mutations>().AddMutation(new FreezingRay(), 5);
							Object.RequirePart<Mutations>().AddMutation(new Cryokinesis(), 5);
							Object.RequirePart<Mutations>().AddMutation(new IceBreather(), 5);
							break;
							/*case 4:
							case 5:
							Object.AddSkill("Acrobatics");
							Object.AddSkill("Tactics_Kickback");
							Object.AddSkill("HeavyWeapons_Tank");
							Object.AddSkill("HeavyWeapons_Sweep");

							Object.ReceiveObject("Freeze Ray", BonusModChance: 50);
							GetTwoHandedWeapon(Object);
							break;*/
					}

					break;

				case "Who Approaches Storms":
					Object.Render.Tile = "Creatures/elementaltroll.png";
					Object.Render.DetailColor = "W";

					Object.GetStat("Strength").BoostStat(1);
					Object.GetStat("Agility").BoostStat(1);

					Object.GetStat("ElectricResistance").BaseValue = 200;

					Object.RequirePart<Mutations>().AddMutation(new ElectricalGeneration(), 10);
					Object.RequirePart<Mutations>().AddMutation(new ElectromagneticPulse(), 10);

					GetTwoHandedWeapon(Object, 0);
					break;

				case "Who Splits the Earth":
					Object.RequirePart<Mutations>().AddMutation(new HeightenedStrength(), 10);

					Object.AddSkill("Cudgel");
					Object.AddSkill("Cudgel_ChargingStrike");
					Object.AddSkill("Cudgel_Bludgeon");
					Object.AddSkill("Cudgel_Backswing");

					Object.AddSkill("Tactics");
					Object.AddSkill("Tactics_Charge");
					Object.AddSkill("Acrobatics");

					if (GetTwoHandedWeapon(Object, "Cudgel"))
					{
						Object.AddSkill("SingleWeaponFighting");
						Object.AddSkill("SingleWeaponFighting_WeaponExpertise");
						Object.AddSkill("SingleWeaponFighting_PenetratingStrikes");
						Object.AddSkill("SingleWeaponFighting_WeaponMastery");
					}
					else
					{
						Object.AddSkill("Multiweapon_Fighting");
						Object.AddSkill("Multiweapon_Expertise");
						Object.AddSkill("Multiweapon_Mastery");
					}
					break;

				case "Who Yearns for Knowledge":
					Object.GetStat("Strength").BoostStat(1);
					Object.GetStat("Agility").BoostStat(1);
					Object.GetStat("Intelligence").BoostStat(1);
					Object.GetStat("Willpower").BoostStat(1);

					Object.AddSkill("Acrobatics");
					Object.AddSkill("Acrobatics_Dodge");

					Object.AddSkill("Endurance");
					Object.AddSkill("Endurance_Weathered");
					Object.AddSkill("Endurance_Calloused");

					Object.AddSkill("Persuasion_MenacingStare");
					Object.AddSkill("Persuasion_Berate");

					Object.AddSkill("Discipline");
					Object.AddSkill("Discipline_Lionheart");
					Object.AddSkill("Discipline_IronMind");

					Object.AddSkill("Tactics");
					Object.AddSkill("Tactics_Juke");

					GetTwoHandedWeapon(Object, 50);
					GetTwoHandedWeapon(Object, 50);

					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));

					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "R", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "R", BonusModChance: 50));

					Object.ReceiveObject(PopulationManager.CreateOneFrom("DynamicObjectsTable:EnergyCells:Tier" + tier, BonusModChance: 50));

					Object.ReceiveObject(PopulationManager.CreateOneFrom("Scrap " + tier + "R"));

					break;

				case "Who Follows the Windy Path":
					Object.GetStat("Strength").BoostStat(1);
					Object.GetStat("Agility").BoostStat(1);

					Object.AddSkill("Tactics");
					Object.AddSkill("Tactics_Charge");
					Object.AddSkill("Endurance_Longstrider");
					Object.AddSkill("Acrobatics");
					Object.AddSkill("Acrobatics_Dodge");

					if (Stat.Rnd.Next(2) == 0)
					{
						Object.RequirePart<Mutations>().AddMutation(new Wings(), 10);
						Object.AddSkill("Acrobatics_Jump");
					}
					else
					{
						Object.RequirePart<Mutations>().AddMutation(new HeightenedSpeed(), 10);
						Object.RequirePart<Mutations>().AddMutation(new AdrenalControl2(), 10);
					}

					GetTwoHandedWeapon(Object, 0);
					break;

				case "Who Mastered the Art":
					Object.RequirePart<Mutations>().AddMutation(new HeightenedAgility(), 10);

					Object.AddSkill("LongBlades");
					Object.AddSkill("LongBladesLunge");
					Object.AddSkill("LongBladesSwipe");
					Object.AddSkill("LongBladesDuelingStance");
					Object.AddSkill("LongBladesDeathblow");

					Object.AddSkill("Tactics");
					Object.AddSkill("Acrobatics");
					Object.AddSkill("Acrobatics_Jump");

					if (GetTwoHandedWeapon(Object, "LongBlades"))
					{
						Object.AddSkill("SingleWeaponFighting");
						Object.AddSkill("SingleWeaponFighting_WeaponExpertise");
						Object.AddSkill("SingleWeaponFighting_PenetratingStrikes");
						Object.AddSkill("SingleWeaponFighting_WeaponMastery");
					}
					else
					{
						Object.AddSkill("Multiweapon_Fighting");
						Object.AddSkill("Multiweapon_Expertise");
						Object.AddSkill("Multiweapon_Mastery");
					}
					break;

				case "Who Hunts Game":
					Object.RequirePart<Mutations>().AddMutation(new HeightenedAgility(), 10);

					Object.AddSkill("Tactics_Kickback");
					Object.AddSkill("Acrobatics");

					Object.AddSkill("Rifles");
					Object.AddSkill("Rifle_SuppressiveFire");
					Object.AddSkill("Rifle_OneShot");

					Object.AddSkill("Pistol_Akimbo");
					Object.AddSkill("HeavyWeapons_Tank");
					Object.AddSkill("HeavyWeapons_Sweep");

					GetMissileWeapon(Object);
					GetTwoHandedWeapon(Object, 0);
					break;
			}

			Object.GetStat("Toughness").BoostStat(1);
			Object.GetStat("Intelligence").BoostStat(1);
			Object.GetStat("Willpower").BoostStat(1);
			Object.GetStat("Ego").BoostStat(1);

			if (tier > 3)
			{
				Object.AwardXP(Leveler.GetXPForLevel(tier * 5 + 5) - Object.GetStat("XP").BaseValue);
				Object.GetStat("MP").BaseValue = 0;
				Object.GetStat("AV").BaseValue += tier - 3;

				Statistic ap = Object.GetStat("AP");
				if (ap.BaseValue > 0)
				{
					Object.GetStat("Toughness").BaseValue += ap.BaseValue;
					ap.BaseValue = 0;
				}

				Object.ReceiveObject("Urberry");
				if (tier > 6)
				{
					Object.ReceiveObject("Urberry");
					Object.ReceiveObject("Urberry");
				}
			}
		}

		public void GetTwoHandedWeapon(GameObject obj, int modChance)
		{
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.InheritsFrom("MeleeWeapon")
				&& EncountersAPI.IsEligibleForDynamicEncounters(blueprint)
				&& blueprint.GetPartParameter("Physics", "UsesTwoSlots", false)
				&& Convert.ToInt32(blueprint.GetTag("Tier", "0")) == ZoneManager.zoneGenerationContextTier)
				{
					list.Add(blueprint);
				}
			}
			if (list.Count > 0)
			{
				obj.ReceiveObject(list.GetRandomElement(Stat.Rnd).Name, BonusModChance: modChance);
			}
		}

		public bool GetTwoHandedWeapon(GameObject obj, string skill)
		{
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.InheritsFrom("MeleeWeapon")
				&& EncountersAPI.IsEligibleForDynamicEncounters(blueprint)
				&& blueprint.GetPartParameter("Physics", "UsesTwoSlots", false)
				&& blueprint.GetPartParameter<string>("MeleeWeapon", "Skill", null) == skill
				&& Convert.ToInt32(blueprint.GetTag("Tier", "0")) == ZoneManager.zoneGenerationContextTier)
				{
					list.Add(blueprint);
				}
			}
			if (list.Count > 0)
			{
				if (Stat.Rnd.Next(2) == 0)
				{
					GameObject weapon = GameObject.Create(list.GetRandomElement(Stat.Rnd).Name);
					if (weapon.GetTag("Mods") != "None")
					{
						weapon.IsGiganticEquipment = true;
					}
					obj.ReceiveObject(weapon);
					return true;
				}
				obj.ReceiveObject(list.GetRandomElement(Stat.Rnd).Name);
				obj.ReceiveObject(list.GetRandomElement(Stat.Rnd).Name);
			}
			return false;
		}

		public void GetMissileWeapon(GameObject obj)
		{
			int tier = ZoneManager.zoneGenerationContextTier;

			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.InheritsFrom("MissileWeapon")
				&& EncountersAPI.IsEligibleForDynamicEncounters(blueprint)
				&& blueprint.GetPartParameter("Physics", "UsesTwoSlots", false)
				&& Convert.ToInt32(blueprint.GetTag("Tier", "0")) == tier)
				{
					list.Add(blueprint);
				}
			}
			if (list.Count > 0)
			{
				if (Stat.Rnd.Next(2) == 0)
				{
					GameObject weapon = ReceiveMissileWeapon(obj, list.GetRandomElement(Stat.Rnd).Name, tier);
					if (weapon.GetTag("Mods") != "None")
					{
						weapon.IsGiganticEquipment = true;
					}
				}
				else
				{
					ReceiveMissileWeapon(obj, list.GetRandomElement(Stat.Rnd).Name, tier);
					ReceiveMissileWeapon(obj, list.GetRandomElement(Stat.Rnd).Name, tier);
				}
			}
		}

		public static GameObject ReceiveMissileWeapon(GameObject obj, string blueprint, int tier)
		{
			GameObject item = GameObject.Create(blueprint, 50);
			obj.ReceiveObject(item);

			for (int i = 0; i < item.PartsList.Count; i++)
			{
				if (item.PartsList[i] is MagazineAmmoLoader loader)
				{
					if (loader.AmmoPart == "AmmoArrow")
					{
						string arrow;
						switch (tier)
						{
							default: arrow = "Wooden Arrow"; break;
							case 2: arrow = "Steel Arrow"; break;
							case 3: arrow = "Carbide Arrow"; break;
							case 4: arrow = "Folded Carbide Arrow"; break;
							case 5: arrow = "Fullerite Arrow"; break;
							case 6: arrow = "Crysteel Arrow"; break;
							case 7: arrow = "Flawless Crysteel Arrow"; break;
							case 8: arrow = "Zetachrome Arrow"; break;
						}
						RestockAmmo(obj, arrow, 10 + 5 * tier);
					}
					else if (loader.AmmoPart == "AmmoSlug")
					{
						RestockAmmo(obj, "Lead Slug", tier > 1 ? (int)(20 * tier * (1 + tier / 8f)) : 20);
					}
					else if (loader.AmmoPart == "AmmoShotgunShell")
					{
						RestockAmmo(obj, "Shotgun Shell", 10 + 5 * tier);
					}
					else if (loader.AmmoPart == "AmmoMissile")
					{
						RestockAmmo(obj, "HE Missile", 4 + 2 * tier);
					}
				}
				else if (item.PartsList[i] is EnergyCellSocket)
				{
					obj.ReceiveObject(PopulationManager.CreateOneFrom("DynamicObjectsTable:EnergyCells:Tier" + tier));
				}
			}

			return item;
		}

		private static void RestockAmmo(GameObject obj, string blueprint, int amount)
		{
			GameObject ammo = GameObjectFactory.Factory.CreateObject(blueprint);
			ammo.GetPart<Stacker>().StackCount = amount;
			obj.ReceiveObject(ammo);
		}
	}
}