using Jace;
using OpenNefia.Core.Log;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;

namespace OpenNefia.Core.Formulae
{
    public interface IFormulaEngine
    {
        void Initialize();
        
        /// <summary>
        /// Calculates a formula. If the formula is invalid, the fallback will be returned.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="variables"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        double Calculate(Formula f, IDictionary<string, double> variables, double fallback = 0f);

        /// <summary>
        /// Caculates a formula. May throw exceptions.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        /// <exception cref="Jace.ParseException"/>
        /// <exception cref="Jace.VariableNotDefinedException"/>
        double CalculateRaw(Formula f, IDictionary<string, double> variables);
    }

    public sealed class FormulaEngine : IFormulaEngine
    {
        [Dependency] private readonly IRandom _rand = default!;

        private readonly CalculationEngine _jaceEngine = new();

        public void Initialize()
        {
            _jaceEngine.AddFunction("clamp", (double value, double min, double max) => double.Clamp(value, min, max));
            _jaceEngine.AddFunction("randInt", (double x) => _rand.Next((int)double.Floor(x)));
            _jaceEngine.AddFunction("randFloat", (double x) => _rand.NextFloat((float)x));
        }

        public double Calculate(Formula f, IDictionary<string, double> variables, double fallback = 0f)
        {
            try
            {
                return CalculateRaw(f, variables);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("formulae", ex, $"Failed to calculate formula {f}");
                return fallback;
            }
        }

        public double CalculateRaw(Formula f, IDictionary<string, double> variables)
        {
            return _jaceEngine.Calculate(f.Body, variables);
        }
    }
}