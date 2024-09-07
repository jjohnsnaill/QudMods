using HarmonyLib;
using System;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;
using ConsoleLib.Console;
using XRL.Core;
using Mods.Gauntlet;
using XRL.Annals;
using HistoryKit;
using XRL.World.Skills.Cooking;

namespace XRL.World.ZoneFactories
{
	[HarmonyPatch]
	public class GauntletWorldZoneFactory : IZoneFactory
	{
		public override Zone BuildZone(ZoneRequest Request)
		{
			if (Request.IsWorldZone)
			{
				Zone worldZone = new Zone(80, 25);
				worldZone.GetCells().ForEach(delegate (Cell c)
				{
					c.AddObject("TerrainTzimtzlum");
				});
				return worldZone;
			}

			string id = ZoneID.Assemble("Gauntlet", 40, 12, 1, 1, Request.Z);
			if (Request.WorldX != 40 || Request.WorldY != 12 || Request.X != 1 || Request.Y != 1)
			{
				return The.ZoneManager.GetZone(id);
			}

			Zone zone = new Zone(80, 25);

			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					GameObject widget = GameObjectFactory.Factory.CreateObject("Widget");
					widget.AddPart<GauntletCell>();
					widget.IntProperty["GauntletObject"] = 3;
					zone.Map[j][i].AddObject(widget);

					ConcreteFloor.PaintCell(zone.Map[j][i]);
				}
			}

			zone.ZoneID = id;
			zone.DisplayName = "The Gauntlet";
			zone.IncludeContextInZoneDisplay = false;
			zone.IncludeStratumInZoneDisplay = false;
			zone.SetMusic("BarathrumsStudy");
			zone.Built = true;

			The.Game.ZoneManager.SetZoneProperty(Request.ZoneID, "SpecialUpMessage", "You’re in a pocket dimension with no worldmap.");

			return zone;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QudChooseStartingLocationModule), "shouldBeEnabled")]
		static bool GauntletSetLocation(QudChooseStartingLocationModule __instance, ref bool __result)
		{
			if (__instance.builder?.GetModule<QudGamemodeModule>()?.data?.Mode == "Gauntlet")
			{
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QudGameBootModule), "bootGame")]
		static bool GauntletBoot(XRLGame game, EmbarkInfo info, QudChooseStartingLocationModule __instance)
		{
			if (__instance.builder?.GetModule<QudGamemodeModule>()?.data?.Mode != "Gauntlet")
			{
				return true;
			}

			info.fireBootEvent(QudGameBootModule.BOOTEVENT_BEGINBOOT, game);

			GameManager.Instance.PushGameView("WorldCreationProgress");
			Loading.SetHideLoadStatus(true);
			WorldCreationProgress.Begin(7);
			WorldCreationProgress.NextStep("Initialize game environment", 2);

			game.GameID = Guid.NewGuid().ToString();
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_AFTERBEGINBOOT, game);
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_CACHERESET, game);

			game.GetCacheDirectory();
			game.bZoned = false;
			The.Core.ResetGameBasedStaticCaches();
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_AFTERCACHERESET, game);
			QudGameBootModule.SeedGame(game, info);

			info.fireBootEvent(QudGameBootModule.BOOTEVENT_INITIALIZESYSTEMS, game);
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_AFTERINITIALIZESYSTEMS, game);
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_INITIALIZEGAMESTATESINGLETONS, game);

			Stat.ReseedFrom("GAMESTATECookingGameState");
			game.SetObjectGameState("CookingGameState", new CookingGameState());

			Stat.ReseedFrom("HISTORYINIT");

			History history = new History(1L);
			QudHistoryFactory.InitializeHistory(history);

			for (int i = 1; i <= 5; i++)
			{
				QudHistoryFactory.GenerateNewRegions(history, 1, i);

				HistoricEntity sultan = history.GetNewEntity(history.currentYear);
				sultan.ApplyEvent(new InitializeSultan(i));
				sultan.ApplyEvent(new SetEntityProperty("isCandidate", "true"));
				sultan.ApplyEvent(new FoundAsBabe(), 1);
				QudHistoryFactory.FillOutLikedFactions(sultan);
				QudHistoryFactory.GenerateCultName(sultan, history);
			}
			QudHistoryFactory.AddSultanCultNames(history);
			QudHistoryFactory.AddResheph(history);
			QudHistoryFactory.GenerateNewVillage(history, 0, VillageZero: true);

			game.sultanHistory = history;

			info.fireBootEvent(QudGameBootModule.BOOTEVENT_INITIALIZEWORLDS, game);
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_AFTERINITIALIZEWORLDS, game);

			game.Running = true;

			WorldCreationProgress.StepProgress("Adding player to world");
			if (string.IsNullOrEmpty(game.PlayerName?.Trim()))
			{
				game.PlayerName = info.fireBootEvent(QudGameBootModule.BOOTEVENT_GENERATERANDOMPLAYERNAME, game, The.Core.GenerateRandomPlayerName(info.getModule<QudSubtypeModule>().data.Subtype));
			}

			GameObject player = info.fireBootEvent<GameObject>(QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT, game, null);
			player = info.fireBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT, game, player);

			Render pRender = player.Render;
			string tile = info.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE, game, null) ?? pRender.Tile;
			string text = info.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEFOREGROUND, game, null) ?? pRender.GetForegroundColor();
			string text2 = info.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEBACKGROUND, game, null) ?? pRender.GetBackgroundColor();
			string detailColor = info.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL, game, null) ?? pRender.DetailColor;
			pRender.Tile = tile;
			pRender.ColorString = "&" + text + "^" + text2;
			pRender.DetailColor = detailColor;

			foreach (Type item3 in ModManager.GetTypesWithAttribute(typeof(PlayerMutator)))
			{
				Stat.ReseedFrom("PLAYERMUTATOR" + item3.Name);
				(Activator.CreateInstance(item3) as IPlayerMutator)?.mutate(player);
			}
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_AFTERBOOTPLAYEROBJECT, game, player);

			player.StripContents(true, true);
			player.IntProperty["GauntletObject"] = 1; // don't end the wave if you dominated something
			player.AddPart<GauntletPart>();
			player.RemovePart<OpeningStory>();

			WorldCreationProgress.StepProgress("Starting game!", Last: true);
			Stat.ReseedFrom("InitialSeeds");
			game.SetIntGameState("RandomSeed", Stat.Rnd.Next());
			Stat.Rnd = new Random(game.GetIntGameState("RandomSeed"));
			game.SetIntGameState("RandomSeed2", Stat.Rnd.Next());
			Stat.Rnd2 = new Random(game.GetIntGameState("RandomSeed2"));
			game.SetIntGameState("RandomSeed3", Stat.Rnd.Next());
			game.SetIntGameState("RandomSeed4", Stat.Rnd.Next());
			Stat.Rnd4 = new Random(game.GetIntGameState("RandomSeed4"));
			Loading.SetHideLoadStatus(hidden: false);
			MetricsManager.LogInfo("Cached objects: " + game.ZoneManager.CachedObjects.Count);
			MemoryHelper.GCCollect();

			Zone zone = XRLCore.Core?.Game?.ZoneManager?.GetZone("Gauntlet.40.12.1.1.10");
			zone.GetCell(40, 12).AddObject(player);
			game.Player.Body = player;

			GauntletSystem system = new GauntletSystem();
			system.zone = zone;
			system.complete = true;
			The.Game.AddSystem(system);

			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					ConcreteFloor.PaintCell(zone.GetCell(j, i));
				}
			}

			Keyboard.ClearInput();
			info.fireBootEvent(QudGameBootModule.BOOTEVENT_GAMESTARTING, game);
			Stat.ReseedFrom("GameStart");

			system.WaveEnd();

			return false;
		}
	}
}