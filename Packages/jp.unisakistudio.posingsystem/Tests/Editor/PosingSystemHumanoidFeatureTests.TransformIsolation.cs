using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(0f)] [TestCase(.5f)] [TestCase(1f)]
        public void FootHeight_SyncedLayer_DoesNotAccumulateUnanimatedAccessoryRotation(float height)
        {
            var hair = new GameObject("Hair").transform;
            hair.SetParent(animator.GetBoneTransform(HumanBodyBones.Head), false);
            hair.localRotation = Quaternion.Euler(12, 23, -9);
            var originalRotation = hair.localRotation;
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "Pose");
            var unused = Object.Instantiate(source);
            assets.Add(source); assets.Add(unused);
            // A different pose animates this bone, so its binding exists in the controller.
            var path = AnimationUtility.CalculateTransformPath(hair, root.transform);
            for (var axis = 0; axis < 4; axis++)
                AnimationUtility.SetEditorCurve(unused, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation." + "xyzw"[axis]),
                    AnimationCurve.Constant(0, 1, originalRotation[axis]));
            var controller = new AnimatorController(); assets.Add(controller);
            var machine = new AnimatorStateMachine(); assets.Add(machine);
            var active = machine.AddState("Active"); active.writeDefaultValues = false;
            var other = machine.AddState("Other"); other.motion = unused; other.writeDefaultValues = false;
            assets.Add(active); assets.Add(other);
            var tree = new BlendTree { blendType = BlendTreeType.Simple1D, blendParameter = "FootHeight", useAutomaticThresholds = false };
            tree.AddChild(source, .5f); assets.Add(tree); active.motion = tree;
            var main = new AnimatorControllerLayer { name = "USSPS_Locomotion", stateMachine = machine, defaultWeight = 1 };
            controller.layers = new[] { main };
            root.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var context = new nadena.dev.ndmf.BuildContext(root, null, false);
            try
            {
                typeof(PosingSystemConverter).GetMethod("BuildAdditiveFootHeight", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] { controller, main, context.AssetSaver });
            }
            finally { context.DeactivateAllExtensionContexts(); }
            var correction = controller.layers.Last();
            assets.Add(correction.avatarMask); assets.Add(correction.stateMachine);
            var graph = PlayableGraph.Create("Accessory isolation regression");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var player = AnimatorControllerPlayable.Create(graph, controller);
                AnimationPlayableOutput.Create(graph, "Avatar", animator).SetSourcePlayable(player);
                player.SetFloat("FootHeight", height);
                graph.Play();
                for (var frame = 0; frame < 120; frame++)
                {
                    graph.Evaluate(1f / 60);
                    Assert.That(Quaternion.Angle(originalRotation, hair.localRotation), Is.LessThan(.05f), "Frame " + frame);
                }
                var correctedY = animator.GetBoneTransform(HumanBodyBones.Hips).position.y;
                player.SetLayerWeight(1, 0); graph.Evaluate(1f / 60);
                var delta = correctedY - animator.GetBoneTransform(HumanBodyBones.Hips).position.y;
                if (height < .5f) Assert.That(delta, Is.LessThan(-.1f));
                else if (height > .5f) Assert.That(delta, Is.GreaterThan(.1f));
                else Assert.That(Mathf.Abs(delta), Is.LessThan(.001f));
                // The original pose's intentional accessory animation remains available.
                player.Play("Other", 0, 0); graph.Evaluate(1f / 60);
                Assert.That(Quaternion.Angle(originalRotation, hair.localRotation), Is.LessThan(.05f));
            }
            finally { graph.Destroy(); }
        }

        [Test]
        public void HeadSensor_NestedToolObject_PreservesWorldCalibrationAndHeadSources()
        {
            root.transform.position = new Vector3(2, .3f, -1);
            root.transform.rotation = Quaternion.Euler(0, 42, 0);
            var direct = PosingSystemHeadSensorUtility.Create(animator, root.transform);
            var tool = new GameObject("可愛いポーズ").transform;
            tool.SetParent(root.transform, false);
            tool.localPosition = new Vector3(.2f, -.05f, .3f);
            tool.localRotation = Quaternion.Euler(10, 35, -5);
            tool.localScale = Vector3.one * .7f;
            var nested = PosingSystemHeadSensorUtility.Create(animator, tool);
            Assert.That(nested.transform.parent, Is.SameAs(tool));
            Assert.That(Vector3.Distance(direct.transform.position, nested.transform.position), Is.LessThan(.0001f));
            Assert.That(Quaternion.Angle(direct.transform.rotation, nested.transform.rotation), Is.LessThan(.01f));
            Assert.That(Vector3.Distance(direct.transform.lossyScale, nested.transform.lossyScale), Is.LessThan(.0001f));
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            foreach (var constraint in nested.GetComponentsInChildren<Component>().Where(c =>
                         c.GetType().Name == "VRCPositionConstraint" || c.GetType().Name == "VRCRotationConstraint"))
                Assert.That(new SerializedObject(constraint).FindProperty("Sources.source0.SourceTransform").objectReferenceValue, Is.SameAs(head));
            var directProbe = direct.transform.Find("USSPS_HeadDirectionProbe");
            var nestedProbe = nested.transform.Find("USSPS_HeadDirectionProbe");
            Assert.That(Quaternion.Angle(directProbe.rotation, nestedProbe.rotation), Is.LessThan(.01f));
        }
    }
}
