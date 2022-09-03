using NativeFileDialogSharp;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Console;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public interface IDialogSystem : IEntitySystem
    {
        void ModifyImpression(EntityUid uid, int delta, DialogComponent? dialog = null);
        int GetImpressionLevel(int impression);
        TurnResult TryToChatWith(EntityUid source, EntityUid target, bool force = false, PrototypeId<DialogPrototype>? dialogID = null);
        TurnResult StartDialog(EntityUid source, EntityUid target, PrototypeId<DialogPrototype> dialogID);
        string GetDefaultSpeakerName(EntityUid uid);
    }

    public sealed partial class DialogSystem : EntitySystem, IDialogSystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ICSharpReplExecutor _compiler = default!;

        public override void Initialize()
        {
            SubscribeComponent<DialogComponent, EntityBeingGeneratedEvent>(InitializePersonality);
            SubscribeEntity<CheckKillEvent>(ProcImpressionChange);
            SubscribeComponent<DialogComponent, WasCollidedWithEventArgs>(HandledCollidedWith, priority: EventPriorities.Low + 1000);

            _protos.PrototypesReloaded += CompileDialog;
        }

        private void CompileDialog(PrototypesReloadedEventArgs args)
        {
            if (args.TryGetModified<DialogPrototype>(_protos, out var dialogs))
            {
                var sw = new Stopwatch();
                sw.Start();

                var compiled = 0;
                var errors = 0;
                var scriptTargets = new Dictionary<string, DialogScriptTarget>();

                foreach (var dialog in dialogs)
                {
                    var needsScript = false;
                    var script = new StringBuilder();

                    script.AppendLine(@"using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Dialog;");

                    foreach (var ns in dialog.ScriptImports)
                    {
                        script.AppendLine($"using {ns};");
                    }

                    foreach (var (varName, type) in dialog.ScriptDependencies)
                    {
                        if (typeof(IEntitySystem).IsAssignableFrom(type))
                            script.AppendLine($"var {varName} = {nameof(EntitySystem)}.Get<{type.Name}>();");
                        else
                            script.AppendLine($"var {varName} = {nameof(IoCManager)}.Resolve<{type.Name}>();");
                    }

                    if (dialog.ScriptHeader != null)
                        script.AppendLine(dialog.ScriptHeader);

                    script.AppendLine("var callbacks = new Dictionary<string, Dictionary<string, Delegate>>();");
                    script.AppendLine("Dictionary<string, Delegate> nodeCallbacks;");

                    foreach (var (nodeID, node) in dialog.Nodes)
                    {
                        if (node is IScriptableDialogNode scriptable)
                        {
                            scriptable.GetCodeToCompile(ref scriptTargets);

                            if (scriptTargets.Count > 0)
                            {
                                script.AppendLine("nodeCallbacks = new Dictionary<string, Delegate>();");

                                needsScript = true;
                                foreach (var (funcName, target) in scriptTargets)
                                {
                                    var fullName = $"GeneratedCallback_{nodeID}_{funcName}";
                                    var signature = PrettyPrint.PrintDelegateTypeSignature(target.DelegateType, nameOverride: fullName);

                                    var code = $@"
public {signature}
{{
{target.Code}
}}
{target.DelegateType.Name} delegate_{fullName} = {fullName};
nodeCallbacks[""{funcName}""] = delegate_{fullName};
";

                                    script.AppendLine(code);
                                }

                                script.AppendLine(@$"callbacks[""{nodeID}""] = nodeCallbacks;");
                            }

                            scriptTargets.Clear();
                        }
                    }

                    script.AppendLine($"return new {nameof(DialogScriptResult)}(callbacks);");
                    var scriptCode = script.ToString();

                    if (needsScript)
                    {
                        try
                        {
                            var result = _compiler.Execute(scriptCode);

                            if (result is ReplExecutionResult.Success success)
                            {
                                dialog.ScriptObject = success.ReturnValue;

                                var scriptResult = (DialogScriptResult)success.ReturnValue!;

                                foreach (var (nodeID, node) in dialog.Nodes)
                                {
                                    if (node is IScriptableDialogNode scriptable && scriptResult.Callbacks.TryGetValue(nodeID, out var callbacks))
                                    {
                                        scriptable.AddCompiledCode(callbacks);
                                    }
                                }
                            }
                            else if (result is ReplExecutionResult.Error err)
                            {
                                throw err.Exception;
                            }
                            else
                            {
                                throw new ArgumentException("Unknown error");
                            }
                            compiled++;
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorS("dialog", ex, $"Failed to compile dialog script! ({dialog.ID})");
                            Logger.ErrorS("dialog", $"Generated script:\n{scriptCode}");
                            errors++;
                            dialog.ScriptObject = null;
                        }
                    }
                    else
                    {
                        dialog.ScriptObject = null;
                    }
                }

                var message = $"Compiled {compiled} dialog scripts";
                var logLevel = LogLevel.Info;
                if (errors > 0)
                {
                    message += $" with {errors} failures";
                    logLevel = LogLevel.Error;
                }
                message += $" in {sw.Elapsed}.";
                Logger.LogS(logLevel, "dialog", message);
            }
        }

        private void InitializePersonality(EntityUid uid, DialogComponent component, ref EntityBeingGeneratedEvent args)
        {
            // TODO: replace with custom talk
            component.Personality = _rand.Next(4);
        }

        private void ProcImpressionChange(EntityUid victim, ref CheckKillEvent args)
        {
            if (!(_parties.IsInPlayerParty(args.Attacker) && !_parties.IsInPlayerParty(victim)))
                return;

            if (HasComp<RoleAdventurerComponent>(victim))
                ModifyImpression(victim, -25);

            if (!_gameSession.IsPlayer(args.Attacker) && TryComp<DialogComponent>(args.Attacker, out var dialog))
            {
                if (dialog.Impression < ImpressionLevels.Marry)
                {
                    if (_rand.OneIn(2))
                    {
                        ModifyImpression(args.Attacker, 1);
                        _emoIcons.SetEmotionIcon(args.Attacker, EmotionIcons.Heart, 3);
                    }
                }
                else
                {
                    if (_rand.OneIn(10))
                    {
                        ModifyImpression(args.Attacker, 1);
                        _emoIcons.SetEmotionIcon(args.Attacker, EmotionIcons.Heart, 3);
                    }
                }
            }
        }

        private void HandledCollidedWith(EntityUid uid, DialogComponent component, WasCollidedWithEventArgs args)
        {
            if (args.Handled)
                return;

            if (!_gameSession.IsPlayer(args.Source))
                return;

            args.Handle(TryToChatWith(args.Source, uid));
        }

        public int GetImpressionLevel(int impression)
        {
            if (impression < ImpressionLevels.Foe)
                return 0;
            else if (impression < ImpressionLevels.Hate)
                return 1;
            else if (impression < ImpressionLevels.Normal - 10)
                return 2;
            else if (impression < ImpressionLevels.Amiable)
                return 3;
            else if (impression < ImpressionLevels.Friend)
                return 4;
            else if (impression < ImpressionLevels.Fellow)
                return 5;
            else if (impression < ImpressionLevels.Marry)
                return 6;
            else if (impression < ImpressionLevels.Soulmate)
                return 7;
            else
                return 8;
        }

        public void ModifyImpression(EntityUid uid, int delta, DialogComponent? dialog = null)
        {
            if (!Resolve(uid, ref dialog))
                return;

            var level = GetImpressionLevel(dialog.Impression);
            if (delta >= 0)
            {
                delta = delta * 100 / (50 + level * level * level);
                if (delta == 0 && level < _rand.Next(10))
                    delta = 1;
            }

            dialog.Impression += delta;

            var newLevel = GetImpressionLevel(dialog.Impression);
            var newLevelText = Loc.GetString($"Elona.Dialog.Impression.Levels.{newLevel}");
            if (level > newLevel)
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Impression.Modify.Lose", ("chara", uid), ("newLevel", newLevelText)), UiColors.MesPurple);
            }
            else if (newLevel > level && _factions.GetRelationTowards(uid, _gameSession.Player) > Relation.Enemy)
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Impression.Modify.Gain", ("chara", uid), ("newLevel", newLevelText)), UiColors.MesGreen);
            }
        }
    }
}