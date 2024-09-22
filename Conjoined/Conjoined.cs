using ConsoleLib.Console;
using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class Conjoined : BaseMutation
	{
		[NonSerialized]
		public GameObject[] creatures;
		[NonSerialized]
		public int[] conjoinmentIDs;

		public Conjoined()
		{
			DisplayName = "Conjoined";
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("Dismember");
			Registrar.Register("AfterLevelGained");
			base.Register(Object, Registrar);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Write(GameObject Basis, SerializationWriter Writer)
		{
			Writer.Write((byte)creatures.Length);
			for (int i = 0; i < creatures.Length; i++)
			{
				Writer.WriteGameObject(creatures[i]);
				Writer.Write(conjoinmentIDs[i]);
			}
		}

		public override void Read(GameObject Basis, SerializationReader Reader)
		{
			creatures = new GameObject[Reader.ReadByte()];
			conjoinmentIDs = new int[creatures.Length];
			for (int i = 0; i < creatures.Length; i++)
			{
				creatures[i] = Reader.ReadGameObject();
				creatures[i].AddPart<ConjoinedTo>().who = ParentObject;

				conjoinmentIDs[i] = Reader.ReadInt32();
			}
		}

		public override bool AffectsBodyParts()
		{
			return true;
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			if (creatures == null)
			{
				int finalLevel;
				if (GO.IsPlayer() || GO.HasIntProperty("SkipConjoinedRandomization"))
				{
					finalLevel = Level;
					creatures = new GameObject[finalLevel];
				}
				else
				{
					finalLevel = Stat.Rand.Next(5) == 0 ? 2 : 1;
					PickRandomCreatures(finalLevel);
				}

				conjoinmentIDs = new int[finalLevel];
				for (int i = 0; i < finalLevel; i++)
				{
					if (GO.Body == null)
					{
						conjoinmentIDs[i] = int.MaxValue;
						continue;
					}
					BodyPart c = new BodyPart("Conjoinment", GO.Body);
					GO.Body.GetBody().AddPart(c, 0);
					conjoinmentIDs[i] = c.ID;
				}

				return base.Mutate(GO, finalLevel);
			}

			if (GO.Body != null)
			{
				for (int i = 0; i < creatures.Length; i++)
				{
					if (GO.GetBodyPartByID(conjoinmentIDs[i], true) == null)
					{
						BodyPart c = new BodyPart("Conjoinment", GO.Body);
						c.ID = conjoinmentIDs[i];
						GO.Body.GetBody().AddPart(c, 0);
					}
				}
			}

			return base.Mutate(GO, Level);
		}

		public override void AfterMutate()
		{
			if (ParentObject.IsPlayer())
			{
				new ConjoinedMenu().Show(ParentObject);
			}
			base.AfterMutate();
		}

		public override IPart DeepCopy(GameObject Parent)
		{
			return new Conjoined
			{
				creatures = new GameObject[creatures.Length],
				conjoinmentIDs = new int[conjoinmentIDs.Length],
				ParentObject = Parent
			};
		}

		public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
		{
			Conjoined original = (Conjoined)Source.GetPart<Mutations>().GetMutation("Conjoined");
			for (int i = 0; i < creatures.Length; i++)
			{
				GameObject copy = original.creatures[i].DeepCopy(CopyEffects, CopyID);
				if (copy.Brain != null)
				{
					copy.Brain.PartyLeader = ParentObject;
				}
				copy.GetPart<ConjoinedTo>().who = ParentObject;
				copy.RemoveIntProperty("ConjoinmentSevered");
				creatures[i] = copy;

				conjoinmentIDs[i] = original.conjoinmentIDs[i];
			}
			ParentObject.SetIntProperty("SpawnConjoinedCreatures", 1);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == ReplicaCreatedEvent.ID || ID == MakeTemporaryEvent.ID || ID == StripContentsEvent.ID || ID == EnteredCellEvent.ID || ID == GetMaxCarriedWeightEvent.ID || ID == RegenerateLimbEvent.ID || ID == OnDeathRemovalEvent.ID || ID == OnDestroyObjectEvent.ID;
		}

		public override bool HandleEvent(ReplicaCreatedEvent E)
		{
			Conjoined original = (Conjoined)E.Original.GetPart<Mutations>().GetMutation("Conjoined");
			for (int i = 0; i < creatures.Length; i++)
			{
				ReplicaCreatedEvent.Send(creatures[i], ParentObject, original.creatures[i], E.Context, E.Temporary);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(MakeTemporaryEvent E)
		{
			for (int i = 0; i < creatures.Length; i++)
			{
				Temporary.AddHierarchically(creatures[i], E.Duration, E.TurnInto, ParentObject);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(StripContentsEvent E)
		{
			for (int i = 0; i < creatures.Length; i++)
			{
				creatures[i].StripContents(E.KeepNatural, E.Silent);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			if (!ParentObject.OnWorldMap())
			{
				if (ParentObject.HasIntProperty("SpawnConjoinedCreatures"))
				{
					for (int i = 0; i < creatures.Length; i++)
					{
						SpawnCreature(i);
					}
					ParentObject.RemoveIntProperty("SpawnConjoinedCreatures");
				}
				for (int i = 0; i < creatures.Length; i++)
				{
					GameObject creature = creatures[i];
					if (creature?.CurrentCell == null)
					{
						continue;
					}

					if (E.Type == "Teleporting")
					{
						creature.TeleportTo(ParentObject.CurrentCell);
					}
					else if (ParentObject.CurrentCell.ParentZone != creature.CurrentCell.ParentZone)
					{
						creature.Brain.Goals.Clear();
						creature.SystemLongDistanceMoveTo(ParentObject.CurrentCell);
						creature.UseEnergy(1000, "Move Conjoined");
					}
					else if (E.Dragging != creature)
					{
						Pull(creature, ParentObject);
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetMaxCarriedWeightEvent E)
		{
			E.AdjustWeight(0.75);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(RegenerateLimbEvent E)
		{
			RespawnCreatures();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(OnDeathRemovalEvent E)
		{
			for (int i = 0; i < creatures.Length; i++)
			{
				creatures[i].RemovePart<ConjoinedTo>();
				string str = ParentObject.the + ParentObject.ShortDisplayName + ".";
				creatures[i].Die(E.Killer, null, "You couldn't survive without " + str, creatures[i].It + " @@couldn't survive without " + str, E.Accidental);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(OnDestroyObjectEvent E)
		{
			for (int i = 0; i < creatures.Length; i++)
			{
				creatures[i].Destroy(null, E.Silent, E.Obliterate);
			}
			return base.HandleEvent(E);
		}

		public override string GetDescription()
		{
			return "Another creature is attached to you.";
		}

		public override bool CanLevel()
		{
			return false;
		}

		public override string GetLevelText(int Level)
		{
			string description = "\n-25% carry capacity\nConjoined creatures over a tile away are pulled back. If this fails, they take damage equal to 1/16th of their health multiplied by distance, minimum 1.\nConjoined creatures return to you on death, keeping all their items, and can be revived by regenerating your connection to them.\nYou can view your conjoined creature's character sheet and skills.";
			if (ParentObject != null)
			{
				List<string> current = new List<string>();
				StringBuilder future = new StringBuilder();

				for (int i = 0; i < creatures.Length; i++)
				{
					if (creatures[i].TryGetIntProperty("SpawnAtLevel", out int level) && ParentObject.Statistics["Level"].Value < level)
					{
						future.Append(creatures[i].A + creatures[i].ShortDisplayName + " will emerge at level " + level + ".\n");
					}
					else if (current.Count == 0)
					{
						current.Add(creatures[i].A + creatures[i].ShortDisplayName);
					}
					else
					{
						current.Add(creatures[i].a + creatures[i].ShortDisplayName);
					}
				}

				future.Append(description);

				if (current.Count == 0)
				{
					return future.ToString();
				}

				return (current.Count == 1 ? current[0] + " is conjoined to you.\n" : Grammar.MakeAndList(current) + " are conjoined to you.\n") + future;
			}
			return "You choose what is conjoined to you upon gaining this mutation. If the creature is a higher level than you, they will not emerge until you reach their level.\n" + description;
		}

		public static List<GameObjectBlueprint> GetAvailableCreatures(bool organic, int maxLevel = 0, int maxOptions = 0)
		{
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(32);
			List<int> levels = new List<int>(32);
			foreach (GameObjectBlueprint bp in GameObjectFactory.Factory.BlueprintList)
			{
				int level = bp.GetStat("Level", new Statistic()).Value;
				if (maxLevel > 0 && level > maxLevel)
				{
					continue;
				}
				if (bp.HasTag("BaseObject") && bp.Name != "Snapjaw Scavenger")
				{
					continue;
				}
				if (bp.HasTag("ExcludeFromDynamicEncounters") && bp.Name != "Phase Spider")
				{
					continue;
				}
				if (!bp.HasPart("Brain") || bp.HasPart("GivesRep") || bp.GetxTag("Grammar", "Proper") != null)
				{
					continue;
				}
				if (organic ?
						(!bp.HasPart("Inorganic") &&
						!bp.HasProperty("Inorganic") &&

						(!bp.HasTag("Humanoid") ||
						bp.Name == "Snapjaw Scavenger" ||
						bp.Inherits == "Snapjaw Troglodyte" ||
						bp.Inherits == "Cragmensch" ||
						bp.Inherits == "Glittermensch" ||
						bp.Inherits == "Troll" ||
						bp.Name == "MopangoPilgrim" ||
						bp.Inherits == "BaseSlynth" ||
						bp.Inherits == "Svardym") &&

						!bp.HasTag("Robot") &&
						(!bp.HasTag("Grazer") || bp.Name == "Pig") &&
						!bp.HasPart("Nest") &&
						!bp.InheritsFrom("LiquidLichen") &&
						!bp.InheritsFrom("Goatfolk") &&
						bp.Name != "ClockworkBeetle" && //not inorganic until it spawns
						bp.Name != "Feral Lah Pod") //stays exploded after regenerating
					:
						(bp.HasProperty("Inorganic") ||
						bp.HasTag("Robot") ||
						bp.HasPart("CherubimSpawner")))
				{
					int i = list.Count;
					while (true)
					{
						if (--i < 0 || level > levels[i] || (level == levels[i] && ColorUtility.StripFormatting(bp.DisplayName()).CompareTo(ColorUtility.StripFormatting(list[i].DisplayName())) >= 0))
						{
							list.Insert(i + 1, bp);
							levels.Insert(i + 1, level);
							break;
						}
					}
				}
			}

			if (maxOptions > 0)
			{
				int lowest = levels[Math.Max(list.Count - 10, 0)];
				for (int i = list.Count - 11; i >= 0; i--)
				{
					if (levels[i] < lowest)
					{
						list.RemoveRange(0, i + 1);
						break;
					}
				}
			}

			return list;
		}

		public void PickRandomCreatures(int amount)
		{
			creatures = new GameObject[amount];

			List<GameObjectBlueprint> list = GetAvailableCreatures(ParentObject.GetIntProperty("Inorganic") == 0, ParentObject.Statistics["Level"].Value, 10);

			if (ParentObject.CurrentCell == null)
			{
				for (int i = 0; i < amount; i++)
				{
					CreateCreature(i, list[Stat.Rand.Next(list.Count)].Name);
				}
				ParentObject.SetIntProperty("SpawnConjoinedCreatures", 1);
				return;
			}

			for (int i = 0; i < amount; i++)
			{
				CreateCreature(i, list[Stat.Rand.Next(list.Count)].Name);
				SpawnCreature(i);
			}
		}

		public void CreateCreature(int index, string blueprint)
		{
			GameObject creature = GameObjectFactory.Factory.CreateObject(blueprint);

			int level = creature.Statistics["Level"].Value;

			if (creature.Statistics["Hitpoints"].BaseValue > 15 + 3 * level)
			{
				creature.Statistics["Hitpoints"].BaseValue = 15 + 3 * level;
			}
			if (creature.Statistics.ContainsKey("XPValue"))
			{
				creature.Statistics["XPValue"].BaseValue = 0;
			}

			if (creature.Brain != null)
			{
				creature.Brain.PartyLeader = ParentObject;
			}

			creature.AddPart<ConjoinedTo>().who = ParentObject;
			creature.SetIntProperty("SpawnAtLevel", level);

			creatures[index] = creature;
		}

		public bool SpawnCreature(int index)
		{
			if (creatures[index].TryGetIntProperty("SpawnAtLevel", out int level))
			{
				if (ParentObject.Statistics["Level"].Value < level)
				{
					return false;
				}
				ParentObject.RemoveIntProperty("SpawnAtLevel");
			}

			if (creatures[index].Physics != null)
			{
				creatures[index].Physics.Temperature = ParentObject.Physics.Temperature;
			}
			ParentObject.CurrentCell.AddObject(creatures[index]);
			XRLCore.Core.Game.ActionManager.AddActiveObject(creatures[index]);

			return true;
		}

		public void RespawnCreatures()
		{
			if (ParentObject.Body == null)
			{
				return;
			}

			List<BodyPart> parts = ParentObject.Body.GetParts();
			for (int i = 0; i < parts.Count; i++)
			{
				if (parts[i].Type == "Conjoinment")
				{
					for (int j = 0; j < conjoinmentIDs.Length; j++)
					{
						if (conjoinmentIDs[j] != parts[i].ID)
						{
							continue;
						}

						GameObject creature = creatures[j];
						if (creature.CurrentCell != null || !SpawnCreature(j))
						{
							continue;
						}

						if (ParentObject.IsPlayer())
						{
							AddPlayerMessage(creature.A + creature.ShortDisplayName + " emerges from your body!");
						}
						else if (ParentObject.IsVisible())
						{
							AddPlayerMessage(creature.A + creature.ShortDisplayName + " emerges from " + Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName) + " body!");
						}
					}
				}
			}
		}

		public static void Pull(GameObject what, GameObject to)
		{
			int distI = to.CurrentZone.ResolvedX - what.CurrentZone.ResolvedX;
			if (distI < -1 || distI > 1)
			{
				return;
			}
			distI = to.CurrentZone.ResolvedY - what.CurrentZone.ResolvedY;
			if (distI < -1 || distI > 1)
			{
				return;
			}

			double dist = what.CurrentCell.RealDistanceTo(to.CurrentCell);
			while ((distI = what.CurrentCell.PathDistanceTo(to.CurrentCell)) > 1)
			{
				Cell validCell = null;
				bool group = false;
				for (int i = 0; i < 8; i++)
				{
					Cell cell = what.CurrentCell.GetCellFromDirection(Cell.DirectionList[i]);
					if (cell != null && CanPullTo(cell, what, to, ref group))
					{
						double newDist = cell.RealDistanceTo(to.CurrentCell);
						if (newDist < dist)
						{
							validCell = cell;
							dist = newDist;
						}
					}
				}
				if (validCell == null || !what.Move(what.CurrentCell.GetDirectionFromCell(validCell), true, IgnoreGravity: distI > 2, Dragging: to))
				{
					if (!group)
					{
						what.TakeDamage(Math.Max((distI - 1) * what.Statistics["Hitpoints"].BaseValue / 16, 1), "from being strained!", "Crushing", "You were pulled too hard.", what.It + what.GetVerb("were") + " @@pulled too hard.");
					}
					break;
				}
			}
		}

		private static bool CanPullTo(Cell cell, GameObject what, GameObject to, ref bool group)
		{
			for (int i = 0; i < cell.Objects.Count; i++)
			{
				if (cell.Objects[i].ConsiderSolidFor(what))
				{
					return false;
				}
				ConjoinedTo conjoinedTo = cell.Objects[i].GetPart<ConjoinedTo>();
				if (conjoinedTo != null && (conjoinedTo.who == to || conjoinedTo.who == what))
				{
					group = true;
					return false;
				}
			}
			return true;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "Dismember")
			{
				for (int i = 0; i < conjoinmentIDs.Length; i++)
				{
					if (conjoinmentIDs[i] != E.GetParameter<BodyPart>("Part").ID)
					{
						continue;
					}

					GameObject creature = creatures[i];
					if (creature.CurrentCell == null)
					{
						continue;
					}

					creature.SetIntProperty("ConjoinmentSevered", 1);
					string str = ParentObject.the + ParentObject.ShortDisplayName + ".";
					creature.Die(null, null, "You were severed from " + str, creature.It + creature.GetVerb("were") + " @@severed from " + str);
				}
			}
			else if (E.ID == "AfterLevelGained")
			{
				RespawnCreatures();
			}
			return base.FireEvent(E);
		}
	}
}