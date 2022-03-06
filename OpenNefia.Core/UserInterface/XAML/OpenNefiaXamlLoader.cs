using System;

namespace OpenNefia.Core.UserInterface.XAML
{
    public static class OpenNefiaXamlLoader
    {
        /// <summary>
        /// Calls to this method are replaced inline with the correct XAML loading code
        /// by the XAML code generator.
        /// </summary>
        public static void Load(object obj)
        {
            throw new Exception(
                $"No precompiled XAML found for {obj.GetType()}, make sure to specify Class or name your class the same as your .xaml");
        }
    }
}
