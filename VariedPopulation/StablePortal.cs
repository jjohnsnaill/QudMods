namespace XRL.World.Parts
{
	public class StablePortal : SpaceTimeVortex
	{
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == PooledEvent<GetPointsOfInterestEvent>.ID;
		}

		public override bool HandleEvent(GetPointsOfInterestEvent E)
		{
			if (E.StandardChecks(this, E.Actor))
			{
				E.Add(ParentObject, ParentObject.GetReferenceDisplayName(), null, null, null, null, null, 2);
			}
			return base.HandleEvent(E);
		}

		public override int SpaceTimeAnomalyEmergencePermillageBaseChance()
		{
			return 0;
		}

		public override bool SpaceTimeAnomalyStationary()
		{
			return true;
		}
	}
}