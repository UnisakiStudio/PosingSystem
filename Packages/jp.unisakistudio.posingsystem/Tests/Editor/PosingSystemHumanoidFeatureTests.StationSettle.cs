using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // VRChat's Sitting controller writes tracking and pose space itself, outside the merge of tracking controls, and
    // finishes late. Up to 3.2.0-alpha.5 (and in 3.1.4) a pose that animates the feet kept them on Tracking after a
    // sit shorter than about 2 to 3 s, and on desktop the pose space of a pose was gone after any sit. Found on
    // 2026-11-20 by playing random events on the built avatar together with VRChat's own controllers. These tests
    // build the avatar and play all its controllers and VRChat's Sitting controller (the SDK sample) side by side.
    // The SDK also has vrc_AvatarV3SittingLayer2, whose first state sets every part to Animation; played all the
    // time, as here, it freezes the avatar by itself, so it is not what an avatar plays by default and is not used.
    public partial class PosingSystemHumanoidFeatureTests
    {
        private const string SdkControllers = "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/Controllers/";

        private static AnimatorController SdkController(string name)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(SdkControllers + name + ".controller");
            if (controller == null) Assert.Ignore("VRChat's " + name + " (SDK sample) is not in this project");
            return controller;
        }

        // Every controller of an avatar, played together. Parameters are shared, and tracking and pose space belong
        // to whoever wrote last, as in VRChat. The first controller moves the avatar, the others run on an empty twin.
        private sealed class StationWorld : System.IDisposable
        {
            private sealed class Player
            {
                internal Animator Animator; internal PlayableGraph Graph; internal AnimatorControllerPlayable Playable;
                internal Dictionary<string, AnimatorControllerParameterType> Types; internal string Name;
            }
            private readonly List<Player> players = new List<Player>();
            private readonly List<GameObject> twins = new List<GameObject>();
            private readonly HashSet<object> hooked = new HashSet<object>();
            private readonly int fps;
            internal readonly Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> Tracking = new Dictionary<string, VRC_AnimatorTrackingControl.TrackingType>();
            internal readonly List<string> Writes = new List<string>();
            internal bool PoseSpace;
            internal float Time;

            internal StationWorld(Animator animator, IEnumerable<(string name, AnimatorController controller)> controllers, bool vr, int fps, bool local = true)
            {
                this.fps = fps;
                foreach (var key in new[] { "head", "leftHand", "rightHand", "hip", "leftFoot", "rightFoot", "leftFingers", "rightFingers", "eyes", "mouth" })
                    Tracking[key] = VRC_AnimatorTrackingControl.TrackingType.Tracking;
                VRC_AvatarParameterDriver.OnApplySettings += Driver;
                VRC_AnimatorTrackingControl.Initialize += Hook;
                VRC_AnimatorTemporaryPoseSpace.Initialize += Hook;
                foreach (var (name, controller) in controllers)
                {
                    var target = animator;
                    if (players.Count > 0)
                    {
                        var twin = new GameObject(name + " twin");
                        twins.Add(twin);
                        target = twin.AddComponent<Animator>();
                        target.avatar = animator.avatar; target.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                    var player = new Player { Animator = target, Name = name, Types = controller.parameters.GroupBy(p => p.name).ToDictionary(g => g.Key, g => g.First().type) };
                    player.Graph = PlayableGraph.Create("Station " + name);
                    player.Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    player.Playable = AnimatorControllerPlayable.Create(player.Graph, controller);
                    AnimationPlayableOutput.Create(player.Graph, name, target).SetSourcePlayable(player.Playable);
                    players.Add(player);
                }
                foreach (var pair in new Dictionary<string, float> { ["IsLocal"] = local ? 1 : 0, ["VRMode"] = vr ? 1 : 0, ["TrackingType"] = 3, ["Grounded"] = 1, ["Upright"] = 1, ["AvatarVersion"] = 3 })
                    Set(pair.Key, pair.Value);
                foreach (var player in players)
                {
                    player.Graph.Play(); player.Graph.Evaluate(0);
                    foreach (var behaviour in player.Animator.GetBehaviours<StateMachineBehaviour>())
                    {
                        if (behaviour is VRC_AnimatorTrackingControl tracking) Hook(tracking);
                        if (behaviour is VRC_AnimatorTemporaryPoseSpace space) Hook(space);
                    }
                }
            }
            private Player Of(Animator target) => players.FirstOrDefault(p => p.Animator == target);
            private void Hook(VRC_AnimatorTrackingControl c) { if (hooked.Add(c)) c.ApplySettings += ApplyTracking; }
            private void Hook(VRC_AnimatorTemporaryPoseSpace c) { if (hooked.Add(c)) c.ApplySettings += ApplySpace; }
            private void ApplyTracking(VRC_AnimatorTrackingControl c, Animator target)
            {
                var who = Of(target); if (who == null) return;
                var before = Controls;
                void Update(string key, VRC_AnimatorTrackingControl.TrackingType value) { if (value != VRC_AnimatorTrackingControl.TrackingType.NoChange) Tracking[key] = value; }
                Update("head", c.trackingHead); Update("leftHand", c.trackingLeftHand); Update("rightHand", c.trackingRightHand); Update("hip", c.trackingHip);
                Update("leftFoot", c.trackingLeftFoot); Update("rightFoot", c.trackingRightFoot); Update("leftFingers", c.trackingLeftFingers);
                Update("rightFingers", c.trackingRightFingers); Update("eyes", c.trackingEyes); Update("mouth", c.trackingMouth);
                if (Controls != before) Writes.Add(Time.ToString("F3") + " " + who.Name + " " + Short);
            }
            private void ApplySpace(VRC_AnimatorTemporaryPoseSpace c, Animator target)
            {
                var who = Of(target); if (who == null) return;
                PoseSpace = c.enterPoseSpace;
                Writes.Add(Time.ToString("F3") + " " + who.Name + " poseSpace=" + c.enterPoseSpace);
            }
            private void Driver(VRC_AvatarParameterDriver driver, Animator target)
            {
                if (Of(target) == null || (driver.localOnly && Get("IsLocal") == 0)) return;
                foreach (var p in driver.parameters)
                    switch (p.type)
                    {
                        case VRC_AvatarParameterDriver.ChangeType.Set: Set(p.name, p.value); break;
                        case VRC_AvatarParameterDriver.ChangeType.Add: Set(p.name, Get(p.name) + p.value); break;
                        case VRC_AvatarParameterDriver.ChangeType.Copy: Set(p.name, Get(p.source)); break;
                        default: throw new System.InvalidOperationException("Untested driver operation: " + p.type);
                    }
            }
            internal void Set(string name, float value)
            {
                foreach (var player in players)
                {
                    if (!player.Types.TryGetValue(name, out var type)) continue;
                    if (type == AnimatorControllerParameterType.Bool) player.Playable.SetBool(name, value != 0);
                    else if (type == AnimatorControllerParameterType.Int) player.Playable.SetInteger(name, Mathf.RoundToInt(value));
                    else if (type == AnimatorControllerParameterType.Float) player.Playable.SetFloat(name, value);
                }
            }
            internal float Get(string name)
            {
                foreach (var player in players)
                    if (player.Types.TryGetValue(name, out var type))
                        return type == AnimatorControllerParameterType.Bool ? (player.Playable.GetBool(name) ? 1 : 0)
                            : type == AnimatorControllerParameterType.Int ? player.Playable.GetInteger(name) : player.Playable.GetFloat(name);
                return float.NaN;
            }
            internal void Sit(bool seated) { Set("Seated", seated ? 1 : 0); Set("InStation", seated ? 1 : 0); }
            internal void Hold(float seconds)
            {
                for (var i = Mathf.Max(1, Mathf.CeilToInt(seconds * fps)); i > 0; i--) Step();
            }
            internal void Step()
            {
                Time += 1f / fps;
                foreach (var player in players) player.Graph.Evaluate(1f / fps);
            }
            private string Short => string.Join(" ", new[] { "head", "leftHand", "hip", "leftFoot" }.Select(k => k + "=" + Tracking[k].ToString().Substring(0, 1)));
            internal string Controls => "space=" + PoseSpace + " " + string.Join(" ", Tracking.OrderBy(p => p.Key).Select(p => p.Key + "=" + p.Value));
            internal string Recent(int count = 16) => string.Join("\n", Writes.Skip(Mathf.Max(0, Writes.Count - count)));
            public void Dispose()
            {
                foreach (var player in players) if (player.Graph.IsValid()) player.Graph.Destroy();
                foreach (var twin in twins) Object.DestroyImmediate(twin);
                VRC_AvatarParameterDriver.OnApplySettings -= Driver;
                VRC_AnimatorTrackingControl.Initialize -= Hook;
                VRC_AnimatorTemporaryPoseSpace.Initialize -= Hook;
                foreach (var hook in hooked)
                {
                    if (hook is VRC_AnimatorTrackingControl tracking && tracking != null) tracking.ApplySettings -= ApplyTracking;
                    if (hook is VRC_AnimatorTemporaryPoseSpace space && space != null) space.ApplySettings -= ApplySpace;
                }
            }
        }

        private sealed class StationAvatar
        {
            internal Animator Animator; internal List<(string name, AnimatorController controller)> Controllers; internal Dictionary<string, int> Selection;
            internal AnimatorController Controller(string name) => Controllers.Single(c => c.name == name).controller;
            // Standing in a chosen pose: the tool animates its feet, and on desktop the pose has its pose space.
            internal StationWorld Standing(bool vr, int fps, out string standing)
            {
                var world = new StationWorld(Animator, Controllers, vr, fps);
                foreach (var pair in Selection) world.Set(pair.Key, pair.Value);
                world.Hold(4);
                standing = world.Controls;
                Assert.That(world.Tracking["leftFoot"], Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Animation), "Fixture: the standing pose animates the feet");
                Assert.That(world.Tracking["head"], Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Tracking), "Fixture: the head is tracked");
                Assert.That(world.PoseSpace, Is.EqualTo(!vr), "Fixture: on desktop the pose has its pose space");
                return world;
            }
        }

        // Builds the avatar as an upload does and hands over its controllers in VRChat's order, with VRChat's own
        // Action controller where the avatar has none, and VRChat's Sitting controller.
        private void WithStationAvatar(bool withSettle, System.Action<StationAvatar> run)
        {
            var ps = CompilerSettings();
            WithPersistedBuild(context =>
            {
                RunBuild(context, BuildPhase.Resolving, BuildPhase.Optimizing);
                context.DeactivateAllExtensionContexts();
                Assert.That(context.Successful, Is.True);
                var built = context.AvatarRootObject;
                var descriptor = built.GetComponent<VRCAvatarDescriptor>();
                var avatar = new StationAvatar { Animator = built.GetComponent<Animator>(), Controllers = new List<(string, AnimatorController)>() };
                foreach (var type in new[] { VRCAvatarDescriptor.AnimLayerType.Base, VRCAvatarDescriptor.AnimLayerType.Additive, VRCAvatarDescriptor.AnimLayerType.Gesture,
                             VRCAvatarDescriptor.AnimLayerType.Action, VRCAvatarDescriptor.AnimLayerType.FX })
                {
                    var own = descriptor.baseAnimationLayers.Where(l => l.type == type && !l.isDefault).Select(l => l.animatorController as AnimatorController).FirstOrDefault(c => c != null);
                    if (own == null && type == VRCAvatarDescriptor.AnimLayerType.Action) own = SdkController("vrc_AvatarV3ActionLayer");
                    if (own != null) avatar.Controllers.Add((type.ToString(), own));
                }
                avatar.Controllers.Add(("Sitting", SdkController("vrc_AvatarV3SittingLayer")));
                var fx = avatar.Controller("FX");
                Assert.That(fx.layers.Count(l => l.name == PosingSystemStationSettle.Layer), Is.EqualTo(1), "The build adds the layer to FX");
                if (!withSettle)
                {
                    // The avatar as it was built before the layer existed.
                    fx.layers = fx.layers.Where(l => l.name != PosingSystemStationSettle.Layer).ToArray();
                    foreach (var state in fx.layers.SelectMany(l => CompilerStates(l.stateMachine)))
                        state.transitions = state.transitions.Where(t => t.conditions.All(c => c.parameter != PosingSystemStationSettle.Refresh)).ToArray();
                }
                var baseController = avatar.Controller("Base");
                avatar.Selection = ps.defines.ToDictionary(group => group.paramName, group => CompilerBinding(baseController, group.paramName).select);
                run(avatar);
            });
        }

        [TestCase(true, 90)]
        [TestCase(true, 30)]
        [TestCase(false, 60)]
        public void StationSettle_AfterASit_ThePoseHasItsTrackingAndPoseSpaceAgain(bool vr, int fps)
        {
            WithStationAvatar(true, avatar =>
            {
                // 2.1 s: leaves while the Sitting controller is still blending to its last state.
                foreach (var sitSeconds in new[] { .05f, .5f, 1.5f, 2.1f, 2.3f, 3.9f, 4.1f, 9f })
                {
                    using var world = avatar.Standing(vr, fps, out var standing);
                    world.Sit(true); world.Hold(sitSeconds);
                    world.Sit(false); world.Hold(8);
                    Assert.That(world.Controls, Is.EqualTo(standing), "After sitting for " + sitSeconds + " s.\n" + world.Recent());
                }
            });
        }

        // The defect itself, on the avatar without the layer.
        [Test]
        public void StationSettle_WithoutTheLayer_AShortSitLeavesTheFeetTracked()
        {
            WithStationAvatar(false, avatar =>
            {
                using var world = avatar.Standing(true, 90, out _);
                world.Sit(true); world.Hold(1.5f);
                world.Sit(false); world.Hold(8);
                TestContext.Out.WriteLine(world.Recent(10));
                Assert.That(world.Tracking["leftFoot"], Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Tracking), "VRChat's late write came last");
            });
        }

        [Test]
        public void StationSettle_WithoutTheLayer_OnDesktopAnySitLosesThePoseSpace()
        {
            WithStationAvatar(false, avatar =>
            {
                using var world = avatar.Standing(false, 60, out _);
                world.Sit(true); world.Hold(9);
                world.Sit(false); world.Hold(8);
                Assert.That(world.PoseSpace, Is.False, "The Sitting controller left pose space and nothing entered it again");
            });
        }

        // While the layer waits for the Sitting controller, an emote or AFK holds the parts. The values are written
        // all the same, so the pose has its feet when the emote ends. (Waiting with the pose until the station has
        // settled, the first attempt at this fix, lost the feet here.)
        [TestCase(true, 7, .5f)]   // an emote that plays to its end
        [TestCase(true, 4, 2f)]    // an emote that loops while it is chosen
        [TestCase(false, 7, .5f)]
        [TestCase(true, 0, 3f)]    // AFK
        public void StationSettle_EmoteOrAfkWhileTheStationSettles_ThePoseHasItsTrackingAgain(bool vr, int emote, float seconds)
        {
            WithStationAvatar(true, avatar =>
            {
                foreach (var after in new[] { .3f, 2.4f, 3.2f, 3.9f })
                {
                    using var world = avatar.Standing(vr, 90, out var standing);
                    world.Sit(true); world.Hold(3f);
                    world.Sit(false); world.Hold(after);
                    if (emote > 0) world.Set("VRCEmote", emote); else world.Set("AFK", 1);
                    world.Hold(seconds);
                    world.Set("VRCEmote", 0); world.Set("AFK", 0);
                    world.Hold(12);
                    Assert.That(world.Controls, Is.EqualTo(standing), (emote > 0 ? "Emote " + emote : "AFK") + " " + after + " s after leaving.\n" + world.Recent());
                }
            });
        }

        // Sitting down and getting up at any moments, again and again, with emotes in between: afterwards the pose is as before.
        [TestCase(true, 90, 1)] [TestCase(true, 30, 2)] [TestCase(false, 60, 3)]
        public void StationSettle_AnySitsAndEmotes_EndWithThePoseOfStanding(bool vr, int fps, int seed)
        {
            WithStationAvatar(true, avatar =>
            {
                using var world = avatar.Standing(vr, fps, out var standing);
                var random = new System.Random(seed);
                var failed = new List<string>();
                for (var round = 0; round < 10; round++)
                {
                    var plan = new List<string>();
                    for (var time = 0f; time < 9f;)
                    {
                        var hold = random.Next(3) == 0 ? .02f + (float)random.NextDouble() * .3f : .2f + (float)random.NextDouble() * 4f;
                        var what = random.Next(5);
                        if (what < 2) { world.Sit(true); plan.Add("sit " + hold.ToString("F2")); }
                        else if (what < 4) { world.Sit(false); world.Set("VRCEmote", 0); plan.Add("up " + hold.ToString("F2")); }
                        else { var emote = random.Next(1, 9); world.Sit(false); world.Set("VRCEmote", emote); plan.Add("emote" + emote + " " + hold.ToString("F2")); }
                        world.Hold(hold); time += hold;
                    }
                    world.Sit(false); world.Set("VRCEmote", 0); world.Hold(14);
                    if (world.Controls != standing) failed.Add(round + ": " + world.Controls + " after " + string.Join(", ", plan));
                }
                Assert.That(failed, Is.Empty, "Rounds that did not end as " + standing);
            });
        }

        // The values are written again a moment after a real sit, and when VRChat's controller has finished after a short one.
        [Test]
        public void StationSettle_WritesAgainSoonAfterALongSit_AndLateAfterAShortOne()
        {
            WithStationAvatar(true, avatar =>
            {
                using var world = avatar.Standing(true, 90, out _);
                float UntilRefreshed(float sitSeconds)
                {
                    world.Sit(true); world.Hold(sitSeconds); world.Sit(false);
                    var left = world.Time;
                    for (var i = 0; i < 90 * 10; i++)
                    {
                        world.Step();
                        if (world.Get(PosingSystemStationSettle.Refresh) != 0) return world.Time - left;
                    }
                    return float.PositiveInfinity;
                }
                var afterLong = UntilRefreshed(8);
                world.Hold(3);
                var afterShort = UntilRefreshed(.5f);
                TestContext.Out.WriteLine($"written again {afterLong:F2} s after a long sit, {afterShort:F2} s after a short one");
                Assert.That(afterLong, Is.InRange(PosingSystemStationSettle.LateSeconds - .05f, PosingSystemStationSettle.LateSeconds + .2f), "After a long sit");
                Assert.That(afterShort, Is.InRange(PosingSystemStationSettle.EarlySeconds - .05f, PosingSystemStationSettle.EarlySeconds + .2f), "After a short sit");
                world.Hold(2);
                Assert.That(world.Get(PosingSystemStationSettle.Refresh), Is.Zero, "The request ends");
            });
        }

        // Another player's avatar: the layer follows the station it sees, and never touches this client's pose space.
        [Test]
        public void StationSettle_OnAnotherPlayersAvatar_DoesNotEnterPoseSpace()
        {
            WithStationAvatar(true, avatar =>
            {
                using var world = new StationWorld(avatar.Animator, avatar.Controllers, false, 60, false);
                foreach (var pair in avatar.Selection) world.Set(pair.Key, pair.Value);
                world.Hold(4);
                world.Writes.Clear();
                world.Sit(true); world.Hold(6); world.Sit(false); world.Hold(6);
                Assert.That(world.Writes.Where(w => w.Contains("FX poseSpace=True")), Is.Empty);
                Assert.That(world.Get(PosingSystemStationSettle.Refresh), Is.Zero);
            });
        }

        // The pass alone, on layers as the merge makes them.
        [Test]
        public void StationSettle_AddsOnlyTheSecondWriteAndItsLayer()
        {
            var controller = new AnimatorController { name = "StationSettleMerge" };
            assets.Add(controller);
            controller.AddParameter(new AnimatorControllerParameter { name = "trackingLeftFoot0", type = AnimatorControllerParameterType.Bool, defaultBool = true });
            var layer = new AnimatorControllerLayer { name = "Merge_trackingLeftFoot", stateMachine = new AnimatorStateMachine() };
            var tracking = layer.stateMachine.AddState("Tracking");
            var animation = layer.stateMachine.AddState("Animation");
            tracking.AddStateMachineBehaviour<VRCAnimatorTrackingControl>().trackingLeftFoot = VRC_AnimatorTrackingControl.TrackingType.Tracking;
            animation.AddStateMachineBehaviour<VRCAnimatorTrackingControl>().trackingLeftFoot = VRC_AnimatorTrackingControl.TrackingType.Animation;
            tracking.AddTransition(animation, false).AddCondition(AnimatorConditionMode.IfNot, 0, "trackingLeftFoot0");
            animation.AddTransition(tracking, false).AddCondition(AnimatorConditionMode.If, 0, "trackingLeftFoot0");
            controller.AddLayer(layer);
            var other = new AnimatorControllerLayer { name = "Other", stateMachine = new AnimatorStateMachine() };
            other.stateMachine.AddState("Nothing");
            controller.AddLayer(other);
            void Keep()
            {
                foreach (var obj in PosingSystemControllerGraph.Collect(new[] { controller }, o => PosingSystemControllerGraph.IsGraphObject(o) && !EditorUtility.IsPersistent(o)))
                    if (!assets.Contains(obj)) assets.Add(obj);
            }
            Keep();

            Assert.That(PosingSystemStationSettle.Apply(controller), Is.EqualTo(1), "One merged layer");
            Assert.That(PosingSystemStationSettle.Apply(controller), Is.Zero, "Applying it again changes nothing");
            Keep();
            Assert.That(controller.layers.Select(l => l.name), Is.EqualTo(new[] { "Merge_trackingLeftFoot", "Other", PosingSystemStationSettle.Layer }), "One layer more, after the others");
            Assert.That(controller.layers[1].stateMachine.states.Select(s => s.state.name), Is.EqualTo(new[] { "Nothing" }));
            var states = controller.layers[0].stateMachine.states.Select(s => s.state).ToDictionary(s => s.name);
            Assert.That(states.Keys, Is.EquivalentTo(new[] { "Tracking", "Animation", "Tracking" + PosingSystemStationSettle.Again, "Animation" + PosingSystemStationSettle.Again }));
            foreach (var name in new[] { "Tracking", "Animation" })
            {
                var twin = states[name + PosingSystemStationSettle.Again];
                Assert.That(twin.behaviours.OfType<VRCAnimatorTrackingControl>().Single().trackingLeftFoot,
                    Is.EqualTo(states[name].behaviours.OfType<VRCAnimatorTrackingControl>().Single().trackingLeftFoot), name + ": the same write");
                Assert.That(states[name].transitions[0].conditions.Single().parameter, Is.EqualTo("trackingLeftFoot0"), name + ": a change of the value comes first");
                Assert.That(states[name].transitions.Length, Is.EqualTo(2));
                Assert.That(twin.transitions.Single().destinationState, Is.EqualTo(states[name]));
            }
            Assert.That(controller.layers[0].stateMachine.defaultState, Is.EqualTo(tracking));
        }
    }
}
