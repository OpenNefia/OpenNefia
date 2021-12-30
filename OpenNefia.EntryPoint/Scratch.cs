using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;

var entMan = IoCManager.Resolve<IEntityManager>();

var mapMan = IoCManager.Resolve<IMapManager>();
var gameSess = IoCManager.Resolve<IGameSessionManager>();
var mapLoad = IoCManager.Resolve<IMapBlueprintLoader>();

Loc.SwitchLanguage(LanguagePrototypeOf.Japanese);
