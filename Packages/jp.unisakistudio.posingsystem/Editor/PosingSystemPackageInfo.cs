using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace jp.unisakistudio.posingsystemeditor
{
    // Read once per project change, never on every Inspector repaint.
    [InitializeOnLoad]
    internal static class PosingSystemPackageInfo
    {
        internal const string ManualRootUrl = "https://docs.unisakistudio.jp/";
        internal const string SupportUrl = "https://help.unisakistudio.jp/";

        [Serializable]
        internal sealed class Info
        {
            public string name;
            public string displayName;
            public string version;
            [NonSerialized] internal string RootPath;
            [NonSerialized] internal readonly Dictionary<string, string> Changelogs = new Dictionary<string, string>();

            internal string VersionLabel => string.IsNullOrEmpty(version) ? PosingSystemLocalization.Text("バージョン不明") : "v" + version;
            internal string ManualUrl
            {
                get
                {
                    if (!string.IsNullOrEmpty(RootPath))
                    {
                        var localized = DocumentationPath(this, "UserGuide.html");
                        if (localized != null) return new Uri(Path.GetFullPath(localized)).AbsoluteUri;
                        var legacyEnglish = Path.Combine(RootPath, "Documentation~", "UserGuide.en.html");
                        if (PosingSystemLocalization.Language != "ja" && File.Exists(legacyEnglish))
                            return new Uri(Path.GetFullPath(legacyEnglish)).AbsoluteUri;
                    }
                    switch (name)
                    {
                        case "jp.unisakistudio.kawaiiposing": return ManualRootUrl + "kawaiiposing/manual/";
                        case "jp.unisakistudio.coolposing": return ManualRootUrl + "coolposing/manual/";
                        case "jp.unisakistudio.virtuallove": return ManualRootUrl + "virtuallove/manual/";
                        default: return ManualRootUrl;
                    }
                }
            }
        }

        private static readonly Dictionary<Type, Info> Cache = new Dictionary<Type, Info>();

        static PosingSystemPackageInfo()
        {
            EditorApplication.projectChanged += Cache.Clear;
        }

        internal static Info Get(Type componentType, MonoScript script = null)
        {
            if (Cache.TryGetValue(componentType, out var cached)) return cached;

            var package = PackageInfo.FindForAssembly(componentType.Assembly);
            var root = package?.resolvedPath;
            // Assets-based imports can have no Package Manager entry.
            if (string.IsNullOrEmpty(root))
            {
                if (script == null)
                {
                    foreach (var candidate in MonoImporter.GetAllRuntimeMonoScripts())
                    {
                        if (candidate.GetClass() != componentType) continue;
                        script = candidate;
                        break;
                    }
                }
                var scriptPath = script != null ? AssetDatabase.GetAssetPath(script) : null;
                if (!string.IsNullOrEmpty(scriptPath))
                {
                    var directory = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(scriptPath)));
                    var assetsRoot = Path.GetFullPath(Application.dataPath);
                    while (directory != null && directory.FullName.StartsWith(assetsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(Path.Combine(directory.FullName, "package.json")))
                        {
                            root = directory.FullName;
                            break;
                        }
                        directory = directory.Parent;
                    }
                }
            }

            var info = new Info
            {
                name = package?.name,
                displayName = package?.displayName ?? componentType.Name,
                version = package?.version,
                RootPath = root,
            };
            if (!string.IsNullOrEmpty(root))
            {
                try
                {
                    // Use the installed file so local version edits are reflected as well.
                    var manifestPath = Path.Combine(root, "package.json");
                    if (File.Exists(manifestPath))
                    {
                        var manifest = JsonUtility.FromJson<Info>(File.ReadAllText(manifestPath));
                        if (manifest != null)
                        {
                            info.name = manifest.name ?? info.name;
                            info.displayName = manifest.displayName ?? info.displayName;
                            info.version = manifest.version ?? info.version;
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
                {
                    // Keep Package Manager metadata if the optional manifest cannot be read.
                }
            }
            Cache[componentType] = info;
            return info;
        }

        // Only known language codes can become a directory name. New packages bundle
        // every locale; English is a compatibility fallback for older partial installs.
        internal static string DocumentationPath(Info info, string filename)
        {
            if (string.IsNullOrEmpty(info.RootPath)) return null;
            var language = PosingSystemLocalization.Language;
            if (Array.IndexOf(PosingSystemLocalization.Codes, language) < 0) language = "en";
            foreach (var candidate in new[] { language, "en" })
            {
                var path = Path.Combine(info.RootPath, "Documentation~", "Localization", candidate, filename);
                if (File.Exists(path)) return path;
            }
            return null;
        }

        internal static string GetChangelog(Info info)
        {
            var language = PosingSystemLocalization.Language;
            if (info.Changelogs.TryGetValue(language, out var cached)) return cached;
            var unavailable = PosingSystemLocalization.Text("変更履歴を読み込めませんでした。");
            if (string.IsNullOrEmpty(info.RootPath)) return unavailable;

            try
            {
                var path = DocumentationPath(info, "CHANGELOG.md") ?? Path.Combine(info.RootPath, "CHANGELOG.md");
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path).Replace("\r\n", "\n").Trim();
                    // Skip the Keep a Changelog preamble; retain all release entries.
                    var firstRelease = text.IndexOf("\n## ", StringComparison.Ordinal);
                    if (firstRelease >= 0) text = text.Substring(firstRelease + 1);
                    if (!string.IsNullOrWhiteSpace(text)) return info.Changelogs[language] = text;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Missing release notes must never prevent configuring the avatar.
            }
            return unavailable;
        }
    }
}
