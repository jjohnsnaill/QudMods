using System.Collections.Generic;
using System.Text;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud
{
	public class QudConjoinedModule : EmbarkBuilderModule<QudConjoinedModuleData>
	{
		public override bool shouldBeEnabled()
		{
			QudMutationsModuleData data = builder?.GetModule<QudMutationsModule>()?.data;
			if (data == null)
			{
				return false;
			}
			for (int i = 0; i < data.selections.Count; i++)
			{
				if (data.selections[i].Mutation == "Conjoined")
				{
					return true;
				}
			}
			return false;
		}

		public override void OnAfterDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
		{
			QudMutationsModuleData mutations = builder?.GetModule<QudMutationsModule>()?.data;
			if (mutations != null)
			{
				for (int i = 0; i < mutations.selections.Count; i++)
				{
					if (mutations.selections[i].Mutation == "Conjoined")
					{
						data.max = mutations.selections[i].Count;
					}
				}
			}
			base.OnAfterDataChange(oldValues, newValues);
		}

		public override void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
		{
			QudMutationsModuleData mutations = builder?.GetModule<QudMutationsModule>()?.data;
			if (mutations != null)
			{
				for (int i = 0; i < mutations.selections.Count; i++)
				{
					if (mutations.selections[i].Mutation == "Conjoined")
					{
						data.max = mutations.selections[i].Count;
					}
				}
			}
			base.handleModuleDataChange(module, oldValues, newValues);
		}

		public override string DataErrors()
		{
			if (data.selections.Count < data.max)
			{
				return "You have not selected enough creatures.";
			}
			if (data.selections.Count > data.max)
			{
				return "You have selected too many creatures.";
			}
			return null;
		}

		public override void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
		{
			//TODO: actually get the index of the mutations module
			windows.InsertRange(8, this.windows.Values);
		}

		public override void InitFromSeed(string seed)
		{

		}

		public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
		{
			if (id == QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT)
			{
				(element as GameObject).SetIntProperty("SkipConjoinedRandomization", 1);
			}
			else if (id == QudGameBootModule.BOOTEVENT_GAMESTARTING)
			{
				The.Player.RemoveIntProperty("SkipConjoinedRandomization");

				Conjoined conjoined = The.Player.GetPart<Mutations>().GetMutation("Conjoined") as Conjoined;
				for (int i = 0; i < conjoined.creatures.Length; i++)
				{
					conjoined.CreateCreature(i, data.selections[i]);
					conjoined.SpawnCreature(i);
				}
			}
			return base.handleBootEvent(id, game, info, element);
		}

		public override SummaryBlockData GetSummaryBlock()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < data.selections.Count; i++)
			{
				GameObjectBlueprint bp = GameObjectFactory.Factory.GetBlueprint(data.selections[i]);
				GamePartBlueprint cherub = bp.GetPart("CherubimSpawner");
				if (cherub != null)
				{
					sb.AppendLine((bp.HasProperty("Inorganic") ? "mechanical cherub (" : "cherub (") + int.Parse(cherub.GetParameterString("Period")) + cherub.GetParameterString("Group") + ")");
					continue;
				}
				sb.AppendLine(bp.DisplayName());
			}

			return new SummaryBlockData()
			{
				Title = "Conjoined",
				Description = sb.ToString(),
				SortOrder = 100
			};
		}
	}
}