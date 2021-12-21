using System;
using System.Reflection;
using System.Text;

namespace Why.Core.Utility
{
    public static class ExceptionHelpers
    {
        public static string ToStringWithLoaderExceptions(this Exception exception)
        {
            switch (exception)
            {
                case ReflectionTypeLoadException reflectionTypeLoad:
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(reflectionTypeLoad.ToString());
                    if (reflectionTypeLoad.LoaderExceptions != null)
                    {
                        var i = 0;
                        foreach (var inner in reflectionTypeLoad.LoaderExceptions)
                        {
                            if (inner != null)
                            {
                                builder.Append($"---> (Loader Exception #{i} {inner.ToStringWithLoaderExceptions()}\n<---");
                                i += 1;
                            }
                        }
                    }

                    return builder.ToString();
                }

                default:
                    return exception.ToString();
            }
        }
    }
}
