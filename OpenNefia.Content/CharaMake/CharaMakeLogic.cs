﻿using OpenNefia.Core.IoC;
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
        void RunCreateChara();
    }
    public class CharaMakeLogic : ICharaMakeLogic
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public List<ICharaMakeLayer> GetDefaultCreationSteps()
        {
            return new List<ICharaMakeLayer>
            {
                //new CharaMakeFeatWindowLayer(),
                new CharaMakeRaceSelectLayer(),
                new CharaMakeGenderSelectLayer(),
                new CharaMakeClassSelectLayer(),
                new CharaMakeAttributeRerollLayer(),
                // TODO add feat selection
                new CharaMakeAliasLayer(),
                // TODO add appearance
                // TODO add character sheet
            };
        }


        public void RunCreateChara()
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
                    Logger.DebugS("charamake", $"Character creation complete, values:" + Environment.NewLine 
                        + string.Join(Environment.NewLine, data.CharaData.SelectMany(x => x.Value.Select(y => $"{y.Key}: {y.Value}"))));

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
                    Logger.WarningS("charamake", $"Create char step for type {type} didn't set a result, aborting.");
                    step = CharaMakeStep.Cancel;
                }

                step = result.Value.Step;
                data.LastStep = step;
                switch(step)
                {
                    case CharaMakeStep.GoBack:
                        if (data.CharaData.ContainsKey(type))
                            data.CharaData.Remove(type);
                        stepIndex--;
                        if (stepIndex < 0)
                            step = CharaMakeStep.Cancel;
                        break;
                    case CharaMakeStep.Cancel:
                        break;
                    default:
                        data.CharaData[type] = result.Value.Added;
                        stepIndex++;
                        break;
                }
            }
        }
    }
}
