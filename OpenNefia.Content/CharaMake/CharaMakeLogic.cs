using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public interface ICharaMakeLogic
    {
        void RunCreateChar();
    }
    public class CharaMakeLogic : ICharaMakeLogic
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public List<ICharaMakeLayer> GetDefaultCreationSteps()
        {
            return new List<ICharaMakeLayer>
            {
                new CharaMakeRaceClassLayer(),
                new CharaMakeAliasLayer(),
                new CharaMakeBackgroundLayer(), //forgot past/background was elona+ stuff
                
            };
        }


        public void RunCreateChar()
        {
            var steps = GetDefaultCreationSteps();
            var data = new CharaMakeData();
            var stepIndex = 0;
            var step = CharaMakeStep.Continue;

            while (step != CharaMakeStep.Cancel)
            {
                if (stepIndex >= steps.Count)
                {
                    //just for testing
                    Logger.Warning($"Character creation complete, values:" + Environment.NewLine 
                        + string.Join(Environment.NewLine, data.CharData.SelectMany(x => x.Value.Select(y => $"{y.Key}: {y.Value}"))));

                    foreach(var creationStep in steps)
                    {
                        //TODO actually apply changes
                        creationStep.ApplyStep(/* character entity (?) */);
                        creationStep.Dispose();
                    }
                    break;
                }

                var currentStep = steps[stepIndex];
                var type = currentStep.GetType();
                var result = _uiManager.Query<CharaMakeResult, CharaMakeLayer, CharaMakeData>(currentStep, data);

                if (!result.HasValue)
                {
                    Logger.Warning($"Create char step for type {type} didn't set a result, aborting.");
                    step = CharaMakeStep.Cancel;
                }

                step = result.Value.Step;
                switch(step)
                {
                    case CharaMakeStep.GoBack:
                        if (data.CharData.ContainsKey(type))
                            data.CharData.Remove(type);
                        stepIndex--;
                        break;
                    case CharaMakeStep.Cancel:
                        break;
                    default:
                        data.CharData[type] = result.Value.Added;
                        stepIndex++;
                        break;
                        
                }
            }
        }
    }
}
