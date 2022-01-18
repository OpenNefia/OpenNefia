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
using OpenNefia.Content.UI;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.CharaSheet")]
    public class CharaMakeCharaSheetLayer : CharaMakeLayer
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly ISaveGameSerializer _saveSerializer = default!;
        [Dependency] private readonly IRandomNameGenerator _randomNames = default!;

        private EntityUid _playerEntity;

        private CharaSheet Sheet = new();

        public CharaMakeCharaSheetLayer()
        {
            CanControlFocus = true;
            AddChild(Sheet);
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);

            Reroll(playSound: false);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Sound.Chara);
        }

        private EntityUid CreatePlayerEntity(IEnumerable<ICharaMakeLayer> steps)
        {
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

        private void Reroll(bool playSound)
        {
            _saveSerializer.ResetGameState();

            _playerEntity = CreatePlayerEntity(Data.AllSteps);

            Sheet.RefreshFromEntity(_playerEntity);
            Sheet.SetSize(Sheet.Width, Sheet.Height);
            Sheet.SetPosition(Sheet.X, Sheet.Y);

            if (playSound)
                Sounds.Play(Sound.Dice);
        }

        private void ResetCaption()
        {
            Caption.Text = Loc.GetString("Elona.CharaMake.CharaSheet.Caption");
        }

        protected override void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UISelect)
            {
                Reroll(playSound: true);
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UICancel)
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
            var caption = Loc.GetString("Elona.CharaMake.CharaSheet.FinalPrompt.Text");
            Caption.Text = caption;

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
                QueryText = caption
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
                        ResetCaption();
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
        }

        /// <summary>
        /// "Last question. What's your name?"
        /// </summary>
        private void ShowLastQuestion()
        {
            var caption = Loc.GetString("Elona.CharaMake.CharaSheet.WhatIsYourName");
            Caption.Text = caption;

            var args = new TextPrompt.Args()
            {
                QueryText = caption,
                MaxLength = 10
            };

            var result = UserInterfaceManager.Query<TextPrompt, TextPrompt.Args, TextPrompt.Result>(args);

            if (result.HasValue)
            {
                var customName = EntityManager.EnsureComponent<CustomNameComponent>(_playerEntity);
                var resultName = result.Value.Text;

                if (string.IsNullOrWhiteSpace(resultName))
                {
                    resultName = _randomNames.GenerateRandomName();
                }

                customName.CustomName = resultName.Trim();

                Sounds.Play(Sound.Skill);

                var ev = new NewPlayerIncarnatedEvent(_playerEntity);
                EntityManager.EventBus.RaiseLocalEvent(_playerEntity, ev);

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
                        { CharaMakeLogic.PlayerEntityResultName, _playerEntity }
                    }));
                }
            }
            else
            {
                ResetCaption();
            }
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            Sheet.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Sheet.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
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
