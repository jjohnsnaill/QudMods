using XRL.UI;

namespace XRL.World.Parts
{
	public class Lovable : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}

		public override bool HandleEvent(BeforeDeathRemovalEvent E)
		{
			if (E.Killer == null || E.Killer == ParentObject)
			{
				return base.HandleEvent(E);
			}

			foreach (Faction faction in Factions.Loop())
			{
				faction.SetFactionFeeling(E.Killer.GetPrimaryFaction(), -500);
				if (E.Killer.IsPlayerControlled())
				{
					The.Game.PlayerReputation.ReputationValues[faction.Name] = -8000;
				}
			}
			if (E.Killer.IsPlayerControlled())
			{
				SoundManager.PlaySound("Sounds/Reputation/sfx_reputation_cacophonic_negative", 0, 1, 1, SoundRequest.SoundEffectType.None);
				Popup.Show("{{O|You sense the ire of all that exists. You have done something truly evil.}}");
			}

			return base.HandleEvent(E);
		}
	}
}