namespace XRL.World.Parts
{
	public class NachashAssassin : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == BeginTakeActionEvent.ID || ID == AfterObjectCreatedEvent.ID;
		}

		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			SyncState();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AfterObjectCreatedEvent E)
		{
			SyncState();
			return base.HandleEvent(E);
		}

		public void SyncState()
		{
			if (ParentObject.Target == null)
			{
				ParentObject.DisplayName = "nachash tree";
				ParentObject.SetIntProperty("HideCon", 1);
			}
			else
			{
				ParentObject.DisplayName = "nachash assassin";
				ParentObject.RemoveIntProperty("HideCon");
			}
		}
	}
}