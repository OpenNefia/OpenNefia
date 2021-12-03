using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Logic
{
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
        public readonly string ID;

        public Verb(string id)
        {
            ID = id;
        }

        public int CompareTo(object? obj)
        {
            if (obj is not Verb otherVerb)
                return -1;

            return string.Compare(ID, otherVerb.ID);
        }
    }
}
