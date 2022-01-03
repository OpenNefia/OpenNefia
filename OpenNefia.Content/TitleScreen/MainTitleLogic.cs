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
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.ContentPack;
using OpenNefia.Content.Skills;

namespace OpenNefia.Content.TitleScreen
{
    public interface IMainTitleLogic
    {
        void RunTitleScreen();
    }

    public class MainTitleLogic : IMainTitleLogic
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSessionManager = default!;
        [Dependency] private readonly IFieldLayer _fieldLayer = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IModLoader _modLoader = default!;

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
                    var result = _uiManager.Query(titleScreen);
                    Console.WriteLine(result);

                    if (result.HasValue)
                    {
                        action = result.Value.Action;
                        switch (action)
                        {
                            case TitleScreenAction.ReturnToTitle:
                                break;
                            case TitleScreenAction.StartGame:
                                StartGame();
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

        private SaveGameHeader MakeSaveGameHeader()
        {
            var engineVersion = Core.Engine.Version;
            var engineCommitHash = "??????";
            var assemblyVersions = new Dictionary<string, Version>();

            foreach (var assembly in _modLoader.LoadedModules)
            {
                if (assembly == typeof(Core.Engine).Assembly)
                    continue;

                var name = assembly.GetName()!;
                assemblyVersions.Add(name.FullName, name.Version!);
            }

            return new SaveGameHeader("ruin", engineVersion, engineCommitHash, assemblyVersions);
        }

        private void StartGame()
        {
            var saveHeader = MakeSaveGameHeader();
            var savePath = ResourcePath.Root / Guid.NewGuid().ToString();
            var save = _saveGameManager.CreateSave(savePath, saveHeader);
            _saveGameManager.SetCurrentSave(save);

            var map = InitMap();

            _mapManager.SetActiveMap(map.Id);

            _saveGameSerializer.SaveGame(save);

            _uiManager.Query(_fieldLayer);

            _mapManager.UnloadMap(map.Id);
        }

        private IMap InitMap()
        {
            var mapId = _mapManager.GetFreeMapId();
            var map = _mapLoader.LoadBlueprint(mapId, new ResourcePath("/Maps/LecchoTorte/Test.yml"));

            var player = EntitySystem.Get<IEntityGen>().SpawnEntity(Chara.Sailor, map.AtPos(2, 2))!;
            player.AddComponent<PlayerComponent>();
            _gameSessionManager.Player = player;

            var skills = _entityManager.EnsureComponent<SkillsComponent>(player.Uid);
            skills.Skills[Skill.StatConstitution].Level = 200;
            skills.Skills[Skill.StatLife].Level = 200;
            EntitySystem.Get<IRefreshSystem>().Refresh(player.Uid);
            EntitySystem.Get<SkillsSystem>().HealToMax(player.Uid);

            map.MemorizeAllTiles();

            return map;
        }
    }
}
