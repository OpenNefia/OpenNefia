using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Maps;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.CharaMake
{
    public abstract record CharaMakeLogicResult
    {
        public sealed record NewPlayerIncarnated(EntityUid NewPlayer) : CharaMakeLogicResult;
        public sealed record Canceled() : CharaMakeLogicResult;
    }

    public interface ICharaMakeLogic
    {
        CharaMakeLogicResult RunCreateChara();
    }

    public class CharaMakeLogic : ICharaMakeLogic
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        /// <summary>
        /// This charamake result must be available to the charamake process once it finishes.
        /// It must be the <see cref="EntityUid"/> of the new player character.
        /// </summary>
        public const string PlayerEntityResultName = "playerEntity";

        public List<CharaMakeLayer> GetDefaultCreationSteps()
        {
            return new List<CharaMakeLayer>
            {
                new CharaMakeRaceSelectLayer(),
                new CharaMakeGenderSelectLayer(),
                new CharaMakeClassSelectLayer(),
                new CharaMakeAttributeRerollLayer(),
                new CharaMakeFeatWindowLayer(),
                new CharaMakeAliasLayer(),
                new CharaMakeAppearanceLayer(),

                // This step should always be last.
                new CharaMakeCharaSheetLayer()
            };
        }

        public CharaMakeLogicResult RunCreateChara()
        {
            var steps = GetDefaultCreationSteps();
            var data = new CharaMakeData(steps);
            var stepIndex = 0;
            var step = CharaMakeStep.Continue;
            var finished = false;

            UiResult<CharaMakeResult> result;
            CharaMakeLayer currentStep;

            void GoBack(Type charaMakeLayerType)
            {
                if (data.CharaData.ContainsKey(charaMakeLayerType))
                    data.CharaData.Remove(charaMakeLayerType);
                stepIndex--;
                if (stepIndex < 0)
                    step = CharaMakeStep.Cancel;
            }

            void Restart()
            {
                foreach (var layer in steps!)
                {
                    layer.Dispose();
                }
                steps = GetDefaultCreationSteps();
                data = new CharaMakeData(steps);
                stepIndex = 0;
            }

            while (step != CharaMakeStep.Cancel && !finished)
            {
                currentStep = steps[stepIndex];

                var charaMakeLayerType = currentStep.GetType();
                result = _uiManager.Query<CharaMakeResult, CharaMakeLayer, CharaMakeData>(currentStep, data);

                if (!result.HasValue)
                {
                    Logger.WarningS("charamake", $"Chara creation step for type {charaMakeLayerType} didn't set a result, aborting.");
                    step = CharaMakeStep.Cancel;
                }
                else
                {
                    step = result.Value.Step;
                    data.LastStep = step;
                }

                switch(step)
                {
                    case CharaMakeStep.GoBack:
                        GoBack(charaMakeLayerType);
                        break;
                    case CharaMakeStep.Restart:
                        Restart();
                        break;
                    case CharaMakeStep.Cancel:
                        break;
                    default:
                        data.CharaData[charaMakeLayerType] = result.Value.Added;
                        stepIndex++;
                        if (stepIndex == steps.Count)
                            finished = true;
                        break;
                }
            }

            foreach (var layer in steps)
            {
                layer.Dispose();
            }

            if (!finished)
            {
                return new CharaMakeLogicResult.Canceled();
            }

            if (!data.TryGetCharaMakeResult<EntityUid>(PlayerEntityResultName, out var newPlayer))
            {
                Logger.ErrorS("charamake", $"Did not find a charamake result with name '{PlayerEntityResultName}' containing the new player!");
                return new CharaMakeLogicResult.Canceled();
            }

            DebugTools.Assert(_entityManager.IsAlive(newPlayer), "New charamake player was not alive!");

            return new CharaMakeLogicResult.NewPlayerIncarnated(newPlayer);
        }
    }
}
