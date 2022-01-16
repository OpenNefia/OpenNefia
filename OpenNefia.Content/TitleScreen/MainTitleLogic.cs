using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Factions;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Areas;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.ContentPack;
using OpenNefia.Content.Skills;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Areas;
using System.Linq;
using OpenNefia.Content.Maps;
using OpenNefia.Content.ConfigMenu;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.DisplayName;
using System.Numerics;

namespace OpenNefia.Content.TitleScreen
{
    public interface IMainTitleLogic
    {
        void RunTitleScreen();
    }

    public class MainTitleLogic : IMainTitleLogic
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSessionManager = default!;
        [Dependency] private readonly IFieldLayer _fieldLayer = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ICharaMakeLogic _charaMakeLogic = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private void Startup()
        {
            _fieldLayer.Startup();
        }

        public void RunTitleScreen()
        {
            Startup();

            var action = TitleScreenAction.ReturnToTitle;

            while (action != TitleScreenAction.Quit)
            {
                using (ITitleScreenLayer titleScreen = new TitleScreenLayer())
                {
                    var bg = new TitleScreenBGLayer();
                    _uiManager.PushLayer(bg);
                    var result = _uiManager.Query(titleScreen);
                    _uiManager.PopLayer(bg);

                    Console.WriteLine(result);

                    if (result.HasValue)
                    {
                        action = result.Value.Action;
                        switch (action)
                        {
                            case TitleScreenAction.ReturnToTitle:
                                break;
                            case TitleScreenAction.StartGame:
                                // StartGame();
                                break;
                            case TitleScreenAction.Generate:
                                CreateChara();
                                break;
                            case TitleScreenAction.Options:
                                _uiManager.PushLayer(bg);
                                ShowConfigMenu();
                                _uiManager.PopLayer(bg);
                                break;
                            case TitleScreenAction.Quit:
                                break;
                        }
                    }
                    else
                    {
                        action = TitleScreenAction.ReturnToTitle;
                    }
                }
            }
        }

        private void CreateChara()
        {
            var result = _charaMakeLogic.RunCreateChara();

            if (result is CharaMakeLogicResult.NewPlayerIncarnated newPlayerResult)
            {
                StartNewGame(newPlayerResult.NewPlayer);
            }
        }

        private void ShowConfigMenu()
        {
            ConfigMenuHelpers.QueryDefaultConfigMenu(_prototypeManager, _uiManager, _config);
        }

        /// <summary>
        /// Does one-time setup of global areas. This is for setting up areas like towns to
        /// be able to generate escort/other quests between them when a new save is being 
        /// initialized.
        /// </summary>
        public void InitializeGlobalAreas()
{
            DebugTools.Assert(_areaManager.LoadedAreas.Count == 0, "Areas were already initialized!");

            foreach (var (areaEntityProto, globalAreaId) in EnumerateGlobalAreas())
            {
                var areaId = areaEntityProto.GetStrongID();
                _areaManager.CreateArea(areaId, globalAreaId);
            }
        }

        private IEnumerable<(EntityPrototype, GlobalAreaId)> EnumerateGlobalAreas()
        {
            foreach (var proto in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.TryGetComponent<AreaEntranceComponent>("AreaEntrance", out var areaEntrance))
                {
                    if (areaEntrance.GlobalId != null)
                    {
                        yield return (proto, areaEntrance.GlobalId.Value);
                    }
                }
            }
        }

        private void StartNewGame(EntityUid player)
        {
            var saveName = EntitySystem.Get<IDisplayNameSystem>().GetBaseName(player);

            var save = _saveGameSerializer.InitializeSaveGame(saveName);
            _saveGameManager.CurrentSave = save;

            _gameSessionManager.Player = player;

            InitializeGlobalAreas();

            var map = _mapLoader.LoadBlueprint(new ResourcePath("/Maps/LecchoTorte/Test.yml"));
            map.MemorizeAllTiles();

            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(player);
            playerSpatial.Coordinates = map.AtPosEntity(2, 2);

            EntitySystem.Get<IRefreshSystem>().Refresh(player);
            EntitySystem.Get<SkillsSystem>().HealToMax(player);

            _mapManager.SetActiveMap(map.Id);

            _mapManager.RefreshVisibility(map);

            _saveGameSerializer.SaveGame(save);

            _uiManager.Query(_fieldLayer);

            _mapManager.UnloadMap(map.Id);

            _saveGameManager.CurrentSave = null;
        }
    }
}
