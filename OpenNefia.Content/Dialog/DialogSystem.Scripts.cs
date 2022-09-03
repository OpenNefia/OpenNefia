using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Fame;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Religion;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.World;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Activity;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Console;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class DialogSystem
    {
        /// <summary>
        /// Extracts code fragments from dialog prototypes and compiles delegate methods for each
        /// one.
        /// </summary>
        private void CompileDialogs(PrototypesReloadedEventArgs args)
        {
            if (!args.TryGetModified<DialogPrototype>(_protos, out var dialogs))
                return;

            var sw = new Stopwatch();
            sw.Start();

            var compiled = 0;
            var errors = 0;
            var scriptTargets = new Dictionary<string, DialogScriptTarget>();

            foreach (var dialog in dialogs)
            {
                var needsScript = false;
                var script = new StringBuilder();

                var imports = new HashSet<string>()
                {
                    typeof(IoCManager).Namespace!,
                    typeof(EntitySystem).Namespace!,
                    typeof(DialogPrototype).Namespace!,
                    typeof(Loc).Namespace!,
                };

                imports.AddRange(dialog.ScriptImports);

                foreach (var type in dialog.ScriptDependencies.Values)
                {
                    if (type.Namespace != null)
                        imports.Add(type.Namespace);
                }

                foreach (var ns in imports)
                {
                    script.AppendLine($"using {ns};");
                }

                //
                // Assign required dependencies to local variables at the top of the script.
                //
                // Example YAML:
                //
                //   scriptDependencies:
                //     _rand: OpenNefia.Core.Random.IRandom
                //     _vanillaDialog: OpenNefia.Content.Dialog.VanillaDialogSystem
                //
                // Result:
                //
                //   var _rand = IoCManager.Resolve<IRandom>();
                //   var _vanillaDialog = EntitySystem.Get<VanillaDialogSystem>();
                //
                foreach (var (varName, type) in dialog.ScriptDependencies)
                {
                    if (typeof(IEntitySystem).IsAssignableFrom(type))
                        script.AppendLine($"var {varName} = {nameof(EntitySystem)}.Get<{type.Name}>();");
                    else
                        script.AppendLine($"var {varName} = {nameof(IoCManager)}.Resolve<{type.Name}>();");
                }

                // Run extra user code at the start of the script.
                if (dialog.ScriptHeader != null)
                    script.AppendLine(dialog.ScriptHeader);

                script.AppendLine("var callbacks = new Dictionary<string, Dictionary<string, Delegate>>();");
                script.AppendLine("Dictionary<string, Delegate> nodeCallbacks;");

                GenerateScriptCallbacks(ref scriptTargets, dialog, ref needsScript, script);

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
                                // Now that the code has been compiled, have the dialog node replace
                                // its internal delegate fields with the results from the script.
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
                message += $" with {errors} errors";
                logLevel = LogLevel.Error;
            }
            message += $" in {sw.Elapsed}.";
            Logger.LogS(logLevel, "dialog", message);
        }

        private static void GenerateScriptCallbacks(ref Dictionary<string, DialogScriptTarget> scriptTargets, DialogPrototype dialog, ref bool needsScript, StringBuilder script)
        {
            foreach (var (nodeID, node) in dialog.Nodes)
            {
                if (node is IScriptableDialogNode scriptable)
                {
                    scriptable.GetCodeToCompile(ref scriptTargets);

                    if (scriptTargets.Count > 0)
                    {
                        // Generate script callbacks for all the code in this dialog node.
                        script.AppendLine("nodeCallbacks = new Dictionary<string, Delegate>();");

                        needsScript = true;
                        foreach (var (funcName, target) in scriptTargets)
                        {
                            string code = GenerateMethodDefinition(nodeID, funcName, target);
                            script.AppendLine(code);
                        }

                        script.AppendLine(@$"callbacks[""{nodeID}""] = nodeCallbacks;");
                    }

                    scriptTargets.Clear();
                }
            }
        }

        private static string GenerateMethodDefinition(string nodeID, string funcName, DialogScriptTarget target)
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
            return code;
        }
    }
}
