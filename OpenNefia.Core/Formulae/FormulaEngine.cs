using Jace;

namespace OpenNefia.Core.Formulae
{
    public interface IFormulaEngine
    {
        double Calculate(Formula f, Dictionary<string, double> variables);
    }
    
    public sealed class FormulaEngine : IFormulaEngine
    {
        private readonly CalculationEngine _jaceEngine = new();
        
        public double Calculate(Formula f, Dictionary<string, double> variables)
        {
            return _jaceEngine.Calculate(f.Body, variables);
        }
    }
}