using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.CustomName;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.CharaSheet")]
    public class CharaMakeCharaSheetLayer : CharaMakeLayer
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly ISaveGameSerializer _saveSerializer = default!;
        [Dependency] private readonly IRandomNameGenerator _randomNames = default!;

        public const string ResultName = "playerEntity";

        private EntityUid _playerEntity;

        private CharaSheet Sheet = new();

        private EntityUid CreatePlayerEntity(IEnumerable<ICharaMakeLayer> steps)
        {
            _saveSerializer.ResetGameState();

            var globalMap = _mapManager.CreateMap(1, 1, MapId.Global);
            var globalMapSpatial = EntityManager.GetComponent<SpatialComponent>(globalMap.MapEntityUid);

            var playerEntity = EntityManager.CreateEntityUninitialized(Protos.Chara.Player);
            var playerSpatial = EntityManager.GetComponent<SpatialComponent>(playerEntity);
            playerSpatial.AttachParent(globalMapSpatial);

            EntityManager.InitializeComponents(playerEntity);
            EntityManager.StartComponents(playerEntity);

            foreach (var creationStep in steps)
            {
                creationStep.ApplyStep(playerEntity);
            }

            var customName = EntityManager.EnsureComponent<CustomNameComponent>(playerEntity);
            customName.CustomName = "????";

            EntityManager.EnsureComponent<PlayerComponent>(playerEntity);

            _entityGen.FireGeneratedEvent(playerEntity);

            return playerEntity;
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);

            Reroll(playSound: false);
        }

        private void Reroll(bool playSound)
        {
            if (EntityManager.IsAlive(_playerEntity))
            {
                EntityManager.DeleteEntity(_playerEntity);
            }

            _playerEntity = CreatePlayerEntity(Data.AllSteps);

            Sheet.RefreshFromEntity(_playerEntity);

            if (playSound)
                Sounds.Play(Sound.Dice);
        }

        protected override void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                ShowFinalPrompt();
                args.Handle();
            }
        }

        private enum FinalPromptChoice
        {
            Yes,
            No,
            Restart,
            GoBack
        }

        /// <summary>
        /// "Are you satisfied now?"
        /// </summary>
        private void ShowFinalPrompt()
        {
            var keyRoot = new LocaleKey("Elona.CharaMake.CharaSheet.FinalPrompt.Choices");
            var choices = new PromptChoice<FinalPromptChoice>[]
            {
#pragma warning disable format
                new(FinalPromptChoice.Yes,     keyRoot.With(nameof(FinalPromptChoice.Yes))),
                new(FinalPromptChoice.No,      keyRoot.With(nameof(FinalPromptChoice.No))),
                new(FinalPromptChoice.Restart, keyRoot.With(nameof(FinalPromptChoice.Restart))),
                new(FinalPromptChoice.GoBack,  keyRoot.With(nameof(FinalPromptChoice.GoBack)))
#pragma warning restore format
            };

            var promptArgs = new Prompt<FinalPromptChoice>.Args(choices)
            {
                QueryText = Loc.GetString("Elona.CharaMake.CharaSheet.FinalPrompt.Text")
            };

            var result = UserInterfaceManager.Query<Prompt<FinalPromptChoice>,
                Prompt<FinalPromptChoice>.Args,
                PromptChoice<FinalPromptChoice>>(promptArgs);

            if (result.HasValue)
            {
                switch (result.Value.ChoiceData)
                {
                    case FinalPromptChoice.Yes:
                        ShowLastQuestion();
                        break;
                    case FinalPromptChoice.No:
                        break;
                    case FinalPromptChoice.Restart:
                        Finish(new CharaMakeResult(new(), CharaMakeStep.Restart));
                        break;
                    case FinalPromptChoice.GoBack:
                        Finish(new CharaMakeResult(new(), CharaMakeStep.GoBack));
                        break;
                    default:
                        break;
                }
            }

            Finish(new CharaMakeResult(new(), CharaMakeStep.GoBack));
        }

        /// <summary>
        /// "Last question. What's your name?"
        /// </summary>
        private void ShowLastQuestion()
        {
            var args = new TextPrompt.Args()
            {
                QueryText = Loc.GetString("Elona.CharaMake.CharaSheet.WhatIsYourName"),
                MaxLength = 10
            };

            var result = UserInterfaceManager.Query<TextPrompt, TextPrompt.Args, string>(args);

            if (result.HasValue)
            {
                var customName = EntityManager.EnsureComponent<CustomNameComponent>(_playerEntity);
                var resultName = result.Value;

                if (string.IsNullOrWhiteSpace(resultName))
                {
                    resultName = _randomNames.GenerateRandomName();
                }

                customName.CustomName = resultName.Trim();

                Sounds.Play(Sound.Skill);

                var ev = new NewPlayerIncarnatedEvent(_playerEntity);
                EntityManager.EventBus.RaiseLocalEvent(_playerEntity, ref ev);

                if (!EntityManager.IsAlive(_playerEntity))
                {
                    Logger.ErrorS("charamake", $"Player entity was not alive when the charamake process finished!");
                    EntityManager.DeleteEntity(_playerEntity);
                    Reroll(playSound: false);
                }
                else
                {
                    Finish(new CharaMakeResult(new Dictionary<string, object>()
                    {
                        { ResultName, _playerEntity }
                    }));
                }
            }
        }

        public override UiResult<CharaMakeResult>? GetResult()
        {
            var result = base.GetResult();

            if (result == null)
                return null;

            Cleanup();
            return result;
        }

        private void Cleanup()
        {
            if (EntityManager.IsAlive(_playerEntity))
            {
                EntityManager.DeleteEntity(_playerEntity);
            }
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Sound.Chara);
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            Sheet.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Sheet.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Sheet.SetPosition(X, Y);
        }

        public override void Draw()
        {
            base.Draw();
            Sheet.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Sheet.Update(dt);
        }
    }
}
