using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts
{
	public class SpawnMetabolizing : IPart
	{
		public CookingRecipe recipe;

		public override void Write(GameObject Basis, SerializationWriter Writer)
		{
			Writer.Write(recipe);
		}

		public override void Read(GameObject Basis, SerializationReader Reader)
		{
			recipe = Reader.ReadComposite<CookingRecipe>();
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == ObjectCreatedEvent.ID || ID == EndTurnEvent.ID;
		}

		public override bool HandleEvent(ObjectCreatedEvent E)
		{
			int tier = ZoneManager.zoneGenerationContextTier;

			List<GameObject> list = new List<GameObject>();
			int chanceFor3 = 75;
			if (tier >= 0 && tier <= 1)
			{
				chanceFor3 = 15;
			}
			else if (tier >= 2 && tier <= 3)
			{
				chanceFor3 = 35;
			}
			else if (tier >= 4 && tier <= 5)
			{
				chanceFor3 = 50;
			}
			else if (tier >= 6 && tier <= 7)
			{
				chanceFor3 = 65;
			}

			int rand = Stat.Random(1, 100);
			int ingredients = rand > chanceFor3 ? 2 : 3;

			int retries = 0;
			for (int i = 0; i < ingredients; i++)
			{
				Reroll:
				string ingredient = CookingRecipe.RollOvenSafeIngredient("Ingredients" + tier);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].Blueprint != ingredient)
					{
						continue;
					}
					if (++retries < 50)
					{
						goto Reroll;
					}
					goto Abort;
				}
				list.Add(GameObject.Create(ingredient));
			}

			Abort:
			recipe = CookingRecipe.FromIngredients(list);
			recipe.ApplyEffectsTo(ParentObject, false);

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			bool reapply = true;
			for (int i = 0; i < ParentObject._Effects.Count; i++)
			{
				if (ParentObject._Effects[i].IsOfType(Effect.TYPE_METABOLIC))
				{
					reapply = false;
					break;
				}
			}
			if (reapply)
			{
				recipe.ApplyEffectsTo(ParentObject, false);
			}

			return base.HandleEvent(E);
		}
	}
}