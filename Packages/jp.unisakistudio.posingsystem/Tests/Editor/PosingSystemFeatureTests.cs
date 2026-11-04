using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemFeatureTests
    {
        private GameObject instance;
        private PosingSystem ps;

        [SetUp]
        public void SetUp()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c"));
            instance = Object.Instantiate(prefab);
            ps = instance.GetComponent<PosingSystem>();
        }

        [TearDown] public void TearDown() => Object.DestroyImmediate(instance);

        [Test]
        public void Defaults_DoNotAllocateNewFeatureBits()
        {
            Assert.That(ps.features.faceForward, Is.False);
            Assert.That(ps.features.photography, Is.False);
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.Zero, "The common settings prefab starts with optional features off");
            Assert.That(ps.GetComponent<ModularAvatarParameters>().parameters.Single(p => p.nameOrPrefix == "HeadTrackingLock").localOnly, Is.True);
        }

        [Test]
        public void MovementDefaults_UseFinalizedImplementations()
        {
            AssertFinalizedMovement(new PosingSystemFeatureSettings());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MovementMigration_UpgradesSerializedComparisonsWithoutChangingFeatureChoices(bool alreadyMigrated)
        {
            foreach (PosingSystemFeatureSettings.MovementBackend backend in System.Enum.GetValues(typeof(PosingSystemFeatureSettings.MovementBackend)))
            foreach (PosingSystemFeatureSettings.FloatingPoseSpaceMode mode in System.Enum.GetValues(typeof(PosingSystemFeatureSettings.FloatingPoseSpaceMode)))
            {
                ps.featureSettingsMigrated = alreadyMigrated;
                ps.features = JsonUtility.FromJson<PosingSystemFeatureSettings>(JsonUtility.ToJson(new PosingSystemFeatureSettings
                {
                    footHeightBackend = PosingSystemFeatureSettings.FootHeightBackend.ClipLevel,
                    photographyBackend = backend, floatingBackend = backend, floatingPoseSpace = mode,
                    photography = true, photographySynced = false, floating = true, breathingControl = false
                }));
                var bits = PosingSystemFeatureUtility.OptionalBits(ps);
                PosingSystemFeatureUtility.Migrate(ps);
                AssertFinalizedMovement(ps.features);
                Assert.That(ps.features.photography && ps.features.floating, Is.True);
                Assert.That(ps.features.photographySynced || ps.features.breathingControl, Is.False);
                Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(bits));
                var saved = JsonUtility.ToJson(ps.features);
                ps.features = JsonUtility.FromJson<PosingSystemFeatureSettings>(saved);
                PosingSystemFeatureUtility.Migrate(ps);
                Assert.That(JsonUtility.ToJson(ps.features), Is.EqualTo(saved));
            }
        }

        private static void AssertFinalizedMovement(PosingSystemFeatureSettings settings)
        {
            Assert.That(settings.footHeightBackend, Is.EqualTo(PosingSystemFeatureSettings.FootHeightBackend.PoseSpaceAdditive));
            Assert.That(settings.photographyBackend, Is.EqualTo(PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive));
            Assert.That(settings.floatingBackend, Is.EqualTo(PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive));
            Assert.That(settings.floatingPoseSpace, Is.EqualTo(PosingSystemFeatureSettings.FloatingPoseSpaceMode.HeightMenuOnly));
        }

        [Test]
        public void AllFeatureCombinations_MatchActualMaCost_AndAreIdempotent()
        {
            for (var flags = 0; flags < 1024; flags++)
            {
                ps.features.breathingControl = (flags & 256) != 0;
                ps.features.photographySynced = (flags & 512) != 0;
                ps.features.mirror = (flags & 1) != 0;
                ps.features.footHeight = (flags & 2) != 0;
                ps.features.breathing = (flags & 4) != 0;
                ps.features.faceForward = (flags & 8) != 0;
                ps.features.photography = (flags & 16) != 0;
                ps.features.floating = (flags & 32) != 0;
                ps.features.rollover = (flags & 64) != 0;
                ps.features.fingerGestureControl = (flags & 128) != 0;
                PosingSystemFeatureUtility.Synchronize(ps);
                var ma = ps.GetComponent<ModularAvatarParameters>();
                var actual = ps.GetComponentsInChildren<ModularAvatarParameters>(true).SelectMany(p => p.parameters)
                    .Where(p => !p.localOnly && p.syncType != ParameterSyncType.NotSynced && !p.nameOrPrefix.StartsWith("USSPS_Pose"))
                    .Sum(p => p.syncType == ParameterSyncType.Bool ? 1 : 8);
                Assert.That(actual, Is.EqualTo(PosingSystemFeatureUtility.OptionalBits(ps)), "flags=" + flags);
                var before = EditorJsonUtility.ToJson(ma);
                PosingSystemFeatureUtility.Synchronize(ps);
                Assert.That(EditorJsonUtility.ToJson(ma), Is.EqualTo(before), "Repeated sync: " + flags);
            }
        }

        [Test]
        public void RolloverMapping_IsMonotonicSymmetricAndHasNeutralFallback()
        {
            Assert.That(PosingSystemHeadSensorUtility.RolloverTime(0, 60, 3), Is.EqualTo(.5f));
            Assert.That(PosingSystemHeadSensorUtility.RolloverTime(.5f, 60, 3), Is.EqualTo(.5f));
            var previous = 0f;
            for (var angle = -90; angle <= 90; angle++)
            {
                var value = .5f + Mathf.Sin(angle * Mathf.Deg2Rad) / PosingSystemHeadSensorUtility.Span;
                var actual = PosingSystemHeadSensorUtility.RolloverTime(value, 60, 3);
                Assert.That(actual, Is.GreaterThanOrEqualTo(previous - .00001f));
                Assert.That(actual + PosingSystemHeadSensorUtility.RolloverTime(1-value, 60, 3), Is.EqualTo(1).Within(.0001f));
                if (Mathf.Abs(angle) <= 3) Assert.That(actual, Is.EqualTo(.5f).Within(.0001f));
                if (angle <= -60) Assert.That(actual, Is.EqualTo(0).Within(.0001f));
                if (angle >= 60) Assert.That(actual, Is.EqualTo(1).Within(.0001f));
                previous = actual;
            }
        }

        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, 10)]
        [TestCase(true, true, 10)]
        public void RolloverAndFaceForward_CountToggleAndTilt_AndShareOneHeadLock(bool faceForward, bool rollover, int expectedBits)
        {
            ps.features = new PosingSystemFeatureSettings {
                mirror = false, footHeight = false, breathing = false, fingerGestureControl = false,
                faceForward = faceForward, rollover = rollover
            };
            PosingSystemFeatureUtility.Synchronize(ps);
            var parameters = ps.GetComponentsInChildren<ModularAvatarParameters>(true)
                .SelectMany(p => p.parameters).Where(p => !p.localOnly && p.syncType != ParameterSyncType.NotSynced &&
                    !p.nameOrPrefix.StartsWith("USSPS_Pose")).ToArray();
            Assert.That(parameters.Sum(p => p.syncType == ParameterSyncType.Bool ? 1 : 8), Is.EqualTo(expectedBits));
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(expectedBits));
            Assert.That(PosingSystemFeatureUtility.AvatarOptionalBits(ps), Is.EqualTo(expectedBits));
            Assert.That(parameters.Count(p => p.nameOrPrefix == "HeadTrackingLock"), Is.EqualTo(faceForward || rollover ? 1 : 0));
            Assert.That(parameters.Any(p => p.nameOrPrefix == PosingSystemFeatureUtility.RollActive && p.syncType == ParameterSyncType.Bool), Is.EqualTo(rollover));
            Assert.That(parameters.Any(p => p.nameOrPrefix == PosingSystemFeatureUtility.RollSensor && p.syncType == ParameterSyncType.Float), Is.EqualTo(rollover));
            // Repeated toggles must release only the independent rollover parameters, retaining the shared lock.
            if (faceForward && rollover)
            {
                ps.features.rollover = false;
                PosingSystemFeatureUtility.Synchronize(ps);
                Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(1));
                ps.features.rollover = true;
                PosingSystemFeatureUtility.Synchronize(ps);
                Assert.That(PosingSystemFeatureUtility.OptionalBits(ps) - 1, Is.EqualTo(9));
                ps.features.faceForward = false;
                PosingSystemFeatureUtility.Synchronize(ps);
                Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(10));
            }
        }

        [TestCase(false, false, 1, 10)]
        [TestCase(true, false, 1, 9)]
        [TestCase(false, true, 0, 10)]
        [TestCase(true, true, 0, 9)]
        public void SharedHeadCardBits_MatchActualParameterChanges_WhenEitherCheckboxIsToggled(
            bool faceForward, bool rollover, int faceBits, int rolloverBits)
        {
            ps.features.faceForward = faceForward;
            ps.features.rollover = rollover;
            PosingSystemFeatureUtility.Synchronize(ps);
            var before = EditorJsonUtility.ToJson(ps);
            Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, false), Is.EqualTo(faceBits));
            Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, true), Is.EqualTo(rolloverBits));
            Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(before), "Measuring the card must not edit settings");
            foreach (var measureRollover in new[] { false, true })
            {
                var displayed = PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, measureRollover);
                if (measureRollover) ps.features.rollover = true; else ps.features.faceForward = true;
                PosingSystemFeatureUtility.Synchronize(ps);
                var onBits = ActualBits();
                if (measureRollover) ps.features.rollover = false; else ps.features.faceForward = false;
                PosingSystemFeatureUtility.Synchronize(ps);
                Assert.That(onBits - ActualBits(), Is.EqualTo(displayed));
                ps.features.faceForward = faceForward;
                ps.features.rollover = rollover;
                PosingSystemFeatureUtility.Synchronize(ps);
            }
            int ActualBits() => ps.GetComponentsInChildren<ModularAvatarParameters>(true).SelectMany(p => p.parameters)
                .Where(p => !p.localOnly && p.syncType != ParameterSyncType.NotSynced)
                .Sum(p => p.syncType == ParameterSyncType.Bool ? 1 : 8);
        }

        [Test]
        public void SharedHeadCardBits_RespectOtherActiveSystems_AndIgnoreExcludedSystems()
        {
            var avatar = new GameObject("Shared-bit avatar");
            avatar.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            instance.transform.SetParent(avatar.transform);
            var otherObject = new GameObject("Other posing system");
            otherObject.transform.SetParent(avatar.transform);
            var other = otherObject.AddComponent<PosingSystem>();
            other.features = new PosingSystemFeatureSettings { faceForward = true };
            ps.features.faceForward = ps.features.rollover = false;
            try
            {
                Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, false), Is.Zero);
                Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, true), Is.EqualTo(9));
                other.features.rollover = true;
                Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, true), Is.Zero);
                otherObject.SetActive(false);
                AssertUnshared();
                otherObject.SetActive(true); otherObject.tag = "EditorOnly";
                AssertUnshared();
                otherObject.tag = "Untagged"; other.isEightBitVersion = true;
                AssertUnshared();
            }
            finally { instance.transform.SetParent(null); Object.DestroyImmediate(avatar); }
            void AssertUnshared()
            {
                Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, false), Is.EqualTo(1));
                Assert.That(PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, true), Is.EqualTo(10));
            }
        }

        [Test]
        public void Migration_ConvertsEightBitOnlyOnce_AndAllowsEnablingFeatures()
        {
            ps.isEightBitVersion = true;
            ps.featureSettingsMigrated = false;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(ps.isEightBitVersion, Is.False);
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.Zero);
            ps.features.mirror = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(1));
        }

        [Test]
        public void DisabledFeatures_ExcludeMenus_AndReenableRestoresThem()
        {
            ps.features.mirror = false;
            ps.features.footHeight = false;
            ps.features.breathing = false;
            PosingSystemFeatureUtility.Synchronize(ps);
            foreach (var name in new[] { "USSPS_Mirror", "FootHeight", "USSPS_Breathing" })
            {
                var item = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true).First(m =>
                    m.Control.parameter?.name == name || (m.Control.subParameters?.Any(p => p.name == name) ?? false));
                Assert.That(item.tag, Is.EqualTo("EditorOnly"), name);
            }
            ps.features.mirror = ps.features.footHeight = ps.features.breathing = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>(true).First(m => m.Control.parameter?.name == "USSPS_Mirror").tag,
                Is.EqualTo("Untagged"));
        }

        [Test]
        public void FeatureChanges_InvalidatePrebuildSignature()
        {
            var original = PosingSystemConverter.GetDefineSerializeJson(ps);
            ps.features.faceForward = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(original));
        }

        [Test]
        public void FloatingHeightRadial_UpgradesExistingMenuWithUnsyncedOpenFlag_AndRemovesItWhenDisabled()
        {
            ps.features.floating = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            var radial = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true).Single(m =>
                m.Control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.RadialPuppet &&
                m.Control.subParameters.Any(p => p.name == PosingSystemFeatureUtility.FloatHeight));
            radial.Control.parameter = null; // Existing scene created before the open/closed gate was added.
            radial.Control.value = 0;
            var bits = PosingSystemFeatureUtility.OptionalBits(ps);
            PosingSystemFeatureUtility.Synchronize(ps);
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(radial.Control.parameter.name, Is.EqualTo(PosingSystemFeatureUtility.FloatHeightAdjusting));
            Assert.That(radial.Control.value, Is.EqualTo(1));
            Assert.That(radial.Control.subParameters.Single().name, Is.EqualTo(PosingSystemFeatureUtility.FloatHeight), "Closing the control must retain height");
            var parameter = ps.GetComponent<ModularAvatarParameters>().parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.FloatHeightAdjusting);
            Assert.That(parameter.localOnly, Is.True);
            Assert.That(parameter.saved, Is.False);
            Assert.That(parameter.defaultValue, Is.Zero);
            Assert.That(parameter.syncType, Is.EqualTo(ParameterSyncType.Bool));
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(bits));
            ps.features.floating = false;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(ps.GetComponent<ModularAvatarParameters>().parameters.Any(p => p.nameOrPrefix == PosingSystemFeatureUtility.FloatHeightAdjusting), Is.False);
            Assert.That(radial.gameObject.activeSelf, Is.False);
            Assert.That(radial.transform.parent.gameObject.activeSelf, Is.True, "Other settings must remain available");
            ps.features.floating = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>().Count(m => m.Control.parameter?.name == PosingSystemFeatureUtility.FloatHeightAdjusting), Is.EqualTo(1));
        }

        [Test]
        public void FloorHeight_MigratesOldSubmenu_AndUsesOnlyOneSyncedFloat()
        {
            ps.features.floating = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            var radial = ps.GetComponentsInChildren<ModularAvatarMenuItem>().Single(m => m.Control.subParameters?.Any(p => p.name == PosingSystemFeatureUtility.FloatHeight) ?? false);
            var parent = radial.transform.parent;
            var old = new GameObject("全身を浮かせる（試験機能）"); old.transform.SetParent(parent);
            old.AddComponent<ModularAvatarMenuItem>();
            radial.transform.SetParent(old.transform);
            var toggle = new GameObject("浮遊"); toggle.transform.SetParent(old.transform);
            toggle.AddComponent<ModularAvatarMenuItem>().Control.parameter = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter { name = PosingSystemFeatureUtility.FloatActive };
            var custom = new GameObject("User control"); custom.transform.SetParent(old.transform);
            PosingSystemFeatureUtility.Synchronize(ps);
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(old == null && toggle == null, Is.True);
            Assert.That(custom.transform.parent, Is.SameAs(parent));
            Assert.That(radial.transform.parent, Is.SameAs(parent));
            Assert.That(radial.label, Is.EqualTo("床の高さ"));
            Assert.That(radial.Control.icon, Is.Not.Null);
            var parameters = ps.GetComponent<ModularAvatarParameters>().parameters;
            Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.FloatActive).localOnly, Is.True);
            Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.FloatActive).saved, Is.False);
            Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.FloatHeight).localOnly, Is.False);
        }

        [TestCase("撮影用の移動・回転")]
        [TestCase("撮影支援機能")]
        [TestCase("撮影モード")]
        public void PhotographyMenu_MigratesNames_LinksOriginalPoses_AndSharesLocks(string previousName)
        {
            ps.features.photography = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            var menu = ps.GetComponentsInChildren<ModularAvatarMenuItem>().Single(m => m.name == "撮影モード");
            menu.name = menu.label = menu.Control.name = previousName;
            var exit = menu.GetComponentsInChildren<ModularAvatarMenuItem>().Single(m => m.Control.parameter?.name == "USSPS_PhotoReset");
            exit.name = exit.label = exit.Control.name = "撮影支援モードを終了";
            PosingSystemFeatureUtility.Synchronize(ps);
            PosingSystemFeatureUtility.Synchronize(ps);
            var children = menu.transform.Cast<Transform>().Select(t => t.GetComponent<ModularAvatarMenuItem>()).ToArray();
            Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>().Count(m => m.name == "撮影モード"), Is.EqualTo(1));
            Assert.That(menu.name, Is.EqualTo("撮影モード"));
            Assert.That(menu.label, Is.EqualTo("撮影モード"));
            Assert.That(menu.Control.name, Is.EqualTo("撮影モード"));
            Assert.That(children.Select(c => c.label), Is.EqualTo(new[] { "姿勢選択", "距離", "向き", "足固定", "ジェスチャー無効", "撮影モードを終了" }));
            Assert.That(children[5], Is.SameAs(exit));
            Assert.That(exit.Control.name, Is.EqualTo("撮影モードを終了"));
            Assert.That(menu.Control.icon, Is.Not.Null);
            Assert.That(children.All(c => c.Control.icon != null), Is.True);
            var originalPoses = ps.SubmenuRoot.GetComponentInParent<ModularAvatarMenuItem>();
            Assert.That(children[0].menuSource_otherObjectChildren, Is.SameAs(originalPoses.gameObject));
            Assert.That(children[0].Control.icon, Is.SameAs(originalPoses.Control.icon));
            Assert.That(children[3].Control.parameter.name, Is.EqualTo("LocomotionLock"));
            Assert.That(children[4].Control.parameter.name, Is.EqualTo("USSPS_FingerGestureDisable_Menu"));
            Assert.That(children[3].Control.icon, Is.Not.Null);
            Assert.That(children[4].Control.icon, Is.Not.Null);
            Assert.That(children.Skip(1).All(c => !c.automaticValue), Is.True);
            ps.features.photography = ps.features.fingerGestureControl = false;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(menu.gameObject.activeSelf, Is.False);
            ps.features.photography = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(menu.gameObject.activeSelf, Is.True);
            Assert.That(children[4].gameObject.activeSelf, Is.False);
            ps.features.fingerGestureControl = true;
            PosingSystemFeatureUtility.Synchronize(ps);
            Assert.That(children[4].gameObject.activeSelf, Is.True);
            Assert.That(menu.transform.childCount, Is.EqualTo(6));
        }

        [Test]
        public void PhotoRootClip_IsHumanoid_WithOnlyRootAndNeutralMarker()
        {
            var clip = PosingSystemConverter.RootOffsetClip(new Vector3(0, 0, 3), Quaternion.Euler(0, 90, 0), "Test");
            try
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                Assert.That(clip.humanMotion, Is.True);
                Assert.That(bindings.Length, Is.EqualTo(8));
                Assert.That(bindings.All(b => b.type == typeof(Animator) && (b.propertyName.StartsWith("RootT.") || b.propertyName.StartsWith("RootQ.") || b.propertyName == "Jaw Left-Right")), Is.True);
                Assert.That(AnimationUtility.GetEditorCurve(clip, bindings.Single(b => b.propertyName == "Jaw Left-Right")).keys.All(k => k.value == 0), Is.True);
                var z = bindings.Single(b => b.propertyName == "RootT.z");
                Assert.That(AnimationUtility.GetEditorCurve(clip, z).Evaluate(.5f), Is.EqualTo(3));
            }
            finally { Object.DestroyImmediate(clip); }
        }
    }
}
