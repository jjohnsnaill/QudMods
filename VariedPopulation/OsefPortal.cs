using XRL.World.Parts;

namespace XRL.World.ZoneBuilders
{
	public class OsefPortal : ZoneBuilderSandbox
	{
		public bool BuildZone(Zone Z)
		{
			GameObject portal = GameObject.Create("Space-Time Anomaly");
			portal.AddPart(new StablePortal()
			{
				DestinationZoneID = The.Game.GetSystem<ClamSystem>().ClamWorldId
			});

			Z.Map[40][12].AddObject(portal);
			return true;
		}
	}
}