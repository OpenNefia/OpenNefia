using OpenNefia.Content.Adventurer;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Roles;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Oracle
{
    public interface IOracleSystem : IEntitySystem
    {
        List<string> ArtifactLocations { get; }

        string? GetOracleText(EntityUid item);
    }

    public sealed class OracleSystem : EntitySystem, IOracleSystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IAdventurerSystem _adv = default!;
        [Dependency] private readonly IWorldSystem _world = default!;

        [RegisterSaveData("Elona.OracleSystem.ArtifactLocations")]
        public List<string> ArtifactLocations { get; } = new();

        public override void Initialize()
        {
            SubscribeComponent<ItemComponent, EntityBeingGeneratedEvent>(AddOracleText);
        }

        private void AddOracleText(EntityUid item, ItemComponent itemComp, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> shade2/item.hsp:628 	itemMemory(1,dbId)++ ...
            var noOracle = false;
            if (args.GenArgs.TryGet<ItemGenArgs>(out var itemGenArgs))
            {
                noOracle = itemGenArgs.NoOracle || itemGenArgs.IsShop;
            }

            if (!noOracle && CompOrNull<QualityComponent>(item)?.Quality == Quality.Unique)
            {
                var text = GetOracleText(item);
                if (text != null)
                ArtifactLocations.Add(text);
            }
            // <<<<<<<< shade2/item.hsp:636  	} ...
        }

        public string? GetOracleText(EntityUid item)
        {
            if (!TryMap(item, out var map))
                return null;

            var date = _world.State.GameDate;

            if (_lookup.TryGetOwningEntity<CharaComponent>(item, out var owner)
                && HasComp<RoleAdventurerComponent>(owner.Value)
                && _adv.TryGetArea(owner.Value, out var area))
            {
                return Loc.GetString("Elona.Oracle.WasHeldBy", ("item", item), ("owner", owner.Value), ("map", map), ("day", date.Day), ("month", date.Month), ("year", date.Year));
            }

            return Loc.GetString("Elona.Oracle.WasCreatedAt", ("item", item), ("map", map), ("day", date.Day), ("month", date.Month), ("year", date.Year));
        }
    }
}