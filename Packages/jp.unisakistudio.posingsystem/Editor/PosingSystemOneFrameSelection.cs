#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    // Default desktop playback accelerator. Public selection/sync producers remain intact.
    // Only observed desktop heights with unambiguous ordinary threshold signs qualify.
    internal static class PosingSystemOneFrameSelection
    {
        internal const string Guard = "USSPS_OneFramePlayback";
        const string Prefix = "USSPS.Compiler.OneFrame.v1:";
        const string MarkerName = Prefix + "guard";
        static readonly string[] Classifiers = { "USSPS_IsShallow", "USSPS_IsDeep", "USSPS_IsSleep" };
        static readonly string[] ClassifierOutputs = { "USSPS_FinalShallowThreshold", "USSPS_FinalDeepThreshold", "USSPS_FinalSleepThreshold" };
        internal static bool IsClassifierOutputLayer(string name) => ClassifierOutputs.Contains(name);
        [Serializable] sealed class Backup { public float[] classifierDefaults; }
        // Desktop VRChat also reports crouching as 0x3f199999 (0.599999964 in OSCQuery),
        // one representable float below .6f. Keep exact matching for both observed values;
        // widening every height to an epsilon band would change boundary/fallback behavior.
        static readonly float[] Heights = { .35f, Adjacent(.6f, -1), .6f, 1f };
        static readonly string[] Menus = { "SitStand", "SitShallow", "SitDeep" };
        static readonly string[] Thresholds = { "USSPS_ShallowThreshold", "USSPS_DeepThreshold", "USSPS_SleepThreshold" };
        // The reference classifier blends +/-1 clips; float cancellation near equality preserves
        // its preceding state. Leave that small neighborhood to the unchanged reference graph.
        const float BoundaryMargin = .00001f;
        static IEnumerable<AnimatorState> States(AnimatorStateMachine machine)
        {
            foreach(var child in machine.states)yield return child.state;
            foreach(var child in machine.stateMachines)foreach(var state in States(child.stateMachine))yield return state;
        }
        internal static bool Restore(AnimatorController controller)
        {
            var layers=controller.layers;
            var index=Array.FindIndex(layers,l=>l.name=="USSPS_Locomotion");
            if(index<0)return false;
            var wrapper=layers[index].stateMachine;
            var marker=wrapper.entryTransitions.SingleOrDefault(t=>t.name.StartsWith(Prefix,StringComparison.Ordinal));
            if(marker==null)return false;
            var backup=JsonUtility.FromJson<Backup>(marker.name.Substring(Prefix.Length));
            var legacy=wrapper.stateMachines.First().stateMachine;
            if(legacy.name!="USSPS_Locomotion" || backup.classifierDefaults.Length!=Classifiers.Length)
                throw new InvalidOperationException("Unknown one-frame controller layout");
            var clones=wrapper.stateMachines.Skip(1).SelectMany(m=>m.stateMachine.states).Select(s=>s.state).ToHashSet();
            clones.UnionWith(wrapper.states.Select(child => child.state));
            foreach(var state in States(legacy))
                state.behaviours=state.behaviours.Where(b=>b==null || b.name!=MarkerName).ToArray();
            layers[index].stateMachine=legacy;controller.layers=layers;
            // SetOverrideMotion(state,null) retains the key in Unity 2022.3. Remove only the
            // generated clone keys, preserving every original override (including empty arrays).
            using(var serialized=new SerializedObject(controller))
            {
                var serializedLayers=serialized.FindProperty("m_AnimatorLayers");
                for(var i=0;i<serializedLayers.arraySize;i++)foreach(var field in new[]{"m_Motions","m_Behaviours"})
                {
                    var list=serializedLayers.GetArrayElementAtIndex(i).FindPropertyRelative(field);
                    for(var j=list.arraySize-1;j>=0;j--)
                        if(clones.Contains(list.GetArrayElementAtIndex(j).FindPropertyRelative("m_State").objectReferenceValue as AnimatorState))list.DeleteArrayElementAtIndex(j);
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            // The earlier layer snapshot still contains the removed serialized override keys.
            // Read it back before removing output layers, or assigning layers would revive them.
            layers=controller.layers;
            var parameters=controller.parameters.Where(p=>p.name!=Guard).ToArray();
            for(var i=0;i<Classifiers.Length;i++)parameters.Single(p=>p.name==Classifiers[i]).defaultFloat=backup.classifierDefaults[i];
            // Older v1 graphs have no final passes. Removing newer passes must also preserve
            // indices of any unrelated synced layers added after the generated graph.
            var kept=layers.Select((layer,i)=>(layer,i)).Where(p=>!IsClassifierOutputLayer(p.layer.name)).ToArray();
            var indices=kept.Select((p,i)=>(old:p.i,index:i)).ToDictionary(p=>p.old,p=>p.index);
            foreach(var pair in kept)if(pair.layer.syncedLayerIndex>=0)
            {
                if(!indices.TryGetValue(pair.layer.syncedLayerIndex,out var target))
                    throw new InvalidOperationException("External layer is synced to a generated classifier pass");
                pair.layer.syncedLayerIndex=target;
            }
            controller.layers=kept.Select(p=>p.layer).ToArray();
            controller.parameters=parameters;return true;
        }
        static AnimatorCondition C(string p, AnimatorConditionMode m, float v=0) => new() {parameter=p,mode=m,threshold=v};
        static AnimatorCondition Eq(string p,int v)=>C(p,AnimatorConditionMode.Equals,v);
        static AnimatorCondition Ne(string p,int v)=>C(p,AnimatorConditionMode.NotEqual,v);
        internal static float Adjacent(float v,int delta)=>BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(v),0)+delta),0);
        static AnimatorCondition[] Exact(string p,float v)=>new[]{C(p,AnimatorConditionMode.Greater,Adjacent(v,-1)),C(p,AnimatorConditionMode.Less,Adjacent(v,1))};
        static AnimatorCondition[] Common=>new[]{C("IsLocal",AnimatorConditionMode.If),Eq("VRMode",0),Eq("TrackingType",3),
            C("Grounded",AnimatorConditionMode.If),C("USSPS_TypeFix",AnimatorConditionMode.IfNot),C("LocomotionLock",AnimatorConditionMode.IfNot),
            Eq("VirtualLoveType",0),C("LocomotionTypeGroup",AnimatorConditionMode.Greater,-1),C("LocomotionTypeGroup",AnimatorConditionMode.Less,3)};
        static AnimatorCondition[] Classify(float u,int group)=>Thresholds.Select((p,i)=>C(p,
            i<group?AnimatorConditionMode.Greater:AnimatorConditionMode.Less,i<group?u+BoundaryMargin:u-BoundaryMargin)).ToArray();
        static IEnumerable<AnimatorCondition[]> Not(AnimatorCondition c)
        {
            if(c.mode==AnimatorConditionMode.Greater || c.mode==AnimatorConditionMode.Less)
            {
                yield return new[]{C(c.parameter,c.mode==AnimatorConditionMode.Greater?AnimatorConditionMode.Less:AnimatorConditionMode.Greater,c.threshold)};
                // Integer boundary equality is an Equals condition; Float uses adjacent values.
                if(c.parameter=="LocomotionTypeGroup")yield return new[]{Eq(c.parameter,(int)c.threshold)};
                else yield return Exact(c.parameter,c.threshold);
            }
            else yield return new[]{C(c.parameter,c.mode switch {
                AnimatorConditionMode.If=>AnimatorConditionMode.IfNot,AnimatorConditionMode.IfNot=>AnimatorConditionMode.If,
                AnimatorConditionMode.Equals=>AnimatorConditionMode.NotEqual,AnimatorConditionMode.NotEqual=>AnimatorConditionMode.Equals,
                _=>throw new InvalidOperationException()},c.threshold)};
        }
        internal static void Apply(AnimatorController controller,Action<Object> own,bool flat=false)
        {
            // Compare with the smaller reference graph too; the accelerator does not require the
            // previous desktop expansion. Recover its original exits before wrapping it.
            Restore(controller);
            PosingSystemDesktopSelection.Restore(controller);
            var layers=controller.layers;
            if(layers.Any(l=>IsClassifierOutputLayer(l.name)))throw new InvalidOperationException("Classifier output layer name is already in use");
            var index=Array.FindIndex(layers,l=>l.name=="USSPS_Locomotion");
            var legacy=layers[index].stateMachine;
            var encoder=layers.Single(l=>l.name=="USSPS_LocomotionType").stateMachine;
            if(controller.parameters.Any(p=>p.name==Guard))throw new InvalidOperationException("Candidate already applied");
            var parameters=controller.parameters;
            var backup=new Backup {classifierDefaults=Classifiers.Select(n=>parameters.Single(p=>p.name==n).defaultFloat).ToArray()};
            // These internal calculated values must be neutral under synced additive layers.
            // A default of -1 adds -1 per additive layer and corrupts the retained dispatcher.
            foreach(var p in parameters.Where(p=>Classifiers.Contains(p.name)))p.defaultFloat=0;
            controller.parameters=parameters;
            controller.AddParameter(Guard,AnimatorControllerParameterType.Int);
            T Own<T>(T o) where T:Object {own(o);return o;}
            var entries=legacy.entryTransitions.Where(t=>t.destinationState!=null && t.conditions.Any(c=>c.parameter=="VRMode" && c.mode==AnimatorConditionMode.Equals && c.threshold==0)).ToArray();
            int Key(IEnumerable<AnimatorCondition> terms)=>terms.Where(c=>c.parameter.StartsWith("USSPS_Pose",StringComparison.Ordinal))
                .Sum(c=>c.parameter=="USSPS_Pose"?(int)c.threshold:c.mode==AnimatorConditionMode.If?1<<(8+int.Parse(c.parameter.Substring(10))):0);
            int DriverKey(AnimatorState s)=>s.behaviours.OfType<VRCAvatarParameterDriver>().SelectMany(d=>d.parameters)
                .Where(p=>p.name.StartsWith("USSPS_Pose",StringComparison.Ordinal)).Sum(p=>p.name=="USSPS_Pose"?(int)p.value:(int)p.value<<(8+int.Parse(p.name.Substring(10))));
            var playback=entries.ToDictionary(t=>Key(t.conditions),t=>t.destinationState);
            var groupStates=new List<(int selection,AnimatorState source)>[3];
            for(var g=0;g<3;g++)
            {
                var group=g;var types=group==0?new[]{0,1}:group==1?new[]{11,12}:new[]{21,22};
                groupStates[g]=encoder.entryTransitions.Where(t=>t.destinationState!=null && t.conditions.Any(c=>c.parameter=="LocomotionType" && types.Contains((int)c.threshold)))
                    .Select(t=>(t.conditions.Where(c=>c.parameter==Menus[group] && c.mode==AnimatorConditionMode.Equals).Select(c=>(int)c.threshold).DefaultIfEmpty(0).Single(),playback[DriverKey(t.destinationState)]))
                    .Distinct().ToList();
            }
            var ordinary=groupStates.SelectMany(s=>s).Select(s=>s.source).ToHashSet();
            VRCAvatarParameterDriver Marker(int value)
            {
                var d=Own(ScriptableObject.CreateInstance<VRCAvatarParameterDriver>());d.name=MarkerName;
                d.parameters.Add(new VRC_AvatarParameterDriver.Parameter {name=Guard,type=VRC_AvatarParameterDriver.ChangeType.Set,value=value});return d;
            }
            var legacyOrdinaryMarker=Marker(0);var legacySpecialMarker=Marker(-1);
            foreach(var state in States(legacy))state.behaviours=state.behaviours.Append(ordinary.Contains(state)?legacyOrdinaryMarker:legacySpecialMarker).ToArray();
            var wrapper=Own(new AnimatorStateMachine {name="USSPS_Locomotion",stateMachines=new[]{new ChildAnimatorStateMachine {stateMachine=legacy}},
                entryTransitions=new[]{Own(new AnimatorTransition {name=Prefix+JsonUtility.ToJson(backup),destinationStateMachine=legacy})}});
            // The VRChat avatar picker samples the root default before normal tracking transitions run.
            var selectionState = legacy.states.FirstOrDefault(child => child.state.name == "AvatarSelecting").state;
            if (selectionState != null)
            {
                var preview = Own(new AnimatorState());
                EditorUtility.CopySerialized(selectionState, preview);
                preview.behaviours = selectionState.behaviours.Where(b => b != legacySpecialMarker && b != legacyOrdinaryMarker).ToArray();
                preview.transitions = new[] { Own(new AnimatorStateTransition {
                    destinationStateMachine = legacy, hasExitTime = false, duration = 0,
                    conditions = new[] { Ne("TrackingType", 0) }
                }) };
                wrapper.states = new[] { new ChildAnimatorState { state = preview } };
                wrapper.defaultState = preview;
                wrapper.entryTransitions = new[] { Own(new AnimatorTransition {
                    destinationState = preview, conditions = new[] { Eq("TrackingType", 0) }
                }) }.Concat(wrapper.entryTransitions).ToArray();
                foreach (var synced in layers.Where(l => l.syncedLayerIndex == index))
                {
                    synced.SetOverrideMotion(preview, synced.GetOverrideMotion(selectionState));
                    synced.SetOverrideBehaviours(preview, synced.GetOverrideBehaviours(selectionState));
                }
            }
            // Legacy exits originally restart the layer's Entry. Retain that behavior when nested.
            wrapper.SetStateMachineTransitions(legacy,new[]{Own(new AnimatorTransition {destinationStateMachine=legacy})});
            var any=new List<AnimatorStateTransition>();
            void Any(AnimatorStateMachine machine,AnimatorState state,IEnumerable<AnimatorCondition> conditions)
            {any.Add(Own(new AnimatorStateTransition {destinationStateMachine=machine,destinationState=state,canTransitionToSelf=false,
                hasExitTime=false,hasFixedDuration=true,duration=0,conditions=conditions.ToArray()}));}
            for(var g=0;g<3;g++)
            {
                var group=g;var menu=Menus[g];var marker=Marker(g+1);var valid=groupStates[g].Select(s=>s.selection).ToArray();
                var machine=Own(new AnimatorStateMachine {name="Direct"+menu});
                wrapper.stateMachines=wrapper.stateMachines.Append(new ChildAnimatorStateMachine {stateMachine=machine}).ToArray();
                wrapper.SetStateMachineTransitions(machine,new[]{Own(new AnimatorTransition {destinationStateMachine=machine})});
                var selections=groupStates[g].Concat(new[]{(-1,playback[0])});
                foreach(var pair in selections)
                {
                    var state=Own(new AnimatorState());EditorUtility.CopySerialized(pair.Item2,state);
                    state.behaviours=pair.Item2.behaviours.Where(b=>b!=legacyOrdinaryMarker && b!=legacySpecialMarker).Append(marker).ToArray();
                    foreach(var synced in layers.Where(l=>l.syncedLayerIndex==index))
                    {
                        synced.SetOverrideMotion(state,synced.GetOverrideMotion(pair.Item2));
                        synced.SetOverrideBehaviours(state,synced.GetOverrideBehaviours(pair.Item2));
                    }
                    // Keep jump/select-avatar edges. Ordinary pose exits are replaced by menu-only
                    // exits; group switching is handled by the small shared AnyState dispatcher.
                    var exits=new List<AnimatorStateTransition>(pair.Item2.transitions.Where(t=>!t.isExit));
                    // Mode/tracking changes hand control to the corresponding original state.
                    // Entering Legacy's Entry instead would select a different VR pose early.
                    foreach(var condition in new[]{Ne("VRMode",0),Ne("TrackingType",3)})
                        exits.Add(Own(new AnimatorStateTransition {destinationState=pair.Item2,hasExitTime=false,hasFixedDuration=true,duration=0,conditions=new[]{condition}}));
                    foreach(var condition in pair.Item1<0?valid.Select(v=>Eq(menu,v)):new[]{Ne(menu,pair.Item1)})
                        exits.Add(Own(new AnimatorStateTransition {isExit=true,hasExitTime=false,hasFixedDuration=true,duration=0,conditions=new[]{condition}}));
                    state.transitions=exits.ToArray();
                    machine.states=machine.states.Append(new ChildAnimatorState {state=state}).ToArray();
                    var selection=pair.Item1<0?valid.Select(v=>Ne(menu,v)).ToArray():new[]{Eq(menu,pair.Item1)};
                    if(pair.Item1<0)machine.defaultState=state;
                    else machine.entryTransitions=machine.entryTransitions.Append(Own(new AnimatorTransition {destinationState=state,conditions=selection})).ToArray();
                    if(flat)foreach(var u in Heights)Any(null,state,Common.Concat(Exact("Upright",u)).Concat(Classify(u,group)).Concat(selection).Append(C(Guard,AnimatorConditionMode.Greater,-1)));
                }
                if(!flat)foreach(var u in Heights)Any(machine,null,Common.Concat(Exact("Upright",u)).Concat(Classify(u,group))
                    .Concat(new[]{Ne(Guard,group+1),C(Guard,AnimatorConditionMode.Greater,-1)}));
            }
            var active=new[]{C(Guard,AnimatorConditionMode.Greater,0)};
            // Jump/fall/landing keep their original edges and duration. A global fallback on
            // Grounded would intercept takeoff; nested special-state guards protect landing.
            foreach(var c in Common.Where(c=>c.parameter!="Grounded" && c.parameter!="VRMode" && c.parameter!="TrackingType"))
                foreach(var term in Not(c))Any(legacy,null,active.Concat(term));
            // Heights outside the explicitly recognized values use legacy.
            Any(legacy,null,active.Append(C("Upright",AnimatorConditionMode.Less,Heights[0])));
            for(var i=0;i<Heights.Length-1;i++)Any(legacy,null,active.Concat(new[]{C("Upright",AnimatorConditionMode.Greater,Heights[i]),C("Upright",AnimatorConditionMode.Less,Heights[i+1])}));
            Any(legacy,null,active.Append(C("Upright",AnimatorConditionMode.Greater,Heights.Last())));
            foreach(var u in Heights)
            {
                var prefix=active.Concat(Exact("Upright",u));
                // Complement of the three strict ordinary sign patterns: equality, sleep, or
                // inverted shallow/deep ordering. Never approximate the legacy boundary memory.
                foreach(var threshold in Thresholds)Any(legacy,null,prefix.Concat(new[]{C(threshold,AnimatorConditionMode.Greater,Adjacent(u-BoundaryMargin,-1)),C(threshold,AnimatorConditionMode.Less,Adjacent(u+BoundaryMargin,1))}));
                Any(legacy,null,prefix.Append(C(Thresholds[2],AnimatorConditionMode.Greater,u)));
                Any(legacy,null,prefix.Concat(new[]{C(Thresholds[0],AnimatorConditionMode.Less,u),C(Thresholds[1],AnimatorConditionMode.Greater,u)}));
            }
            wrapper.anyStateTransitions=any.ToArray();layers[index].stateMachine=wrapper;controller.layers=layers;
            // Synced additive feature layers can retain/accumulate the calculated floats in
            // VRChat even when standalone Unity evaluation settles correctly. Re-evaluate the
            // same arithmetic after those layers, without writing humanoid/default channels.
            // Reuse the original motions; public selection/sync drivers remain their owners.
            for(var i=0;i<Classifiers.Length;i++)
            {
                var source=layers.Single(l=>l.name==ClassifierOutputs[i].Replace("_Final","_"));
                var state=Own(new AnimatorState {name="Calculate",writeDefaultValues=false,
                    motion=source.stateMachine.states.Single(s=>s.state.motion is BlendTree).state.motion});
                var machine=Own(new AnimatorStateMachine {name=ClassifierOutputs[i],defaultState=state,
                    states=new[]{new ChildAnimatorState {state=state}}});
                controller.AddLayer(new AnimatorControllerLayer {name=machine.name,stateMachine=machine,defaultWeight=1});
            }
        }
    }
}
#endif
