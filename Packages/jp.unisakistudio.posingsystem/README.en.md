# Unisaki Posing System

The shared posing system used by Kawaii Posing Tool, Cool Posing Tool and related products. It uses Modular Avatar and NDMF to generate the required menus and animations when you upload an avatar.

This README covers v3.2.0, currently in development. A release date has not been set. Check the installed version at the top of the posing tool's Inspector. See also [new features and changelog](Documentation~/Localization/en/CHANGELOG.md).

## Display language and offline documentation

Choose a display language with Language / 言語 at the top of the Inspector. One selection controls both the Inspector and generated VRChat menu. Re-upload the avatar to change its in-game language.

Without a saved choice, the tool follows the computer's language and uses English for unsupported languages. This works on Windows, macOS and Linux. The last selection is saved in EditorPrefs and reused in other projects belonging to the same OS user on this computer. An explicit selection already saved on an avatar is preserved. Preferences do not sync to other computers. License storage is unchanged.

Choose a README, changelog or manual from the bundled [language index](Documentation~/Localization/index.html). The Inspector's Manual and View changelog also follow the selected language. Original agreements and reference translations remain separately available.

## Switching posture in desktop mode

The delay before the selected pose takes effect when switching between standing, crouching and sitting in desktop mode has been reduced. This applies to normal builds and prebuilds, with no special command or extra setting. Existing behavior is retained in VR mode and special states. Update older prebuilds before uploading.

## Installation and updates

Follow the purchased product's installation instructions and add the posing tool using VCC or ALCOM. This system is installed as a dependency of the posing tool.

Requirements for v3.2.0:

- A Unity 2022.3 VRChat avatar project
- VRChat Avatars SDK 3.10.4 or later
- Modular Avatar 1.x, version 1.15.1 or later
- NDMF (Non-Destructive Modular Framework) 1.x, version 1.14.0 or later

For Kawaii Posing Tool, update the product to 3.2.0 in ALCOM / VCC and check that Unisaki Posing System is also 3.2.0. If you use prebuilds, click Update prebuild and save the scene before uploading the avatar.

Cool Posing Tool 1.2.0 uses shared system 3.2.0. Update the product and shared system together to compatible versions.

### If you used the 8bit edition

New installations now use the regular Prefab. Turn off unneeded items under Features to reduce parameter usage. Select Pose alone uses a base of 8 bit.

The old 8bit Prefabs remain in each product's Editor/Legacy folder to preserve references from existing avatars and presets. You can keep using an existing 8bit edition without deleting or replacing its Prefab. The Unity menu no longer offers the 8bit edition for new installations.

## Understanding the settings

In the Hierarchy, select the tool object inside the avatar, such as 可愛いポーズ or カッコいいポーズ, to configure it.

| Tab | Settings |
| --- | --- |
| Features | In-game menu features, convenience features, icon images and ExpressionParameters usage |
| Poses | Select, add and reorder poses; preview in the Scene; pose details and animation adjustment |
| Avatar Setup | Body-specific correction, knee correction, original/custom motion imports and presets |

For items with detailed settings, click Open settings to expand the controls below. Breathing settings are also under Features.

Use Help / Changelog at the top to open the manual, support or changelog. Switch between the product and Unisaki Posing System when reading the changelog.

## Features and ExpressionParameters usage

Items enabled under Features > Menu features appear in the in-game menu. For example, enabling Mirror adds a control to switch mirroring in VRChat. Checking it in the Inspector does not make every pose permanently mirrored.

| Feature | Additional bit usage |
| --- | ---: |
| Mirror | 1 bit |
| Disable Gestures | 1 bit |
| Animate breathing | 0 bit |
| Breathing (in-game ON/OFF) | 1 bit |
| Foot Height | 8 bit |
| Floor Height | 8 bit |
| Photo Mode (local display only) | 0 bit |
| Sync Photo Mode | 16 bit |
| Keep your head visible in Photo Mode | 0 bit |
| Disable VRChat idle sway | 0 bit |
| Reduce pose distortion from head rotation | 1 bit (shared with Roll Over) |
| Roll Over | 10 bit (including 1 bit shared with pose-distortion reduction) |

Roll Over uses 1 bit for ON/OFF, 8 bit for head tilt and 1 bit for head lock. If pose-distortion reduction is already enabled, adding Roll Over costs an additional 9 bit. Shared parameters are counted once; the display updates as you change the checkboxes.

The displayed total covers the posing tools installed on that avatar. It is not the avatar's overall total including clothing and other gimmicks. Shared settings are counted once even when multiple posing tools are installed. If the pose count exceeds the basic index range, pose selection can also require additional bits.

### Foot Height and floor height

- Foot Height adjusts the feet side while keeping the head position. Use it to align the feet with a floor or bed.
- Floor Height moves the whole body, including the head, vertically. 50% is the original height, 0% is 3m below and 100% is 3m above. The default is 50%.

Both are under Settings in the in-game menu; Floor Height follows Foot Height. Move Floor Height away from the center to move vertically. Return near 50% to restore the original height and end the correction. There is no separate floating ON/OFF switch. Viewpoint correction occurs when floating starts and while adjusting floor height.

For regular animations, Foot Height no longer duplicates whole-body motion data to adjust height. This reduces generated animation data; it does not reduce every avatar's total size to one third.

### Photo Mode

1. Enable Features > Menu features > Photo Mode.
2. In VRChat, open Photo Mode and select a pose under Select Pose.
3. Use Distance to move up to 3m forward and Direction to turn the avatar. At distances above 0, the avatar faces you.
4. Enable Lock Feet if needed. It is not forced on at startup, so you can position the avatar before locking. If Disable Gestures is included in the menu, you can also change hand shapes on the same screen.
5. Exit Photo Mode resets distance and rotation and releases foot lock.

Photo Mode movement and rotation use 0 bit when visible only on your screen. To show the position and rotation to others, enable Convenience > Sync Photo Mode. Distance and rotation together use 16 bit.

If your avatar's head disappears when it is nearby, enable Convenience > Keep your head visible in Photo Mode. This affects only your local display during Photo Mode and returns to normal when it ends.

### Switch lying posture by jumping

When both back-lying and front-lying poses are selected, jump in place to switch between them. If only one is selected, the avatar keeps that posture. To disable jump switching, turn off Switch lying posture by jumping under Features > Convenience in Unity. This adds 0 bit of synchronized parameter usage. If the avatar has multiple posing tools, disabling it in any one applies to the whole avatar.

### Roll Over

In supported lying poses, tilting your head sideways smoothly rolls the body. Not every lying pose supports this automatically.

1. Enable Features > Menu features > Roll Over.
2. Under Poses, open Details for the lying pose and enable Enable roll over.
3. Assign a side-lying animation to Sideways roll-over pose. The original and side-lying poses are used to generate rolls in both directions. Adjust Ground correction strength if needed.
4. In VRChat, select a supported pose and enable Roll Over. Release head lock before using it.

When Roll Over is enabled for a pose, head tilt determines the animation's playback position, so normal MotionTime settings are unavailable. To use a complete roll-over animation, assign it to the regular animation field and leave Sideways roll-over pose empty. Prepare an animation with the left side at playback position 0, back-lying at 0.5 and the right side at 1.

### Breathing and default idle sway

Features > Convenience > Animate breathing adds breathing motion to the chest and other parts. Open settings lets you choose None, Weak, Medium, Strong, Rapid or Custom for each posture category. Rapid uses faster breathing. Custom lets you assign your own breathing animation.

Breathing animation alone uses 0 bit. To stop or resume it in VRChat, also enable Menu features > Breathing. Synchronizing ON/OFF uses 1 bit. Changing the intensity does not increase bit usage.

The neck, shoulders and arms move with the chest to reduce head and hand displacement. Their positions are not perfectly fixed, so choose an intensity suited to the body shape and pose.

Convenience > Disable VRChat idle sway reduces the idle body sway applied by VRChat. It defaults to off and preserves the posing tool's breathing and other custom motions. It can be enabled without breathing and uses no additional bits. It does not stop movement caused by something other than the default idle animation.

With multiple posing tools, enabling idle-sway suppression in any one affects the whole avatar. To restore it, turn this setting off in every tool and re-upload.

### Reduce pose distortion from head rotation

Use this if releasing head lock distorts a pose because the neck orientation differs from the original pose. It is under Features > Convenience and does not add an in-game menu entry. While head lock is active, the original pose's orientation is used. Back-lying poses are excluded to preserve their original upward-facing orientation.

### Menu icon images

- Generate pose icons creates images of the avatar in each pose for the menu. This makes poses easier to recognize but adds image-generation time to the build.
- Reduce icon size lowers the image resolution to reduce avatar size.

Pose icons are not generated for Android builds. Use Retake icons after changing clothing or appearance. Use Update pose thumbnails to refresh the pose-list images. These operations take time; read the confirmation dialog before proceeding.

## Adding, reordering and previewing poses

Choose a posture category on the left of Poses. The magnifying-glass tab at the top searches pose names across categories. Details groups the animation, icon, MotionTime and other settings.

To add multiple animations, drag them into the add area, or select them in the Project window and click Load selected clips from Project. Duplicate clips are excluded.

To reorder a pose, choose its destination under Details > Order. The up/down buttons are also available. Moving a pose preserves whether its details are expanded.

Click Preview for a pose to inspect it in the Scene without opening the adjustment window. Ending the preview restores the original display. This quick preview does not run MA or NDMF builds, so it can differ from the final clothing or FloorAdjuster result assembled during a build. Check the final clothing and ground contact after building the avatar as well.

## Animation adjustment and automatic limb adjustment (IK)

Open Animation adjustment under Poses to adjust the body and limbs while previewing. Save the adjustment animation using the window's save controls when finished.

Supported products and poses use automatic limb adjustment to fit limb positions to the avatar's body. Use Enable automatic limb adjustment in the animation adjustment window for the overall setting and Enable IK for this pose for individual poses. Poses without automatic adjustment are identified as such.

To finish the automatic result by hand, click Bake IK into adjustments. This writes the current IK result to a new adjustment animation and disables IK for that pose on that avatar. The original animation is preserved. The button is available when overall and per-pose automatic adjustment are enabled and usable IK settings exist.

Under Avatar Setup, Prevent reverse knee bending toggles knee correction. Set Minimum knee bend (degrees) in its details. Enable avatar correction is available when compatible correction data exists. Corrections that overlap with automatic limb adjustment may not be usable together.

## Original poses, custom motions and presets

Configure these under Avatar Setup.

- Use original standing, crouching and prone poses automatically imports the poses originally assigned to the avatar.
- Use your own motions opens import settings to assign a motion separately for each type, such as standing, Crouching or Prone. Normally, assign one motion per type.
- Integrate tracking locks with other tools can be enabled if the locking features do not work correctly alongside other gimmicks.

Manually assigning motions in the import settings is separate from automatically importing the avatar's original poses.

Presets are at the bottom of the tab. Search by name and click Apply this preset to replace the pose settings. Presets are primarily a starting configuration for installation. They replace current settings, so save any existing adjustments before switching.

Displaying the preset list loads only names and supported-avatar information. Adjustment animations are loaded when the selected preset is applied. Even the first listing does not preload animations for every avatar.

The list is grouped into Compatible with avatar, User-created and Unisaki Studio official. Hide bundled presets is a shared display preference within the project. Adding, editing or deleting presets refreshes the list. Click Reload if the display has not updated.

## Prebuilds and warnings

With Use original standing, crouching and prone poses enabled, motions are detected from the avatar's Base each time data is generated. Detection does not write back to the manual import settings, so automatically imported types remain unassigned in that list. Changes to Base motions or automatic-import ON/OFF take effect at the next generation.

Automatic imports at upload time apply to the build copy. They do not modify the original scene object, import settings, source Controller or motions. An existing prebuild is reused when its inputs have not changed.

A type with a manual assignment uses that setting, including rotation, adjustment animation and playback-position settings. Turning Use off or removing an assigned motion stops automatic import for that type. Clear manual selection removes that type's settings and adjustments; if automatic import is enabled, the next generation detects it again. This operation supports Undo. If multiple legacy settings exist for one type, the valid setting selected in the Inspector takes precedence.

Settings added to the list by older automatic-import versions cannot be distinguished from manually registered or adjusted settings and are not automatically deleted. Use Clear manual selection only for types you want to return to automatic import. With multiple products on an avatar, manual settings in the other products also affect the shared original poses.

Prebuilds reuse generated data to shorten avatar builds. Create or refresh them with Update prebuild. Normal avatar builds also generate the necessary data, so you do not need to prebuild manually every time.

Changing settings marks the prebuild as needing an update. Check settings at the top of the Inspector and the Hierarchy markers notify you. Refreshing the prebuild after changing poses or IK settings reduces subsequent build time.

Warning display settings are inside Check settings. You can hide individual warning types or Hierarchy markers. Hiding a warning does not fix its cause or refresh generated data. Errors and required generation still apply.

## Usage notes

- Intended for three-point tracking and desktop use. Pose locking during full-body tracking is unsupported.
- Body shape and avatar modifications may require further adjustments to limb contact or clothing.
- Special Base layers or other pose-changing gimmicks may prevent the intended behavior.
- Photo Mode movement/rotation and Floor Height correct tool poses and the default/custom motions used when no tool pose is selected. Check the actual VRChat client for replaced default motions and viewpoint behavior.

## Terms of use and support

The source code and resources in this repository are not open source. Use requires a legitimate product purchase and agreement to the [bundled terms of use](%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Kawaii Posing Tool manual](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Cool Posing Tool manual](https://docs.unisakistudio.jp/coolposing/manual/)
- [Support](https://help.unisakistudio.jp/)

When reporting an issue, include the product and Unisaki Posing System versions; Unity, VRChat SDK, Modular Avatar and NDMF versions; avatar used; reproduction steps; and Console error details.

🌐 [日本語 · English · Français · Italiano · 简体中文 · 繁體中文 · 한국어 · ไทย · Deutsch · Español · Português · Português (Brasil) · Русский · Українська · Polski · Nederlands · Svenska · Dansk · Suomi · Norsk · Čeština · Türkçe · Bahasa Indonesia · Tiếng Việt · हिन्दी · বাংলা · தமிழ் · తెలుగు · العربية · עברית](Documentation~/Localization/index.md)
