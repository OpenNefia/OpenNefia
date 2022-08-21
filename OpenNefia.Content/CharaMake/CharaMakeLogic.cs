using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.UserInterface;
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
        bool CharaMakeIsActive { get; }
        CharaMakeLogicResult RunCreateChara();
    }

    public class CharaMakeLogic : ICharaMakeLogic
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public bool CharaMakeIsActive { get; private set; } = false;

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
            try
            {
                CharaMakeIsActive = true;
                return DoCreateChara();
            }
            finally
            {
                CharaMakeIsActive = false;
            }

            CharaMakeLogicResult DoCreateChara()
            {
                var steps = GetDefaultCreationSteps();
                var data = new CharaMakeResultSet(steps);
                var stepIndex = 0;
                var step = CharaMakeStep.Continue;
                var finished = false;

                UiResult<CharaMakeUIResult> result;
                CharaMakeLayer currentStep;

                void GoBack(CharaMakeLayer layer)
                {
                    data.AllResults.Remove(layer);
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
                    data = new CharaMakeResultSet(steps);
                    stepIndex = 0;
                }

                while (step != CharaMakeStep.Cancel && !finished)
                {
                    currentStep = steps[stepIndex];

                    var charaMakeLayerType = currentStep.GetType();
                    result = _uiManager.Query<CharaMakeUIResult, CharaMakeLayer, CharaMakeResultSet>(currentStep, (CharaMakeResultSet)data);

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

                    switch (step)
                    {
                        case CharaMakeStep.GoBack:
                            GoBack(currentStep);
                            break;
                        case CharaMakeStep.Restart:
                            Restart();
                            break;
                        case CharaMakeStep.Cancel:
                            break;
                        default:
                            if (result.Value.Added != null)
                                data.AllResults.Add(currentStep, result.Value.Added);
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

                if (!data.TryGet<CharaMakeCharaSheetLayer, CharaMakeCharaSheetLayer.ResultData>(out var finalResult))
                {
                    Logger.ErrorS("charamake", $"Did not find a charamake result with type '{typeof(CharaMakeCharaSheetLayer.ResultData).FullName}' containing the new player!");
                    return new CharaMakeLogicResult.Canceled();
                }

                DebugTools.Assert(_entityManager.IsAlive(finalResult.PlayerEntity), "New charamake player was not alive!");

                return new CharaMakeLogicResult.NewPlayerIncarnated(finalResult.PlayerEntity);
            }
        }
    }
}
