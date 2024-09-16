using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
	public class AIGasSpam : AIBehaviorPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == AIGetOffensiveAbilityListEvent.ID;
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			Mutations part = ParentObject.GetPart<Mutations>();
			if (part == null)
			{
				return base.HandleEvent(E);
			}

			for (int i = 0; i < part.MutationList.Count; i++)
			{
				if (part.MutationList[i] is GasGeneration gas)
				{
					for (int j = E.List.Count - 1; j >= 0; j--)
					{
						if (E.List[j].Command == gas.GetReleaseAbilityCommand())
						{
							E.List.RemoveAt(j);
						}
					}

					if (gas.IsMyActivatedAbilityAIUsable(gas.ActivatedAbilityID) && !gas.IsMyActivatedAbilityToggledOn(gas.ActivatedAbilityID))
					{
						E.Add(gas.GetReleaseAbilityCommand());
					}
				}
			}

			return base.HandleEvent(E);
		}
	}
}