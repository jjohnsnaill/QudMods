using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts
{
	public class DuneWraithSpawner : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override void TurnTick(long TurnNumber)
		{
			long time = XRLCore.Core.Game.TimeTicks % 1200;
			if (ParentObject.InActiveZone() && (time <= 300 || time > 950 || The.Player.HasEffect<Lost>()))
			{
				if (ParentObject.CurrentCell.IsSolid())
				{
					return;
				}

				foreach (Cell cell in ParentObject.CurrentCell.GetAdjacentCells(2))
				{
					if (cell.HasCombatObject())
					{
						return;
					}
				}

				ParentObject.CurrentCell.AddObject(GameObject.Create("Aleksh_DuneWraith"));
				ParentObject.Obliterate();
			}
		}
	}
}