// Generated from the owned reference template. See docs/animator-controller-master.md.
// Regenerate deliberately with Tools/AnimatorController/export_definition.py; review semantic tests.
#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
using VRC.SDK3.Avatars.Components;
namespace jp.unisakistudio.posingsystemeditor
{
    internal static partial class PosingSystemControllerCompiler
    {
        internal const string ReferenceSha256 = "172b37d792080ff705400cfe316a048a7bcc813137f46b48040adb36a7858cdb";
        internal const long RootId = 9100000L;
        private static readonly DefinitionNode[] Definition = new DefinitionNode[]
        {
            // VRCAnimatorLocomotionControl: VRCAnimatorLocomotionControl
            new DefinitionNode(-9186497385310866338L, typeof(VRCAnimatorLocomotionControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-9041582441132130758L, typeof(AnimatorStateTransition)),
            // AnimatorState: Prone
            new DefinitionNode(-9005117094314692198L, typeof(AnimatorState)),
            // AnimatorState: SitShallowFix
            new DefinitionNode(-8932902451538939385L, typeof(AnimatorState)),
            // AnimatorState: SitSleepUpFix
            new DefinitionNode(-8864015587925826694L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-8860047223606620120L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8834675512016674655L, typeof(AnimatorStateTransition)),
            // AnimatorState: StandingFix
            new DefinitionNode(-8702174327853843556L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8697339347119874417L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8656182349382953715L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8650761958584520158L, typeof(AnimatorStateTransition)),
            // AnimatorState: Join
            new DefinitionNode(-8645080741607707847L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8459800422002895952L, typeof(AnimatorStateTransition)),
            // AnimatorState: FootAnimation
            new DefinitionNode(-8404606552488539494L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8261843421433052835L, typeof(AnimatorStateTransition)),
            // AnimatorStateMachine: USSPS_Standing
            new DefinitionNode(-8245003282242720658L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8209404191640466415L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(-8175709723608764254L, typeof(VRCAnimatorTrackingControl)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-8141161951190543856L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8048898670150383199L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8036590881233857385L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-8023998724020434991L, typeof(AnimatorStateTransition)),
            // AnimatorState: HeadTrackingEnable
            new DefinitionNode(-8017980792900509518L, typeof(AnimatorState)),
            // BlendTree: Blend Tree
            new DefinitionNode(-8004176462185219728L, typeof(BlendTree)),
            // AnimatorStateMachine: USSPS_SleepThreshold
            new DefinitionNode(-8002262568585033896L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7970294490550426928L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7892066716135729141L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7799726958536779017L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-7770581767140748123L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7769555712526887171L, typeof(AnimatorStateTransition)),
            // AnimatorState: DefaultProne
            new DefinitionNode(-7635459915192288971L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-7603224308718728774L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7597102065752932933L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-7441972870797367197L, typeof(VRCAvatarParameterDriver)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-7336693693371250455L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7307792389382163182L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-7198435202628228089L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7141771334478199010L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7120339767120960190L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-7113914827911885267L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(-7104748241824787929L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-7065306517354729922L, typeof(AnimatorTransition)),
            // AnimatorState: Standing
            new DefinitionNode(-7045312971827608917L, typeof(AnimatorState)),
            // AnimatorStateMachine: USSPS_LocomotionSwitch
            new DefinitionNode(-6971688220828030419L, typeof(AnimatorStateMachine)),
            // BlendTree: Blend Tree
            new DefinitionNode(-6950988609560855343L, typeof(BlendTree)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6903717907105434782L, typeof(AnimatorStateTransition)),
            // AnimatorState: LocomotionDisable
            new DefinitionNode(-6856407911483962358L, typeof(AnimatorState)),
            // AnimatorStateMachine: USSPS_VirtualLoveSwitch
            new DefinitionNode(-6845057007244637955L, typeof(AnimatorStateMachine)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-6783036152695484752L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6604618971732755321L, typeof(AnimatorStateTransition)),
            // AnimatorState: Entry
            new DefinitionNode(-6566766572468628972L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6415051091675185968L, typeof(AnimatorStateTransition)),
            // AnimatorState: Disable
            new DefinitionNode(-6360838821582537359L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6323350822355403767L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6285975002101225164L, typeof(AnimatorStateTransition)),
            // AnimatorState: HandTrackingEnable
            new DefinitionNode(-6285261398426582342L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6197697154766616892L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6166227932418429399L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-6125976586834926885L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5944060894418307968L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5903129708236449206L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5901622704670638806L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(-5892629620592856542L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5886460287939798480L, typeof(AnimatorStateTransition)),
            // AnimatorState: Long Fall
            new DefinitionNode(-5844303027226531482L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-5730762408894148515L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5727673598108981153L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(-5727440760980115769L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5694856564387115746L, typeof(AnimatorStateTransition)),
            // AnimatorState: Stop
            new DefinitionNode(-5562372364005365487L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5544446046194506728L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5542750504764759278L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-5417529116244516423L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5319712610730910797L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5218943721489528098L, typeof(AnimatorStateTransition)),
            // AnimatorState: UpProneFix
            new DefinitionNode(-5168612796223331972L, typeof(AnimatorState)),
            // AnimatorStateMachine: USSPS_TypeFix
            new DefinitionNode(-5111778154121381482L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-5088535173409756075L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4985871882379064051L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4931698144660190948L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-4892243885510418539L, typeof(AnimatorTransition)),
            // AnimatorState: Short Fall
            new DefinitionNode(-4877121330473877102L, typeof(AnimatorState)),
            // AnimatorState: Jump
            new DefinitionNode(-4818397362414333830L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4764950795729499102L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4739147669117192047L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4739064376117261167L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4633391771650121302L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4556331504857612271L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4490324775617798010L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-4490047851752770823L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4458598059707279393L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4429133510348322952L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4410738918830966216L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-4393836963890062772L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: OneSecondWait
            new DefinitionNode(-4339557960422095324L, typeof(AnimatorState)),
            // AnimatorState: DefaultProne
            new DefinitionNode(-4270703006882285157L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4093295808392903961L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4092243626676135150L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4077871164977856785L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4047869482565893700L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4032073773152921836L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-4013092394574969362L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3969498822161387054L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3930588932700599529L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitSleepUp
            new DefinitionNode(-3901770471267843045L, typeof(AnimatorState)),
            // AnimatorStateMachine: USSPS_SitDeepSwitch
            new DefinitionNode(-3893363884970523633L, typeof(AnimatorStateMachine)),
            // AnimatorState: CrouchingFix
            new DefinitionNode(-3846663604578221537L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-3839532406342582009L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: SitSleepDownFix
            new DefinitionNode(-3811653152629470206L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3800776800523237437L, typeof(AnimatorStateTransition)),
            // AnimatorStateMachine: USSPS_Tracking Control
            new DefinitionNode(-3727612667016530745L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3716887931807921347L, typeof(AnimatorStateTransition)),
            // AnimatorState: TypeFix
            new DefinitionNode(-3712177435076253093L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3680471589971892313L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3651208793443136456L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTemporaryPoseSpace: VRCAnimatorTemporaryPoseSpace
            new DefinitionNode(-3604961411213884633L, typeof(VRCAnimatorTemporaryPoseSpace)),
            // AnimatorStateMachine: USSPS_Locomotion
            new DefinitionNode(-3587347608197814683L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3582622527005598074L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3545597654819374863L, typeof(AnimatorStateTransition)),
            // AnimatorState: Standing
            new DefinitionNode(-3467196034403600288L, typeof(AnimatorState)),
            // VRCAnimatorLocomotionControl: VRCAnimatorLocomotionControl
            new DefinitionNode(-3465558766042067479L, typeof(VRCAnimatorLocomotionControl)),
            // AnimatorStateMachine: Jump and Fall
            new DefinitionNode(-3347014144008424580L, typeof(AnimatorStateMachine)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-3325139139692850949L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: Fast Landing
            new DefinitionNode(-3313271818847335558L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-3278793716759011763L, typeof(VRCAvatarParameterDriver)),
            // VRCAnimatorTemporaryPoseSpace: VRCAnimatorTemporaryPoseSpace
            new DefinitionNode(-3241035390664179058L, typeof(VRCAnimatorTemporaryPoseSpace)),
            // AnimatorState: DefaultCrouching
            new DefinitionNode(-3179510343952230707L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3148944356999250741L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-3039184747823374742L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2998249581155981522L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2984900675289911214L, typeof(AnimatorStateTransition)),
            // AnimatorState: Walk and Run
            new DefinitionNode(-2966583916767690452L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2939351280546459283L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2904763195063475070L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2855425809496482469L, typeof(AnimatorStateTransition)),
            // AnimatorState: Hard Landing
            new DefinitionNode(-2719085198984213895L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2717066020364977067L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2705170155286840210L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2675132093838501883L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2673054541247106931L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitDeep
            new DefinitionNode(-2638163585814864993L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2382240343630593977L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2362141101285587683L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2159987392019257731L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2147214659878197416L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2141583219888525641L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-2041725854586730037L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-2037901565460727606L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: OneSecondWait
            new DefinitionNode(-2023537337292720408L, typeof(AnimatorState)),
            // AnimatorState: OneSecondWait
            new DefinitionNode(-2004458968883147084L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1999803211623187898L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1843558209269489132L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitSleepDown_Judge
            new DefinitionNode(-1812671021901136412L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1777459694007759284L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1753680661996891263L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1744727940728115676L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-1712034754366725262L, typeof(AnimatorTransition)),
            // AnimatorState: ProneFix
            new DefinitionNode(-1678464195704557861L, typeof(AnimatorState)),
            // BlendTree: Blend Tree
            new DefinitionNode(-1651988166975395410L, typeof(BlendTree)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1518427238338952022L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(-1516259275484931110L, typeof(VRCAnimatorTrackingControl)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-1442879809448930102L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1422814277246122385L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1393014878813885700L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1337486633971422945L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(-1184063776615288629L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1143999056076423901L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1136115322684838591L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-1000496266171292664L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-980734864861727136L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-951723123244295483L, typeof(VRCAvatarParameterDriver)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-843063836517135681L, typeof(VRCAvatarParameterDriver)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(-696054486529424352L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-595198038418888927L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-560210975563570168L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-496132143941428408L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-388284037969056490L, typeof(AnimatorStateTransition)),
            // AnimatorState: Join
            new DefinitionNode(-349621165224456771L, typeof(AnimatorState)),
            // AnimatorState: Squat
            new DefinitionNode(-299330859164557815L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(-202653390581040172L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-186133935249454295L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(-115950569129989543L, typeof(AnimatorStateTransition)),
            // AnimatorState: Join
            new DefinitionNode(-103457401292027055L, typeof(AnimatorState)),
            // AnimatorController: PosingSystem_Locomotion
            new DefinitionNode(9100000L, typeof(AnimatorController)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(84475701052679949L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(129464835733613725L, typeof(AnimatorStateTransition)),
            // AnimatorState: VirtualLove
            new DefinitionNode(227787750552542906L, typeof(AnimatorState)),
            // AnimatorState: DownProneFix
            new DefinitionNode(298896050601818704L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(379417497168460212L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(391415899355187890L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(419226578169971810L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(432206888958564812L, typeof(AnimatorStateTransition)),
            // AnimatorState: Disable
            new DefinitionNode(453041842244362146L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(482370611428124149L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(502048708777428999L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(594789565523897185L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(601066298698880126L, typeof(AnimatorStateTransition)),
            // AnimatorState: Disable
            new DefinitionNode(714047217337884449L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(756147506764110838L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(764772978942960397L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(798334010872562004L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(806805306300751934L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1072588658339816713L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1116395482624539591L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1122576841650856659L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1203794141084539354L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1265814052827145713L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1286468929964916310L, typeof(AnimatorStateTransition)),
            // AnimatorState: Soft Landing
            new DefinitionNode(1354550426224836391L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1363906358365990913L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1384751506030358378L, typeof(VRCAvatarParameterDriver)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1399527563043926047L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1401632601616513706L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1452103737129381529L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1469906211573443703L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1490469521323756835L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1510441490450271334L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1517698849993058894L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitStand
            new DefinitionNode(1576225024602180665L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1587647406013258175L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1593007504243141818L, typeof(AnimatorStateTransition)),
            // AnimatorState: Long Fall
            new DefinitionNode(1604650454966052080L, typeof(AnimatorState)),
            // AnimatorState: Full Tracking
            new DefinitionNode(1712195311405322793L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1719486859539669268L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1749850713827377960L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1754398245324062402L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1789313070330357987L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1798686694172708565L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1844831855078504998L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(1864864674872881857L, typeof(AnimatorStateTransition)),
            // AnimatorState: Short Fall
            new DefinitionNode(1880796748334177671L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(1977173550619976611L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2007422437012546751L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2021898683366842873L, typeof(AnimatorStateTransition)),
            // AnimatorState: Crouching
            new DefinitionNode(2105239521671056306L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2116281846392818972L, typeof(AnimatorStateTransition)),
            // AnimatorStateMachine: USSPS_IsDefault
            new DefinitionNode(2180333409937386894L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2197463268544749337L, typeof(AnimatorStateTransition)),
            // AnimatorState: TrakingInterrupt
            new DefinitionNode(2220530340456395032L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2239910879985913726L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2250561715789973429L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitSleep
            new DefinitionNode(2304600037645116604L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2317581107861137475L, typeof(AnimatorStateTransition)),
            // AnimatorState: Jump
            new DefinitionNode(2324222784202074907L, typeof(AnimatorState)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(2401664761968955162L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorState: Soft Landing
            new DefinitionNode(2559699162213682565L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(2567460502834823033L, typeof(AnimatorTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(2571336413791974414L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateMachine: USSPS_DeepThreshold
            new DefinitionNode(2603720499104488284L, typeof(AnimatorStateMachine)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(2613236588274559036L, typeof(AnimatorTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(2656058518000263450L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2736271136708466153L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2750504046476625766L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2795331923501773778L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(2867927301204189250L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitDeep
            new DefinitionNode(2889423520103684507L, typeof(AnimatorState)),
            // AnimatorState: HeadTrackingDisable
            new DefinitionNode(2908134339019974356L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(3013019888045244337L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitShallow
            new DefinitionNode(3379010938111213117L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(3489219220771691851L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(3552508835962816256L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(3667213902195132249L, typeof(AnimatorStateTransition)),
            // AnimatorState: TrakingInterrupt
            new DefinitionNode(3702736981160456832L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(3762845737126933226L, typeof(AnimatorTransition)),
            // AnimatorStateMachine: Locomotion
            new DefinitionNode(3767330295973062343L, typeof(AnimatorStateMachine)),
            // AnimatorState: Prone_Desktop
            new DefinitionNode(3804034536764950122L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(3810127428883359455L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(3820359631143990329L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(3933092430513553610L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(3972420135149772085L, typeof(VRCAvatarParameterDriver)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(4015429179568715505L, typeof(VRCAvatarParameterDriver)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4026227302735420188L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitSleepDown
            new DefinitionNode(4103769183520935968L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(4109111520343839319L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4114055265070591450L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4116948194780940074L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4164546560753720868L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTemporaryPoseSpace: VRCAnimatorTemporaryPoseSpace
            new DefinitionNode(4172481162230376389L, typeof(VRCAnimatorTemporaryPoseSpace)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4309911278319336740L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4391080863437436926L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4417705372750419013L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4471879395897769715L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4482920509623862174L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(4559535226270099636L, typeof(AnimatorTransition)),
            // AnimatorState: SitStandFix
            new DefinitionNode(4678283637976753075L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4695751726135262112L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4789322797793541702L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(4841821248554250212L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4859216590737945141L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(4877294916774001240L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4908361856066237886L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4928037174197198983L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4978172694119603683L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(4986884675560532293L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5119081850154939649L, typeof(AnimatorStateTransition)),
            // AnimatorState: Disable
            new DefinitionNode(5152644201869288264L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(5292446822696091174L, typeof(AnimatorTransition)),
            // AnimatorStateMachine: USSPS_HandTrackingSwitch
            new DefinitionNode(5329465683777952040L, typeof(AnimatorStateMachine)),
            // AnimatorState: Crouching
            new DefinitionNode(5333673405549284324L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5399631605789754668L, typeof(AnimatorStateTransition)),
            // AnimatorState: During Action
            new DefinitionNode(5419878531496524280L, typeof(AnimatorState)),
            // AnimatorState: USSPS_ShallowThreshold
            new DefinitionNode(5472656285924103385L, typeof(AnimatorState)),
            // AnimatorState: AvatarSelecting
            new DefinitionNode(5605469346567921054L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5724526291205095571L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5726658522617829103L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitSleepDown_WaitGrounded
            new DefinitionNode(5733085905354127431L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5736735072107563317L, typeof(AnimatorStateTransition)),
            // AnimatorStateMachine: USSPS_SitSleepSwitch
            new DefinitionNode(5818780192655738863L, typeof(AnimatorStateMachine)),
            // AnimatorStateMachine: USSPS_ShallowThreshold
            new DefinitionNode(5839198801620385252L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5881606704422301066L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5891708341949712635L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(5949007334460600902L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(6073009986445721659L, typeof(AnimatorTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(6182935751605298541L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: Standing
            new DefinitionNode(6265879694267861960L, typeof(AnimatorState)),
            // AnimatorState: Out of Action
            new DefinitionNode(6282442230083955286L, typeof(AnimatorState)),
            // AnimatorStateMachine: USSPS_HeadTrackingSwitch
            new DefinitionNode(6286522972071658989L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6300822957838065156L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6351051122216418814L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6357599874705349242L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6432721423726265048L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6449877733818211227L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6460447332502924240L, typeof(AnimatorStateTransition)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(6465487550691516935L, typeof(AnimatorTransition)),
            // AnimatorState: Disable
            new DefinitionNode(6599431597184223555L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6625378491677362194L, typeof(AnimatorStateTransition)),
            // AnimatorState: SitSleepUp_Judge
            new DefinitionNode(6654961008985081469L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(6709823407313137214L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6755566116793462516L, typeof(AnimatorStateTransition)),
            // AnimatorState: VirtualLove
            new DefinitionNode(6942399150830552079L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(6999784229182159729L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7076597759820747927L, typeof(AnimatorStateTransition)),
            // AnimatorState: Crouching_Desktop
            new DefinitionNode(7084976065006286425L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7123272291577341557L, typeof(AnimatorStateTransition)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(7126018751568176052L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorState: USSPS_DeepThreshold
            new DefinitionNode(7131328635292920187L, typeof(AnimatorState)),
            // AnimatorStateMachine: Jump and Fall
            new DefinitionNode(7179414089209466606L, typeof(AnimatorStateMachine)),
            // AnimatorStateMachine: USSPS_SitShallowSwitch
            new DefinitionNode(7218008230595816444L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7227685032877018402L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7306743110195958494L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7377458783020609161L, typeof(AnimatorStateTransition)),
            // AnimatorState: Join
            new DefinitionNode(7387581240911423071L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7393650693768139024L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7398648284079141536L, typeof(AnimatorStateTransition)),
            // AnimatorState: Prone
            new DefinitionNode(7523425782058141989L, typeof(AnimatorState)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(7596079111083747982L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: LocomotionEnable
            new DefinitionNode(7619068478626901828L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7632189824403938265L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7742523317881465365L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7781073268693224667L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(7882097368720023956L, typeof(VRCAvatarParameterDriver)),
            // VRCAnimatorTrackingControl: VRCAnimatorTrackingControl
            new DefinitionNode(7932453726705980427L, typeof(VRCAnimatorTrackingControl)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(7990622540315469992L, typeof(AnimatorStateTransition)),
            // AnimatorState: Standing_Desktop
            new DefinitionNode(8005255496095670369L, typeof(AnimatorState)),
            // AnimatorState: SitShallow
            new DefinitionNode(8032172764612423471L, typeof(AnimatorState)),
            // AnimatorState: SitSleepUp_WaitGrounded
            new DefinitionNode(8106206982272668552L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(8150215709620021398L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(8175923819678673175L, typeof(AnimatorStateTransition)),
            // AnimatorState: Standing
            new DefinitionNode(8183258488441076315L, typeof(AnimatorState)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(8190105288054215298L, typeof(AnimatorStateTransition)),
            // AnimatorState: LocomotionLock
            new DefinitionNode(8260197079921956935L, typeof(AnimatorState)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(8266720753606980800L, typeof(AnimatorTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(8288814374951543783L, typeof(AnimatorStateTransition)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(8350906287002060581L, typeof(AnimatorStateTransition)),
            // AnimatorState: Hard Landing
            new DefinitionNode(8527324074706241840L, typeof(AnimatorState)),
            // AnimatorState: Disable
            new DefinitionNode(8570312493231049636L, typeof(AnimatorState)),
            // AnimatorStateMachine: USSPS_GroupSwitch
            new DefinitionNode(8571016880964741915L, typeof(AnimatorStateMachine)),
            // AnimatorStateMachine: USSPS_LocomotionType
            new DefinitionNode(8576868798655692200L, typeof(AnimatorStateMachine)),
            // AnimatorStateTransition: AnimatorStateTransition
            new DefinitionNode(8601217045347607153L, typeof(AnimatorStateTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(8658828139118200110L, typeof(VRCAvatarParameterDriver)),
            // AnimatorTransition: AnimatorTransition
            new DefinitionNode(8675687255005010764L, typeof(AnimatorTransition)),
            // VRCAvatarParameterDriver: VRCAvatarParameterDriver
            new DefinitionNode(8819143897568466539L, typeof(VRCAvatarParameterDriver)),
            // AnimatorState: SitDeepFix
            new DefinitionNode(9009721933184062462L, typeof(AnimatorState)),
            // AnimatorState: USSPS_SleepThreshold
            new DefinitionNode(9088504416208506103L, typeof(AnimatorState)),
            // AnimatorState: HandTrackingDisable
            new DefinitionNode(9149627423983647829L, typeof(AnimatorState)),
        };
        private static void ConfigureNative(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            Configure0(objects, external);
            Configure1(objects, external);
            Configure2(objects, external);
            Configure3(objects, external);
            Configure4(objects, external);
            Configure5(objects, external);
            Configure6(objects, external);
            Configure7(objects, external);
            Configure8(objects, external);
            Configure9(objects, external);
            ((AnimatorStateMachine)objects[-3727612667016530745L]).SetStateMachineTransitions((AnimatorStateMachine)objects[-3347014144008424580L], new AnimatorTransition[] { (AnimatorTransition)objects[4559535226270099636L], (AnimatorTransition)objects[8675687255005010764L], (AnimatorTransition)objects[6465487550691516935L], (AnimatorTransition)objects[4841821248554250212L], (AnimatorTransition)objects[-7065306517354729922L] });
            ((AnimatorStateMachine)objects[-3727612667016530745L]).SetStateMachineTransitions((AnimatorStateMachine)objects[3767330295973062343L], new AnimatorTransition[] { (AnimatorTransition)objects[379417497168460212L], (AnimatorTransition)objects[3762845737126933226L] });
            ((AnimatorStateMachine)objects[-3587347608197814683L]).SetStateMachineTransitions((AnimatorStateMachine)objects[7179414089209466606L], new AnimatorTransition[] { (AnimatorTransition)objects[-7336693693371250455L], (AnimatorTransition)objects[5292446822696091174L] });
        }
        private static void Configure0(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (VRCAnimatorLocomotionControl)objects[-9186497385310866338L];
                o.name = @"";
                o.debugString = @"";
                o.disableLocomotion = false;
            }
            {
                var o = (AnimatorStateTransition)objects[-9041582441132130758L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2908134339019974356L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HeadTrackingLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-9005117094314692198L];
                o.name = @"Prone";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"667633d86ecc9c0408e81156d77d9a83", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[7227685032877018402L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[-8932902451538939385L];
                o.name = @"SitShallowFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[482370611428124149L], (AnimatorStateTransition)objects[5949007334460600902L], (AnimatorStateTransition)objects[4114055265070591450L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1719486859539669268L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[-8864015587925826694L];
                o.name = @"SitSleepUpFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-1337486633971422945L], (AnimatorStateTransition)objects[7306743110195958494L], (AnimatorStateTransition)objects[-4092243626676135150L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-3325139139692850949L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-8860047223606620120L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 22.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-8834675512016674655L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsSleep", mode = (AnimatorConditionMode)4, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-8702174327853843556L];
                o.name = @"StandingFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-6415051091675185968L], (AnimatorStateTransition)objects[7123272291577341557L], (AnimatorStateTransition)objects[-1753680661996891263L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[756147506764110838L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-8697339347119874417L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8932902451538939385L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-8656182349382953715L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[7131328635292920187L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.01f;
                o.hasExitTime = true;
                o.hasFixedDuration = false;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-8650761958584520158L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8005255496095670369L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-8645080741607707847L];
                o.name = @"Join";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2382240343630593977L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-8459800422002895952L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)3, threshold = 0.85f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-8404606552488539494L];
                o.name = @"FootAnimation";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4490324775617798010L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1977173550619976611L], (StateMachineBehaviour)objects[2401664761968955162L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-8261843421433052835L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3811653152629470206L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 33.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateMachine)objects[-8245003282242720658L];
                o.name = @"USSPS_Standing";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-7045312971827608917L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[6599431597184223555L], position = new Vector3(600.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[1576225024602180665L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-7045312971827608917L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[-8209404191640466415L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[-8175709723608764254L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-8141161951190543856L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 21.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-8048898670150383199L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2889423520103684507L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 2.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-8036590881233857385L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-8023998724020434991L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6856407911483962358L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 60.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-8017980792900509518L];
                o.name = @"HeadTrackingEnable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-7597102065752932933L], (AnimatorStateTransition)objects[-9041582441132130758L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-8175709723608764254L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (BlendTree)objects[-8004176462185219728L];
                o.name = @"Blend Tree";
                o.blendType = (BlendTreeType)3;
                o.blendParameter = @"Upright";
                o.blendParameterY = @"USSPS_DeepThreshold";
                o.useAutomaticThresholds = false;
                o.children = new ChildMotion[] { new ChildMotion { motion = (Motion)external(@"175d3c4613ec4874d951566c337943fc", 7400000L), threshold = 0.0f, position = new Vector2(0.0f, 1.0f), timeScale = 1.0f, cycleOffset = 0.0f, directBlendParameter = @"VelocityX", mirror = false }, new ChildMotion { motion = (Motion)external(@"cc6a61b2932ae9c439f6e1748b39a93e", 7400000L), threshold = 1.0f, position = new Vector2(1.0f, 0.0f), timeScale = 1.0f, cycleOffset = 0.0f, directBlendParameter = @"VelocityX", mirror = false } };
                o.minThreshold = 0.0f;
                o.maxThreshold = 1.0f;
                o.useAutomaticThresholds = true;
                using (var serialized = new SerializedObject(o)) { serialized.FindProperty("m_NormalizedBlendValues").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            }
            {
                var o = (AnimatorStateMachine)objects[-8002262568585033896L];
                o.name = @"USSPS_SleepThreshold";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[9088504416208506103L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-349621165224456771L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-349621165224456771L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[-7970294490550426928L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[3379010938111213117L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsShallow", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-7892066716135729141L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-7799726958536779017L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorTransition)objects[-7770581767140748123L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[7523425782058141989L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 21.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[-7769555712526887171L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2304600037645116604L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsSleep", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-7635459915192288971L];
                o.name = @"DefaultProne";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4032073773152921836L], (AnimatorStateTransition)objects[-3969498822161387054L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-496132143941428408L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-7603224308718728774L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 11.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-7597102065752932933L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[3702736981160456832L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingHeadReset", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-7441972870797367197L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorTransition)objects[-7336693693371250455L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3467196034403600288L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[-7307792389382163182L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6285261398426582342L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HandTrackingLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-7198435202628228089L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 32.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-7141771334478199010L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-7120339767120960190L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-7113914827911885267L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8404606552488539494L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 41.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
        }
        private static void Configure1(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (VRCAnimatorTrackingControl)objects[-7104748241824787929L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
            }
            {
                var o = (AnimatorTransition)objects[-7065306517354729922L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1712195311405322793L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
            }
            {
                var o = (AnimatorState)objects[-7045312971827608917L];
                o.name = @"Standing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1510441490450271334L], (AnimatorStateTransition)objects[5724526291205095571L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-3278793716759011763L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[-6971688220828030419L];
                o.name = @"USSPS_LocomotionSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-2004458968883147084L], position = new Vector3(50.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-6856407911483962358L], position = new Vector3(300.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[7619068478626901828L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-2004458968883147084L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (BlendTree)objects[-6950988609560855343L];
                o.name = @"Blend Tree";
                o.blendType = (BlendTreeType)3;
                o.blendParameter = @"Upright";
                o.blendParameterY = @"USSPS_ShallowThreshold";
                o.useAutomaticThresholds = false;
                o.children = new ChildMotion[] { new ChildMotion { motion = (Motion)external(@"4193d75343a88024f8c74d97c44a6327", 7400000L), threshold = 0.0f, position = new Vector2(0.0f, 1.0f), timeScale = 1.0f, cycleOffset = 0.0f, directBlendParameter = @"VelocityX", mirror = false }, new ChildMotion { motion = (Motion)external(@"9ac85bf8631bce348a6ad45dd9a5f7c3", 7400000L), threshold = 1.0f, position = new Vector2(1.0f, 0.0f), timeScale = 1.0f, cycleOffset = 0.0f, directBlendParameter = @"VelocityX", mirror = false } };
                o.minThreshold = 0.0f;
                o.maxThreshold = 1.0f;
                o.useAutomaticThresholds = true;
                using (var serialized = new SerializedObject(o)) { serialized.FindProperty("m_NormalizedBlendValues").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            }
            {
                var o = (AnimatorStateTransition)objects[-6903717907105434782L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-6856407911483962358L];
                o.name = @"LocomotionDisable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5544446046194506728L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-3465558766042067479L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[-6845057007244637955L];
                o.name = @"USSPS_VirtualLoveSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[453041842244362146L], position = new Vector3(600.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[227787750552542906L], position = new Vector3(300.0f, 0.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[453041842244362146L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorTransition)objects[-6783036152695484752L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[7084976065006286425L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)6, threshold = 1.0f }, new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[-6604618971732755321L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[-3347014144008424580L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)3, threshold = 0.85f }, new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-6566766572468628972L];
                o.name = @"Entry";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-186133935249454295L], (AnimatorStateTransition)objects[-1843558209269489132L], (AnimatorStateTransition)objects[1749850713827377960L], (AnimatorStateTransition)objects[-7769555712526887171L], (AnimatorStateTransition)objects[-4013092394574969362L], (AnimatorStateTransition)objects[-7970294490550426928L], (AnimatorStateTransition)objects[5891708341949712635L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-6415051091675185968L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-6360838821582537359L];
                o.name = @"Disable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[2250561715789973429L], (AnimatorStateTransition)objects[8350906287002060581L], (AnimatorStateTransition)objects[1363906358365990913L], (AnimatorStateTransition)objects[7632189824403938265L], (AnimatorStateTransition)objects[4482920509623862174L], (AnimatorStateTransition)objects[-3582622527005598074L], (AnimatorStateTransition)objects[4309911278319336740L], (AnimatorStateTransition)objects[5119081850154939649L], (AnimatorStateTransition)objects[6357599874705349242L], (AnimatorStateTransition)objects[-8261843421433052835L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-6323350822355403767L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 22.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-6285975002101225164L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[6654961008985081469L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)3, threshold = 1.99f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = 0.01f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = -0.01f }, new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = 0.01f }, new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)3, threshold = -0.01f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-6285261398426582342L];
                o.name = @"HandTrackingEnable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4417705372750419013L], (AnimatorStateTransition)objects[-6166227932418429399L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[7126018751568176052L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-6197697154766616892L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-6166227932418429399L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[9149627423983647829L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HandTrackingLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-6125976586834926885L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-5944060894418307968L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8106206982272668552L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.03f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-5903129708236449206L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.15f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-5901622704670638806L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2719085198984213895L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.05f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[-5892629620592856542L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-5886460287939798480L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VirtualLoveType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-5844303027226531482L];
                o.name = @"Long Fall";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5901622704670638806L], (AnimatorStateTransition)objects[8190105288054215298L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-1184063776615288629L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[-5730762408894148515L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-4877121330473877102L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -4.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[-5727673598108981153L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4103769183520935968L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[-5727440760980115769L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
            }
            {
                var o = (AnimatorStateTransition)objects[-5694856564387115746L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1354550426224836391L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-5562372364005365487L];
                o.name = @"Stop";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4556331504857612271L], (AnimatorStateTransition)objects[3013019888045244337L], (AnimatorStateTransition)objects[2007422437012546751L], (AnimatorStateTransition)objects[5881606704422301066L], (AnimatorStateTransition)objects[3810127428883359455L], (AnimatorStateTransition)objects[1864864674872881857L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-5544446046194506728L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[7619068478626901828L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-5542750504764759278L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[7619068478626901828L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 60.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-5417529116244516423L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingHeadReset", source = @"", value = 0.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-5319712610730910797L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 22.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-5218943721489528098L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3901770471267843045L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-5168612796223331972L];
                o.name = @"UpProneFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6999784229182159729L], (AnimatorStateTransition)objects[3933092430513553610L], (AnimatorStateTransition)objects[-3680471589971892313L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-951723123244295483L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[-5111778154121381482L];
                o.name = @"USSPS_TypeFix";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-6360838821582537359L], position = new Vector3(600.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-8932902451538939385L], position = new Vector3(300.0f, -20.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-8702174327853843556L], position = new Vector3(300.0f, -200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[4678283637976753075L], position = new Vector3(300.0f, -140.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3846663604578221537L], position = new Vector3(300.0f, -80.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-1678464195704557861L], position = new Vector3(300.0f, 40.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[9009721933184062462L], position = new Vector3(300.0f, 100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-5168612796223331972L], position = new Vector3(300.0f, 160.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-8864015587925826694L], position = new Vector3(300.0f, 220.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[298896050601818704L], position = new Vector3(300.0f, 280.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3811653152629470206L], position = new Vector3(300.0f, 340.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-6360838821582537359L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[-5088535173409756075L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5419878531496524280L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VRCEmote", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = false;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4985871882379064051L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3901770471267843045L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4931698144660190948L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 33.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
        }
        private static void Configure2(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (AnimatorTransition)objects[-4892243885510418539L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2105239521671056306L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 11.0f } };
            }
            {
                var o = (AnimatorState)objects[-4877121330473877102L];
                o.name = @"Short Fall";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5694856564387115746L], (AnimatorStateTransition)objects[-4458598059707279393L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[3820359631143990329L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[-4818397362414333830L];
                o.name = @"Jump";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"a5f9dea4a0261414ab29c975b6f70fda", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5903129708236449206L], (AnimatorStateTransition)objects[7398648284079141536L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-4764950795729499102L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3901770471267843045L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"KawaiiPosing_IsDown", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4739147669117192047L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.1f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4739064376117261167L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4103769183520935968L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"KawaiiPosing_IsDown", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4633391771650121302L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.2f;
                o.offset = 0.0f;
                o.exitTime = 0.6f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4556331504857612271L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)3, threshold = 0.85f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4490324775617798010L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1712195311405322793L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 12.0f }, new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 22.0f }, new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 32.0f }, new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 33.0f }, new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 41.0f }, new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 1.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorTransition)objects[-4490047851752770823L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-5844303027226531482L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"IsLocal", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -20.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[-4458598059707279393L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-5844303027226531482L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"IsLocal", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -20.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4429133510348322952L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2559699162213682565L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.1f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4410738918830966216L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-4393836963890062772L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"USSPS_Pose", source = @"", value = 2.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[-4339557960422095324L];
                o.name = @"OneSecondWait";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2855425809496482469L], (AnimatorStateTransition)objects[2795331923501773778L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[-4270703006882285157L];
                o.name = @"DefaultProne";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[3667213902195132249L], (AnimatorStateTransition)objects[1122576841650856659L], (AnimatorStateTransition)objects[-5218943721489528098L], (AnimatorStateTransition)objects[-4764950795729499102L], (AnimatorStateTransition)objects[-4739064376117261167L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[6182935751605298541L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-4093295808392903961L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5472656285924103385L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.01f;
                o.hasExitTime = true;
                o.hasFixedDuration = false;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4092243626676135150L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-5168612796223331972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-4077871164977856785L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 33.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-4047869482565893700L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsSleep", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-4032073773152921836L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2889423520103684507L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-4013092394574969362L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2638163585814864993L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsDeep", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-3969498822161387054L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[714047217337884449L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 2.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-3930588932700599529L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VirtualLoveType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-3901770471267843045L];
                o.name = @"SitSleepUp";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6755566116793462516L], (AnimatorStateTransition)objects[4908361856066237886L], (AnimatorStateTransition)objects[1844831855078504998L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[7596079111083747982L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[-3893363884970523633L];
                o.name = @"USSPS_SitDeepSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[2889423520103684507L], position = new Vector3(300.0f, 100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-7635459915192288971L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[714047217337884449L], position = new Vector3(600.0f, 0.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[714047217337884449L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorState)objects[-3846663604578221537L];
                o.name = @"CrouchingFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-3545597654819374863L], (AnimatorStateTransition)objects[-7120339767120960190L], (AnimatorStateTransition)objects[-8697339347119874417L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[8658828139118200110L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-3839532406342582009L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionTypeGroup", source = @"", value = 4.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[-3811653152629470206L];
                o.name = @"SitSleepDownFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1517698849993058894L], (AnimatorStateTransition)objects[-4077871164977856785L], (AnimatorStateTransition)objects[1798686694172708565L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[798334010872562004L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-3800776800523237437L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8404606552488539494L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 32.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateMachine)objects[-3727612667016530745L];
                o.name = @"USSPS_Tracking Control";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-8645080741607707847L], position = new Vector3(300.0f, 110.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[1712195311405322793L], position = new Vector3(600.0f, 110.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[5419878531496524280L], position = new Vector3(300.0f, 10.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[6282442230083955286L], position = new Vector3(600.0f, 10.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-8404606552488539494L], position = new Vector3(600.0f, 200.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] { new ChildAnimatorStateMachine { stateMachine = (AnimatorStateMachine)objects[-3347014144008424580L], position = new Vector3(900.0f, 210.0f, 0.0f) }, new ChildAnimatorStateMachine { stateMachine = (AnimatorStateMachine)objects[3767330295973062343L], position = new Vector3(900.0f, 110.0f, 0.0f) } };
                o.defaultState = (AnimatorState)objects[-8645080741607707847L];
                o.anyStateTransitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[601066298698880126L], (AnimatorStateTransition)objects[4978172694119603683L], (AnimatorStateTransition)objects[-5088535173409756075L] };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(1220.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[-3716887931807921347L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = 0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-3712177435076253093L];
                o.name = @"TypeFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5886460287939798480L], (AnimatorStateTransition)objects[-7141771334478199010L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[2571336413791974414L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-3680471589971892313L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8864015587925826694L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-3651208793443136456L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[6282442230083955286L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"InStation", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"AFK", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"VRCEmote", mode = (AnimatorConditionMode)4, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAnimatorTemporaryPoseSpace)objects[-3604961411213884633L];
                o.name = @"";
                o.debugString = @"";
                o.delayTime = 0.0f;
                o.enterPoseSpace = true;
                o.fixedDelay = true;
            }
            {
                var o = (AnimatorStateMachine)objects[-3587347608197814683L];
                o.name = @"USSPS_Locomotion";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-9005117094314692198L], position = new Vector3(500.0f, 300.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3467196034403600288L], position = new Vector3(500.0f, 60.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[5333673405549284324L], position = new Vector3(500.0f, 180.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[3804034536764950122L], position = new Vector3(500.0f, 240.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[7084976065006286425L], position = new Vector3(500.0f, 120.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[8005255496095670369L], position = new Vector3(500.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[5605469346567921054L], position = new Vector3(800.0f, 0.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] { new ChildAnimatorStateMachine { stateMachine = (AnimatorStateMachine)objects[7179414089209466606L], position = new Vector3(800.0f, -160.0f, 0.0f) } };
                o.defaultState = (AnimatorState)objects[-3467196034403600288L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] { (AnimatorTransition)objects[4109111520343839319L], (AnimatorTransition)objects[2567460502834823033L], (AnimatorTransition)objects[6073009986445721659L], (AnimatorTransition)objects[6709823407313137214L], (AnimatorTransition)objects[-6783036152695484752L], (AnimatorTransition)objects[3489219220771691851L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[-3582622527005598074L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[9009721933184062462L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 22.0f }, new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-3545597654819374863L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 12.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-3467196034403600288L];
                o.name = @"Standing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"b7ff0bc6ae31ce4458992fa6ce9f6897", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6625378491677362194L], (AnimatorStateTransition)objects[-2939351280546459283L], (AnimatorStateTransition)objects[-8650761958584520158L], (AnimatorStateTransition)objects[7076597759820747927L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
        }
        private static void Configure3(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (VRCAnimatorLocomotionControl)objects[-3465558766042067479L];
                o.name = @"";
                o.debugString = @"";
                o.disableLocomotion = true;
            }
            {
                var o = (AnimatorStateMachine)objects[-3347014144008424580L];
                o.name = @"Jump and Fall";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-4877121330473877102L], position = new Vector3(300.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-2719085198984213895L], position = new Vector3(600.0f, 400.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-5844303027226531482L], position = new Vector3(300.0f, 400.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[1354550426224836391L], position = new Vector3(600.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[2324222784202074907L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3313271818847335558L], position = new Vector3(600.0f, 0.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[2324222784202074907L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] { (AnimatorTransition)objects[-4490047851752770823L], (AnimatorTransition)objects[-5730762408894148515L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(900.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(30.0f, 310.0f, 0.0f);
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-3325139139692850949L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 32.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[-3313271818847335558L];
                o.name = @"Fast Landing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2147214659878197416L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-3278793716759011763L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 0.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (VRCAnimatorTemporaryPoseSpace)objects[-3241035390664179058L];
                o.name = @"";
                o.debugString = @"";
                o.delayTime = 0.0f;
                o.enterPoseSpace = false;
                o.fixedDelay = true;
            }
            {
                var o = (AnimatorState)objects[-3179510343952230707L];
                o.name = @"DefaultCrouching";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[2750504046476625766L], (AnimatorStateTransition)objects[8150215709620021398L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-7603224308718728774L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-3148944356999250741L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[714047217337884449L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 2.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-3039184747823374742L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 0.5f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)3;
            }
            {
                var o = (AnimatorStateTransition)objects[-2998249581155981522L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-4270703006882285157L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 3.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2984900675289911214L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8527324074706241840L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.1f;
                o.offset = 0.0f;
                o.exitTime = 0.05f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-2966583916767690452L];
                o.name = @"Walk and Run";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-8459800422002895952L], (AnimatorStateTransition)objects[-2041725854586730037L], (AnimatorStateTransition)objects[7742523317881465365L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-7104748241824787929L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-2939351280546459283L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[7179414089209466606L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"USSPS_IsShallow", mode = (AnimatorConditionMode)4, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2904763195063475070L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1712195311405322793L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2855425809496482469L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8017980792900509518L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HeadTrackingLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 60.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-2719085198984213895L];
                o.name = @"Hard Landing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1469906211573443703L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-2717066020364977067L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[9009721933184062462L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-2705170155286840210L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-2675132093838501883L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4103769183520935968L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 3.0f }, new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2673054541247106931L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsShallow", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-2638163585814864993L];
                o.name = @"SitDeep";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-6125976586834926885L], (AnimatorStateTransition)objects[-4047869482565893700L], (AnimatorStateTransition)objects[2867927301204189250L], (AnimatorStateTransition)objects[2736271136708466153L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[502048708777428999L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-2382240343630593977L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1712195311405322793L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.25f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2362141101285587683L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8404606552488539494L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2159987392019257731L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8702174327853843556L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-2147214659878197416L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.25f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2141583219888525641L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3901770471267843045L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 3.0f }, new AnimatorCondition { parameter = @"KawaiiPosing_IsDown", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-2041725854586730037L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-5562372364005365487L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)3, threshold = -0.1f }, new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = 0.1f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = -0.1f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = 0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-2037901565460727606L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingInterrupt", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[-2023537337292720408L];
                o.name = @"OneSecondWait";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[7377458783020609161L], (AnimatorStateTransition)objects[1401632601616513706L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[-2004458968883147084L];
                o.name = @"OneSecondWait";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-8023998724020434991L], (AnimatorStateTransition)objects[-5542750504764759278L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-1999803211623187898L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1576225024602180665L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-1843558209269489132L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[6942399150830552079L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VirtualLoveType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[-1812671021901136412L];
                o.name = @"SitSleepDown_Judge";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4789322797793541702L], (AnimatorStateTransition)objects[-1518427238338952022L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[-1777459694007759284L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = -0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-1753680661996891263L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4678283637976753075L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-1744727940728115676L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-1678464195704557861L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorTransition)objects[-1712034754366725262L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1880796748334177671L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -4.0f } };
            }
            {
                var o = (AnimatorState)objects[-1678464195704557861L];
                o.name = @"ProneFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5319712610730910797L], (AnimatorStateTransition)objects[-6903717907105434782L], (AnimatorStateTransition)objects[-2717066020364977067L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-8141161951190543856L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (BlendTree)objects[-1651988166975395410L];
                o.name = @"Blend Tree";
                o.blendType = (BlendTreeType)3;
                o.blendParameter = @"Upright";
                o.blendParameterY = @"USSPS_SleepThreshold";
                o.useAutomaticThresholds = false;
                o.children = new ChildMotion[] { new ChildMotion { motion = (Motion)external(@"9ce00a2516add244586f2076c4bb1468", 7400000L), threshold = 0.0f, position = new Vector2(0.0f, 1.0f), timeScale = 1.0f, cycleOffset = 0.0f, directBlendParameter = @"VelocityX", mirror = false }, new ChildMotion { motion = (Motion)external(@"9740526c4ac725a4fae20b50f737ee96", 7400000L), threshold = 1.0f, position = new Vector2(1.0f, 0.0f), timeScale = 1.0f, cycleOffset = 0.0f, directBlendParameter = @"VelocityX", mirror = false } };
                o.minThreshold = 0.0f;
                o.maxThreshold = 1.0f;
                o.useAutomaticThresholds = true;
                using (var serialized = new SerializedObject(o)) { serialized.FindProperty("m_NormalizedBlendValues").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            }
            {
                var o = (AnimatorStateTransition)objects[-1518427238338952022L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5733085905354127431L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.03f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
        }
        private static void Configure4(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (VRCAnimatorTrackingControl)objects[-1516259275484931110L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-1442879809448930102L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 21.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-1422814277246122385L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4103769183520935968L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 3.0f }, new AnimatorCondition { parameter = @"KawaiiPosing_IsDown", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-1393014878813885700L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-1337486633971422945L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[-1184063776615288629L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-1143999056076423901L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5152644201869288264L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-1136115322684838591L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-4270703006882285157L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-1000496266171292664L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-980734864861727136L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)3, threshold = -0.1f }, new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = 0.1f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = -0.1f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = 0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-951723123244295483L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 21.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-843063836517135681L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorTransition)objects[-696054486529424352L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1604650454966052080L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -20.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[-595198038418888927L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-7635459915192288971L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[-560210975563570168L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1880796748334177671L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)3, threshold = -20.0f } };
                o.duration = 0.2f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-496132143941428408L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 21.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-388284037969056490L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3901770471267843045L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 3.0f }, new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-349621165224456771L];
                o.name = @"Join";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[5399631605789754668L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[-299330859164557815L];
                o.name = @"Squat";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1286468929964916310L], (AnimatorStateTransition)objects[-980734864861727136L], (AnimatorStateTransition)objects[-115950569129989543L], (AnimatorStateTransition)objects[6300822957838065156L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-5727440760980115769L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[-202653390581040172L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionTypeGroup", source = @"", value = 3.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[-186133935249454295L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8260197079921956935L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[-115950569129989543L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2966583916767690452L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)3, threshold = 0.85f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[-103457401292027055L];
                o.name = @"Join";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4093295808392903961L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorController)objects[9100000L];
                o.name = @"PosingSystem_Locomotion";
                o.parameters = new AnimatorControllerParameter[] { new AnimatorControllerParameter { name = @"IsLocal", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"VelocityX", type = (AnimatorControllerParameterType)1, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"VelocityY", type = (AnimatorControllerParameterType)1, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"VelocityZ", type = (AnimatorControllerParameterType)1, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"Upright", type = (AnimatorControllerParameterType)1, defaultFloat = 1.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"Grounded", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"InStation", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"AFK", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"TrackingType", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"VRMode", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"VRCEmote", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"TrackingHeadReset", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"TrackingHandReset", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"LocomotionTypeGroup", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"LocomotionType", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"SitStand", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"SitShallow", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"SitDeep", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"SitSleepUp", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"SitSleepDown", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"VirtualLoveType", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"LocomotionLock", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"HeadTrackingLock", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"HandTrackingLock", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"FootHeight", type = (AnimatorControllerParameterType)1, defaultFloat = 0.5f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"KawaiiPosing_IsDown", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"AlwaysZero", type = (AnimatorControllerParameterType)1, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_ShallowThreshold", type = (AnimatorControllerParameterType)1, defaultFloat = 0.8f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_DeepThreshold", type = (AnimatorControllerParameterType)1, defaultFloat = 0.59f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_SleepThreshold", type = (AnimatorControllerParameterType)1, defaultFloat = 0.34f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_IsShallow", type = (AnimatorControllerParameterType)1, defaultFloat = -1.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_IsDeep", type = (AnimatorControllerParameterType)1, defaultFloat = -1.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_IsSleep", type = (AnimatorControllerParameterType)1, defaultFloat = -1.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_Pose", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_Mirror", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_LastMenu", type = (AnimatorControllerParameterType)3, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false }, new AnimatorControllerParameter { name = @"USSPS_TypeFix", type = (AnimatorControllerParameterType)4, defaultFloat = 0.0f, defaultInt = 0, defaultBool = false } };
                o.layers = new AnimatorControllerLayer[] { new AnimatorControllerLayer { name = @"USSPS_ShallowThreshold", stateMachine = (AnimatorStateMachine)objects[5839198801620385252L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 1.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_DeepThreshold", stateMachine = (AnimatorStateMachine)objects[2603720499104488284L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 1.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_SleepThreshold", stateMachine = (AnimatorStateMachine)objects[-8002262568585033896L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 1.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_GroupSwitch", stateMachine = (AnimatorStateMachine)objects[8571016880964741915L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_Standing", stateMachine = (AnimatorStateMachine)objects[-8245003282242720658L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_SitShallowSwitch", stateMachine = (AnimatorStateMachine)objects[7218008230595816444L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_SitDeepSwitch", stateMachine = (AnimatorStateMachine)objects[-3893363884970523633L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_SitSleepSwitch", stateMachine = (AnimatorStateMachine)objects[5818780192655738863L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_VirtualLoveSwitch", stateMachine = (AnimatorStateMachine)objects[-6845057007244637955L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_TypeFix", stateMachine = (AnimatorStateMachine)objects[-5111778154121381482L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_LocomotionType", stateMachine = (AnimatorStateMachine)objects[8576868798655692200L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_Locomotion", stateMachine = (AnimatorStateMachine)objects[-3587347608197814683L], avatarMask = null, blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 1.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_Tracking Control", stateMachine = (AnimatorStateMachine)objects[-3727612667016530745L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_LocomotionSwitch", stateMachine = (AnimatorStateMachine)objects[-6971688220828030419L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_HeadTrackingSwitch", stateMachine = (AnimatorStateMachine)objects[6286522972071658989L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_HandTrackingSwitch", stateMachine = (AnimatorStateMachine)objects[5329465683777952040L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false }, new AnimatorControllerLayer { name = @"USSPS_IsDefault", stateMachine = (AnimatorStateMachine)objects[2180333409937386894L], avatarMask = (AvatarMask)external(@"c2ad7bc317a70494fb76fb1c9f13172d", 31900000L), blendingMode = (AnimatorLayerBlendingMode)0, syncedLayerIndex = -1, defaultWeight = 0.0f, iKPass = false, syncedLayerAffectsTiming = false } };
            }
            {
                var o = (AnimatorStateTransition)objects[84475701052679949L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8404606552488539494L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 33.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[129464835733613725L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2220530340456395032L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingHandReset", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[227787750552542906L];
                o.name = @"VirtualLove";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1587647406013258175L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[2656058518000263450L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[298896050601818704L];
                o.name = @"DownProneFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4931698144660190948L], (AnimatorStateTransition)objects[-2705170155286840210L], (AnimatorStateTransition)objects[4928037174197198983L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-1442879809448930102L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[379417497168460212L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[-3347014144008424580L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)3, threshold = 0.85f } };
            }
            {
                var o = (AnimatorStateTransition)objects[391415899355187890L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[419226578169971810L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3179510343952230707L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[432206888958564812L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8404606552488539494L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 12.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[453041842244362146L];
                o.name = @"Disable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4695751726135262112L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[482370611428124149L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[502048708777428999L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionTypeGroup", source = @"", value = 2.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[594789565523897185L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[601066298698880126L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5419878531496524280L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"InStation", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = false;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[714047217337884449L];
                o.name = @"Disable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4391080863437436926L], (AnimatorStateTransition)objects[-8048898670150383199L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[756147506764110838L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 0.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[764772978942960397L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1604650454966052080L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -20.0f } };
                o.duration = 0.2f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
        }
        private static void Configure5(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (VRCAvatarParameterDriver)objects[798334010872562004L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 33.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[806805306300751934L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8570312493231049636L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 3.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1072588658339816713L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VirtualLoveType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1116395482624539591L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingHandReset", source = @"", value = 0.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[1122576841650856659L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8570312493231049636L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 3.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1203794141084539354L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 33.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[1265814052827145713L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3313271818847335558L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1286468929964916310L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)3, threshold = 0.85f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[1354550426224836391L];
                o.name = @"Soft Landing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4164546560753720868L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[1363906358365990913L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4678283637976753075L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 1.0f }, new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1384751506030358378L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 33.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1399527563043926047L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"USSPS_Pose", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[1401632601616513706L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6285261398426582342L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HandTrackingLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 60.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1452103737129381529L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[9149627423983647829L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HandTrackingLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1469906211573443703L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1490469521323756835L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 22.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[1510441490450271334L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[6599431597184223555L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1517698849993058894L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[1576225024602180665L];
                o.name = @"SitStand";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[3552508835962816256L], (AnimatorStateTransition)objects[1789313070330357987L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-7441972870797367197L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[1587647406013258175L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[453041842244362146L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 4.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1593007504243141818L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6285261398426582342L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HandTrackingLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[1604650454966052080L];
                o.name = @"Long Fall";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"704174e3f5fd50044a762a02753e8bee", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2984900675289911214L], (AnimatorStateTransition)objects[-560210975563570168L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[1712195311405322793L];
                o.name = @"Full Tracking";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-6604618971732755321L], (AnimatorStateTransition)objects[2197463268544749337L], (AnimatorStateTransition)objects[8175923819678673175L], (AnimatorStateTransition)objects[-1777459694007759284L], (AnimatorStateTransition)objects[-3716887931807921347L], (AnimatorStateTransition)objects[-7113914827911885267L], (AnimatorStateTransition)objects[-3800776800523237437L], (AnimatorStateTransition)objects[5736735072107563317L], (AnimatorStateTransition)objects[432206888958564812L], (AnimatorStateTransition)objects[84475701052679949L], (AnimatorStateTransition)objects[-2362141101285587683L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[4877294916774001240L], (StateMachineBehaviour)objects[8819143897568466539L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1719486859539669268L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 12.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[1749850713827377960L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3712177435076253093L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1754398245324062402L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 12.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[1789313070330357987L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-7045312971827608917L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1798686694172708565L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[298896050601818704L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[1844831855078504998L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-1812671021901136412L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)3, threshold = 1.99f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = 0.01f }, new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = -0.01f }, new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = 0.01f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[1864864674872881857L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.1f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[1880796748334177671L];
                o.name = @"Short Fall";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"a5f9dea4a0261414ab29c975b6f70fda", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[764772978942960397L], (AnimatorStateTransition)objects[-4429133510348322952L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[1977173550619976611L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingHeadReset", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f }, new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingHandReset", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[2007422437012546751L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2966583916767690452L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)3, threshold = 0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[2021898683366842873L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8017980792900509518L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HeadTrackingLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[2105239521671056306L];
                o.name = @"Crouching";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4986884675560532293L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1399527563043926047L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[2116281846392818972L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[7179414089209466606L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)2, threshold = 0.0f }, new AnimatorCondition { parameter = @"USSPS_IsShallow", mode = (AnimatorConditionMode)4, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateMachine)objects[2180333409937386894L];
                o.name = @"USSPS_IsDefault";
                o.states = new ChildAnimatorState[] {  };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = null;
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[2197463268544749337L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = -0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[2220530340456395032L];
                o.name = @"TrakingInterrupt";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1452103737129381529L], (AnimatorStateTransition)objects[-7307792389382163182L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1116395482624539591L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[2239910879985913726L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3179510343952230707L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
        }
        private static void Configure6(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (AnimatorStateTransition)objects[2250561715789973429L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8932902451538939385L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 12.0f }, new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[2304600037645116604L];
                o.name = @"SitSleep";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[7990622540315469992L], (AnimatorStateTransition)objects[-8834675512016674655L], (AnimatorStateTransition)objects[-7892066716135729141L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-202653390581040172L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[2317581107861137475L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2908134339019974356L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HeadTrackingLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[2324222784202074907L];
                o.name = @"Jump";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[1265814052827145713L], (AnimatorStateTransition)objects[5726658522617829103L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-1516259275484931110L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[2401664761968955162L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
            }
            {
                var o = (AnimatorState)objects[2559699162213682565L];
                o.name = @"Soft Landing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"89bd787608751aa46a91bbb4478f52b2", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4739147669117192047L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[2567460502834823033L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3467196034403600288L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
            }
            {
                var o = (VRCAvatarParameterDriver)objects[2571336413791974414L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionTypeGroup", source = @"", value = 5.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateMachine)objects[2603720499104488284L];
                o.name = @"USSPS_DeepThreshold";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[7131328635292920187L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[7387581240911423071L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[7387581240911423071L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorTransition)objects[2613236588274559036L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-299330859164557815L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)3, threshold = 5.0f }, new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)4, threshold = 0.8f } };
            }
            {
                var o = (VRCAvatarParameterDriver)objects[2656058518000263450L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 41.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[2736271136708466153L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_TypeFix", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[2750504046476625766L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5152644201869288264L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[2795331923501773778L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2908134339019974356L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HeadTrackingLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 60.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[2867927301204189250L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsDeep", mode = (AnimatorConditionMode)4, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[2889423520103684507L];
                o.name = @"SitDeep";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-595198038418888927L], (AnimatorStateTransition)objects[-3148944356999250741L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1490469521323756835L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[2908134339019974356L];
                o.name = @"HeadTrackingDisable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6351051122216418814L], (AnimatorStateTransition)objects[2021898683366842873L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-5892629620592856542L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[3013019888045244337L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2966583916767690452L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = -0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[3379010938111213117L];
                o.name = @"SitShallow";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-3930588932700599529L], (AnimatorStateTransition)objects[4471879395897769715L], (AnimatorStateTransition)objects[8601217045347607153L], (AnimatorStateTransition)objects[391415899355187890L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[3972420135149772085L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[3489219220771691851L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8005255496095670369L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[3552508835962816256L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[6599431597184223555L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[3667213902195132249L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4103769183520935968L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[3702736981160456832L];
                o.name = @"TrakingInterrupt";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[2317581107861137475L], (AnimatorStateTransition)objects[4859216590737945141L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-5417529116244516423L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[3762845737126933226L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1712195311405322793L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
            }
            {
                var o = (AnimatorStateMachine)objects[3767330295973062343L];
                o.name = @"Locomotion";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[-2966583916767690452L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-5562372364005365487L], position = new Vector3(600.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-299330859164557815L], position = new Vector3(300.0f, 200.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-2966583916767690452L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] { (AnimatorTransition)objects[2613236588274559036L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-2037901565460727606L] };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(900.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(30.0f, 230.0f, 0.0f);
            }
            {
                var o = (AnimatorState)objects[3804034536764950122L];
                o.name = @"Prone_Desktop";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"667633d86ecc9c0408e81156d77d9a83", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4116948194780940074L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-3604961411213884633L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[3810127428883359455L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2966583916767690452L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = 0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[3820359631143990329L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
            }
            {
                var o = (AnimatorStateTransition)objects[3933092430513553610L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[3972420135149772085L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionTypeGroup", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[4015429179568715505L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionTypeGroup", source = @"", value = 0.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorStateTransition)objects[4026227302735420188L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6856407911483962358L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 1.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[4103769183520935968L];
                o.name = @"SitSleepDown";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-1136115322684838591L], (AnimatorStateTransition)objects[806805306300751934L], (AnimatorStateTransition)objects[-6285975002101225164L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1384751506030358378L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[4109111520343839319L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-9005117094314692198L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)6, threshold = 2.0f }, new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[4114055265070591450L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3846663604578221537L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[4116948194780940074L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)7, threshold = 2.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[4164546560753720868L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.6f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAnimatorTemporaryPoseSpace)objects[4172481162230376389L];
                o.name = @"";
                o.debugString = @"";
                o.delayTime = 0.0f;
                o.enterPoseSpace = true;
                o.fixedDelay = true;
            }
            {
                var o = (AnimatorStateTransition)objects[4309911278319336740L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-5168612796223331972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 32.0f }, new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[4391080863437436926L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-7635459915192288971L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 2.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
        }
        private static void Configure7(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (AnimatorStateTransition)objects[4417705372750419013L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[2220530340456395032L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingHandReset", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[4471879395897769715L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsDeep", mode = (AnimatorConditionMode)3, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[4482920509623862174L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-1678464195704557861L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 22.0f }, new AnimatorCondition { parameter = @"SitDeep", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorTransition)objects[4559535226270099636L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)4, threshold = -0.1f } };
            }
            {
                var o = (AnimatorState)objects[4678283637976753075L];
                o.name = @"SitStandFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-8036590881233857385L], (AnimatorStateTransition)objects[594789565523897185L], (AnimatorStateTransition)objects[-2159987392019257731L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-843063836517135681L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[4695751726135262112L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[227787750552542906L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 4.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[4789322797793541702L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3901770471267843045L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorTransition)objects[4841821248554250212L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)3, threshold = 0.1f } };
            }
            {
                var o = (AnimatorStateTransition)objects[4859216590737945141L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8017980792900509518L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HeadTrackingLock", mode = (AnimatorConditionMode)2, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[4877294916774001240L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
            }
            {
                var o = (AnimatorStateTransition)objects[4908361856066237886L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8570312493231049636L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 3.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[4928037174197198983L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3811653152629470206L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepDown", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[4978172694119603683L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5419878531496524280L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"AFK", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = false;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[4986884675560532293L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 11.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[5119081850154939649L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8864015587925826694L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 32.0f }, new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[5152644201869288264L];
                o.name = @"Disable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6432721423726265048L], (AnimatorStateTransition)objects[419226578169971810L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[5292446822696091174L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8005255496095670369L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
            }
            {
                var o = (AnimatorStateMachine)objects[5329465683777952040L];
                o.name = @"USSPS_HandTrackingSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[2220530340456395032L], position = new Vector3(600.0f, 100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-2023537337292720408L], position = new Vector3(50.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-6285261398426582342L], position = new Vector3(300.0f, 100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[9149627423983647829L], position = new Vector3(300.0f, 200.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-2023537337292720408L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorState)objects[5333673405549284324L];
                o.name = @"Crouching";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"1fe93258fe621c344be8713451c5104f", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-3039184747823374742L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[5399631605789754668L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[9088504416208506103L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] {  };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.01f;
                o.hasExitTime = true;
                o.hasFixedDuration = false;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[5419878531496524280L];
                o.name = @"During Action";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-3651208793443136456L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[5472656285924103385L];
                o.name = @"USSPS_ShallowThreshold";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = true;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)objects[-6950988609560855343L];
                o.transitions = new AnimatorStateTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[5605469346567921054L];
                o.name = @"AvatarSelecting";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"b7ff0bc6ae31ce4458992fa6ce9f6897", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-6197697154766616892L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[5724526291205095571L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1576225024602180665L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[5726658522617829103L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-4877121330473877102L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -8.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[5733085905354127431L];
                o.name = @"SitSleepDown_WaitGrounded";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[7781073268693224667L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1203794141084539354L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[5736735072107563317L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8404606552488539494L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 22.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateMachine)objects[5818780192655738863L];
                o.name = @"USSPS_SitSleepSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[4103769183520935968L], position = new Vector3(300.0f, 100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-4270703006882285157L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[8570312493231049636L], position = new Vector3(800.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3901770471267843045L], position = new Vector3(300.0f, -100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-1812671021901136412L], position = new Vector3(300.0f, -200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[6654961008985081469L], position = new Vector3(300.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[5733085905354127431L], position = new Vector3(550.0f, -200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[8106206982272668552L], position = new Vector3(550.0f, 200.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[8570312493231049636L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateMachine)objects[5839198801620385252L];
                o.name = @"USSPS_ShallowThreshold";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[5472656285924103385L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-103457401292027055L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-103457401292027055L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[5881606704422301066L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2966583916767690452L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = -0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[5891708341949712635L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8183258488441076315L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsShallow", mode = (AnimatorConditionMode)4, threshold = 0.0f }, new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[5949007334460600902L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 12.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorTransition)objects[6073009986445721659L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5333673405549284324L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)6, threshold = 1.0f }, new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
            }
            {
                var o = (VRCAvatarParameterDriver)objects[6182935751605298541L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 21.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[6265879694267861960L];
                o.name = @"Standing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6460447332502924240L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[7882097368720023956L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[6282442230083955286L];
                o.name = @"Out of Action";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2904763195063475070L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[6286522972071658989L];
                o.name = @"USSPS_HeadTrackingSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[3702736981160456832L], position = new Vector3(600.0f, 100.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-4339557960422095324L], position = new Vector3(50.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[2908134339019974356L], position = new Vector3(300.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-8017980792900509518L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-4339557960422095324L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[6300822957838065156L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-2966583916767690452L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)4, threshold = 6.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[6351051122216418814L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[3702736981160456832L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingHeadReset", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[6357599874705349242L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[298896050601818704L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 33.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
        }
        private static void Configure8(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (AnimatorStateTransition)objects[6432721423726265048L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8032172764612423471L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)7, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 1.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[6449877733818211227L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 21.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[6460447332502924240L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorTransition)objects[6465487550691516935L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityZ", mode = (AnimatorConditionMode)4, threshold = -0.1f } };
            }
            {
                var o = (AnimatorState)objects[6599431597184223555L];
                o.name = @"Disable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[8288814374951543783L], (AnimatorStateTransition)objects[-1999803211623187898L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[6625378491677362194L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.5f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)3;
            }
            {
                var o = (AnimatorState)objects[6654961008985081469L];
                o.name = @"SitSleepUp_Judge";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-5727673598108981153L], (AnimatorStateTransition)objects[-5944060894418307968L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[6709823407313137214L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[3804034536764950122L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)6, threshold = 2.0f }, new AnimatorCondition { parameter = @"VRMode", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[6755566116793462516L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-4270703006882285157L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitSleepUp", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[6942399150830552079L];
                o.name = @"VirtualLove";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[7393650693768139024L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-3839532406342582009L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[6999784229182159729L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 32.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[7076597759820747927L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[5605469346567921054L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = false;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[7084976065006286425L];
                o.name = @"Crouching_Desktop";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"1fe93258fe621c344be8713451c5104f", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4410738918830966216L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[4172481162230376389L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[7123272291577341557L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)7, threshold = 5.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[7126018751568176052L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)1;
            }
            {
                var o = (AnimatorState)objects[7131328635292920187L];
                o.name = @"USSPS_DeepThreshold";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = true;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)objects[-8004176462185219728L];
                o.transitions = new AnimatorStateTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[7179414089209466606L];
                o.name = @"Jump and Fall";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[1880796748334177671L], position = new Vector3(300.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[8527324074706241840L], position = new Vector3(600.0f, 400.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[1604650454966052080L], position = new Vector3(300.0f, 400.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[2559699162213682565L], position = new Vector3(600.0f, 200.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-4818397362414333830L], position = new Vector3(300.0f, 0.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-4818397362414333830L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] { (AnimatorTransition)objects[-1712034754366725262L], (AnimatorTransition)objects[-696054486529424352L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(900.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateMachine)objects[7218008230595816444L];
                o.name = @"USSPS_SitShallowSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[5152644201869288264L], position = new Vector3(600.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3179510343952230707L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[8032172764612423471L], position = new Vector3(300.0f, 100.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[5152644201869288264L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(800.0f, 120.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[7227685032877018402L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = null;
                o.isExit = true;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_Pose", mode = (AnimatorConditionMode)7, threshold = 2.0f } };
                o.duration = 0.5f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)3;
            }
            {
                var o = (AnimatorStateTransition)objects[7306743110195958494L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6360838821582537359L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)7, threshold = 32.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[7377458783020609161L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[9149627423983647829L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"HandTrackingLock", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 60.0f;
                o.hasExitTime = true;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[7387581240911423071L];
                o.name = @"Join";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-8656182349382953715L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[7393650693768139024L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VirtualLoveType", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[7398648284079141536L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[1880796748334177671L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)4, threshold = -8.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[7523425782058141989L];
                o.name = @"Prone";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[6449877733818211227L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-4393836963890062772L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (VRCAvatarParameterDriver)objects[7596079111083747982L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 32.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[7619068478626901828L];
                o.name = @"LocomotionEnable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[4026227302735420188L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-9186497385310866338L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[7632189824403938265L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-3846663604578221537L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 12.0f }, new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorStateTransition)objects[7742523317881465365L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-299330859164557815L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"TrackingType", mode = (AnimatorConditionMode)3, threshold = 5.0f }, new AnimatorCondition { parameter = @"Upright", mode = (AnimatorConditionMode)4, threshold = 0.8f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[7781073268693224667L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[4103769183520935968L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"Grounded", mode = (AnimatorConditionMode)1, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[7882097368720023956L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"USSPS_Pose", source = @"", value = 0.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (VRCAnimatorTrackingControl)objects[7932453726705980427L];
                o.name = @"";
                o.debugString = @"";
                o.trackingEyes = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHead = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingHip = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingLeftHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
                o.trackingMouth = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFingers = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightFoot = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)0;
                o.trackingRightHand = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)2;
            }
            {
                var o = (AnimatorStateTransition)objects[7990622540315469992L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VirtualLoveType", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[8005255496095670369L];
                o.name = @"Standing_Desktop";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"b7ff0bc6ae31ce4458992fa6ce9f6897", 20600000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-1393014878813885700L], (AnimatorStateTransition)objects[2116281846392818972L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-3241035390664179058L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[8032172764612423471L];
                o.name = @"SitShallow";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-1143999056076423901L], (AnimatorStateTransition)objects[2239910879985913726L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[1754398245324062402L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[8106206982272668552L];
                o.name = @"SitSleepUp_WaitGrounded";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4985871882379064051L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-7198435202628228089L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[8150215709620021398L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[8032172764612423471L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitShallow", mode = (AnimatorConditionMode)7, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[8175923819678673175L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)3, threshold = 0.1f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorState)objects[8183258488441076315L];
                o.name = @"Standing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2673054541247106931L], (AnimatorStateTransition)objects[1072588658339816713L], (AnimatorStateTransition)objects[-7799726958536779017L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[4015429179568715505L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateTransition)objects[8190105288054215298L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-4877121330473877102L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityY", mode = (AnimatorConditionMode)3, threshold = -20.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.75f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
        }
        private static void Configure9(Dictionary<long, Object> objects, Func<string, long, Object> external)
        {
            {
                var o = (AnimatorState)objects[8260197079921956935L];
                o.name = @"LocomotionLock";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-8209404191640466415L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorTransition)objects[8266720753606980800L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[6265879694267861960L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionType", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
            }
            {
                var o = (AnimatorStateTransition)objects[8288814374951543783L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-7045312971827608917L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)6, threshold = 0.0f }, new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)0;
            }
            {
                var o = (AnimatorStateTransition)objects[8350906287002060581L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-8702174327853843556L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"LocomotionTypeGroup", mode = (AnimatorConditionMode)6, threshold = 5.0f }, new AnimatorCondition { parameter = @"USSPS_LastMenu", mode = (AnimatorConditionMode)6, threshold = 1.0f }, new AnimatorCondition { parameter = @"SitStand", mode = (AnimatorConditionMode)6, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (AnimatorState)objects[8527324074706241840L];
                o.name = @"Hard Landing";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"6b72ff6fb01b52d42af614751aa23f73", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-4633391771650121302L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[8570312493231049636L];
                o.name = @"Disable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-2998249581155981522L], (AnimatorStateTransition)objects[-1422814277246122385L], (AnimatorStateTransition)objects[-388284037969056490L], (AnimatorStateTransition)objects[-2675132093838501883L], (AnimatorStateTransition)objects[-2141583219888525641L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorStateMachine)objects[8571016880964741915L];
                o.name = @"USSPS_GroupSwitch";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[8183258488441076315L], position = new Vector3(700.0f, 360.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-6566766572468628972L], position = new Vector3(300.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[2304600037645116604L], position = new Vector3(700.0f, 180.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-2638163585814864993L], position = new Vector3(700.0f, 240.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[3379010938111213117L], position = new Vector3(700.0f, 300.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[6942399150830552079L], position = new Vector3(700.0f, 60.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[-3712177435076253093L], position = new Vector3(700.0f, 120.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[8260197079921956935L], position = new Vector3(700.0f, 0.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[-6566766572468628972L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 0.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 100.0f, 0.0f);
                o.exitPosition = new Vector3(1120.0f, 0.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateMachine)objects[8576868798655692200L];
                o.name = @"USSPS_LocomotionType";
                o.states = new ChildAnimatorState[] { new ChildAnimatorState { state = (AnimatorState)objects[6265879694267861960L], position = new Vector3(500.0f, 0.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[2105239521671056306L], position = new Vector3(500.0f, 60.0f, 0.0f) }, new ChildAnimatorState { state = (AnimatorState)objects[7523425782058141989L], position = new Vector3(500.0f, 120.0f, 0.0f) } };
                o.stateMachines = new ChildAnimatorStateMachine[] {  };
                o.defaultState = (AnimatorState)objects[6265879694267861960L];
                o.anyStateTransitions = new AnimatorStateTransition[] {  };
                o.entryTransitions = new AnimatorTransition[] { (AnimatorTransition)objects[8266720753606980800L], (AnimatorTransition)objects[-4892243885510418539L], (AnimatorTransition)objects[-7770581767140748123L] };
                o.behaviours = new StateMachineBehaviour[] {  };
                o.anyStatePosition = new Vector3(50.0f, 20.0f, 0.0f);
                o.entryPosition = new Vector3(50.0f, 120.0f, 0.0f);
                o.exitPosition = new Vector3(900.0f, 100.0f, 0.0f);
                o.parentStateMachinePosition = new Vector3(800.0f, 20.0f, 0.0f);
            }
            {
                var o = (AnimatorStateTransition)objects[8601217045347607153L];
                o.name = @"";
                o.destinationState = (AnimatorState)objects[-6566766572468628972L];
                o.destinationStateMachine = null;
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"USSPS_IsShallow", mode = (AnimatorConditionMode)4, threshold = 0.0f } };
                o.duration = 0.0f;
                o.offset = 0.0f;
                o.exitTime = 0.0f;
                o.hasExitTime = false;
                o.hasFixedDuration = true;
                o.orderedInterruption = true;
                o.canTransitionToSelf = true;
                o.interruptionSource = (TransitionInterruptionSource)2;
            }
            {
                var o = (VRCAvatarParameterDriver)objects[8658828139118200110L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"LocomotionType", source = @"", value = 11.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorTransition)objects[8675687255005010764L];
                o.name = @"";
                o.destinationState = null;
                o.destinationStateMachine = (AnimatorStateMachine)objects[3767330295973062343L];
                o.isExit = false;
                o.solo = false;
                o.mute = false;
                o.conditions = new AnimatorCondition[] { new AnimatorCondition { parameter = @"VelocityX", mode = (AnimatorConditionMode)3, threshold = 0.1f } };
            }
            {
                var o = (VRCAvatarParameterDriver)objects[8819143897568466539L];
                o.name = @"";
                o.debugString = @"";
                o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingHeadReset", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f }, new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { type = (VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)0, name = @"TrackingHandReset", source = @"", value = 1.0f, valueMin = 0.0f, valueMax = 0.0f, chance = 0.0f, convertRange = false, sourceMin = 0.0f, sourceMax = 0.0f, destMin = 0.0f, destMax = 0.0f } };
                o.localOnly = true;
            }
            {
                var o = (AnimatorState)objects[9009721933184062462L];
                o.name = @"SitDeepFix";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"FootHeight";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[-1000496266171292664L], (AnimatorStateTransition)objects[-6323350822355403767L], (AnimatorStateTransition)objects[-1744727940728115676L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[-8860047223606620120L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[9088504416208506103L];
                o.name = @"USSPS_SleepThreshold";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = true;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)objects[-1651988166975395410L];
                o.transitions = new AnimatorStateTransition[] {  };
                o.behaviours = new StateMachineBehaviour[] {  };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
            {
                var o = (AnimatorState)objects[9149627423983647829L];
                o.name = @"HandTrackingDisable";
                o.speed = 1.0f;
                o.cycleOffset = 0.0f;
                o.mirror = false;
                o.iKOnFeet = false;
                o.writeDefaultValues = false;
                o.speedParameterActive = false;
                o.mirrorParameterActive = false;
                o.cycleOffsetParameterActive = false;
                o.timeParameterActive = false;
                o.tag = @"";
                o.speedParameter = @"";
                o.mirrorParameter = @"";
                o.cycleOffsetParameter = @"";
                o.timeParameter = @"";
                o.motion = (Motion)external(@"52aff6f8eb70ac94ca2d631a92b39a80", 7400000L);
                o.transitions = new AnimatorStateTransition[] { (AnimatorStateTransition)objects[129464835733613725L], (AnimatorStateTransition)objects[1593007504243141818L] };
                o.behaviours = new StateMachineBehaviour[] { (StateMachineBehaviour)objects[7932453726705980427L] };
                using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); if (legacy != null) { legacy.vector3Value = new Vector3(50.0f, 50.0f, 0.0f); serialized.ApplyModifiedPropertiesWithoutUndo(); } }
            }
        }
    }
}
#endif
