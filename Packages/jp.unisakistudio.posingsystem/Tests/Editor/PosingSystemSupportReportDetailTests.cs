using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // The support draft carries technical details for triage, but never names chosen by the user.
    public class PosingSystemSupportReportDetailTests : global::jp.unisakistudio.posingsystemeditor.tests.SharedLanguagePreferenceTestBase
    {
        private const string PrivatePose = "ひみつのポーズ名";
        private const string PrivateObject = "PrivateAvatarName";

        [Test]
        public void DraftDescribesToolStateWithoutUserNames()
        {
            var avatar = new GameObject(PrivateObject);
            var tree = new BlendTree { name = "PrivateBlendTree" };
            var side = new AnimationClip { name = "PrivateSideClip" };
            try
            {
                avatar.AddComponent<Animator>();
                avatar.AddComponent<VRCAvatarDescriptor>();
                var tool = new GameObject("PrivateToolName");
                tool.transform.SetParent(avatar.transform);
                var ps = tool.AddComponent<PosingSystem>();
                var layer = new PosingSystem.LayerDefine("", "PrivateStateMachine", "PrivateParameter", 0, "PrivateCategory", new List<PosingSystem.AnimationDefine>());
                layer.animations.Add(new PosingSystem.AnimationDefine(null, PrivatePose) { animationClip = tree, enabled = true, isRollover = true, rolloverSideAnimation = side });
                layer.animations.Add(new PosingSystem.AnimationDefine(null, "立ち姿勢") { enabled = false });
                ps.defines = new List<PosingSystem.LayerDefine> { layer };
                foreach (var language in new[] { "ja", "en", "ar" })
                {
                    PosingSystemLocalization.Select(language);
                    var draft = PosingSystemSupportReport.Draft("[PS070] " + PrivatePose, ps);
                    Assert.That(draft, Does.Contain("[env] os="), language);
                    Assert.That(draft, Does.Contain(" lang=" + language), language);
                    Assert.That(draft, Does.Contain("[tool] source=no-prefab poses=1/2 custom=1 "), language);
                    Assert.That(draft, Does.Contain("[features] ").And.Contain("footHeight=").And.Contain("breathingControl="), language);
                    Assert.That(draft, Does.Contain("[issues] ").And.Contain("PS070(E)"), language);
                    Assert.That(draft, Does.Contain("[PS070] 1-1 center=BlendTree side=Clip:generic standardName=0"), language);
                    foreach (var secret in new[] { PrivatePose, PrivateObject, "PrivateToolName", "PrivateCategory", "PrivateBlendTree", "PrivateSideClip", "PrivateStateMachine", "PrivateParameter" })
                        Assert.That(draft, Does.Not.Contain(secret), language + ": " + secret);
                }
            }
            finally
            {
                Object.DestroyImmediate(avatar);
                Object.DestroyImmediate(tree);
                Object.DestroyImmediate(side);
            }
        }

        [Test]
        public void ExceptionDetailKeepsOnlyStudioCodeLocations()
        {
            const string entry = "NullReferenceException: Object reference not set to an instance of an object\n" +
                "jp.unisakistudio.posingsystemeditor.PosingSystemConverter.ConvertAnimatorController (jp.unisakistudio.posingsystem.PosingSystem posingSystem) (at ./Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemConverter.cs:1234)\n" +
                "jp.unisakistudio.posingsystemeditor.PosingSystemEditor.Prebuild (jp.unisakistudio.posingsystem.PosingSystem posingSystem) (at C:/Users/secret-name/Project/Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemEditor.cs:600)\n" +
                "UnityEditor.EditorApplication.Internal_CallDelayFunctions () (at <0>:0)";
            Assert.That(PosingSystemSupportReport.ExceptionDetail(entry), Is.EqualTo(
                "NullReferenceException: Object reference not set to an instance of an object @ " +
                "PosingSystemConverter.ConvertAnimatorController (PosingSystemConverter.cs:1234) < PosingSystemEditor.Prebuild (PosingSystemEditor.cs:600)"));
            // Other tools' exceptions and this tool's own diagnostics add nothing here.
            Assert.That(PosingSystemSupportReport.ExceptionDetail("NullReferenceException: x\nOtherTool.Run () (at ./Packages/other/Editor/Run.cs:3)"), Is.Empty);
            Assert.That(PosingSystemSupportReport.ExceptionDetail("DiagnosticException: [PosingSystem / PS010] " + PrivatePose + "\njp.unisakistudio.posingsystemeditor.A.B () (at ./Packages/jp.unisakistudio.posingsystem/Editor/A.cs:1)"), Is.Empty);
            var withPath = PosingSystemSupportReport.ExceptionDetail("IOException: Could not read C:/Users/secret-name/Documents/Avatar/file.asset\n" +
                "jp.unisakistudio.posingsystemeditor.A.B () (at ./Packages/jp.unisakistudio.posingsystem/Editor/A.cs:1)");
            Assert.That(withPath, Does.Not.Contain("secret-name"));
            Assert.That(withPath, Does.EndWith("@ A.B (A.cs:1)"));
        }
    }
}
