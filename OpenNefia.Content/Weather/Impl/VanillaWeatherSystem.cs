using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Weather;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Activity;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Food;
using Jace.Operations;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Content.EtherDisease;
using OpenNefia.Content.World;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Home;

namespace OpenNefia.Content.Weather
{
    public sealed class VanillaWeatherSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IWeatherSystem _weather = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;
        [Dependency] private readonly IFoodSystem _foods = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IEtherDiseaseSystem _etherDiseases = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IEffectSystem _effects = default!;

        public override void Initialize()
        {
            SubscribeComponent<WeatherTypeSunnyComponent, WeatherOnChangeEvent>(Sunny_OnWeatherChange);

            SubscribeComponent<WeatherTypeEtherwindComponent, WeatherCalcTravelSpeedModifierEvent>(Etherwind_CalcTravelSpeedModifier);
            SubscribeComponent<WeatherTypeEtherwindComponent, WeatherOnChangeEvent>(Etherwind_OnWeatherChange);
            SubscribeComponent<WeatherTypeEtherwindComponent, WeatherOnTurnStartEvent>(Etherwind_OnTurnStart);
            SubscribeComponent<WeatherTypeEtherwindComponent, WeatherGetDrawableEvent>(Etherwind_GetDrawable);

            SubscribeComponent<WeatherTypeSnowComponent, WeatherCalcTravelSpeedModifierEvent>(Snow_CalcTravelSpeedModifier);
            SubscribeComponent<WeatherTypeSnowComponent, WeatherOnTravelEvent>(Snow_OnTravel);
            SubscribeComponent<WeatherTypeSnowComponent, WeatherOnChangeEvent>(Snow_OnWeatherChange);
            SubscribeComponent<WeatherTypeSnowComponent, WeatherGetDrawableEvent>(Snow_GetDrawable);

            SubscribeComponent<WeatherTypeRainComponent, WeatherCalcTravelSpeedModifierEvent>(Rain_CalcTravelSpeedModifier);
            SubscribeComponent<WeatherTypeRainComponent, WeatherOnChangeEvent>(Rain_OnWeatherChange);
            SubscribeComponent<WeatherTypeRainComponent, WeatherOnTurnStartEvent>(Rain_OnTurnStart);
            SubscribeComponent<WeatherTypeRainComponent, WeatherGetDrawableEvent>(Rain_GetDrawable);
            SubscribeComponent<WeatherTypeRainComponent, WeatherCalcMapShadowEvent>(Rain_CalcMapShadow);

            SubscribeComponent<WeatherTypeHardRainComponent, WeatherCalcTravelSpeedModifierEvent>(HardRain_CalcTravelSpeedModifier);
            SubscribeComponent<WeatherTypeHardRainComponent, WeatherOnTravelEvent>(HardRain_OnTravel);
            SubscribeComponent<WeatherTypeHardRainComponent, WeatherOnChangeEvent>(HardRain_OnWeatherChange);
            SubscribeComponent<WeatherTypeHardRainComponent, WeatherOnTurnStartEvent>(HardRain_OnTurnStart);
            SubscribeComponent<WeatherTypeHardRainComponent, WeatherGetDrawableEvent>(HardRain_GetDrawable);
            SubscribeComponent<WeatherTypeHardRainComponent, WeatherCalcMapShadowEvent>(HardRain_CalcMapShadow);
        }

        #region Elona.Sunny

        private void Sunny_OnWeatherChange(EntityUid uid, WeatherTypeSunnyComponent component, WeatherOnChangeEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/main.hsp:589 		if p=weatherSunny{ ...
            var player = _gameSession.Player;

            if (_feats.HasFeat(player, Protos.Feat.EtherRain) && _rand.OneIn(4))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Feat.DrawCloud", ("player", player)));
                args.Handle(Protos.Weather.Rain);
                return;
            }

            if (!_weather.TryGetPositionInWorldMap(player, out var coords))
                return;

            // TODO: Look up what climate we're in inside this area
            // for now these coordinates assume north tyris
            var isSnowClimate = coords.Value.Position.X > 65 && coords.Value.Position.Y < 10;

            if (isSnowClimate)
            {
                if (_rand.OneIn(2))
                {
                    _mes.Display(Loc.GetString("Elona.Weather.Types.Snow.Starts"));
                    args.Handle(Protos.Weather.Snow);
                    return;
                }
            }
            else
            {
                if (_rand.OneIn(10))
                {
                    _mes.Display(Loc.GetString("Elona.Weather.Types.Rain.Starts"));
                    args.Handle(Protos.Weather.Rain);
                    return;
                }
                if (_rand.OneIn(40))
                {
                    _mes.Display(Loc.GetString("Elona.Weather.Types.HardRain.Starts"));
                    args.Handle(Protos.Weather.HardRain);
                    return;
                }
                if (_rand.OneIn(60))
                {
                    _mes.Display(Loc.GetString("Elona.Weather.Types.Snow.Starts"));
                    args.Handle(Protos.Weather.Snow);
                    return;
                }
            }
            // <<<<<<<< shade2/main.hsp:599 			} ..
        }

        #endregion

        #region Elona.Etherwind

        private void Etherwind_OnWeatherChange(EntityUid uid, WeatherTypeEtherwindComponent component, WeatherOnChangeEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/main.hsp:607 		if p=weatherEther{ ...
            if (_rand.OneIn(2))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.EtherWind.Stops"));
                args.Handle(Protos.Weather.Sunny);
                return;
            }
            // <<<<<<<< shade2/main.hsp:609 			} ..
        }

        private void Etherwind_CalcTravelSpeedModifier(EntityUid uid, WeatherTypeEtherwindComponent component, WeatherCalcTravelSpeedModifierEvent args)
        {
            // >>>>>>>> shade2/proc.hsp:790 		if gWeather=weatherEther	:cActionPeriod(cc)=cAct ...
            args.OutTurns = (int)(args.OutTurns * 0.5);
            // <<<<<<<< shade2/proc.hsp:790 		if gWeather=weatherEther	:cActionPeriod(cc)=cAct ..
        }

        private bool CanBeAffectedByEtherwind(EntityUid entity)
        {
            return _gameSession.IsPlayer(entity);
        }

        private bool IsIndoorsMapAffectedByEtherwind(IMap map)
        {
            if (!TryArea(map, out var area))
                return true;

            // TODO make into its own component
            return !HasComp<AreaHomeComponent>(area.AreaEntityUid) 
                && !HasComp<MapTypeShelterComponent>(map.MapEntityUid);
        }

        private void Etherwind_OnTurnStart(EntityUid weatherUid, WeatherTypeEtherwindComponent component, WeatherOnTurnStartEvent args)
        {
            // >>>>>>>> elona122/shade2/main.hsp:764 		if gWeather=weatherEther{ ...
            if (!CanBeAffectedByEtherwind(args.Entity) || !TryMap(args.Entity, out var map))
                return;

            var isProtected = TryComp<CommonProtectionsComponent>(args.Entity, out var prot)
                && prot.IsProtectedFromEtherwind.Buffed;

            var isIndoors = CompOrNull<MapCommonComponent>(map.MapEntityUid)?.IsIndoors ?? false;
            if (isIndoors)
            {
                if (_rand.OneIn(1500) && IsIndoorsMapAffectedByEtherwind(map))
                {
                    _etherDiseases.ModifyCorruption(args.Entity, 10);
                }
            }
            else
            {
                if (_rand.OneIn(2))
                {
                    if (!isProtected)
                    {
                        _etherDiseases.ModifyCorruption(args.Entity, 5 + int.Clamp(_world.State.PlayTurns / 20000, 0, 15));
                    }
                    else
                    {
                        if (_rand.OneIn(10))
                        {
                            _etherDiseases.ModifyCorruption(args.Entity, 5);
                        }
                    }
                }

                if (!isProtected || _rand.OneIn(4))
                {
                    if (_rand.OneIn(2000))
                    {
                        _effects.Apply<EffectMutation>(args.Entity, args.Entity, Spatial(args.Entity).Coordinates, weatherUid, new EffectArgSet() { Power = 100 });
                    }
                }
            }


            // <<<<<<<< elona122/shade2/main.hsp:772 		} ...
        }

        private void Etherwind_GetDrawable(EntityUid uid, WeatherTypeEtherwindComponent component, WeatherGetDrawableEvent args)
        {
            args.OutDrawable = new EtherwindWeatherDrawable();
        }

        #endregion

        #region Elona.Snow

        private void Snow_OnWeatherChange(EntityUid uid, WeatherTypeSnowComponent component, WeatherOnChangeEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/main.hsp:610 		if p=weatherSnow{ ...
            if (_rand.OneIn(3))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.Snow.Stops"));
                args.Handle(Protos.Weather.Sunny);
                return;
            }
            // <<<<<<<< shade2/main.hsp:612 			} ..
        }

        private void Snow_CalcTravelSpeedModifier(EntityUid uid, WeatherTypeSnowComponent component, WeatherCalcTravelSpeedModifierEvent args)
        {
            // >>>>>>>> shade2/proc.hsp:789 		if (gWeather=weatherSnow)or(tRole(map(cx(cc),cy( ...

            // Snow tiles will also apply the speed penalty, so don't accidentally double it.
            // if (gWeather=weatherSnow)or(tRole(map(cx(cc),cy(cc),0))=tSnow) : cActionPeriod(cc)=cActionPeriod(cc)*22/10
            if (_protos.Index(args.TileID).Kind != TileKind.Snow)
                args.OutTurns = (int)(args.OutTurns * 2.2);

            // <<<<<<<< shade2/proc.hsp:789 		if (gWeather=weatherSnow)or(tRole(map(cx(cc),cy( ..
        }

        private void Snow_OnTravel(EntityUid uid, WeatherTypeSnowComponent component, ref WeatherOnTravelEvent args)
        {
            if (TryComp<CommonProtectionsComponent>(args.Activity.Actor, out var prot) && prot.IsProtectedFromBadWeather.Buffed)
            {
                return;
            }

            if (_rand.OneIn(100) && (!prot?.IsFloating?.Buffed ?? false))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.Snow.Travel.Sound"), color: UiColors.MesSkyBlue);
                args.Activity.TurnsRemaining += 10;
            }
            if (_rand.OneIn(1000))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.Snow.Travel.Hindered"), color: UiColors.MesPurple);
                args.Activity.TurnsRemaining += 50;
            }

            if (TryComp<HungerComponent>(args.Activity.Actor, out var hunger)
                && hunger.Nutrition <= HungerLevels.Hungry
                && !hunger.IsAnorexic)
            {
                _audio.Play(Protos.Sound.Eat1, args.Activity.Actor);
                _mes.Display(Loc.GetString("Elona.Weather.Types.Snow.Travel.Eat"));
                hunger.Nutrition += 5000;
                _mes.Display(_hunger.GetNutritionMessage(hunger.Nutrition), color: UiColors.MesGreen);
                _statusEffects.Apply(args.Activity.Actor, Protos.StatusEffect.Dimming, 1000);
            }
        }

        private void Snow_GetDrawable(EntityUid uid, WeatherTypeSnowComponent component, WeatherGetDrawableEvent args)
        {
            args.OutDrawable = new SnowWeatherDrawable();
        }

        #endregion

        #region Elona.Rain

        private void Rain_OnWeatherChange(EntityUid uid, WeatherTypeRainComponent component, WeatherOnChangeEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/main.hsp:600 		if p=weatherRain{ ...
            if (_rand.OneIn(4))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.Rain.Stops"));
                args.Handle(Protos.Weather.Sunny);
                return;
            }
            if (_rand.OneIn(15))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.Rain.BecomesHeavier"));
                args.Handle(Protos.Weather.HardRain);
                return;
            }
            // <<<<<<<< shade2/main.hsp:603 			} ...   end,
        }

        private void Rain_CalcTravelSpeedModifier(EntityUid uid, WeatherTypeRainComponent component, WeatherCalcTravelSpeedModifierEvent args)
        {
            args.OutTurns = (int)(args.OutTurns * 1.3);
        }

        private void Rain_OnTurnStart(EntityUid uid, WeatherTypeRainComponent component, WeatherOnTurnStartEvent args)
        {
            // >>>>>>>> shade2/main.hsp:867 	if mField=mFieldOutdoor : if gWeather>=weatherRai ...
            var map = _mapManager.ActiveMap!;
            var isIndoors = CompOrNull<MapCommonComponent>(map.MapEntityUid)?.IsIndoors ?? false;
            if (!isIndoors)
                _statusEffects.SetTurns(args.Entity, Protos.StatusEffect.Wet, 50);
            // <<<<<<<< shade2/main.hsp:867 	if mField=mFieldOutdoor : if gWeather>=weatherRai ..
        }

        private void Rain_GetDrawable(EntityUid uid, WeatherTypeRainComponent component, WeatherGetDrawableEvent args)
        {
            args.OutDrawable = new RainWeatherDrawable(3, 1, new Vector2(2, 16));
        }

        private void Rain_CalcMapShadow(EntityUid uid, WeatherTypeRainComponent component, ref WeatherCalcMapShadowEvent args)
        {
            // >>>>>>>> shade2/map.hsp:2298 		if gWeather=weatherRain:if p<40:p=40,40,40 ...
            args.OutShadow = new Color(
                byte.Max(args.OutShadow.RByte, 40),
                byte.Max(args.OutShadow.GByte, 40),
                byte.Max(args.OutShadow.BByte, 40));
            // <<<<<<<< shade2 / map.hsp:2298       if gWeather = weatherRain:if p < 40:p = 40,40,40..
        }

        #endregion

        #region Elona.HardRain

        private void HardRain_OnWeatherChange(EntityUid uid, WeatherTypeHardRainComponent component, WeatherOnChangeEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/main.hsp:604 		if p=weatherHardRain{ ...
            if (_rand.OneIn(3))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.HardRain.BecomesLighter"));
                args.Handle(Protos.Weather.Rain);
                return;
            }
            // <<<<<<<< shade2/main.hsp:606 			} ..
        }

        private void HardRain_CalcTravelSpeedModifier(EntityUid uid, WeatherTypeHardRainComponent component, WeatherCalcTravelSpeedModifierEvent args)
        {
            // >>>>>>>> shade2/proc.hsp:788 		if gWeather=weatherHardRain	:cActionPeriod(cc)=c ...
            args.OutTurns = (int)(args.OutTurns * 1.6);
            // <<<<<<<< shade2/proc.hsp:788 		if gWeather=weatherHardRain	:cActionPeriod(cc)=c ..
        }

        private void HardRain_OnTravel(EntityUid uid, WeatherTypeHardRainComponent component, ref WeatherOnTravelEvent args)
        {
            // >>>>>>>> shade2/proc.hsp:826 	if gWeather=weatherHardRain{ ...
            if (TryComp<CommonProtectionsComponent>(args.Activity.Actor, out var prot) && prot.IsProtectedFromBadWeather.Buffed)
            {
                return;
            }

            if (_rand.OneIn(100) && (!prot?.IsFloating?.Buffed ?? false))
            {
                _mes.Display(Loc.GetString("Elona.Weather.Types.HardRain.Travel.Sound"), color: UiColors.MesSkyBlue);
                args.Activity.TurnsRemaining += 5;
            }

            if (!_statusEffects.HasEffect(args.Activity.Actor, Protos.StatusEffect.Confusion))
            {
                if (_rand.OneIn(500))
                {
                    _mes.Display(Loc.GetString("Elona.Weather.Types.HardRain.Travel.Hindered"), color: UiColors.MesPurple);
                    _statusEffects.SetTurns(args.Activity.Actor, Protos.StatusEffect.Confusion, 10);
                }
            }
            else
            {
                if (_rand.OneIn(5))
                {
                    _statusEffects.SetTurns(args.Activity.Actor, Protos.StatusEffect.Confusion, 10);
                }
            }

            _statusEffects.SetTurns(args.Activity.Actor, Protos.StatusEffect.Blindness, 3);
            // <<<<<<<< shade2/proc.hsp:846 		} ..
        }

        private void HardRain_OnTurnStart(EntityUid uid, WeatherTypeHardRainComponent component, WeatherOnTurnStartEvent args)
        {
            // >>>>>>>> shade2/main.hsp:867 	if mField=mFieldOutdoor : if gWeather>=weatherRai ...
            var map = _mapManager.ActiveMap!;
            var isIndoors = CompOrNull<MapCommonComponent>(map.MapEntityUid)?.IsIndoors ?? false;
            if (!isIndoors)
                _statusEffects.SetTurns(args.Entity, Protos.StatusEffect.Wet, 50);
            // <<<<<<<< shade2/main.hsp:867 	if mField=mFieldOutdoor : if gWeather>=weatherRai ..
        }

        private void HardRain_GetDrawable(EntityUid uid, WeatherTypeHardRainComponent component, WeatherGetDrawableEvent args)
        {
            args.OutDrawable = new RainWeatherDrawable(5, 4, new Vector2(1, 24));
        }

        private void HardRain_CalcMapShadow(EntityUid uid, WeatherTypeHardRainComponent component, ref WeatherCalcMapShadowEvent args)
        {
            // >>>>>>>> shade2/map.hsp:2299 		if gWeather=weatherHardRain:if p<65:p=65,65,65 ...
            args.OutShadow = new Color(
                byte.Max(args.OutShadow.RByte, 65),
                byte.Max(args.OutShadow.GByte, 65),
                byte.Max(args.OutShadow.BByte, 65));
            // <<<<<<<< shade2/map.hsp:2299 		if gWeather=weatherHardRain:if p<65:p=65,65,65 ...
        }

        #endregion
    }
}
