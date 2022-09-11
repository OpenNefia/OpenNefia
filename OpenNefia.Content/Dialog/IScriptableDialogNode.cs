namespace OpenNefia.Content.Dialog
{
    /// <summary>
    /// Tagged source code to be compiled by the script engine.
    /// </summary>
    /// <param name="DelegateType"></param>
    /// <param name="Code"></param>
    public sealed record DialogScriptTarget(Type DelegateType, string Code);

    public sealed record DialogScriptResult(Dictionary<string, Dictionary<string, Delegate>> Callbacks);

    /// <summary>
    /// Indicates this is a dialog node that requires code compilation at runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Problems this is attempting to solve:
    /// </para>
    /// <para>
    /// <list type="number">
    /// <item>
    /// Dialog nodes are defined in YAML, but there is often a large amount of one-off logic per
    /// dialog run before and after each node that is not very reusable (so it's not as useful to
    /// define data definition classes holding that logic).
    /// </item>
    /// <item>
    /// Putting the code for that logic in an assembly splits the definition of the dialog between
    /// C# and YAML, so it's not obvious what's going on by just looking at one or the other. It
    /// also hampers the ease of use since it forces scripters to set up a development toolchain if
    /// they need any custom logic.
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// This proposed solution is to embed the C# in YAML and compile it at runtime. This makes all
    /// dialog prototypes self-contained.
    /// </para>
    /// <para>
    /// I'm still very much on the fence about this, however. It offloads the need for compilation
    /// to runtime and forces a hard dependency on Roslyn, which impacts startup time and memory
    /// usage.
    /// </para>
    /// <para>
    /// This could potentially be expanded into a generalized feature of the serialization system if
    /// necessary (but I very much hope that will not be the case; it adds a lot of complexity).
    /// </para>
    /// </remarks>
    public interface IScriptableDialogNode : IDialogNode
    {
        /// <summary>
        /// Asks the dialog node to give a set of script code/target delegate types for compilation.
        /// </summary>
        void GetCodeToCompile(ref Dictionary<string, DialogScriptTarget> targets);

        /// <summary>
        /// Given the compiled code, asks the dialog node to update its delegate fields with the
        /// results.
        /// </summary>
        void AddCompiledCode(IReadOnlyDictionary<string, Delegate> compiled);
    }
}