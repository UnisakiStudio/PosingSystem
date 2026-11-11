# Changelog

## [3.2.0] - Unreleased

### Added

- Added thirty-language display for the Inspector, editing actions, installation dialogs, standard pose names and generated VRChat menus.
- Unified language selection for the Inspector and in-game menu. The default follows the computer's language, and the last choice is reused in other projects belonging to the same OS user.
- README, changelog and offline manuals can be opened in the selected language.
- Prepared display names for all 128 standard poses and feature descriptions using the established terminology.

### Compatibility

- Existing avatar settings, custom names, asset references, parameter identifiers and license storage are preserved.

## [3.1.4] - 2026-11-11

- Fixed avatar-specific crouching and prone animations being ignored and replaced by the default VRChat poses. Both automatic import and manually assigned animations are covered.
- Fixed hair, glasses and other accessories attached with MA Bone Proxy staying in their original positions while editing or previewing poses. They now follow the preview pose while preserving the original avatar settings and placement.
- Fixed pose correction using the viewpoint position failing when capturing pose thumbnails such as 仰向け寝_お疲れ. Thumbnails now use animations with the pose correction and floor adjustments applied during the build.

Update Unisaki Posing System to 3.1.4 in ALCOM / VCC. Kawaii Posing Tool 3.1.2 and Cool Posing Tool 1.1.0 can continue to be used as they are. If you use a prebuild, click Update prebuild after updating, save the scene, then build and upload the avatar again.

## [3.1.3] - 2026-11-09

- Running Update pose thumbnails or a prebuild on a duplicated avatar no longer deletes the original avatar’s pose thumbnails.
- Fixed Settings changed appearing after only recapturing thumbnails when the prebuild was created before the scene was first saved or while editing a Prefab.
- Fixed a subsequent prebuild sometimes stopping partway through when pose data from an earlier prebuild remained.
- Reduced the work needed to open the pose list when thumbnails do not yet exist. Also revised the process that blocked Unity while waiting for package information at the start of a prebuild or build.
- The pose tool’s editing components are now also removed from PC and Android builds when the tool is disabled within the avatar.
- Ask about this error can now include diagnostic codes from later Console entries in the draft even when the first errors do not identify the cause. For errors involving Roll Over clip settings, the draft also includes what to check.

Update Unisaki Posing System to 3.1.3 in ALCOM / VCC. Kawaii Posing Tool 3.1.2 and Cool Posing Tool 1.1.0 can continue to be used as they are. Because these products specify an earlier shared system version, you may see a conflict confirmation when updating. If you use a prebuild, click Update prebuild after updating, save the scene, then build and upload the avatar again.

## [3.1.2] - 2026-11-09

- Fixed the face remaining tilted upward after getting up from a prone pose when Reduce pose distortion from head rotation was enabled.
- Head lock can now be toggled under Settings > Automatic head lock when supine in the EX menu even when Foot Height is disabled. Avatars where the item disappeared in 3.1.0 or 3.1.1 regain it when built after updating.
- Update prebuild now works even when the avatar itself is unchecked in Unity.
- Fixed pose generation sometimes failing after an update if an older shared posing Controller remained in Assets. User-edited Controller settings are also carried over.
- When the tool or a parent is disabled and the shared posing Animator cannot be used, the affected object's name is shown. Open it with Check settings > Select target.
- When a prebuild shows Settings changed, use Details > Review changes since saving to see which settings differ from the previous build.
- If script errors stop the build (PS050), up to 5 original Unity errors are shown. Ask about this error opens a draft containing Unity/tool versions and the error code for you to review, edit and send.

If you use prebuilds, click Update prebuild after updating, save the scene, then build and upload the avatar again.

## [3.1.1] - 2026-11-08

- Enabled Floor Height and Roll Over entries now appear under Settings in the VRChat EX menu even when Mirror is disabled.
- Correct roll-over settings are carried over for Legs Out (足伸ばし), Relaxed (リラックス) and One Knee Up (膝立て) even if an older version's poses were reordered, deleted or renamed. Fully unpacked objects are supported when the original pose can be identified. Custom poses, explicitly disabled roll-over settings and replacement side-lying motions are preserved.
- Fixed a brief prone pose when selecting an avatar in VRChat. Also fixed a brief supine pose when jumping with only a prone pose selected. Jump switching can be disabled under Features > Convenience > Switch lying posture by jumping; it defaults to on.
- If an update breaks a reference to the shared posing Animator Controller, a warning is shown and the Controller is rebuilt from settings when recovery is possible. References to Controllers shared by multiple avatars are preserved.
- Projects with compilation errors now stop at the start of the build with guidance about the cause. Automatic cleanup of generated Controllers is deferred while script errors prevent checking references.
- Adding a tool prioritizes its official Prefab even if another Prefab with the same product name exists in the project. Prebuilds can continue when shared Controllers differ only in line endings.
- Write Defaults on FX layers added by the posing tool, including Floor Height, now follows the avatar's settings.

If you use prebuilds, click Update prebuild on the scene's tool after updating, save the scene, then build and upload the avatar again.

## [3.1.0] - 2026-11-07

- Added automatic limb adjustment (IK) for supported poses, avatar-specific correction and prevention of backward knee bending.
- Photo Mode adjusts avatar position and rotation, with optional synchronization and visibility of your own head during photography.
- Floor Height moves the entire body vertically by up to 3m and can be used separately from Foot Height.
- Added breathing motion, suppression of default idle sway and reduction of pose distortion caused by neck orientation.
- The Inspector shows enabled features and parameter usage. Added pose search, batch addition, reordering and quick Scene previews.
- Sped up switching between standing, crouching and sitting in desktop mode.
- Improved prebuild warnings after settings changes, Inspector scrolling overhead and long stalls during generated-data cleanup.
- Fixed unchanged prebuilds being flagged for an update and regenerated on upload when automatic import was enabled. Imported results apply to the generated avatar without changing the original object's import settings.
- Added head-tilt roll over and per-pose motion settings.
- Improved migration of old preset roll-over settings, custom motion imports and generated-asset management.

## [3.1.0-beta.2] - Open beta (2026-11-05)

- Added fixed length and the Head-to-Viewpoint vertical (Y) and front/back (Z) differences as IK position-length references.
- Inspector warnings also appear to the left of object names in the Hierarchy, covering missing/stale prebuilds and current motion-import or preset-operation warnings.
- Fixed Unity stalling for a long time when automatic generated-data cleanup on scene opening loaded generated Controllers from other scenes.
- Fixed prebuild history disappearing after changing poses or features. The display now says settings have changed since the prebuild rather than Not run, with a matching Hierarchy update warning.
- Shared prebuild comparison results between the Inspector and Hierarchy to reduce scrolling overhead while settings and referenced assets are unchanged. Builds still compare every time.

## [3.1.0-beta.1] - Open beta (2026-11-04)

- Changed normal desktop posture switching to a single-evaluation approach for regular builds and prebuilds. VR and special conditions retain their existing paths.
- Prebuilds from alpha.3 or earlier are recognized as needing an update.
- Preserved the viewpoint when changing floor height during photography, then restored the latest floor height when photography ends.

- Automatic import now detects motions only during generation, without adding or modifying manual import settings in prebuilds/builds.
- Automatic-import ON/OFF, Base changes and removal of manual overrides now trigger regeneration. Shared manual assignments/adjustments across products are preserved, and obsolete imported motions return to the default posture.
- Generated prebuild Controllers are managed in one file per avatar and scene GUID. Shared references from duplicates, other scenes or Prefabs are separated before updating.
- Builds use NDMF's temporary asset management and do not add editable generated files.
- Generated data is checked when scenes open or are saved. Unsaved edits, Undo/Redo data and external references are retained; only clearly owned unused files are moved to the recycle bin.
- Imported motion rotation, adjustment animations and playback positions are applied. Rotation, RootT/RootQ and Humanoid adjustments use the same processing as regular poses.
- Nested BlendTree adjustments are copied for the import destination, preserving the original motion and walking blend settings.
- Standing-only playback positions are expanded into static poses at the original frame rate without stopping playback on the walking side.

## [3.1.0-alpha.2] - Closed test (2026-10-26)

### Fixed and changed

- Photo Mode and Floor Height correction also apply to default/custom motions when no tool pose is selected.
- Expanded Floor Height to 3m up/down. 50% is now the original height and default, 0% is 3m below and 100% is 3m above.
- Fixed Standing-only overrides modifying the source BlendTree.
- Poses without IK rules use manual correction and no longer show unnecessary generation warnings.
- Fixed adjustment drafts being confused between identically named poses and motion changes inside the same asset being missed.
- Fixed an invalid initial-pose index selecting another pose.
- Unsupported AnimatorOverrideController imports now show guidance instead of throwing an exception.
- Applying old presets preserves explicitly disabled roll-over settings.
- Improved feature icon visibility in the light Editor theme.

VRChat client checks of viewpoint behavior and replaced default motions remain in progress.

## [3.1.0-alpha.1] - Before publication

### Added

- Added per-feature ON/OFF and ExpressionParameters usage display.
- Added Photo Mode to move the avatar forward and rotate it.
- Added optional Photo Mode synchronization: 0 bit locally, 16 bit synchronized.
- Added an option to keep your avatar's head visible during Photo Mode.
- Added Floor Height to move the whole body, including the head, up/down by 3m. 50% is the original height and default, 0% is 3m below and 100% is 3m above.
- Added head-tilt roll over and per-pose animation settings.
- Added breathing motion with Weak, Medium, Strong, Rapid and Custom settings.
- Breathing motion and its in-game ON/OFF menu can be selected separately: 0 bit for motion alone, 1 bit for synchronized ON/OFF.
- Added suppression of VRChat's default idle sway.
- Added reduction of pose distortion caused by neck-orientation differences.
- Added automatic limb adjustment (IK) for supported poses, with overall and per-pose ON/OFF.
- Added Bake IK into adjustments to save IK results to an adjustment animation.
- Added avatar-specific pose correction, prevention of backward knee bending and minimum bend angle settings.
- Added quick pose previews in the Scene from the pose list.
- Added category tabs, cross-category search, batch animation addition and destination-order selection.
- Added product and shared-system changelogs within the Inspector.

### Changed

- Sped up preset loading to shorten waits when adding tools or displaying presets.
- Adding, editing or deleting presets automatically updates the list.
- Organized the Inspector into Features, Poses and Avatar Setup tabs.
- Added setting descriptions and icons and standardized expandable details.
- Moved breathing details into Features and presets to the bottom of Avatar Setup.
- Separated manual custom-motion assignments from automatic original-pose imports.
- Reduced duplicated whole-body animation data generated for foot-height adjustment.
- Pose image updates now use a button with a confirmation dialog.
- Grouped warning display settings inside Check settings.
- Removed new installation of dedicated 8bit Prefabs in favor of feature selection on regular Prefabs. Existing references are preserved.
- Raised the minimum VRChat Avatars SDK version to 3.10.4.

### Fixed

- Photo Mode and Floor Height correction now also cover default/custom motions when no tool pose is selected.
- Fixed Standing-only overrides modifying the source BlendTree.
- Poses without valid IK rules use manual adjustments and avoid unnecessary IK generation warnings.
- Fixed adjustments mixing between identically named poses and replacements of motions within one asset not triggering regeneration.
- A disabled initial-pose index no longer selects another pose if reused.
- Unsupported AnimatorOverrideController imports of original poses now show guidance instead of an exception.
- Applying old presets preserves explicitly disabled roll-over settings.
- Improved Inspector icon backgrounds so feature icons remain identifiable in the light Editor theme.

- Fixed missing Roll Over, Floor Height and Photo Mode icons when adding a tool to an avatar.
- Fixed loss of supported poses' roll-over settings when applying old presets.
- Fixed name editing moving to another pose when opening different details.
- Fixed pose details closing after reordering and corrected settings Undo/Redo handling.
- Fixed previews overlapping the original avatar or adding placement offsets twice.
- Fixed preview errors on avatars with missing components.
- Fixed IK limb orientation/position offsets caused by avatar bone orientation or scale differences.
- Fixed warnings and prebuild status not updating immediately after settings changes.
- Fixed reuse of stale prebuilds after changing IK settings.
- Fixed build errors when generating additional features in some configurations.

## [3.0.16] - 2026-09-14
- Raised the minimum NDMF dependency to 1.14.0 because the system uses the `VirtualAnimatorController.SetParameter` API.

## [3.0.15] - 2026-09-13
- Fixed large changes to body position and knee bending when toggling supine head lock on avatars using both Hips scaling and FloorAdjuster.

## [3.0.14] - 2026-09-09
- Fixed the Disable Gestures entry disappearing for the second and subsequent posing tools on the same avatar.
- Fixed USSPS_Locomotion sometimes returning to Standing even when a posture such as SleepDown was selected.
- Fixed pose height and X/Z offsets when using FloorAdjuster, accounting for processing order in both the MA and AAO editions.
- Generated prebuilt AnimatorControllers that have been deleted now recover automatically to the standard PosingSystem_Locomotion.
- Fixed build errors when every standing pose had been removed.
- Presets are now listed in avatar-name order.

## [3.0.13] - 2026-09-04
- Fixed unstable posture recovery caused by a standing self-transition when returning to standing immediately after beginning to crouch in VR mode.

## [3.0.12] - 2026-09-04
- Fixed tools without a prebuild being incorrectly marked as having changed settings during an NDMF build.
- Fixed `AddAssetToSameFile` failures caused by adding generated assets to a temporary AnimatorController during an NDMF build.

## [3.0.11] - 2026-08-31
- Fixed processing failures caused by the prebuild avatar's Scene becoming invalid in NDMF 1.14.7 and later.
- Fixed `MissingReferenceException` when the NDMF error display referenced a destroyed LocalizationAsset.

## [3.0.10] - 2026-08-29
- Preset definition assets are now loaded only when the preset selector opens, with cached data reused while the GUID inventory is unchanged.
- Hierarchy warning/error icons are reevaluated only after changes, and product-menu asset searches share a cache. This reduces Editor freezes occurring every few seconds in scenes with many posing tools.
- Fixed `ArgumentException` while registering duplicate parameter values when prebuilding multiple posing tools on the same avatar.

## [3.0.9] - 2026-08-26
- Prevented NullReferenceException when a VRCAvatarDescriptor cannot be obtained from PosingSystem during conversion, and added guidance about the build logs needed for investigation.
- Fixed subsequent prebuilds/builds failing with `Avatar is null` when they reused a preview referencing an NDMF-generated and deleted temporary Avatar. Conversion is separated from the preview cache, and NDMF temporary asset-name collisions between previews are prevented.
- Fixed hidden preview avatars accumulating because the 可愛いポーズ Prefab's preview reference was not saved in the scene. Previews are excluded from scene saving, and old previews are collected on scene load, entering Play mode and script reload.

## [3.0.8] - 2026-08-18
- Fixed Unity Assertions and potentially invalid pose data when FloorAdjuster or similar tools reapplied height correction to prebuilt posture motions.
- Improved height measurement so it does not interfere with the original avatar or another tool's AnimationMode state.

## [3.0.7] - 2026-08-12
- Fixed displaced viewpoints and abnormal supine knee bending when combined with height-adjustment tools such as FloorAdjuster.
- Fixed errors when PosingSystem was outside an avatar (thanks to PancakeTorto1se).
- Fixed errors from continuing processing after an avatar upload failed.
- Fixed settings sometimes being incorrectly marked as changed immediately after prebuilding.
- Improved automatic locomotion import.
- Added warnings for avatars with certain unusual rigs.
- Added an unsaved-changes confirmation and adjustment-value mirroring to the animation adjustment window (thanks to kraihd).
- Added guidance for errors in other tools and checks for common failure points.

## [3.0.6] - 2026-03-17
- Fixed severely distorted poses on some avatars with their own original posture controls.

## [3.0.5] - 2026-01-31
- Fixed Unity crashes with some ExpressionMenus.
- Fixed avatar problems when using the ごろ寝EX integration Prefab (thanks to あずき).
- Fixed incorrect rotation when composing adjustment animations.

## [3.0.4] - 2026-01-11
- Improved some behavior when avatars are placed in Prefabs.
- Added license removal.
- Fixed incorrect neck behavior in many poses.
- Slightly reduced the overhead of opening the Inspector.
- Prebuilds now also create icons; changed the avatar-update button text.
- Warnings are hidden if a prebuild has never been run, as many users may not use prebuilds.
- Removed unnecessary resources.
- Fixed fingers being impossible to adjust.

## [3.0.3] - 2026-01-04
- Fixed a different preset being applied from the one selected (thanks to あずき).

## [3.0.2] - 2026-01-02
- Added support for Unity's dark theme.
- Fixed pose distortion when rotating in animation adjustment.
- Changed generated menu icon sizes, enabled compression and added an option for smaller icons.
- Restored tracking integration and automatic avatar-motion import, which had been removed in 3.0.0.

## [3.0.1] - 2025-12-29
- Fixed default-motion configuration.
- Fixed avatar poses becoming distorted unexpectedly.

## [3.0.0] - 2025-12-28
- Added prebuilds.
- Added animation adjustment.
- Added presets.

## [2.2.5] - 2025-05-08
- Resolved compatibility issues with some gimmicks.

## [2.2.4] - 2025-04-04
- Fixed poses no longer changing with some avatars and ModularAvatar versions.

## [2.2.3] - 2025-03-13
- Fixed incorrect motions on some avatars, including 森羅's jump.
- Fixed the initial Disable Gestures setting sometimes being incorrect.

## [2.2.2] - 2025-03-06
- Fixed errors in some environments.

## [2.2.1] - 2025-02-05
- Fixed Unity crashes.
- Added the version to the tool name shown in the NDMF Console.

## [2.2.0] - 2025-02-02
- Added a setting to disable gestures and use the animation's hand shapes.

## [2.1.6] - 2025-02-02
- Fixed poses breaking when combined with some tools.
- Fixed errors with certain settings.

## [2.1.5] - 2024-12-10
- Resolved conflicts with some gimmicks.
- Added a button to remove the Unitypackage edition.

## [2.1.4] - 2024-12-08
- Fixed errors with some avatars.
- Fixed foot lock becoming unavailable because of gimmick/avatar conflicts.
- Fixed avatar posture reverting to VRChat's default in desktop mode.

## [2.1.3] - 2024-12-01
- Fixed poses no longer changing when used with avatars or gimmicks that assign the wrong type to VRMode.
- Added NDMF-Console reporting for parameter-type conflicts between gimmicks.

## [2.1.2] - 2024-12-01
- Added NDMF-Console guidance because errors in other tools can cause Unisaki Posing System errors during avatar builds.
- Fixed build errors with some avatars.
- Improved behavior immediately after loading an avatar.
- The avatar's original locomotion processing is now removed because it prevents operation on some avatars.

## [2.1.1] - 2024-11-20
- Fixed MergeAnimator-related problems in PosingOverride.

## [2.1.0] - 2024-11-20
- Resolved feet and hips sinking into the ground in desktop mode so poses display correctly.
- Added posture changes independent of head height.
- Added an option to skip icon generation for Quest avatar uploads.
- Added several warnings for avatar issues.
- Resolved lock conflicts with AFK and other tools' arm-lock gimmicks.
- Added automatic head lock while supine.

## [2.0.0] - 2024-11-12
- Released the VPM/VCC-compatible version.
- Changed the tool display name shown for errors #1.
- Added terms of use #5.
