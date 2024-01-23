using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class CommonProtectionsComponent : Component, IComponentRefreshable
    {
        // TODO figure out how traits and temporary flags will work
        // slots? refreshing?

        [DataField]
        public Stat<bool> IsProtectedFromRottenFood { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsProtectedFromTheft { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsProtectedFromCurse { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsProtectedFromMutation { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsProtectedFromEtherwind { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsProtectedFromBadWeather { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsImmuneToMines { get; set; } = new(false);

        // TODO move
        [DataField]
        public Stat<bool> CanCatchGodSignals { get; set; } = new(false);

        // TODO move
        [DataField]
        public Stat<bool> CanDetectReligion { get; set; } = new(false);

        // TODO move
        // TODO implement status effect gravity
        [DataField]
        public Stat<bool> IsFloating { get; set; } = new(false);

        public void Refresh()
        {
            IsProtectedFromRottenFood.Reset();
            IsProtectedFromTheft.Reset();
            IsProtectedFromCurse.Reset();
            IsProtectedFromMutation.Reset();
            IsProtectedFromEtherwind.Reset();
            IsProtectedFromBadWeather.Reset();
            IsImmuneToMines.Reset();
            CanCatchGodSignals.Reset();
            CanDetectReligion.Reset();
            IsFloating.Reset();
        }
    }
}