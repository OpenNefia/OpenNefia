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
using OpenNefia.Core.Log;
using OpenNefia.Content.UI;
using OpenNefia.Content.CharaInfo;
using OpenNefia.Core.Graphics;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.Parties;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.CharaSheet")]
    public class CharaMakeCharaSheetLayer : CharaMakeLayer
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly ISaveGameSerializer _saveSerializer = default!;
        [Dependency] private readonly IRandomNameGenerator _randomNames = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        private EntityUid _playerEntity;

        [Child] private CharaInfoPagesControl CharaInfoPages = new();
        [Child] private UiKeyHintBar KeyHintBar = new();

        public CharaMakeCharaSheetLayer()
        {
            CanControlFocus = true;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            CharaInfoPages.GrabFocus();
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

        private void AddPlayerOnlyComponents(EntityUid playerEntity)
        {
            EntityManager.EnsureComponent<PlayerComponent>(playerEntity);
            EntityManager.EnsureComponent<KarmaComponent>(playerEntity);
            EntityManager.EnsureComponent<FameComponent>(playerEntity);
            EntityManager.EnsureComponent<SleepExperienceComponent>(playerEntity);
        }

        public EntityUid CreatePlayerEntity(IEnumerable<ICharaMakeLayer> steps)
        {
            var globalMap = _mapManager.CreateMap(1, 1, MapId.Global);
            var globalMapSpatial = EntityManager.GetComponent<SpatialComponent>(globalMap.MapEntityUid);

            var playerEntity = EntityManager.CreateEntityUninitialized(Chara.Player);
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

            AddPlayerOnlyComponents(playerEntity);

            _entityGen.FireGeneratedEvent(playerEntity);

            return playerEntity;
        }

        private void Reroll(bool playSound)
        {
            _saveSerializer.ResetGameState();

            _playerEntity = CreatePlayerEntity(Data.AllSteps);

            CharaInfoPages.Initialize(_playerEntity);
            CharaInfoPages.RefreshFromEntity();
            KeyHintBar.Text = UserInterfaceManager.FormatKeyHints(MakeKeyHints());

            if (playSound)
                Sounds.Play(Sound.Dice);
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = new List<UiKeyHint>();
            
            keyHints.Add(new(new LocaleKey("Elona.CharaMake.Common.KeyHint.Reroll"), UiKeyNames.EnterKey));
            keyHints.AddRange(CharaInfoPages.MakeKeyHints());
            keyHints.Add(new(new LocaleKey("Elona.CharaMake.CharaSheet.KeyHint.FinalConfirmation"), EngineKeyFunctions.UICancel));

            return keyHints;
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

            ResetCaption();
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
                EntityManager.EventBus.RaiseEvent(_playerEntity, ev);

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
            CharaInfoPages.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            CharaInfoPages.SetSize(Width, Height);
            KeyHintBar.SetSize(_graphics.WindowSize.X - 226, 16);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            CharaInfoPages.SetPosition(X, Y);
            KeyHintBar.SetPosition(226, 0);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            CharaInfoPages.Update(dt);
            KeyHintBar.Update(dt);
        }

        public override void Draw()
        {
            base.Draw();
            CharaInfoPages.Draw();
            KeyHintBar.Draw();
        }
    }
}
