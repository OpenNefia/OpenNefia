﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Logic
{
    public delegate TurnResult VerbAction();

    /// <summary>
    /// A Verb is an action that can be applied to an entity. Examples
    /// include throwing, praying, opening, eating, and drinking.
    /// 
    /// This allows querying an entity for a list of verbs that can be
    /// applied to it with events. Making an entity drinkable or throwable
    /// by the player then becomes as simple as creating a component/system that adds
    /// the verb to the <see cref="VerbSystem.GetVerbsEventArgs">'s valid 
    /// verb list.
    /// 
    /// Note that this system is mainly relevant to the player, and is meant to be
    /// used for UI interactivity for the time being. How to allow the AI to dynamically
    /// increase its repertoire of actions hasn't been decided yet.
    /// </summary>
    public sealed class Verb : IComparable
    {
        /// <summary>
        /// What type of verb this is.
        /// </summary>
        public readonly string VerbType;

        /// <summary>
        /// Human-readable name of the verb.
        /// </summary>
        public string DisplayName = string.Empty;

        /// <summary>
        ///     This is an action that will be run when the verb is "acted" out.
        /// </summary>
        /// <remarks>
        ///     This delegate probably just points to some function in the system assembling this verb.
        /// </remarks>
        public readonly VerbAction Act;

        /// <summary>
        ///     Determines the priority of the verb.
        /// </summary>
        /// <remarks>
        ///     Smaller is higher priority (gets executed preferentially).
        /// </remarks>
        public int Priority;

        public Verb(string verbType, string name, VerbAction act)
        {
            VerbType = verbType;
            DisplayName = name;
            Act = act;
        }

        public int CompareTo(object? obj)
        {
            if (obj is not Verb otherVerb)
                return -1;

            var comp = string.Compare(VerbType, otherVerb.VerbType);
            if (comp != 0)
                return comp;

            return string.Compare(DisplayName, otherVerb.DisplayName);
        }
    }

    /// <summary>
    /// Sent to an entity via <see cref="VerbSystem.GetVerbsEventArgs"> to gather
    /// its list of verbs.
    /// </summary>
    public sealed class VerbRequest
    {
        /// <summary>
        /// What type of verb is being requested.
        /// </summary>
        public readonly string VerbType;

        public VerbRequest(string verbType)
        {
            VerbType = verbType;
        }
    }
}
