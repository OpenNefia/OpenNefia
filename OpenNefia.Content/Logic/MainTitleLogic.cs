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

namespace OpenNefia.Content.Logic
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
        [Dependency] private readonly IMapBlueprintLoader _mapBlueprints = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

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
                    var result = titleScreen.Query();
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

        private void StartGame()
        {
            var saveHeader = new SaveGameHeader("ruin");
            var savePath = ResourcePath.Root / Guid.NewGuid().ToString();
            var save = _saveGameManager.CreateSave(savePath, saveHeader);
            _saveGameManager.SetCurrentSave(save);

            var map = InitMap();

            _mapManager.SetActiveMap(map.Id);

            _fieldLayer.Query();

            _mapManager.UnloadMap(map.Id);
        }

        private IMap InitMap()
        {
            var mapId = _mapManager.GetFreeMapId();
            var map = _mapBlueprints.LoadBlueprint(mapId, new ResourcePath("/Maps/LecchoTorte/Test.yml"));

            var player = _entityManager.SpawnEntity(Protos.Chara.RedBaptist, map.AtPos(2, 2));
            player.AddComponent<PlayerComponent>();
            _gameSessionManager.Player = player;
            map.MemorizeAllTiles();

            return map;
        }
    }
}
