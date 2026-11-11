using UnityEditor;
namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    internal static class PosingSystemTextRenderingSetup
    {
        static PosingSystemTextRenderingSetup()
        {
            PosingSystemTextRenderer.LanguageProvider = () => PosingSystemLocalization.Language;
            PosingSystemTextGUI.PropertyDrawerOwnsLabel = property =>
                property.type == "AnimationDefine" || property.type == "OverrideAnimationDefine" ||
                property.type == "PosingSystemAutoPoseLimb" || property.type == "PosingSystemAutoPoseTargetSpace" ||
                property.type == "PosingSystemAutoPoseBendDirectionMode" || property.type == "PosingSystemAutoPosePositionSpace" ||
                property.type == "PosingSystemAutoPoseLengthReference";
        }
    }
}
