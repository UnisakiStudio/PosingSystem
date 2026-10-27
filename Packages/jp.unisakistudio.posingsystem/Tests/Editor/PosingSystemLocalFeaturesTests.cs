using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StateType = jp.unisakistudio.posingsystem.PosingSystem.OverrideAnimationDefine.AnimationStateType;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemLocalFeaturesTests
    {
        private GameObject root;
        private PosingSystem ps;
        [SetUp] public void SetUp()
        {
            root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")));
            ps = root.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { mirror = false, footHeight = false, fingerGestureControl = false,
                breathing = true, breathingControl = false, photography = true, photographySynced = false };
            ps.featureSettingsMigrated = true;
            ps.additiveBreathing.enabled = true;
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void LocalFeatures_KeepMotionAndPhotoMenus_WithNoExtraBits_AndRestoreSync()
        {
            foreach (var synced in new[] { false, true, false })
            {
                ps.features.photographySynced = ps.features.breathingControl = synced;
                PosingSystemFeatureUtility.Synchronize(ps);
                var parameters = ps.GetComponent<ModularAvatarParameters>().parameters;
                Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.PhotoDistance).localOnly, Is.EqualTo(!synced));
                Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.PhotoYaw).localOnly, Is.EqualTo(!synced));
                Assert.That(parameters.Any(p => p.nameOrPrefix == "USSPS_Breathing"), Is.EqualTo(synced));
                Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(synced ? 17 : 0));
                Assert.That(PosingSystemFeatureUtility.UsesBreathing(ps), Is.True);
                Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>().Any(m => m.name == PosingSystemFeatureUtility.PhotoMenuName), Is.True);
                Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>().Any(m => m.Control.parameter?.name == "USSPS_Breathing"), Is.EqualTo(synced));
            }
        }

        [Test]
        public void SyncChoices_AreSaved_AndInvalidatePrebuildIndependently()
        {
            var initial = PosingSystemConverter.GetDefineSerializeJson(ps, true);
            ps.features.breathingControl = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.Not.EqualTo(initial));
            ps.features.breathingControl = false; ps.features.photographySynced = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.Not.EqualTo(initial));
            ps.features.photographySynced = false;
            var saved = EditorJsonUtility.ToJson(ps);
            ps.features.photographySynced = ps.features.breathingControl = true;
            EditorJsonUtility.FromJsonOverwrite(saved, ps);
            Assert.That(ps.features.photographySynced || ps.features.breathingControl, Is.False);
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.EqualTo(initial));
        }

        [Test]
        public void ImportSelection_UsesOneMotionPerType_PreservesAlternativesAndUndo()
        {
            var first = new AnimationClip(); var second = new AnimationClip();
            try
            {
                ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine> {
                    new() { stateType=StateType.Stand, animationClip=first },
                    new() { stateType=StateType.Stand, animationClip=second },
                    new() { stateType=StateType.Jump, animationClip=first } };
                Assert.That(PosingSystemMotionImportUtility.SelectedIndex(ps, StateType.Stand), Is.EqualTo(1));
                Undo.IncrementCurrentGroup();
                PosingSystemMotionImportUtility.Select(ps, 0);
                Undo.FlushUndoRecordObjects();
                Assert.That(ps.overrideDefines.Count(d => d.stateType == StateType.Stand && d.enabled), Is.EqualTo(1));
                Assert.That(ps.overrideDefines.Select(d => d.animationClip), Does.Contain(second));
                Assert.That(ps.overrideDefines[2].enabled, Is.True);
                Undo.PerformUndo();
                Assert.That(ps.overrideDefines[1].enabled, Is.True);
                PosingSystemMotionImportUtility.SetMotion(ps, StateType.Stand, null);
                Assert.That(ps.overrideDefines.Any(d => d.stateType == StateType.Stand && d.enabled), Is.False);
                Assert.Throws<System.ArgumentException>(() => PosingSystemMotionImportUtility.SetMotion(ps, StateType.StandWalkRun, first));
                PosingSystemMotionImportUtility.SetMotion(ps, StateType.AvatarSelect, first);
                PosingSystemMotionImportUtility.SetMotion(ps, StateType.AvatarSelect, second);
                Assert.That(ps.overrideDefines.Count(d => d.stateType == StateType.AvatarSelect), Is.EqualTo(1));
                Assert.That(ps.overrideDefines.Last().animationClip, Is.SameAs(second));
            }
            finally { Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        }
    }
}
