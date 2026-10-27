using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // Capability probes use actual sampled Transform curves, not just StateInfo or parameters.
    // They are deliberately separate from product compatibility tests.
    [UnityEngine.TestTools.PrebuildSetup(typeof(ControllerCompilerBatchSetup))]
    [UnityEngine.TestTools.PostBuildCleanup(typeof(ControllerCompilerBatchSetup))]
    public class PosingSystemOneFramePrimitiveTests
    {
        [TestCase("direct-float")]
        [TestCase("any-state-float")]
        [TestCase("blend-float")]
        [TestCase("blend-int")]
        [TestCase("animated-transition")]
        [TestCase("animated-blend")]
        public void ONEP01_CharacterizeNativeLatencyAndUnsupportedIntegerBlend(string strategy)
        {
            using var p = new Probe();
            var integer = strategy == "blend-int";
            p.Controller.AddParameter("Input", integer ? AnimatorControllerParameterType.Int : AnimatorControllerParameterType.Float);
            p.Controller.AddParameter("Decision", AnimatorControllerParameterType.Float);
            var zero = p.Curve(0); var one = p.Curve(1);
            var animated = strategy.StartsWith("animated", StringComparison.Ordinal);
            if (animated)
                p.Layer("Math", p.Tree("Input", p.Curve(0, "Decision"), p.Curve(1, "Decision")));
            var input = animated ? "Decision" : "Input";
            if (strategy.Contains("blend")) p.Layer("Playback", p.Tree(input, zero, one));
            else p.TransitionLayer(input, zero, one, strategy == "any-state-float");
            if(integer)UnityEngine.TestTools.LogAssert.Expect(LogType.Error,"BlendTree \"Select\" in state \"Output\" in Controller \"OneFrameProbe\" uses parameter \"Input\" which is not float type.");
            p.Start(); p.Tick(10);
            var failures = new List<string>();
            {
                foreach (var value in new[] { 1, 0, 1, 0 })
                {
                    if (integer) p.Playable.SetInteger("Input", value); else p.Playable.SetFloat("Input", value);
                    p.Tick(); var first = p.Sample;
                    p.Tick(); var second = p.Sample;
                    TestContext.WriteLine($"{strategy}: input={value}, first={first:R}, second={second:R}, decision={p.Playable.GetFloat("Decision"):R}");
                    var expectedFirst=integer?0:animated?1-value:value;
                    var expectedSecond=integer?0:value;
                    if (Mathf.Abs(first-expectedFirst)>.00001f || Mathf.Abs(second-expectedSecond)>.00001f)
                        failures.Add(strategy + ": input=" + value + ", first=" + first+", second="+second);
                }
            }
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [TestCase(.0001f, false)] [TestCase(.000001f, false)]
        [TestCase(.0001f, true)] [TestCase(.000001f, true)]
        public void ONEP02_CharacterizeDiagonalComparisonAndBoundaryCounterexample(float epsilon, bool middle)
        {
            using var p = new Probe();
            p.Controller.AddParameter("Input", AnimatorControllerParameterType.Float);
            p.Controller.AddParameter("Threshold", AnimatorControllerParameterType.Float);
            var tree = p.Own(new BlendTree { name = "LiveComparison", blendType = BlendTreeType.FreeformCartesian2D,
                blendParameter = "Input", blendParameterY = "Threshold", useAutomaticThresholds = false });
            var children = new List<ChildMotion> {
                new() { motion=p.Curve(0), position=new Vector2(.5f-epsilon,.5f+epsilon), timeScale=1 },
                new() { motion=p.Curve(1), position=new Vector2(.5f+epsilon,.5f-epsilon), timeScale=1 } };
            if (middle) children.Insert(1, new ChildMotion { motion=p.Curve(2), position=new Vector2(.5f,.5f), timeScale=1 });
            tree.children=children.ToArray(); p.Layer("Playback",tree); p.Start(); p.Tick(10);
            var failures = new List<string>();
            {
                foreach (var threshold in new[] { .2f, .5f, .8f })
                foreach (var delta in new[] { -.1f, -.001f, 0f, .001f, .1f })
                {
                    var expected=delta<0?0f:delta>0?1f:middle?2f:.5f;
                    p.Playable.SetFloat("Input",threshold+delta); p.Playable.SetFloat("Threshold",threshold);
                    p.Tick();
                    TestContext.WriteLine($"diagonal epsilon={epsilon:R}, middle={middle}, threshold={threshold:R}, delta={delta:R}, actual={p.Sample:R}, expected={expected}");
                    if (!(Mathf.Abs(p.Sample-expected)<=.001f)) failures.Add("threshold="+threshold+", delta="+delta+", actual="+p.Sample+", expected="+expected);
                }
            }
            if(middle)Assert.That(failures.Count,Is.GreaterThan(0),"This candidate has an exact-boundary counterexample and must not replace the legacy comparator");
            else Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [TestCase(false)] [TestCase(true)]
        public void ONEP03_SdkDriverRequiresSecondPlaybackEvaluation(bool blend)
        {
            using var p=new Probe();
            p.Controller.AddParameter("Input",AnimatorControllerParameterType.Int);
            p.Controller.AddParameter("Decision",AnimatorControllerParameterType.Float);
            p.DriverSelector();
            if(blend)p.Layer("Playback",p.Tree("Decision",p.Curve(0),p.Curve(1)));
            else p.TransitionLayer("Decision",p.Curve(0),p.Curve(1),false);
            var callbacks=0;
            void Apply(VRC_AvatarParameterDriver driver,Animator target)
            {
                if(target!=p.Animator)return;
                foreach(var value in driver.parameters)p.Playable.SetFloat(value.name,value.value);
                callbacks++;
            }
            VRC_AvatarParameterDriver.OnApplySettings+=Apply;
            try
            {
                p.Start();p.Tick(10);var failures=new List<string>();
                foreach(var value in new[]{1,0,1,0})
                {
                    p.Playable.SetInteger("Input",value);p.Tick();var first=p.Sample;p.Tick();var second=p.Sample;
                    TestContext.WriteLine($"driver blend={blend}, input={value}, first={first:R}, second={second:R}, callbacks={callbacks}");
                    if(Mathf.Abs(first-(1-value))>.00001f || Mathf.Abs(second-value)>.00001f)failures.Add("input="+value+", first="+first+", second="+second);
                }
                Assert.That(callbacks,Is.GreaterThanOrEqualTo(5));
                Assert.That(failures,Is.Empty,string.Join("\n",failures));
            }
            finally {VRC_AvatarParameterDriver.OnApplySettings-=Apply;}
        }

        [TestCase(0f,12)] [TestCase(.5f,12)] [TestCase(0f,20)] [TestCase(.5f,20)]
        [TestCase(0f,24)] [TestCase(.5f,24)] [TestCase(0f,28)] [TestCase(.5f,28)]
        public void ONEP04_DiagonalTreeHasStrictBoundaryCounterexamples(float center,int exponent)
        {
            using var p=new Probe();
            p.Controller.AddParameter("Input",AnimatorControllerParameterType.Float);
            p.Controller.AddParameter("Threshold",AnimatorControllerParameterType.Float);
            var epsilon=Mathf.Pow(2,-exponent);
            var tree=p.Own(new BlendTree {name="ExactBoundary",blendType=BlendTreeType.FreeformCartesian2D,
                blendParameter="Input",blendParameterY="Threshold",useAutomaticThresholds=false,
                children=new[]{
                    new ChildMotion {motion=p.Curve(0),position=new Vector2(center-epsilon,center+epsilon),timeScale=1},
                    new ChildMotion {motion=p.Curve(2),position=new Vector2(center,center),timeScale=1},
                    new ChildMotion {motion=p.Curve(1),position=new Vector2(center+epsilon,center-epsilon),timeScale=1}}});
            p.Layer("Playback",tree);p.Start();p.Tick(10);var failures=new List<string>();
            foreach(var threshold in new[]{.2f,.35f,.5f,.59f,.6f,.8f,1f})
            foreach(var direction in new[]{-1,0,1})
            {
                var bits=BitConverter.ToInt32(BitConverter.GetBytes(threshold),0);
                var input=BitConverter.ToSingle(BitConverter.GetBytes(bits+direction),0);
                p.Playable.SetFloat("Input",input);p.Playable.SetFloat("Threshold",threshold);p.Tick();
                var expected=direction<0?0f:direction>0?1f:2f;
                if(!(Mathf.Abs(p.Sample-expected)<=.001f))failures.Add($"t={threshold:R}, u={input:R}, actual={p.Sample:R}, expected={expected}");
            }
            TestContext.WriteLine($"diagonal boundary center={center}, exponent={exponent}, mismatches={failures.Count}/21\n"+string.Join("\n",failures));
            Assert.That(failures.Count,Is.GreaterThan(0),"All tested diagonal placements have a boundary counterexample; they are rejected candidates");
        }

        [TestCase(false, false)] [TestCase(true, false)] [TestCase(true, true)]
        public void ONEP06_SubMachineEntrySelectsMotionWithoutRestartingEveryEvaluation(bool anyState,bool guarded)
        {
            using var p=new Probe();
            p.Controller.AddParameter("Input",AnimatorControllerParameterType.Float);
            p.Controller.AddParameter("Choice",AnimatorControllerParameterType.Int);
            p.Controller.AddParameter("Ready",AnimatorControllerParameterType.Int);
            if(guarded)p.EnableDriverBridge();
            AnimatorStateMachine Sub(int group)
            {
                AnimatorState State(int choice)
                {
                    var exit=p.Own(new AnimatorStateTransition {isExit=true,hasExitTime=false,hasFixedDuration=true,duration=0,
                        conditions=new[]{new AnimatorCondition {parameter="Choice",mode=AnimatorConditionMode.NotEqual,threshold=choice}}});
                    var state=p.Own(new AnimatorState {name="Choice"+choice,motion=p.Curve(10+group*10+choice),speed=1,
                        writeDefaultValues=true,transitions=new[]{exit}});
                    if(guarded)
                    {
                        var driver=p.Own(ScriptableObject.CreateInstance<VRCAvatarParameterDriver>());
                        driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter {name="Ready",type=VRC_AvatarParameterDriver.ChangeType.Set,value=group});
                        state.behaviours=new StateMachineBehaviour[]{driver};
                    }
                    return state;
                }
                var a=State(0);var b=State(1);
                var entry=p.Own(new AnimatorTransition {destinationState=b,conditions=new[]{new AnimatorCondition {parameter="Choice",mode=AnimatorConditionMode.Equals,threshold=1}}});
                return p.Own(new AnimatorStateMachine {name="Group"+group,states=new[]{new ChildAnimatorState{state=a},new ChildAnimatorState{state=b}},defaultState=a,entryTransitions=new[]{entry}});
            }
            var a=Sub(0);var b=Sub(1);
            var root=p.Own(new AnimatorStateMachine {name="Root",stateMachines=new[]{new ChildAnimatorStateMachine{stateMachine=a},new ChildAnimatorStateMachine{stateMachine=b}},
                entryTransitions=new[]{p.Own(new AnimatorTransition {destinationStateMachine=a})}});
            foreach(var machine in new[]{a,b})root.SetStateMachineTransitions(machine,new[]{p.Own(new AnimatorTransition {destinationStateMachine=machine})});
            if(anyState)root.anyStateTransitions=new[]{
                p.Own(new AnimatorStateTransition {destinationStateMachine=a,canTransitionToSelf=false,hasExitTime=false,hasFixedDuration=true,duration=0,
                    conditions=new[]{new AnimatorCondition{parameter="Input",mode=AnimatorConditionMode.Less,threshold=.5f}}
                        .Concat(guarded?new[]{new AnimatorCondition{parameter="Ready",mode=AnimatorConditionMode.NotEqual,threshold=0}}:Array.Empty<AnimatorCondition>()).ToArray()}),
                p.Own(new AnimatorStateTransition {destinationStateMachine=b,canTransitionToSelf=false,hasExitTime=false,hasFixedDuration=true,duration=0,
                    conditions=new[]{new AnimatorCondition{parameter="Input",mode=AnimatorConditionMode.Greater,threshold=.5f}}
                        .Concat(guarded?new[]{new AnimatorCondition{parameter="Ready",mode=AnimatorConditionMode.NotEqual,threshold=1}}:Array.Empty<AnimatorCondition>()).ToArray()})};
            p.AddLayer("Playback",root);p.Start();p.Tick(60);
            var failures=new List<string>();
            foreach(var step in new[]{(1,0),(1,1),(0,1),(0,0)})
            {
                var group=anyState?step.Item1:0; var choice=step.Item2;
                p.Playable.SetFloat("Input",group);p.Playable.SetInteger("Choice",choice);p.Tick();
                var first=p.Sample;var firstTime=p.Playable.GetCurrentAnimatorStateInfo(0).normalizedTime;
                p.Tick(10);var later=p.Sample;var laterTime=p.Playable.GetCurrentAnimatorStateInfo(0).normalizedTime;
                TestContext.WriteLine($"submachine any={anyState}, guarded={guarded}, group={group}, choice={choice}, first={first:R}, later={later:R}, time={firstTime:R}->{laterTime:R}");
                if(Mathf.Abs(first-(10+group*10+choice))>.0001f)failures.Add("Wrong first sample: "+first);
                var shouldRestart=anyState && !guarded;
                if(shouldRestart ? laterTime!=0 || firstTime!=0 : laterTime-firstTime<.1f)
                    failures.Add("Unexpected time progression, restart="+shouldRestart+": "+firstTime+"->"+laterTime);
            }
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }

        internal sealed class Probe : IDisposable
        {
            readonly List<Object> objects = new();
            readonly GameObject root;
            readonly Transform target;
            readonly Animator animator;
            PlayableGraph graph;
            bool driverBridge;
            internal AnimatorController Controller { get; }
            internal AnimatorControllerPlayable Playable { get; private set; }
            internal float Sample => target.localPosition.x;
            internal Animator Animator => animator;
            internal void EnableDriverBridge() {driverBridge=true;VRC_AvatarParameterDriver.OnApplySettings+=ApplySdk;}
            void ApplySdk(VRC_AvatarParameterDriver driver,Animator target)
            {
                if(target!=animator)return;
                foreach(var value in driver.parameters)
                {
                    if(value.type!=VRC_AvatarParameterDriver.ChangeType.Set)throw new InvalidOperationException("Probe only supports Set");
                    var type=Controller.parameters.Single(p=>p.name==value.name).type;
                    if(type==AnimatorControllerParameterType.Int)Playable.SetInteger(value.name,(int)value.value);
                    else if(type==AnimatorControllerParameterType.Bool)Playable.SetBool(value.name,value.value!=0);
                    else Playable.SetFloat(value.name,value.value);
                }
            }
            internal Probe()
            {
                root=new GameObject("OneFrameProbe"); animator=root.AddComponent<Animator>(); animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                target=new GameObject("Probe").transform; target.SetParent(root.transform,false);
                Controller=Own(new AnimatorController { name="OneFrameProbe" });
            }
            internal T Own<T>(T obj) where T:Object { objects.Add(obj); return obj; }
            internal AnimationClip Curve(float value,string parameter=null)
            {
                var clip=Own(new AnimationClip { name=(parameter??"Output")+value });
                var binding=parameter==null?EditorCurveBinding.FloatCurve("Probe",typeof(Transform),"m_LocalPosition.x"):
                    EditorCurveBinding.FloatCurve("",typeof(Animator),parameter);
                AnimationUtility.SetEditorCurve(clip,binding,AnimationCurve.Constant(0,1,value)); return clip;
            }
            internal BlendTree Tree(string parameter,Motion zero,Motion one)
            {
                return Own(new BlendTree { name="Select", blendType=BlendTreeType.Simple1D, blendParameter=parameter, useAutomaticThresholds=false,
                    children=new[]{new ChildMotion { motion=zero,threshold=0,timeScale=1 },new ChildMotion { motion=one,threshold=1,timeScale=1 }} });
            }
            internal void Layer(string name,Motion motion)
            {
                var state=Own(new AnimatorState { name="Output",motion=motion,writeDefaultValues=true });
                var machine=Own(new AnimatorStateMachine { name=name,states=new[]{new ChildAnimatorState {state=state}},defaultState=state });
                AddLayer(name,machine);
            }
            internal void AddLayer(string name,AnimatorStateMachine machine) => Controller.layers=Controller.layers.Concat(new[]{
                new AnimatorControllerLayer {name=name,stateMachine=machine,defaultWeight=1,blendingMode=AnimatorLayerBlendingMode.Override,syncedLayerIndex=-1}}).ToArray();
            internal void TransitionLayer(string input,Motion zero,Motion one,bool any)
            {
                var a=Own(new AnimatorState {name="Zero",motion=zero,writeDefaultValues=true});
                var b=Own(new AnimatorState {name="One",motion=one,writeDefaultValues=true});
                var machine=Own(new AnimatorStateMachine {name="Playback",states=new[]{new ChildAnimatorState{state=a},new ChildAnimatorState{state=b}},defaultState=a});
                AnimatorStateTransition To(AnimatorState state,AnimatorConditionMode mode) => Own(new AnimatorStateTransition {
                    destinationState=state,canTransitionToSelf=false,hasExitTime=false,duration=0,hasFixedDuration=true,
                    conditions=new[]{new AnimatorCondition{parameter=input,mode=mode,threshold=.5f}}});
                var ab=To(b,AnimatorConditionMode.Greater); var ba=To(a,AnimatorConditionMode.Less);
                if(any)machine.anyStateTransitions=new[]{ab,ba}; else {a.transitions=new[]{ab};b.transitions=new[]{ba};}
                AddLayer("Playback",machine);
            }
            internal void DriverSelector()
            {
                AnimatorState State(int value)
                {
                    var driver=Own(ScriptableObject.CreateInstance<VRCAvatarParameterDriver>());
                    driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter {name="Decision",type=VRC_AvatarParameterDriver.ChangeType.Set,value=value});
                    return Own(new AnimatorState {name="Select"+value,writeDefaultValues=false,
                        motion=Own(new AnimationClip {name="Empty"}),behaviours=new StateMachineBehaviour[]{driver}});
                }
                var a=State(0);var b=State(1);
                AnimatorStateTransition To(AnimatorState target,int value)=>Own(new AnimatorStateTransition {destinationState=target,
                    hasExitTime=false,hasFixedDuration=true,duration=0,conditions=new[]{new AnimatorCondition {parameter="Input",mode=AnimatorConditionMode.Equals,threshold=value}}});
                a.transitions=new[]{To(b,1)};b.transitions=new[]{To(a,0)};
                var machine=Own(new AnimatorStateMachine {name="Driver",states=new[]{new ChildAnimatorState{state=a},new ChildAnimatorState{state=b}},defaultState=a});
                AddLayer("Driver",machine);
            }
            internal void Start()
            {
                graph=PlayableGraph.Create("One evaluation output probe"); graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                Playable=AnimatorControllerPlayable.Create(graph,Controller);
                AnimationPlayableOutput.Create(graph,"Output",animator).SetSourcePlayable(Playable); graph.Play();
            }
            internal void Tick(int count=1) {for(var i=0;i<count;i++)graph.Evaluate(1f/60);}
            public void Dispose()
            {
                if(graph.IsValid())graph.Destroy(); Object.DestroyImmediate(root);
                if(driverBridge)VRC_AvatarParameterDriver.OnApplySettings-=ApplySdk;
                foreach(var obj in objects.AsEnumerable().Reverse())if(obj!=null){Undo.ClearUndo(obj);Object.DestroyImmediate(obj);}
            }
        }
    }
}
