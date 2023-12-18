using Jace;
using OpenNefia.Core.Log;

namespace OpenNefia.Core.Formulae
{
    public interface IFormulaEngine
    {
        double Calculate(Formula f, IDictionary<string, double> variables, double fallback = 0f);
    }
    
    public sealed class FormulaEngine : IFormulaEngine
    {
        private readonly CalculationEngine _jaceEngine = new();
        
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