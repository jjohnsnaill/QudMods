namespace XRL.World.ZoneBuilders
{
	[HasGameBasedStaticCache]
	public class PossibleTrollHero : ZoneBuilderSandbox
	{
		public bool BuildZone(Zone Z)
		{
			if (4.in100())
			{
				PlaceObjectInArea(Z, Z.area, GameObjectFactory.Factory.CreateObject("Aleksh_TrollHero"));
			}
			return true;
		}

		[GameBasedCacheInit]
		public static void AddBuilder()
		{
			CellBlueprint cell = WorldFactory.Factory.getWorld("JoppaWorld").CellBlueprintsByName["DefaultJoppaCell"];
			OrderedBuilderBlueprint bp = new OrderedBuilderBlueprint(new ZoneBuilderBlueprint("PossibleTrollHero"), 3999);
			for (int i = 21; i < 50; i++)
			{
				bool add = true;
				foreach (ZoneBuilderBlueprint bp2 in cell.LevelBlueprint[1, 1, i].Builders)
				{
					if (bp2.Class == "PossibleTrollHero")
					{
						add = false;
					}
				}
				if (add)
				{
					cell.LevelBlueprint[1, 1, i].Builders.Add(bp);
				}
			}
		}
	}
}