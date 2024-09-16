using System.Collections.Generic;

namespace XRL.World.Parts
{
	public class HungeringTar : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == EnteredCellEvent.ID || ID == BeforeDeathRemovalEvent.ID;
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			GameObject pool = E.Cell.GetOpenLiquidVolume();
			if (pool == null)
			{
				E.Cell.AddObject(GameObject.Create("AsphaltPuddle"));
			}
			else
			{
				LiquidVolume liquid = pool.LiquidVolume;
				if (liquid.Amount("asphalt") < 500)
				{
					liquid.MixWith(new LiquidVolume("asphalt", 500));
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeDeathRemovalEvent E)
		{
			if (ParentObject.CurrentCell != null)
			{
				List<Cell> cells = ParentObject.CurrentCell.GetAdjacentCells();
				foreach (Cell cell in cells)
				{
					if (!cell.IsOccluding())
					{
						cell.AddObject(GameObject.Create("AsphaltPuddle"));
					}
				}
			}
			return base.HandleEvent(E);
		}
	}
}