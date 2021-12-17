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

        public void RunTitleScreen()
        {
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
                                var map = InitMap();

                                _mapManager.ChangeActiveMap(map.Id);
                                _fieldLayer.SetMap(map);

                                _fieldLayer.Query();

                                _mapManager.UnloadMap(map.Id);
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

        private IMap InitMap()
        {
            var mapId = _mapManager.GetFreeMapId();
            var map = _mapBlueprints.LoadBlueprint(mapId, new ResourcePath("/Elona/Map/Test.yml"));

            var player = _entityManager.SpawnEntity(Protos.Chara.Putit, map.AtPos(2, 2));
            player.AddComponent<PlayerComponent>();
            _gameSessionManager.Player = player;
            map.MemorizeAllTiles();

            return map;
        }
    }
}
