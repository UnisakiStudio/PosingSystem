#if NDMF && MODULAR_AVATAR
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using nadena.dev.ndmf.vrchat;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        public const string PhotographyHeadObject = "USSPS_PhotographyHead";
        public const string PhotographyHeadLayer = "USSPS_PhotographyHeadVisibility";

        private static bool UsesPhotographyHeadVisibility(PosingSystem ps) =>
            ps != null && ps.gameObject.activeInHierarchy && !ps.isEightBitVersion &&
            ps.features?.photography == true && ps.features.photographyKeepHeadVisible &&
            !ps.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly"));

        public static PosingSystemDiagnostics.Issue PhotographyHeadIssue(PosingSystem ps)
        {
            if (!UsesPhotographyHeadVisibility(ps)) return null;
            var avatar = ps.GetAvatar();
            if (avatar == null) return null;
            var headChops = avatar.GetComponentsInChildren<VRCHeadChop>(true)
                .Where(c => !c.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly"))).ToArray();
            var animator = avatar.GetComponent<Animator>();
            var head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            // Two components targeting the same bone have no documented priority. Do not
            // replace someone else's settings or animations to make our override win.
            var conflict = head == null ? null : headChops.FirstOrDefault(c =>
                c.targetBones != null && c.targetBones.Any(b => b != null && b.transform == head));
            if (conflict != null)
                return new PosingSystemDiagnostics.Issue("PS080", "頭のボーンには既にVRC Head Chopが設定されています。",
                    "既存のHead Chop設定との重複を避けるため、「撮影モード中は頭を非表示にしない」をオフにするか、既存設定のHead対象を見直してください。耳など別のボーンの設定はそのまま併用できます。",
                    MessageType.Error, conflict);
            if (headChops.Length >= VRCHeadChop.MaxComponentCount)
                return new PosingSystemDiagnostics.Issue("PS081", "VRC Head Chopの数がVRChatの上限に達しています。",
                    "「撮影モード中は頭を非表示にしない」をオフにするか、既存のVRC Head Chopをまとめて1個分の空きを作ってください。",
                    MessageType.Error, ps);
            return null;
        }

        private static void BuildPhotographyHeadVisibility(BuildContext ctx)
        {
            // Build-only: never put this component or rewrite a renderer in the user's scene.
            // Any enabled posing system opts the avatar in; generate only one shared override.
            var owner = ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>()
                .FirstOrDefault(UsesPhotographyHeadVisibility);
            if (owner == null) return;
            var issue = PhotographyHeadIssue(owner);
            if (issue != null)
                // A conflicting Head Chop may live outside the tool hierarchy.
                // Use the owner here so its explicit language also applies to build errors.
                throw PosingSystemDiagnostics.Failure(issue.Code, issue.Summary, issue.Action, owner);
            var head = ctx.AvatarRootObject.GetComponent<Animator>()?.GetBoneTransform(HumanBodyBones.Head);
            if (head == null)
                throw new System.InvalidOperationException(PosingSystemLocalization.FormatFor(owner, "撮影中の頭表示に必要なHeadボーンを取得できません。"));
            var descriptor = ctx.VRChatAvatarDescriptor();
            var slots = descriptor.baseAnimationLayers;
            var fxIndex = System.Array.FindIndex(slots, l => l.type == VRCAvatarDescriptor.AnimLayerType.FX);
            if (fxIndex < 0 || slots[fxIndex].animatorController is not AnimatorController controller ||
                !ctx.IsTemporaryAsset(controller))
                throw new System.InvalidOperationException(PosingSystemLocalization.FormatFor(owner, "撮影中の頭表示にはビルド用FX Controllerの複製が必要です。"));

            var obj = new GameObject(PhotographyHeadObject);
            obj.transform.SetParent(owner.transform, false);
            var chop = obj.AddComponent<VRCHeadChop>();
            chop.enabled = false;
            chop.globalScaleFactor = 1;
            chop.targetBones = new[] { new VRCHeadChop.HeadChopBone {
                transform = head, scaleFactor = 1,
                applyCondition = VRCHeadChop.HeadChopBone.ApplyCondition.AlwaysApply
            } };

            FeatureParameter(controller, "IsLocal", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.PhotoActive, AnimatorControllerParameterType.Bool);
            var machine = new AnimatorStateMachine { name = PhotographyHeadLayer };
            ctx.AssetSaver.SaveAsset(machine);
            var layer = new AnimatorControllerLayer { name = PhotographyHeadLayer, stateMachine = machine,
                defaultWeight = 1, blendingMode = AnimatorLayerBlendingMode.Override };
            var off = machine.AddState("通常の頭表示");
            var on = machine.AddState("撮影中は頭を表示");
            machine.defaultState = off;
            var path = AnimationUtility.CalculateTransformPath(obj.transform, ctx.AvatarRootTransform);
            foreach (var state in new[] { off, on })
            {
                state.writeDefaultValues = false;
                var clip = new AnimationClip { name = PhotographyHeadLayer + (state == on ? "_On" : "_Off") };
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(VRCHeadChop), "m_Enabled"),
                    AnimationCurve.Constant(0, 1, state == on ? 1 : 0));
                state.motion = clip;
                ctx.AssetSaver.SaveAsset(clip);
            }
            var enter = FeatureTransition(off.AddTransition(on));
            enter.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");
            enter.AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.PhotoActive);
            FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");
            FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.PhotoActive);
            PersistFeatureStates(layer, controller, ctx.AssetSaver);
            // Reassign layers to invalidate NDMF's LastCommit cache, including after MA Replace.
            controller.layers = controller.layers.Concat(new[] { layer }).ToArray();
            slots[fxIndex].isDefault = false;
            descriptor.customizeAnimationLayers = true;
            descriptor.baseAnimationLayers = slots;
        }
    }
}
#endif
