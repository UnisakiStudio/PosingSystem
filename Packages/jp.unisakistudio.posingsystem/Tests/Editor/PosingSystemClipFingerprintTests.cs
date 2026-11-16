using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // A finished prebuild must stay current when nothing was changed: after a restart, and after
    // something merely looked at a clip. Unity adds "editor curves" in memory to clips whose file
    // has none once their curve list is read; the VRChat SDK's fall clips are such files.
    public class PosingSystemClipFingerprintTests
    {
        private static string SdkClipPath(string name) => AssetDatabase.FindAssets(name + " t:AnimationClip", new[] { "Packages/com.vrchat.avatars" })
            .Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => System.IO.Path.GetFileNameWithoutExtension(p) == name);

        // The clip as a new Unity session has it: loaded from its file, curves not read yet.
        private static AnimationClip Fresh(string path)
        {
            var loaded = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (loaded != null) Resources.UnloadAsset(loaded);
            PosingSystemClipFingerprint.Forget();
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        [Test]
        public void OnlyDerivedMembersAreLeftOut()
        {
            const string json = "{\"AnimationClip\":{\"m_Name\":\"a]\\\"[,}\",\"m_FloatCurves\":[{\"attribute\":\"x[0]\",\"path\":\"a,b}\"}]," +
                "\"m_SampleRate\":60.0,\"m_Bounds\":{\"m_Center\":{\"x\":0.0}},\"m_ClipBindingConstant\":{\"genericBindings\":[{\"path\":1}],\"pptrCurveMapping\":[]}," +
                "\"m_AnimationClipSettings\":{\"m_LoopTime\":true},\"m_EditorCurves\":[{\"curve\":{\"m_Curve\":[{\"time\":0},{\"time\":1}]},\"attribute\":\"]x[\\\\\"}]," +
                "\"m_EulerEditorCurves\":[{\"a\":[1,[2]]}],\"m_HasGenericRootTransform\":true,\"m_HasMotionFloatCurves\":false,\"m_Events\":[]}}";
            Assert.That(PosingSystemClipFingerprint.Authored(json), Is.EqualTo(
                "\"m_Name\":\"a]\\\"[,}\",\"m_FloatCurves\":[{\"attribute\":\"x[0]\",\"path\":\"a,b}\"}]," +
                "\"m_SampleRate\":60.0,\"m_AnimationClipSettings\":{\"m_LoopTime\":true},\"m_Events\":[],"));
            // Text in another form is not touched.
            foreach (var other in new[] { "", "[1,2]", "{\"AnimationClip\":{}}", "{\"AnimationClip\":{\"m_EditorCurves\":[{\"a\":1}" })
                Assert.That(PosingSystemClipFingerprint.Authored(other), Is.EqualTo(other), other);
        }

        [Test]
        public void ReadingTheCurvesOfAClipDoesNotChangeItsFingerprint()
        {
            var path = SdkClipPath("proxy_fall_long");
            Assume.That(path, Is.Not.Null, "the VRChat SDK sample clips are installed");
            var clip = Fresh(path);
            var file = EditorJsonUtility.ToJson(clip);
            Assume.That(file, Does.Contain("\"m_EditorCurves\":[]"), "this clip's file has no editor curves");
            var before = PosingSystemClipFingerprint.Of(clip);

            Assert.That(AnimationUtility.GetCurveBindings(clip).Length, Is.GreaterThan(0));
            var read = EditorJsonUtility.ToJson(clip);
            Assume.That(read, Is.Not.EqualTo(file), "Unity adds the editor curves in memory");
            // What the fingerprint is made of, and the fingerprint itself without its memory.
            Assert.That(PosingSystemClipFingerprint.Authored(read), Is.EqualTo(PosingSystemClipFingerprint.Authored(file)));
            PosingSystemClipFingerprint.Forget();
            Assert.That(PosingSystemClipFingerprint.Of(clip), Is.EqualTo(before));

            // As after a restart: the clip comes from its file again.
            Assert.That(PosingSystemClipFingerprint.Of(Fresh(path)), Is.EqualTo(before));
        }

        [Test]
        public void TheImportedMotionSignatureIsTheSameBeforeAndAfterTheCurvesAreRead()
        {
            var path = SdkClipPath("proxy_fall_long");
            var otherPath = SdkClipPath("proxy_fall_short");
            Assume.That(path, Is.Not.Null.And.Not.EqualTo(otherPath), "the VRChat SDK sample clips are installed");
            var root = new GameObject("ClipFingerprintAvatar");
            var controller = new AnimatorController { name = "ClipFingerprintBase" };
            try
            {
                var avatar = root.AddComponent<VRCAvatarDescriptor>();
                controller.AddLayer("Base");
                var state = controller.layers[0].stateMachine.AddState("Fall");
                avatar.customizeAnimationLayers = true;
                avatar.baseAnimationLayers = new[] { new VRCAvatarDescriptor.CustomAnimLayer
                    { type = VRCAvatarDescriptor.AnimLayerType.Base, isDefault = false, animatorController = controller } };
                var tool = new GameObject("Tool").AddComponent<PosingSystem>();
                tool.transform.SetParent(root.transform);
                tool.autoImportAvatarAnimations = true;

                state.motion = Fresh(path);
                Assume.That(EditorJsonUtility.ToJson(state.motion), Does.Contain("\"m_EditorCurves\":[]"));
                var before = PosingSystemConverter.ImportedMotionInputSignature(tool);
                AnimationUtility.GetCurveBindings((AnimationClip)state.motion);
                Assume.That(EditorJsonUtility.ToJson(state.motion), Does.Not.Contain("\"m_EditorCurves\":[]"));
                PosingSystemClipFingerprint.Forget();
                Assert.That(PosingSystemConverter.ImportedMotionInputSignature(tool), Is.EqualTo(before), "after something read the curves");
                state.motion = Fresh(path);
                Assert.That(PosingSystemConverter.ImportedMotionInputSignature(tool), Is.EqualTo(before), "after the clip was loaded anew");

                // Another motion is still another signature.
                state.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(otherPath);
                Assert.That(PosingSystemConverter.ImportedMotionInputSignature(tool), Is.Not.EqualTo(before));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(controller, true);
            }
        }

        [Test]
        public void AnEditToASavedClipChangesItsRememberedFingerprint()
        {
            var folder = "Assets/USSPS_ClipFingerprintTests_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            try
            {
                var clip = new AnimationClip { name = "Saved" };
                clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 1));
                AssetDatabase.CreateAsset(clip, folder + "/Saved.anim");
                var first = PosingSystemClipFingerprint.Of(clip);
                Assert.That(PosingSystemClipFingerprint.Of(clip), Is.EqualTo(first));

                Undo.RecordObject(clip, "edit");
                clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 2));
                EditorUtility.SetDirty(clip);
                var edited = PosingSystemClipFingerprint.Of(clip);
                Assert.That(edited, Is.Not.EqualTo(first));

                // Saved and loaded again (as after a restart) the clip has a binding table and other
                // derived data that the edited clip in memory lacked: still the same fingerprint.
                var memory = EditorJsonUtility.ToJson(clip);
                AssetDatabase.SaveAssetIfDirty(clip);
                var loaded = Fresh(folder + "/Saved.anim");
                Assume.That(EditorJsonUtility.ToJson(loaded), Is.Not.EqualTo(memory), "loading derives data the edited clip did not have");
                Assert.That(PosingSystemClipFingerprint.Of(loaded), Is.EqualTo(edited));
                AnimationUtility.GetCurveBindings(loaded);
                PosingSystemClipFingerprint.Forget();
                Assert.That(PosingSystemClipFingerprint.Of(loaded), Is.EqualTo(edited));
            }
            finally { AssetDatabase.DeleteAsset(folder); PosingSystemClipFingerprint.Forget(); }
        }
    }
}
