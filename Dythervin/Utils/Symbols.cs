using UnityEditor;

namespace Dythervin.Utils
{
    public static class Symbols
    {
        public static void AddSymbol(string define)
        {
#if UNITY_EDITOR
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (defines.Contains(define))
                return;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, $"{defines};{define}");
#endif
        }
    }
}