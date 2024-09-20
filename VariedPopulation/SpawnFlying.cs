using XRL.World.Capabilities;

namespace XRL.World.Parts
{
	public class SpawnFlying : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == EnteredCellEvent.ID;
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			if (!ParentObject.HasPropertyOrTag("IgnoresGravity") && !Flight.StartFlying(ParentObject, ParentObject))
			{
				ParentObject.SetIntProperty("IgnoresGravity", 1);
			}
			ParentObject.RemovePart(this);

			return base.HandleEvent(E);
		}
	}
}