using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

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
                                _mapManager.RegisterMap(map);

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

        private Map InitMap()
        {
            var map = new Map(25, 25);

            var player = _entityManager.SpawnEntity(new("Putit"), map.AtPos(2, 2));
            player.AddComponent<PlayerComponent>();
            _gameSessionManager.Player = player;
            map.Clear(TilePrototypeOf.Grass);
            map.MemorizeAll();

            for (int i = 0; i < 10; i++)
            {
                _entityManager.SpawnEntity(new("Yeek"), map.AtPos(i + 5, 5));
                _entityManager.SpawnEntity(new("Potion"), map.AtPos(i + 5, 2));
                _entityManager.SpawnEntity(new("Computer"), map.AtPos(i + 5, 3));
            }

            return map;
        }
    }
}