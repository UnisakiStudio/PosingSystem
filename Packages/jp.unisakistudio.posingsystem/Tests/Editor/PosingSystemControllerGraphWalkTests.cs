#if NDMF && MODULAR_AVATAR
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // The reference walk skips strings, curves and arrays of plain values. It must still find
    // every reference that visiting every property finds.
    public class PosingSystemControllerGraphWalkTests
    {
        private static List<Object> Everything(Object obj)
        {
            var found = new List<Object>();
            using var serialized = new SerializedObject(obj);
            var it = serialized.GetIterator();
            while (it.Next(true))
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                    found.Add(it.objectReferenceValue);
            return found;
        }

        [Test]
        public void SkippingPlainDataFindsTheSameReferencesInTheStandardController()
        {
            var controller = PosingSystemDiagnostics.CommonAnimatorTemplate()?.animator as AnimatorController;
            Assert.That(controller, Is.Not.Null, "standard controller");
            var graph = PosingSystemControllerGraph.Collect(new Object[] { controller }, PosingSystemControllerGraph.IsGraphObject);
            Assert.That(graph.Count, Is.GreaterThan(100));
            Assert.That(graph.OfType<AnimationClip>().Any() && graph.OfType<AnimatorStateTransition>().Any() && graph.OfType<StateMachineBehaviour>().Any(), Is.True);
            foreach (var obj in graph)
                Assert.That(PosingSystemControllerGraph.References(obj).ToList(), Is.EqualTo(Everything(obj)), obj.GetType().Name + " " + obj.name);
        }

        [Test]
        public void ReferencesInsideClipsAndComponentsAreFound()
        {
            var material = new Material(Shader.Find("Hidden/Internal-Colored"));
            var clip = new AnimationClip();
            var go = new GameObject("graph walk");
            try
            {
                // Position curves are stored as arrays of keyframes, float curves as curves, and an
                // object curve holds the reference that must be found.
                clip.SetCurve("Child", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 1));
                clip.SetCurve("", typeof(Animator), "RootT.y", AnimationCurve.Linear(0, 0, 1, 1));
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Body", typeof(Renderer), "m_Materials.Array.data[0]"),
                    new[] { new ObjectReferenceKeyframe { time = 0, value = material } });
                Assert.That(PosingSystemControllerGraph.References(clip).ToList(), Is.EqualTo(Everything(clip)));
                Assert.That(PosingSystemControllerGraph.References(clip), Does.Contain(material));

                var merge = go.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>();
                merge.animator = PosingSystemDiagnostics.CommonAnimatorTemplate().animator;
                Assert.That(PosingSystemControllerGraph.References(merge).ToList(), Is.EqualTo(Everything(merge)));
                Assert.That(PosingSystemControllerGraph.References(merge), Does.Contain(merge.animator));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(material);
            }
        }
    }
}
#endif
