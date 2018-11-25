using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEditor.Build.Player;

namespace Build.Pipeline.Interfaces
{
    /// <summary>
    /// Base interface for the build results container
    /// </summary>
    public interface IBuildResults : IContextObject
    {
        /// <summary>
        /// Results from the script compiling step.
        /// </summary>
        ScriptCompilationResult ScriptResults { get; set; }

        /// <summary>
        /// Map of serialized file name to results for built content.
        /// </summary>
        Dictionary<string, WriteResult> WriteResults { get; }
    }


}