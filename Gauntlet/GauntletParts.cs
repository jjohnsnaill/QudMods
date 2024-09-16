using ConsoleLib.Console;
using System.Collections.Generic;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.AI;
using XRL.World.Effects;
using XRL.World.Parts;

namespace Mods.Gauntlet
{
	public class GauntletPart : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == GetTradePerformanceEvent.ID || ID == BeforeRenderEvent.ID;
		}

		public override bool HandleEvent(GetTradePerformanceEvent E)
		{
			E.FactorAdjustment = 0;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeRenderEvent E)
		{
			if (ParentObject.IsPlayer())
			{
				ParentObject.CurrentZone.AddLight(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 40, LightLevel.Light);
			}
			return base.HandleEvent(E);
		}
	}

	public class GauntletCell : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}

		public static void SetGauntletProperties(GameObject obj)
		{
			if (!obj.HasIntProperty("GauntletObject"))
			{
				obj.AddPart<GauntletPart>();
				obj.RemovePart<GivesRep>();

				if (obj.Statistics.TryGetValue("XPValue", out Statistic xp))
				{
					xp.BaseValue = 0;
				}

				if (obj.Brain != null)
				{
					obj.Brain.Factions = "Mean-100,Playerhater-99";
					obj.Brain.AddOpinion<OpinionInscrutable>(The.Player);
				}

				obj.IntProperty["GauntletObject"] = 0;
			}
		}

		public override bool HandleEvent(ObjectEnteredCellEvent E)
		{
			if (E.Object.HasPart<StairsUp>() || E.Object.HasPart<StairsDown>())
			{
				E.Object.Obliterate();
				return false;
			}

			SetGauntletProperties(E.Object);

			return base.HandleEvent(E);
		}
	}

	public class GauntletBaetyl : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == GetInventoryActionsAlwaysEvent.ID || ID == InventoryActionEvent.ID || ID == CanSmartUseEvent.ID || ID == CommandSmartUseEvent.ID;
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			E.AddAction("Advance", "advance", "NextWave", null, 'a', Default: 100);

			if (CountTokens(E.Actor) > 0)
			{
				E.AddAction("Buy", "buy", "SpendTokens", null, 'b', Default: 110);
			}

			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "NextWave")
			{
				The.Game.GetSystem<GauntletSystem>().Advance();
				E.RequestInterfaceExit();
			}
			else if (E.Command == "SpendTokens")
			{
				List<string> options = new List<string>()
				{
					"Data Disk",
					"Cookbook",
					"Military Security Card",
					"Ruin of House Isner",
					"Kindrish",
					"Stopsvalinn"
				};
				List<IRenderable> icons = new List<IRenderable>()
				{
					new Render() { Tile = "items/sw_data_disc.bmp", DetailColor = "W", ColorString = "&c" },
					new Render() { Tile = "items/sw_book_1.bmp", DetailColor = "W", ColorString = "&y" },
					new Render() { Tile = "items/sw_security_card.bmp", ColorString = "&M" },
					new Render() { Tile = "items/sw_revolver.bmp", DetailColor = "w", ColorString = "&M" },
					new Render() { Tile = "items/sw_kindrish.bmp", DetailColor = "Y", ColorString = "&g" },
					new Render() { Tile = "items/sw_buckler2.bmp", DetailColor = "r", ColorString = "&M" }
				};
				List<int> costs = new List<int>()
				{
					1,
					4,
					15,
					15,
					15,
					20
				};

				int wave = The.Game.GetSystem<GauntletSystem>().wave;
				if (wave >= 40)
				{
					options.Add("Still Crystal Chime");
					icons.Add(new Render() { Tile = "terrain/sw_chavvah_chime_9.bmp", DetailColor = "y", ColorString = "&m" });
					costs.Add(20);
				}
				if (wave >= 15)
				{
					options.Add("Cloaca Surprise");
					icons.Add(new Render() { Tile = "items/sw_oven.bmp", DetailColor = "R", ColorString = "&g" });
					costs.Add(8);
				}
				if (wave >= 40)
				{
					options.Add("Crystal Delight");
					icons.Add(new Render() { Tile = "items/sw_oven.bmp", DetailColor = "R", ColorString = "&m" });
					costs.Add(12);
				}

				while (true)
				{
					int tokens = CountTokens(E.Actor);

					List<string> display = new List<string>(options.Count);
					for (int i = 0; i < display.Count; i++)
					{
						if (tokens < costs[i])
						{
							display.Add("{{K|" + options[i] + "}} [" + costs[i] + "]");
							continue;
						}
						display.Add(options[i] + " [" + costs[i] + "]");
					}

					int index = Popup.PickOption("Spend " + tokens + " tokens", Options: display, Icons: icons, Spacing: 60, AllowEscape: true);
					if (index < 0)
					{
						break;
					}
					if (index >= options.Count)
					{
						continue;
					}
					if (tokens < costs[index])
					{
						Popup.Show("You don't have enough tokens to buy that.");
						continue;
					}

					SpendTokens(E.Actor, costs[index], options[index]);
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanSmartUseEvent E)
		{
			return false;
		}

		public override bool HandleEvent(CommandSmartUseEvent E)
		{
			The.Game.GetSystem<GauntletSystem>().Advance();
			return false;
		}

		private int CountTokens(GameObject obj)
		{
			int count = 0;
			foreach (GameObject item in obj.Inventory.Objects)
			{
				if (item.Blueprint == "Merchant's Token")
				{
					count += item.GetPart<Stacker>().StackCount;
				}
			}
			return count;
		}

		private void SpendTokens(GameObject obj, int cost, string choice)
		{
			foreach (GameObject item in obj.Inventory.Objects)
			{
				if (item.Blueprint != "Merchant's Token")
				{
					continue;
				}

				int stack = item.GetPart<Stacker>().StackCount;
				if (cost >= stack)
				{
					item.Obliterate();
				}
				else
				{
					for (int i = 0; i < cost; i++)
					{
						item.Destroy();
					}
				}

				cost -= stack;
				if (cost <= 0)
				{
					switch (choice)
					{
						case "Data Disk":
							obj.ReceiveObject(GameObjectFactory.Factory.CreateObject("DataDisk"));
							break;
						case "Cookbook":
							obj.ReceiveObject(GameObjectFactory.Factory.CreateObject("FocalCookbook"));
							break;
						case "Military Security Card":
							obj.ReceiveObject(GameObjectFactory.Factory.CreateObject("Purple Security Card"));
							break;
						case "Ruin of House Isner":
							{
								GameObject purchase = GameObjectFactory.Factory.CreateObject("Ruin of House Isner");
								purchase.RemovePart<TakenAchievement>();
								obj.ReceiveObject(purchase);
								break;
							}
						case "Kindrish":
							{
								GameObject purchase = GameObjectFactory.Factory.CreateObject("Kindrish");
								purchase.RemovePart<TakenAchievement>();
								obj.ReceiveObject(purchase);
								break;
							}
						case "Stopsvalinn":
							{
								GameObject purchase = GameObjectFactory.Factory.CreateObject("Stopsvaalinn");
								purchase.RemovePart<TakenAchievement>();
								obj.ReceiveObject(purchase);
								break;
							}
						case "Still Crystal Chime":
							obj.ReceiveObject(GameObjectFactory.Factory.CreateObject("TauChime"));
							break;
						case "Cloaca Surprise":
							CookingDomainSpecial_UnitSlogTransform.ApplyTo(obj);
							break;
						case "Crystal Delight":
							CookingDomainSpecial_UnitCrystalTransform.ApplyTo(obj);
							break;
					}
					break;
				}
			}
		}
	}

	public class GauntletContainer : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == GetInventoryActionsAlwaysEvent.ID || ID == InventoryActionEvent.ID || ID == CanSmartUseEvent.ID || ID == CommandSmartUseEvent.ID || ID == OnDestroyObjectEvent.ID;
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			if (The.Game.GetSystem<GauntletSystem>().complete)
			{
				E.AddAction("Open", "open", "GetRewards", null, 'o', Default: 100);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "GetRewards")
			{
				GiveItems();
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanSmartUseEvent E)
		{
			return false;
		}

		public override bool HandleEvent(CommandSmartUseEvent E)
		{
			GiveItems();
			return false;
		}

		public override bool HandleEvent(OnDestroyObjectEvent E)
		{
			if (!ParentObject.HasIntProperty("GaveItems"))
			{
				The.Player.ReceiveObject(GameObjectFactory.Factory.CreateObject("Merchant's Token"));
			}
			return base.HandleEvent(E);
		}

		private void GiveItems()
		{
			GameObject[] items = GetItems(ParentObject.CurrentZone.NewTier, (The.Game.GetSystem<GauntletSystem>().wave + 1) % 3);
			List<string> names = new List<string>(items.Length);
			List<IRenderable> icons = new List<IRenderable>(items.Length);

			for (int i = 0; i < items.Length; i++)
			{
				names.Add(items[i].GetDisplayName(1120, AsIfKnown: true));
				icons.Add(items[i].Render);
			}

			List<(int, int)> choices = Popup.PickSeveral("Choose 3 items", Options: names, Stacks: null, Icons: icons, Amount: 3, Spacing: 60);
			if (!choices.IsNullOrEmpty())
			{
				for (int i = 0; i < choices.Count; i++)
				{
					items[choices[i].Item1].MakeUnderstood();
					The.Player.ReceiveObject(items[choices[i].Item1]);
				}
				ParentObject.SetIntProperty("GaveItems", 1);
			}

			ParentObject.TeleportSwirl(IsOut: true);
			ParentObject.Obliterate();
		}

		private GameObject[] GetItems(int tier, int extra)
		{
			List<GameObject> list = new List<GameObject>();

			Add(list, "Melee Weapons " + tier + "C");
			Add(list, "Melee Weapons " + tier + "R");
			Add(list, "Missile " + tier);

			Add(list, "Armor " + tier + "C");
			Add(list, "Armor " + tier + "R");
			Add(list, "Artifact " + tier + "R");

			if (extra != 0)
			{
				Add(list, extra == 2 ? "Meds " + tier : "Artifact " + tier + "C");
			}

			if (The.Player.IsTrueKin())
			{
				GameObject item = GameObjectFactory.Factory.CreateObject("CyberneticsCreditWedge");
				item.IntProperty["GauntletObject"] = 2;
				list.Add(item);

				if (extra == 2)
				{
					Add(list, "Cybernetics" + tier);
				}
			}

			return list.ToArray();
		}

		private void Add(List<GameObject> list, string table)
		{
			GameObject item = PopulationManager.CreateOneFrom(table);
			item.IntProperty["GauntletObject"] = 2;
			list.Add(item);
		}
	}
}