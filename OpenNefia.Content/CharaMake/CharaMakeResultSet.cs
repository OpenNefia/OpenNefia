using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeResultSet
    {
        public OrderedDictionary<ICharaMakeLayer, ICharaMakeResult> AllResults { get; } = new();
        public CharaMakeStep LastStep { get; set; }
        public IReadOnlyList<ICharaMakeLayer> AllSteps { get; } = new List<ICharaMakeLayer>();

        public CharaMakeResultSet(IEnumerable<ICharaMakeLayer> allSteps)
        {
            AllSteps = allSteps.ToList();
        }

        public bool TryGet<T>([NotNullWhen(true)] out T? resultData)
            where T : class, ICharaMakeResult
        {
            foreach (var (layer, result) in AllResults)
            {
                if (result is T)
                {
                    resultData = (T)result;
                    return true;
                }
            }
            resultData = null;
            return false;
        }

        public bool TryGet<TLayer, TResult>([NotNullWhen(true)] out TResult? resultData)
            where TLayer : ICharaMakeLayer<TResult>
            where TResult : class, ICharaMakeResult
        {
            foreach (var (layer, result) in AllResults)
            {
                if (layer is TLayer)
                {
                    resultData = (TResult)result;
                    return true;
                }
            }
            resultData = null;
            return false;
        }
    }
}