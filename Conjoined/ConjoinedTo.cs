using System.Collections.Generic;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
	public class ConjoinedTo : IPart
	{
		public GameObject who;

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("BeginUnequip");
			base.Register(Object, Registrar);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override IPart DeepCopy(GameObject Parent)
		{
			return null;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == EnteredCellEvent.ID || ID == GetMaxCarriedWeightEvent.ID || ID == BeforeDieEvent.ID || ID == BeforeDestroyObjectEvent.ID || ID == GetInventoryActionsEvent.ID || ID == InventoryActionEvent.ID;
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			if (!ParentObject.OnWorldMap() && who?.CurrentCell != null)
			{
				if (E.Type == "Teleporting")
				{
					who.TeleportTo(ParentObject.CurrentCell);
				}
				else if (ParentObject.CurrentCell.ParentZone != who.CurrentCell.ParentZone)
				{
					who.Brain.Goals.Clear();
					who.SystemLongDistanceMoveTo(ParentObject.CurrentCell);
					who.UseEnergy(1000, "Move Conjoined");
				}
				else if (E.Dragging != who)
				{
					Conjoined.Pull(who, ParentObject);
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetMaxCarriedWeightEvent E)
		{
			E.AdjustWeight(0.5);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeDieEvent E)
		{
			if (!ParentObject.HasIntProperty("ConjoinmentSevered"))
			{
				ParentObject.SetIntProperty("SuppressCorpseDrops", 1);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeDestroyObjectEvent E)
		{
			Conjoined conjoined = (Conjoined)who.GetPart<Mutations>().GetMutation("Conjoined");

			if (conjoined != null && who.Body != null)
			{
				List<BodyPart> parts = who.Body.GetParts();
				for (int i = 0; i < conjoined.creatures.Length; i++)
				{
					if (ParentObject != conjoined.creatures[i])
					{
						continue;
					}

					for (int j = parts.Count - 1; j >= 0; j--)
					{
						if (parts[j].ID != conjoined.conjoinmentIDs[i])
						{
							continue;
						}

						Statistic hp = ParentObject.Statistics["Hitpoints"];

						string str = ParentObject.DefiniteArticle(UseAsDefault: who.its) + ParentObject.ShortDisplayName;
						who.TakeDamage((hp.BaseValue + 3) / 4, "from " + str + " dying!", "Unavoidable", "You couldn't survive the death of " + str + ".", who.It + " @@couldn't survive the death of " + str + ".");

						hp.Penalty = hp.BaseValue - hp.BaseValue / 4;

						Body body = ParentObject.Body;
						if (body.DismemberedParts != null)
						{
							foreach (Body.DismemberedPart part in body.DismemberedParts)
							{
								if (part.Part.Mortal && part.Part.IsRegenerable() && part.IsReattachable(body))
								{
									body.RegenerateLimb(false, part);
								}
							}
						}

						int count = ParentObject.Effects.Count;
						for (int k = count - 1; k >= 0; k--)
						{
							Effect effect = ParentObject.Effects[k];
							if (effect.IsOfTypes(117440512) && !effect.IsOfType(134247430))
							{
								ParentObject.RemoveEffect(effect, false);
							}
						}
						if (ParentObject.Effects.Count != count)
						{
							ParentObject.CheckStack();
						}

						ParentObject.RemoveIntProperty("SuppressCorpseDrops");

						ParentObject.Physics?.InInventory?.Inventory.RemoveObject(ParentObject);
						Cell cell = ParentObject.CurrentCell;
						if (cell != null)
						{
							if (cell.ParentZone != null && ParentObject.WantEvent(CheckExistenceSupportEvent.ID, CheckExistenceSupportEvent.CascadeLevel))
							{
								cell.ParentZone.WantSynchronizeExistence();
							}
							cell.RemoveObject(ParentObject);
						}
						The.ActionManager?.RemoveActiveObject(ParentObject);

						if (ParentObject.HasIntProperty("ConjoinmentSevered"))
						{
							ParentObject.RemoveIntProperty("ConjoinmentSevered");
						}
						else
						{
							who.Body.CutAndQueueForRegeneration(parts[j]);
							who.Body.UpdateBodyParts();
						}
						return false;
					}
				}
			}
			return true;
		}

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			if (E.Actor == who)
			{
				E.AddAction("View Stats", "view stats", "ConjoinedStats", Key: 'X');
				//E.AddAction("View Skills", "view skills", "ConjoinedSkills", Key: 'P');
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "ConjoinedStats")
			{
				Screens.CurrentScreen = 1;
				Screens.Show(E.Item);
				//new StatusScreen().Show(E.Item);
			}
			/*else if (E.Command == "ConjoinedSkills")
			{
				Screens.CurrentScreen = 0;
				Screens.Show(E.Item);
				//new SkillsAndPowersScreen().Show(E.Item);
			}*/
			return base.HandleEvent(E);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "BeginUnequip" && E.HasFlag("Forced") && ParentObject.IsDying && !ParentObject.HasIntProperty("ConjoinmentSevered"))
			{
				return false;
			}
			return base.FireEvent(E);
		}
	}
}