namespace XRL.World.Parts
{
	public class TradeArtifactSecrets : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == GetWaterRitualSecretWeightEvent.ID;
		}

		public override bool HandleEvent(GetWaterRitualSecretWeightEvent E)
		{
			if (E.Secret.TryGetAttribute("artifact", out _) && ((E.Buy && E.Secret.CanBuy()) || (E.Sell && E.Secret.CanSell())))
			{
				E.BaseWeight = 100;
				E.Weight = 100;
			}
			else
			{
				E.BaseWeight = 0;
				E.Weight = 0;
			}
			return base.HandleEvent(E);
		}
	}
}