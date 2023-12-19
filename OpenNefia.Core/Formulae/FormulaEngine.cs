using Jace;
using OpenNefia.Core.Log;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;

namespace OpenNefia.Core.Formulae
{
    public interface IFormulaEngine
    {
        void Initialize();
        double Calculate(Formula f, IDictionary<string, double> variables, double fallback = 0f);
    }
    
    public sealed class FormulaEngine : IFormulaEngine
    {
        [Dependency] private readonly IRandom _rand = default!;

        private readonly CalculationEngine _jaceEngine = new();

        public void Initialize()
        {
            _jaceEngine.AddFunction("randInt", (double x) => _rand.Next((int)double.Floor(x)));
            _jaceEngine.AddFunction("randFloat", (double x) => _rand.NextFloat((float)x));
        }
        
        public double Calculate(Formula f, IDictionary<string, double> variables, double fallback = 0f)
        {
            try
            {
                return _jaceEngine.Calculate(f.Body, variables);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("formulae", ex, $"Failed to calculate formula {f}");
                return fallback;
            }
        }
    }
}