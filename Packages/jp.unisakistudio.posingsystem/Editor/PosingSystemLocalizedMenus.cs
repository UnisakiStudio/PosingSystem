namespace jp.unisakistudio.posingsystemeditor
{
    // The same registrar is available before installation in the standalone setup helper.
    internal static class PosingSystemLocalizedMenus
    {
        internal static string PathFor(string source, string locale)
            => UnisakiStudio.Setup.LocalizedMenus.PathFor(source, locale);
    }
}
