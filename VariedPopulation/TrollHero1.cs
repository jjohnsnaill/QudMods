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

					Object.GetStat("HeatResistance").BaseValue = 200;

					switch (Stat.Rnd.Next(6))
					{
						case 0:
							Object.RequirePart<Mutations>().AddMutation(new FlamingRay(), 10);
							GetTwoHandedWeapon(Object);
							break;
						case 1:
							Object.RequirePart<Mutations>().AddMutation(new Pyrokinesis(), 10);
							GetTwoHandedWeapon(Object);
							break;
						case 2:
							Object.RequirePart<Mutations>().AddMutation(new FireBreather(), 10);
							GetTwoHandedWeapon(Object);
							break;
						case 3:
							Object.RequirePart<Mutations>().AddMutation(new FlamingRay(), 5);
							Object.RequirePart<Mutations>().AddMutation(new Pyrokinesis(), 5);
							Object.RequirePart<Mutations>().AddMutation(new FireBreather(), 5);

							GetTwoHandedWeapon(Object);
							break;
						case 4:
						case 5:
							Object.AddSkill("Tactics_Kickback");
							Object.AddSkill("HeavyWeapons_Tank");
							Object.AddSkill("HeavyWeapons_Sweep");

							Object.ReceiveObject("Flamethrower", BonusModChance: 50);
							Object.ReceiveObject("Oilskin");
							break;
					}

					break;

				case "Who Leads the Way of Frost":
					Object.Render.Tile = "Creatures/elementaltroll.png";
					Object.Render.DetailColor = "C";

					Object.GetStat("ColdResistance").BaseValue = 200;

					switch (Stat.Rnd.Next(4))
					{
						case 0:
							Object.RequirePart<Mutations>().AddMutation(new FreezingRay(), 10);
							GetTwoHandedWeapon(Object);
							break;
						case 1:
							Object.RequirePart<Mutations>().AddMutation(new Cryokinesis(), 10);
							GetTwoHandedWeapon(Object);
							break;
						case 2:
							Object.RequirePart<Mutations>().AddMutation(new IceBreather(), 10);
							GetTwoHandedWeapon(Object);
							break;
						case 3:
							Object.RequirePart<Mutations>().AddMutation(new FreezingRay(), 5);
							Object.RequirePart<Mutations>().AddMutation(new Cryokinesis(), 5);
							Object.RequirePart<Mutations>().AddMutation(new IceBreather(), 5);

							GetTwoHandedWeapon(Object);
							break;
							/*case 4:
							case 5:
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

					Object.GetStat("ElectricResistance").BaseValue = 200;

					Object.RequirePart<Mutations>().AddMutation(new ElectricalGeneration(), 10);
					Object.RequirePart<Mutations>().AddMutation(new ElectromagneticPulse(), 10);

					GetTwoHandedWeapon(Object);
					break;

				case "Who Yearns for Knowledge":
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Melee Weapons " + tier + "R", BonusModChance: 100));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "C", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Artifact " + tier + "R", BonusModChance: 50));
					Object.ReceiveObject(PopulationManager.CreateOneFrom("Scrap " + tier + "R"));
					break;

				case "Who Follows the Windy Path":
					Object.AddSkill("Tactics");
					Object.AddSkill("Tactics_Charge");
					Object.AddSkill("Endurance_Longstrider");
					Object.AddSkill("Acrobatics");
					Object.AddSkill("Acrobatics_Spry");

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

					GetTwoHandedWeapon(Object);
					break;

				default:
					GetTwoHandedWeapon(Object);
					break;
			}

			Object.GetStat("Strength").BoostStat(1);
			Object.GetStat("Agility").BoostStat(1);
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

		public void GetTwoHandedWeapon(GameObject obj)
		{
			obj.ReceiveObject(PopulationManager.CreateOneFrom("Melee Weapons " + ZoneManager.zoneGenerationContextTier + "R", BonusModChance: 50));
		}
	}
}