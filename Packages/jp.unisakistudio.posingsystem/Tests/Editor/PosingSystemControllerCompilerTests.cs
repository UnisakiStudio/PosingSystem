using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    [UnityEngine.TestTools.PrebuildSetup(typeof(ControllerCompilerBatchSetup))]
    [UnityEngine.TestTools.PostBuildCleanup(typeof(ControllerCompilerBatchSetup))]
    public class PosingSystemControllerCompilerTests
    {
        private const string Template = "Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller";

        [Test]
        public void GEN01_SourceFactoryMatchesEveryReachableSerializedProperty()
        {
            var template = AssetDatabase.LoadAssetAtPath<AnimatorController>(Template);
            Assert.That(template, Is.Not.Null);
            var originals = AssetDatabase.LoadAllAssetsAtPath(Template).ToDictionary(Id);
            using var result = PosingSystemControllerCompiler.Create(false);
            var reachable = PosingSystemControllerGraph.Collect(new[] {template},
                o => AssetDatabase.GetAssetPath(o) == Template);
            CollectionAssert.AreEquivalent(reachable.Select(Id), result.Objects.Keys);
            Assert.That(result.Controller.layers.Length, Is.EqualTo(17));
            Assert.That(result.Controller.parameters.Length, Is.EqualTo(37));
            var resultIds = result.Objects.ToDictionary(p => p.Value, p => p.Key);
            var differences = new List<string>();
            foreach (var item in result.Objects)
            {
                var source = originals[item.Key];
                Assert.That(item.Value.GetType(), Is.EqualTo(source.GetType()), source.name);
                var expected = Serialize(source, Id);
                var actual = Serialize(item.Value, o => resultIds.TryGetValue(o, out var id) ? id : Id(o));
                if (expected != actual)
                {
                    differences.Add(source.GetType().Name + ":" + source.name + ":" + item.Key);
                    var directory = "Recordings/AnimatorCompiler_20261027/property-differences";
                    System.IO.Directory.CreateDirectory(directory);
                    System.IO.File.WriteAllText(directory + "/" + item.Key + "-source.txt", expected);
                    System.IO.File.WriteAllText(directory + "/" + item.Key + "-generated.txt", actual);
                }
            }
            Assert.That(differences, Is.Empty, string.Join("\n", differences));
            Assert.That(result.ReachableObjects().Count(), Is.EqualTo(reachable.Count));
        }

        [Test]
        public void GEN02_RepeatedGenerationHasIndependentIdenticalObjects()
        {
            using var a = PosingSystemControllerCompiler.Create(false);
            using var b = PosingSystemControllerCompiler.Create(false);
            Assert.That(a.Objects.Values.Intersect(b.Objects.Values), Is.Empty);
            foreach (var id in a.Objects.Keys)
                Assert.That(a.Objects[id].name, Is.EqualTo(b.Objects[id].name));
            var state = a.Objects.Values.OfType<AnimatorState>().First();
            var key = a.Objects.Single(p => p.Value == state).Key;
            var originalName = b.Objects[key].name;
            state.name = "Changed instance";
            Assert.That(b.Objects[key].name, Is.EqualTo(originalName));
        }

        [Test]
        public void GEN03_GenerationDoesNotChangeTemplateOrExternalAssets()
        {
            var original = AssetDatabase.LoadAssetAtPath<AnimatorController>(Template);
            var originalJson = EditorJsonUtility.ToJson(original);
            var deps = AssetDatabase.GetDependencies(Template).Where(p => p.StartsWith("Packages/"))
                .ToDictionary(p => p, p => AssetDatabase.GetAssetDependencyHash(p));
            using (var result = PosingSystemControllerCompiler.Create(true))
            {
                result.Controller.name = "Independent generated controller";
                result.Controller.layers = Array.Empty<AnimatorControllerLayer>();
            }
            Assert.That(EditorJsonUtility.ToJson(original), Is.EqualTo(originalJson));
            foreach (var pair in deps)
                Assert.That(AssetDatabase.GetAssetDependencyHash(pair.Key), Is.EqualTo(pair.Value), pair.Key);
        }

        [Test]
        public void GEN04_MissingDependencyFailsWithoutLeakingOwnedObjects()
        {
            var before = Resources.FindObjectsOfTypeAll<AnimatorController>().Select(o => o.GetInstanceID()).ToHashSet();
            Assert.Throws<InvalidOperationException>(() => PosingSystemControllerCompiler.Create(false, (_, _) => null));
            Assert.That(Resources.FindObjectsOfTypeAll<AnimatorController>().Select(o => o.GetInstanceID())
                .Where(id => !before.Contains(id)), Is.Empty);
        }

        [TestCase(false)] [TestCase(true)]
        public void GEN02_SaveReloadPreservesCrossMachineTransitionsAndDrivers(bool optimize)
        {
            var folder = "Assets/USSPS_Compiler_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            try
            {
                using var expected = PosingSystemControllerCompiler.Create(optimize);
                using var result = PosingSystemControllerCompiler.Create(optimize);
                // Unity names a persisted main asset after its filename.
                expected.Controller.name = result.Controller.name = "Generated";
                var expectedReachable = result.ReachableObjects().Count();
                var path = folder + "/Generated.controller";
                AssetDatabase.CreateAsset(result.Controller, path);
                foreach (var obj in result.Objects.Values.Where(o => o != result.Controller))
                    AssetDatabase.AddObjectToAsset(obj, result.Controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var loaded = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                var graph = PosingSystemControllerGraph.Collect(new[] {loaded}, o => AssetDatabase.GetAssetPath(o) == path);
                Assert.That(graph.Count, Is.EqualTo(expectedReachable));
                Assert.That(graph.OfType<StateMachineBehaviour>().Count(), Is.EqualTo(52));
                var locomotion = loaded.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
                Assert.That(locomotion.GetStateMachineTransitions(locomotion.stateMachines[0].stateMachine), Is.Not.Empty);
                Assert.That(graph.OfType<AnimatorState>().All(s => s.motion != null), Is.True);
                Assert.That(PosingSystemControllerEquivalence.Matches(expected.Controller, loaded, out var reason),
                    Is.True, "Saved graph differs from an independently generated controller: " + reason);
            }
            finally { AssetDatabase.DeleteAsset(folder); }
        }

        [Test]
        public void PERF01_DeduplicationKeepsOrderingAndAllDistinctTransitions()
        {
            using var original = PosingSystemControllerCompiler.Create(false);
            using var optimized = PosingSystemControllerCompiler.Create(false);
            PosingSystemControllerCompiler.RemoveDuplicates(optimized);
            Assert.That(optimized.RemovedDuplicateTransitions, Is.GreaterThan(0));
            var originalIds = original.Objects.ToDictionary(p => p.Value, p => p.Key);
            var optimizedIds = optimized.Objects.ToDictionary(p => p.Value, p => p.Key);
            foreach (var pair in original.Objects.Where(p => p.Value is AnimatorState))
            {
                var before = (AnimatorState)pair.Value;
                var after = (AnimatorState)optimized.Objects[pair.Key];
                Assert.That(after.transitions.Length, Is.LessThanOrEqualTo(before.transitions.Length));
                var expected = before.transitions.Select(t => Signature(t, o => originalIds[o])).Distinct().ToArray();
                var actual = after.transitions.Select(t => Signature(t, o => optimizedIds[o])).ToArray();
                Assert.That(actual, Is.EqualTo(expected), before.name + "\nExpected:\n" + string.Join("\n", expected)
                    + "\nActual:\n" + string.Join("\n", actual));
            }
        }

        [TestCase("unchanged")]
        [TestCase("condition")]
        [TestCase("driver")]
        [TestCase("parameter")]
        [TestCase("target")]
        public void GEN05_AnonymousTemplateRecognitionPreservesEveryCustomChange(string mutation)
        {
            var source = AssetDatabase.LoadAssetAtPath<AnimatorController>(Template);
            using var generated = PosingSystemControllerCompiler.Create(false);
            switch (mutation)
            {
                case "condition":
                    var t = generated.Objects.Values.OfType<AnimatorStateTransition>().First(x => x.conditions.Length > 0);
                    var c = t.conditions; c[0].threshold += 1; t.conditions = c; break;
                case "driver":
                    generated.Objects.Values.OfType<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>().First().parameters[0].value += 1;
                    break;
                case "parameter":
                    var parameters = generated.Controller.parameters; parameters[0].defaultBool = !parameters[0].defaultBool;
                    generated.Controller.parameters = parameters; break;
                case "target":
                    generated.Objects.Values.OfType<AnimatorStateTransition>().First().destinationState =
                        generated.Objects.Values.OfType<AnimatorState>().Last(); break;
            }
            var matches = PosingSystemControllerEquivalence.Matches(source, generated.Controller, out var reason);
            Assert.That(matches, Is.EqualTo(mutation == "unchanged"), reason);
        }

        private static string Signature(AnimatorStateTransition t, Func<Object, long> id)
        {
            return string.Join("|", t.name, t.destinationState != null ? id(t.destinationState) : 0,
                t.destinationStateMachine != null ? id(t.destinationStateMachine) : 0, t.isExit,
                t.hasExitTime, t.exitTime.ToString("R"), t.duration.ToString("R"), t.offset.ToString("R"), t.hasFixedDuration, t.interruptionSource,
                t.orderedInterruption, t.canTransitionToSelf, t.mute, t.solo,
                string.Join(";", t.conditions.Select(c => c.parameter + ":" + c.mode + ":" + c.threshold.ToString("R"))));
        }

        private static long Id(Object obj)
        {
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long id), Is.True,
                "Expected a persisted source reference: " + obj?.name);
            return id;
        }

        // Compare serialized data independently of the generator's interpretation of those fields.
        // Only editor hide flags and the script reference (the concrete type is checked separately) are excluded.
        private static string Serialize(Object obj, Func<Object, long> localId)
        {
            var values = new List<string>();
            using var serialized = new SerializedObject(obj);
            var p = serialized.GetIterator();
            var descend = true;
            while (p.Next(descend))
            {
                // Object references have implementation-specific instance-ID children. Compare the
                // normalized GUID/local-ID above them, never those per-instance storage details.
                descend = p.propertyType != SerializedPropertyType.ObjectReference;
                if (p.propertyPath == "m_ObjectHideFlags" || p.propertyPath == "m_Script") continue;
                object value;
                switch (p.propertyType)
                {
                    case SerializedPropertyType.Generic: value = "container"; break;
                    case SerializedPropertyType.Integer: value = p.longValue; break;
                    case SerializedPropertyType.Boolean: value = p.boolValue; break;
                    case SerializedPropertyType.Float: value = p.doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture); break;
                    case SerializedPropertyType.String: value = p.stringValue; break;
                    case SerializedPropertyType.Enum: value = p.intValue; break;
                    case SerializedPropertyType.ArraySize: value = p.intValue; break;
                    case SerializedPropertyType.Vector2: value = p.vector2Value.ToString("R"); break;
                    case SerializedPropertyType.Vector3: value = p.vector3Value.ToString("R"); break;
                    case SerializedPropertyType.Vector4: value = p.vector4Value.ToString("R"); break;
                    case SerializedPropertyType.Quaternion: value = p.quaternionValue.ToString("R"); break;
                    case SerializedPropertyType.ObjectReference:
                        var reference = p.objectReferenceValue;
                        if (reference == null) value = "null";
                        else
                        {
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long id);
                            var external = EditorUtility.IsPersistent(reference) && AssetDatabase.GetAssetPath(reference) != Template;
                            value = external ? guid + ":" + id : "local:" + localId(reference);
                        }
                        break;
                    default: throw new NotSupportedException("Untested serialized property: " + p.propertyPath + ":" + p.propertyType);
                }
                values.Add(p.propertyPath + "=" + value);
            }
            // Unity stores this dictionary in API-dependent key order. Transition order WITHIN
            // each source machine is significant and remains unchanged in this canonical form.
            if (obj is AnimatorStateMachine)
            {
                var pattern = new System.Text.RegularExpressions.Regex(@"^m_StateMachineTransitions.Array.data\[(\d+)\]");
                var entries = values.Where(v => pattern.IsMatch(v)).GroupBy(v => pattern.Match(v).Groups[1].Value)
                    .OrderBy(g => g.Single(v => v.Contains("].first=")).Split('=').Last(), StringComparer.Ordinal).ToArray();
                values.RemoveAll(v => pattern.IsMatch(v));
                for (var i = 0; i < entries.Length; i++)
                    values.AddRange(entries[i].Select(v => pattern.Replace(v, "m_StateMachineTransitions.Array.data[" + i + "]")));
            }
            return string.Join("\n", values);
        }
    }

    // NDMF opens its preview scene during batch startup. Unity Test Framework 1.3.9 cannot
    // add the test scene while the other startup scene is still untitled. Save only the new,
    // empty batch scene; never save or close a user scene through this setup.
    public sealed class ControllerCompilerBatchSetup : UnityEngine.TestTools.IPrebuildSetup, UnityEngine.TestTools.IPostBuildCleanup
    {
        private const string Key = "USSPS.ControllerCompiler.BatchScene";
        public void Setup()
        {
            if (!Application.isBatchMode) return;
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!string.IsNullOrEmpty(scene.path) && scene.path != "Packages/nadena.dev.ndmf/Runtime/___NDMF Preview___.unity")
                    throw new InvalidOperationException("Refusing to replace a saved user scene during batch setup: " + scene.path);
            }
            var empty = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var path = "Assets/USSPS_CompilerBatch_" + Guid.NewGuid().ToString("N") + ".unity";
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(empty, path))
                throw new InvalidOperationException("Could not create isolated batch scene");
            SessionState.SetString(Key, path);
        }
        public void Cleanup()
        {
            if (!Application.isBatchMode) return;
            var path = SessionState.GetString(Key, "");
            if (!path.StartsWith("Assets/USSPS_CompilerBatch_", StringComparison.Ordinal) || !path.EndsWith(".unity", StringComparison.Ordinal)) return;
            // Test Framework restores its saved setup after IPostBuildCleanup. Removing the
            // isolated scene here makes that restoration fail. Delete it only on batch exit.
            EditorApplication.quitting += () => {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                    UnityEditor.SceneManagement.NewSceneMode.Single);
                AssetDatabase.DeleteAsset(path);
            };
            SessionState.EraseString(Key);
        }
    }
}
