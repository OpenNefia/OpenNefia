using OpenNefia.Core.UserInterface;
using OpenNefia.XamlNameGenerator.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNefia.XamlNameGenerator.Generator
{
    internal static class AccessHelpers
    {
        internal static string GetAccessString(this ResolvedName info)
        {
            string accessStr;
            switch (info.Access)
            {
                case AccessLevel.Public:
                    accessStr = "public";
                    break;
                case AccessLevel.Private:
                    accessStr = "private";
                    break;
                case AccessLevel.Protected:
                    accessStr = "protected";
                    break;
                case AccessLevel.PrivateProtected:
                    accessStr = "private protected";
                    break;
                case AccessLevel.Internal:
                    accessStr = "internal";
                    break;
                case AccessLevel.ProtectedInternal:
                    accessStr = "protected internal";
                    break;
                default:
                    throw new ArgumentException($"Invalid access level \"{Enum.GetName(typeof(AccessLevel), info.Access)}\" " +
                                                $"for control {info.Name}.");
            }

            return accessStr;
        }
    }
}
