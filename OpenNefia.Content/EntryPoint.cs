﻿using OpenNefia.Content.ConfigMenu;
using OpenNefia.Content.Input;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;

namespace OpenNefia.Content
{
    /// <summary>
    /// second-system syndrome is now in effect
    /// </summary>
    public class EntryPoint : ModEntryPoint
    {
        [Dependency] private readonly IRandomAliasGenerator _aliasGen = default!;
        [Dependency] private readonly IRandomNameGenerator _nameGen = default!;
        [Dependency] private readonly IConfigMenuUICellFactory _configMenuUICellFactory = default!;

        public override void PreInit()
        {
        }

        public override void Init()
        {
            ContentIoC.Register();
            IoCManager.BuildGraph();

            IoCManager.InjectDependencies(this);

            InitializeDependencies();
        }

        private void InitializeDependencies()
        {
            _aliasGen.Initialize();
            _nameGen.Initialize();
            _configMenuUICellFactory.Initialize();
        }

        public override void PostInit()
        {
            // Setup key contexts
            var inputMan = IoCManager.Resolve<IInputManager>();
            ContentContexts.SetupContexts(inputMan.Contexts);

            var gc = IoCManager.Resolve<IGameController>();
            var mainTitle = IoCManager.Resolve<IMainTitleLogic>();

            gc.MainCallback += () => mainTitle.RunTitleScreen();
        }
    }
}