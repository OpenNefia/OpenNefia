﻿using OpenNefia.Core.GameObjects;
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

namespace OpenNefia.Content.CharaMake
{
    public interface ICharaMakeLogic
    {
        void RunCreateChara();
    }
    public class CharaMakeLogic : ICharaMakeLogic
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveSerializer = default!;

        public List<ICharaMakeLayer> GetDefaultCreationSteps()
        {
            return new List<ICharaMakeLayer>
            {
                new CharaMakeRaceSelectLayer(),
                new CharaMakeGenderSelectLayer(),
                new CharaMakeClassSelectLayer(),
                new CharaMakeAttributeRerollLayer(),
                new CharaMakeFeatWindowLayer(),
                new CharaMakeAliasLayer(),
                new CharaMakeAppearanceLayer(),
            };
        }


        public void RunCreateChara()
        {
            var steps = GetDefaultCreationSteps();
            var data = new CharaMakeData();
            var stepIndex = 0;
            var step = CharaMakeStep.Continue;
            UiResult<CharaMakeResult> result;
            ICharaMakeLayer currentStep;
            while (step != CharaMakeStep.Cancel)
            {
                if (stepIndex >= steps.Count)
                {
                    _saveSerializer.ResetGameState();
                    //just for testing
                    Logger.DebugS("charamake", $"Character creation complete, values:" + Environment.NewLine 
                        + string.Join(Environment.NewLine, data.CharaData.SelectMany(x => x.Value.Select(y => $"{y.Key}: {y.Value}"))));

                    var globalMap = _mapManager.CreateMap(1, 1, MapId.Global);
                    var globalMapSpatial = _entityManager.GetComponent<SpatialComponent>(globalMap.MapEntityUid);

                    var playerEntity = _entityManager.CreateEntityUninitialized(Protos.Chara.Player);
                    var playerSpatial = _entityManager.GetComponent<SpatialComponent>(playerEntity);
                    playerSpatial.AttachParent(globalMapSpatial);

                    var entityGen = EntitySystem.Get<IEntityGen>();
                    _entityManager.InitializeComponents(playerEntity);
                    _entityManager.StartComponents(playerEntity);
                    foreach(var creationStep in steps)
                    {
                        creationStep.ApplyStep(playerEntity);
                    }
                    entityGen.FireGeneratedEvent(playerEntity);
                    currentStep = new CharaMakeCharaSheetLayer(playerEntity);
                }
                else
                {
                    currentStep = steps[stepIndex];
                }
                var type = currentStep.GetType();
                result = _uiManager.Query<CharaMakeResult, CharaMakeLayer, CharaMakeData>(currentStep, data);

                if (!result.HasValue)
                {
                    Logger.WarningS("charamake", $"Chara creation step for type {type} didn't set a result, aborting.");
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
