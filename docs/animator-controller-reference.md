# AnimatorController 現行参照付録

この付録は移行元の参照グラフの機械的な記録。正しい仕様という認定ではない。要件・課題・変更方針は [マスター仕様](animator-controller-master.md) を参照。

基準SHA-256: `462fa9b486999c6e53727dffa657bc24c80dff31fd4242c06efec92dc8bcd6c7`。更新元: `Tools/AnimatorController/export_specification.py`。

## パラメーター

| 名前 | 型 | 初期値 |
|---|---|---|
| IsLocal | bool | 0 |
| VelocityX | float | 0 |
| VelocityY | float | 0 |
| VelocityZ | float | 0 |
| Upright | float | 1 |
| Grounded | bool | 0 |
| InStation | bool | 0 |
| AFK | bool | 0 |
| TrackingType | int | 0 |
| VRMode | int | 0 |
| VRCEmote | int | 0 |
| TrackingHeadReset | bool | 0 |
| TrackingHandReset | bool | 0 |
| LocomotionTypeGroup | int | 0 |
| LocomotionType | int | 0 |
| SitStand | int | 0 |
| SitShallow | int | 0 |
| SitDeep | int | 0 |
| SitSleepUp | int | 0 |
| SitSleepDown | int | 0 |
| VirtualLoveType | int | 0 |
| LocomotionLock | bool | 0 |
| HeadTrackingLock | bool | 0 |
| HandTrackingLock | bool | 0 |
| FootHeight | float | 0.5 |
| KawaiiPosing_IsDown | bool | 0 |
| AlwaysZero | float | 0 |
| USSPS_ShallowThreshold | float | 0.8 |
| USSPS_DeepThreshold | float | 0.59 |
| USSPS_SleepThreshold | float | 0.34 |
| USSPS_IsShallow | float | -1 |
| USSPS_IsDeep | float | -1 |
| USSPS_IsSleep | float | -1 |
| USSPS_Pose | int | 0 |
| USSPS_Mirror | bool | 0 |
| USSPS_LastMenu | int | 0 |
| USSPS_TypeFix | bool | 0 |

## USSPS_ShallowThreshold

初期State: `Join`

### USSPS_ShallowThreshold

Motion: `Blend Tree`; Write Defaults=1; Foot IK=0。


### Join

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **USSPS_ShallowThreshold**: `(条件なし)`; duration=0, fixed=0, exitTime=1:0.01, interruption=0

## USSPS_DeepThreshold

初期State: `Join`

### USSPS_DeepThreshold

Motion: `Blend Tree`; Write Defaults=1; Foot IK=0。


### Join

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **USSPS_DeepThreshold**: `(条件なし)`; duration=0, fixed=0, exitTime=1:0.01, interruption=0

## USSPS_SleepThreshold

初期State: `Join`

### USSPS_SleepThreshold

Motion: `Blend Tree`; Write Defaults=1; Foot IK=0。


### Join

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **USSPS_SleepThreshold**: `(条件なし)`; duration=0, fixed=0, exitTime=1:0.01, interruption=0

## USSPS_GroupSwitch

初期State: `Entry`

### Standing

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionTypeGroup=0` (0)

1. → **Entry**: `USSPS_IsShallow > 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Entry**: `VirtualLoveType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **Entry**: `USSPS_TypeFix is true`; duration=0, fixed=1, exitTime=0:0, interruption=2

### Entry

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **LocomotionLock**: `LocomotionLock is true`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **VirtualLove**: `VirtualLoveType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **TypeFix**: `USSPS_TypeFix is true`; duration=0, fixed=1, exitTime=0:0, interruption=2
4. → **SitSleep**: `USSPS_IsSleep > 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
5. → **SitDeep**: `USSPS_IsDeep > 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
6. → **SitShallow**: `USSPS_IsShallow > 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
7. → **Standing**: `USSPS_IsShallow < 0 AND TrackingType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitSleep

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionTypeGroup=3` (0)

1. → **Entry**: `VirtualLoveType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Entry**: `USSPS_IsSleep < 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **Entry**: `USSPS_TypeFix is true`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitDeep

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionTypeGroup=2` (0)

1. → **Entry**: `USSPS_TypeFix is true`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Entry**: `USSPS_IsSleep > 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **Entry**: `USSPS_IsDeep < 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
4. → **Entry**: `USSPS_TypeFix is true`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitShallow

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionTypeGroup=1` (0)

1. → **Entry**: `VirtualLoveType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Entry**: `USSPS_IsDeep > 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **Entry**: `USSPS_IsShallow < 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
4. → **Entry**: `USSPS_TypeFix is true`; duration=0, fixed=1, exitTime=0:0, interruption=2

### VirtualLove

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionTypeGroup=4` (0)

1. → **Entry**: `VirtualLoveType == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### TypeFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionTypeGroup=5` (0)

1. → **Entry**: `VirtualLoveType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Entry**: `USSPS_TypeFix is false`; duration=0, fixed=1, exitTime=0:0, interruption=2

### LocomotionLock

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Entry**: `LocomotionLock is false`; duration=0, fixed=1, exitTime=0:0, interruption=2

## USSPS_Standing

初期State: `Standing`

### Standing

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=0` (0)

1. → **Disable**: `LocomotionTypeGroup != 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitStand**: `SitStand != 0`; duration=0, fixed=1, exitTime=0:0, interruption=0

### Disable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Standing**: `SitStand == 0 AND LocomotionTypeGroup == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitStand**: `SitStand != 0 AND LocomotionTypeGroup == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0

### SitStand

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=1` (0)

1. → **Disable**: `LocomotionTypeGroup != 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Standing**: `SitStand == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0

## USSPS_SitShallowSwitch

初期State: `Disable`

### Disable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **SitShallow**: `SitShallow != 0 AND LocomotionTypeGroup == 1`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **DefaultCrouching**: `SitShallow == 0 AND LocomotionTypeGroup == 1`; duration=0, fixed=1, exitTime=0:0, interruption=0

### DefaultCrouching

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=11` (0)

1. → **Disable**: `LocomotionTypeGroup != 1`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitShallow**: `SitShallow != 0`; duration=0, fixed=1, exitTime=0:0, interruption=0

### SitShallow

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=12` (0)

1. → **Disable**: `LocomotionTypeGroup != 1`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **DefaultCrouching**: `SitShallow == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0

## USSPS_SitDeepSwitch

初期State: `Disable`

### SitDeep

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=22` (0)

1. → **DefaultProne**: `SitDeep == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Disable**: `LocomotionTypeGroup != 2`; duration=0, fixed=1, exitTime=0:0, interruption=0

### DefaultProne

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=21` (0)

1. → **SitDeep**: `SitDeep != 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Disable**: `LocomotionTypeGroup != 2`; duration=0, fixed=1, exitTime=0:0, interruption=0

### Disable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **DefaultProne**: `SitDeep == 0 AND LocomotionTypeGroup == 2`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitDeep**: `SitDeep != 0 AND LocomotionTypeGroup == 2`; duration=0, fixed=1, exitTime=0:0, interruption=0

## USSPS_SitSleepSwitch

初期State: `Disable`

### SitSleepDown

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=33` (0)

1. → **DefaultProne**: `SitSleepDown == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Disable**: `LocomotionTypeGroup != 3`; duration=0, fixed=1, exitTime=0:0, interruption=0
3. → **SitSleepUp_Judge**: `Grounded is false AND SitSleepDown != 0 AND VelocityY > 1.99 AND VelocityZ < 0.01 AND VelocityZ > -0.01 AND VelocityX < 0.01 AND VelocityX > -0.01`; duration=0, fixed=1, exitTime=0:0, interruption=0

### DefaultProne

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=21` (0)

1. → **SitSleepDown**: `SitSleepDown != 0 AND SitSleepUp == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Disable**: `LocomotionTypeGroup != 3`; duration=0, fixed=1, exitTime=0:0, interruption=0
3. → **SitSleepUp**: `SitSleepUp != 0 AND SitSleepDown == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
4. → **SitSleepUp**: `SitSleepUp != 0 AND KawaiiPosing_IsDown is false`; duration=0, fixed=1, exitTime=0:0, interruption=0
5. → **SitSleepDown**: `SitSleepDown != 0 AND KawaiiPosing_IsDown is true`; duration=0, fixed=1, exitTime=0:0, interruption=0

### Disable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **DefaultProne**: `SitSleepUp == 0 AND SitSleepDown == 0 AND LocomotionTypeGroup == 3`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitSleepDown**: `SitSleepDown != 0 AND LocomotionTypeGroup == 3 AND KawaiiPosing_IsDown is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
3. → **SitSleepUp**: `SitSleepUp != 0 AND LocomotionTypeGroup == 3 AND SitSleepDown == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
4. → **SitSleepDown**: `SitSleepDown != 0 AND LocomotionTypeGroup == 3 AND SitSleepUp == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
5. → **SitSleepUp**: `SitSleepUp != 0 AND LocomotionTypeGroup == 3 AND KawaiiPosing_IsDown is false`; duration=0, fixed=1, exitTime=0:0, interruption=0

### SitSleepUp

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=32` (0)

1. → **DefaultProne**: `SitSleepUp == 0`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Disable**: `LocomotionTypeGroup != 3`; duration=0, fixed=1, exitTime=0:0, interruption=0
3. → **SitSleepDown_Judge**: `Grounded is false AND SitSleepDown != 0 AND VelocityY > 1.99 AND VelocityZ < 0.01 AND VelocityZ > -0.01 AND VelocityX < 0.01`; duration=0, fixed=1, exitTime=0:0, interruption=0

### SitSleepDown_Judge

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **SitSleepUp**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitSleepDown_WaitGrounded**: `(条件なし)`; duration=0, fixed=1, exitTime=1:0.03, interruption=0

### SitSleepUp_Judge

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **SitSleepDown**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **SitSleepUp_WaitGrounded**: `(条件なし)`; duration=0, fixed=1, exitTime=1:0.03, interruption=0

### SitSleepDown_WaitGrounded

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=33` (0)

1. → **SitSleepDown**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0, interruption=0

### SitSleepUp_WaitGrounded

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=32` (0)

1. → **SitSleepUp**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0, interruption=0

## USSPS_VirtualLoveSwitch

初期State: `Disable`

### Disable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **VirtualLove**: `LocomotionTypeGroup == 4`; duration=0, fixed=1, exitTime=0:0, interruption=0

### VirtualLove

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=41` (0)

1. → **Disable**: `LocomotionTypeGroup != 4`; duration=0, fixed=1, exitTime=0:0, interruption=0

## USSPS_TypeFix

初期State: `Disable`

### Disable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **SitShallowFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 12 AND SitShallow != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **StandingFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 1 AND SitStand == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **SitStandFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 1 AND SitStand != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
4. → **CrouchingFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 12 AND SitShallow == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
5. → **ProneFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 22 AND SitDeep == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
6. → **SitDeepFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 22 AND SitDeep != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
7. → **UpProneFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 32 AND SitSleepUp == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
8. → **SitSleepUpFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 32 AND SitSleepUp != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
9. → **DownProneFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 33`; duration=0, fixed=1, exitTime=0:0, interruption=2
10. → **SitSleepDownFix**: `LocomotionTypeGroup == 5 AND USSPS_LastMenu == 33`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitShallowFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=12` (0)

1. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `USSPS_LastMenu != 12`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **CrouchingFix**: `SitShallow == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### StandingFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=0` (0)

1. → **Disable**: `USSPS_LastMenu != 1`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **SitStandFix**: `SitStand != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitStandFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=1` (0)

1. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `USSPS_LastMenu != 1`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **StandingFix**: `SitStand == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### CrouchingFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=11` (0)

1. → **Disable**: `USSPS_LastMenu != 12`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **SitShallowFix**: `SitShallow != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### ProneFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=21` (0)

1. → **Disable**: `USSPS_LastMenu != 22`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **SitDeepFix**: `SitDeep != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitDeepFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=22` (0)

1. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `USSPS_LastMenu != 22`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **ProneFix**: `SitDeep == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### UpProneFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=21` (0)

1. → **Disable**: `USSPS_LastMenu != 32`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **SitSleepUpFix**: `SitSleepUp != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitSleepUpFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=32` (0)

1. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `USSPS_LastMenu != 32`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **UpProneFix**: `SitSleepUp == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### DownProneFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=21` (0)

1. → **Disable**: `USSPS_LastMenu != 33`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **SitSleepDownFix**: `SitSleepDown != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

### SitSleepDownFix

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `LocomotionType=33` (0)

1. → **Disable**: `LocomotionTypeGroup != 5`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Disable**: `USSPS_LastMenu != 33`; duration=0, fixed=1, exitTime=0:0, interruption=2
3. → **DownProneFix**: `SitSleepDown == 0`; duration=0, fixed=1, exitTime=0:0, interruption=2

## USSPS_LocomotionType

初期State: `Standing`

Entry遷移（順序を保持）:
1. → **Standing**: `LocomotionType == 0`
2. → **Crouching**: `LocomotionType == 11`
3. → **Prone**: `LocomotionType == 21`

### Standing

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `USSPS_Pose=0` (0)

1. → **Exit**: `LocomotionType != 0`; duration=0, fixed=1, exitTime=0:0, interruption=0

### Crouching

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `USSPS_Pose=1` (0)

1. → **Exit**: `LocomotionType != 11`; duration=0, fixed=1, exitTime=0:0, interruption=0

### Prone

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `USSPS_Pose=2` (0)

1. → **Exit**: `LocomotionType != 21`; duration=0, fixed=1, exitTime=0:0, interruption=0

## USSPS_Locomotion

初期State: `Standing`

Entry遷移（順序を保持）:
1. → **Prone**: `USSPS_Pose == 2 AND VRMode == 1`
2. → **Standing**: `USSPS_Pose == 0 AND VRMode == 1`
3. → **Crouching**: `USSPS_Pose == 1 AND VRMode == 1`
4. → **Prone_Desktop**: `USSPS_Pose == 2 AND VRMode == 0`
5. → **Crouching_Desktop**: `USSPS_Pose == 1 AND VRMode == 0`
6. → **Standing_Desktop**: `USSPS_Pose == 0 AND VRMode == 0`

### Prone

Motion: `external GUID 667633d86ecc9c0408e81156d77d9a83`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `USSPS_Pose != 2`; duration=0.5, fixed=1, exitTime=0:0, interruption=3

### Standing

Motion: `external GUID b7ff0bc6ae31ce4458992fa6ce9f6897`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `USSPS_Pose != 0`; duration=0.5, fixed=1, exitTime=0:0, interruption=3
2. → **Jump and Fall**: `Grounded is false AND USSPS_IsShallow < 0`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
3. → **Standing_Desktop**: `VRMode == 0 AND TrackingType != 0`; duration=0, fixed=1, exitTime=0:0.75, interruption=2
4. → **AvatarSelecting**: `TrackingType == 0`; duration=0, fixed=0, exitTime=0:0, interruption=0

### Crouching

Motion: `external GUID 1fe93258fe621c344be8713451c5104f`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `USSPS_Pose != 1`; duration=0.5, fixed=1, exitTime=0:0, interruption=3

### Prone_Desktop

Motion: `external GUID 667633d86ecc9c0408e81156d77d9a83`; Write Defaults=0; Foot IK=0。
Behaviour: `enterPoseSpace=1`, `fixedDelay=1`, `delayTime=0`

1. → **Exit**: `USSPS_Pose != 2`; duration=0, fixed=1, exitTime=0:0, interruption=2

### Crouching_Desktop

Motion: `external GUID 1fe93258fe621c344be8713451c5104f`; Write Defaults=0; Foot IK=0。
Behaviour: `enterPoseSpace=1`, `fixedDelay=1`, `delayTime=0`

1. → **Exit**: `USSPS_Pose != 1`; duration=0, fixed=1, exitTime=0:0, interruption=2

### Standing_Desktop

Motion: `external GUID b7ff0bc6ae31ce4458992fa6ce9f6897`; Write Defaults=0; Foot IK=0。
Behaviour: `enterPoseSpace=0`, `fixedDelay=1`, `delayTime=0`

1. → **Exit**: `USSPS_Pose != 0`; duration=0, fixed=1, exitTime=0:0, interruption=2
2. → **Jump and Fall**: `Grounded is false AND USSPS_IsShallow < 0`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

### AvatarSelecting

Motion: `external GUID b7ff0bc6ae31ce4458992fa6ce9f6897`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `TrackingType != 0`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

### Jump and Fall

初期State: `Jump`

Entry遷移（順序を保持）:
1. → **Short Fall**: `VelocityY < -4`
2. → **Long Fall**: `VelocityY < -20`

#### Short Fall

Motion: `external GUID a5f9dea4a0261414ab29c975b6f70fda`; Write Defaults=0; Foot IK=0。

1. → **Long Fall**: `VelocityY < -20`; duration=0.2, fixed=1, exitTime=0:0.75, interruption=0
2. → **Soft Landing**: `Grounded is true`; duration=0.1, fixed=1, exitTime=0:0, interruption=0

#### Hard Landing

Motion: `external GUID 6b72ff6fb01b52d42af614751aa23f73`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `(条件なし)`; duration=0.2, fixed=1, exitTime=1:0.6, interruption=0

#### Long Fall

Motion: `external GUID 704174e3f5fd50044a762a02753e8bee`; Write Defaults=0; Foot IK=0。

1. → **Hard Landing**: `Grounded is true`; duration=0.1, fixed=1, exitTime=0:0.05, interruption=0
2. → **Short Fall**: `VelocityY > -20`; duration=0.2, fixed=1, exitTime=0:0.75, interruption=0

#### Soft Landing

Motion: `external GUID 89bd787608751aa46a91bbb4478f52b2`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `(条件なし)`; duration=0.1, fixed=1, exitTime=1:1, interruption=0

#### Jump

Motion: `external GUID a5f9dea4a0261414ab29c975b6f70fda`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `Grounded is true`; duration=0.15, fixed=1, exitTime=0:0.75, interruption=0
2. → **Short Fall**: `VelocityY < -8`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

子StateMachine `Jump and Fall` の出る遷移:
1. → **Standing**: `VRMode == 1`
2. → **Standing_Desktop**: `VRMode == 0`

## USSPS_Tracking Control

初期State: `Join`

AnyState遷移（順序を保持）:
1. → **During Action**: `InStation is true`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
2. → **During Action**: `AFK is true`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
3. → **During Action**: `VRCEmote > 0`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

### Join

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Full Tracking**: `(条件なし)`; duration=0, fixed=1, exitTime=1:0.25, interruption=0

### Full Tracking

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=1`, `trackingLeftHand=1`, `trackingRightHand=1`, `trackingHip=1`, `trackingLeftFoot=1`, `trackingRightFoot=1`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`
Driver (localOnly=1): `TrackingHeadReset=1` (0), `TrackingHandReset=1` (0)

1. → **Jump and Fall**: `Upright > 0.85 AND Grounded is false`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
2. → **Locomotion**: `VelocityX < -0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
3. → **Locomotion**: `VelocityX > 0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
4. → **Locomotion**: `VelocityZ < -0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
5. → **Locomotion**: `VelocityZ > 0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
6. → **FootAnimation**: `LocomotionType == 41`; duration=0, fixed=1, exitTime=0:0, interruption=0
7. → **FootAnimation**: `LocomotionType == 32`; duration=0, fixed=1, exitTime=0:0, interruption=0
8. → **FootAnimation**: `LocomotionType == 22`; duration=0, fixed=1, exitTime=0:0, interruption=0
9. → **FootAnimation**: `LocomotionType == 12`; duration=0, fixed=1, exitTime=0:0, interruption=0
10. → **FootAnimation**: `LocomotionType == 33`; duration=0, fixed=1, exitTime=0:0, interruption=0
11. → **FootAnimation**: `LocomotionType == 1`; duration=0, fixed=1, exitTime=0:0, interruption=0

### During Action

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Out of Action**: `InStation is false AND AFK is false AND VRCEmote < 1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

### Out of Action

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Full Tracking**: `(条件なし)`; duration=0, fixed=1, exitTime=1:1, interruption=0

### FootAnimation

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `TrackingHeadReset=1` (0), `TrackingHandReset=1` (0)
Behaviour: `trackingHead=0`, `trackingLeftHand=0`, `trackingRightHand=0`, `trackingHip=0`, `trackingLeftFoot=2`, `trackingRightFoot=2`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **Full Tracking**: `LocomotionType != 12 AND LocomotionType != 22 AND LocomotionType != 32 AND LocomotionType != 33 AND LocomotionType != 41 AND LocomotionType != 1`; duration=1, fixed=1, exitTime=0:0, interruption=0

### Jump and Fall

初期State: `Jump`

Entry遷移（順序を保持）:
1. → **Long Fall**: `IsLocal is false AND VelocityY < -20`
2. → **Short Fall**: `VelocityY < -4`

#### Short Fall

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=1`, `trackingLeftHand=1`, `trackingRightHand=1`, `trackingHip=1`, `trackingLeftFoot=2`, `trackingRightFoot=2`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **Soft Landing**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Long Fall**: `IsLocal is false AND VelocityY < -20`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

#### Hard Landing

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `(条件なし)`; duration=0, fixed=1, exitTime=1:1, interruption=0

#### Long Fall

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=2`, `trackingLeftHand=2`, `trackingRightHand=2`, `trackingHip=2`, `trackingLeftFoot=2`, `trackingRightFoot=2`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **Hard Landing**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0.05, interruption=0
2. → **Short Fall**: `VelocityY > -20`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

#### Soft Landing

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `(条件なし)`; duration=0, fixed=1, exitTime=1:0.6, interruption=0

#### Jump

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=1`, `trackingLeftHand=1`, `trackingRightHand=1`, `trackingHip=1`, `trackingLeftFoot=2`, `trackingRightFoot=2`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **Fast Landing**: `Grounded is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **Short Fall**: `VelocityY < -8`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

#### Fast Landing

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `(条件なし)`; duration=0, fixed=1, exitTime=1:0.25, interruption=0

### Locomotion

初期State: `Walk and Run`

Entry遷移（順序を保持）:
1. → **Squat**: `TrackingType > 5 AND Upright < 0.8`

#### Walk and Run

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=1`, `trackingLeftHand=1`, `trackingRightHand=1`, `trackingHip=1`, `trackingLeftFoot=2`, `trackingRightFoot=2`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **Exit**: `Grounded is false AND Upright > 0.85`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
2. → **Stop**: `VelocityX > -0.1 AND VelocityX < 0.1 AND VelocityZ > -0.1 AND VelocityZ < 0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
3. → **Squat**: `TrackingType > 5 AND Upright < 0.8`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

#### Stop

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **Exit**: `Grounded is false AND Upright > 0.85`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
2. → **Walk and Run**: `VelocityX < -0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
3. → **Walk and Run**: `VelocityX > 0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
4. → **Walk and Run**: `VelocityZ < -0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
5. → **Walk and Run**: `VelocityZ > 0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
6. → **Exit**: `(条件なし)`; duration=0, fixed=1, exitTime=1:0.1, interruption=0

#### Squat

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=1`, `trackingLeftHand=1`, `trackingRightHand=1`, `trackingHip=1`, `trackingLeftFoot=1`, `trackingRightFoot=1`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **Exit**: `Grounded is false AND Upright > 0.85`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
2. → **Exit**: `VelocityX > -0.1 AND VelocityX < 0.1 AND VelocityZ > -0.1 AND VelocityZ < 0.1`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
3. → **Walk and Run**: `Upright > 0.85`; duration=0, fixed=1, exitTime=0:0.75, interruption=0
4. → **Walk and Run**: `TrackingType < 6`; duration=0, fixed=1, exitTime=0:0.75, interruption=0

子StateMachine `Jump and Fall` の出る遷移:
1. → **Locomotion**: `VelocityX < -0.1`
2. → **Locomotion**: `VelocityX > 0.1`
3. → **Locomotion**: `VelocityZ < -0.1`
4. → **Locomotion**: `VelocityZ > 0.1`
5. → **Full Tracking**: `(条件なし)`

子StateMachine `Locomotion` の出る遷移:
1. → **Jump and Fall**: `Grounded is false AND Upright > 0.85`
2. → **Full Tracking**: `(条件なし)`

## USSPS_LocomotionSwitch

初期State: `OneSecondWait`

### OneSecondWait

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **LocomotionDisable**: `LocomotionLock is true`; duration=0, fixed=1, exitTime=1:60, interruption=0
2. → **LocomotionEnable**: `LocomotionLock is false`; duration=0, fixed=1, exitTime=1:60, interruption=0

### LocomotionDisable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `disableLocomotion=1`

1. → **LocomotionEnable**: `LocomotionLock is false`; duration=0, fixed=1, exitTime=1:1, interruption=0

### LocomotionEnable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `disableLocomotion=0`

1. → **LocomotionDisable**: `LocomotionLock is true`; duration=0, fixed=1, exitTime=1:1, interruption=0

## USSPS_HeadTrackingSwitch

初期State: `OneSecondWait`

### TrakingInterrupt

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `TrackingHeadReset=0` (0)

1. → **HeadTrackingDisable**: `HeadTrackingLock is true`; duration=0, fixed=1, exitTime=1:0, interruption=0
2. → **HeadTrackingEnable**: `HeadTrackingLock is false`; duration=0, fixed=1, exitTime=1:0, interruption=0

### OneSecondWait

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **HeadTrackingEnable**: `HeadTrackingLock is false`; duration=0, fixed=1, exitTime=1:60, interruption=0
2. → **HeadTrackingDisable**: `HeadTrackingLock is true`; duration=0, fixed=1, exitTime=1:60, interruption=0

### HeadTrackingDisable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=2`, `trackingLeftHand=0`, `trackingRightHand=0`, `trackingHip=0`, `trackingLeftFoot=0`, `trackingRightFoot=0`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **TrakingInterrupt**: `TrackingHeadReset is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **HeadTrackingEnable**: `HeadTrackingLock is false`; duration=0, fixed=1, exitTime=1:1, interruption=0

### HeadTrackingEnable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=1`, `trackingLeftHand=0`, `trackingRightHand=0`, `trackingHip=0`, `trackingLeftFoot=0`, `trackingRightFoot=0`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **TrakingInterrupt**: `TrackingHeadReset is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **HeadTrackingDisable**: `HeadTrackingLock is true`; duration=0, fixed=1, exitTime=1:1, interruption=0

## USSPS_HandTrackingSwitch

初期State: `OneSecondWait`

### TrakingInterrupt

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Driver (localOnly=1): `TrackingHandReset=0` (0)

1. → **HandTrackingDisable**: `HandTrackingLock is true`; duration=0, fixed=1, exitTime=1:0, interruption=0
2. → **HandTrackingEnable**: `HandTrackingLock is false`; duration=0, fixed=1, exitTime=1:0, interruption=0

### OneSecondWait

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。

1. → **HandTrackingDisable**: `HandTrackingLock is true`; duration=0, fixed=1, exitTime=1:60, interruption=0
2. → **HandTrackingEnable**: `HandTrackingLock is false`; duration=0, fixed=1, exitTime=1:60, interruption=0

### HandTrackingEnable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=0`, `trackingLeftHand=1`, `trackingRightHand=1`, `trackingHip=0`, `trackingLeftFoot=0`, `trackingRightFoot=0`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **TrakingInterrupt**: `TrackingHandReset is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **HandTrackingDisable**: `HandTrackingLock is true`; duration=0, fixed=1, exitTime=1:1, interruption=0

### HandTrackingDisable

Motion: `external GUID 52aff6f8eb70ac94ca2d631a92b39a80`; Write Defaults=0; Foot IK=0。
Behaviour: `trackingHead=0`, `trackingLeftHand=2`, `trackingRightHand=2`, `trackingHip=0`, `trackingLeftFoot=0`, `trackingRightFoot=0`, `trackingLeftFingers=0`, `trackingRightFingers=0`, `trackingEyes=0`, `trackingMouth=0`

1. → **TrakingInterrupt**: `TrackingHandReset is true`; duration=0, fixed=1, exitTime=0:0, interruption=0
2. → **HandTrackingEnable**: `HandTrackingLock is false`; duration=0, fixed=1, exitTime=1:1, interruption=0

## USSPS_IsDefault

空の識別レイヤー。

## BlendTreeによる閾値計算

3個の参照BlendTreeはUprightと設定閾値を使う。内部ClipのAnimator float曲線が判定値を書く。単純な比較式に置き換える場合、等値・初期値・加算レイヤーを別途検証する。

- `Blend Tree`: type=3; x=`Upright`, y=`USSPS_DeepThreshold`; children=2
- `Blend Tree`: type=3; x=`Upright`, y=`USSPS_ShallowThreshold`; children=2
- `Blend Tree`: type=3; x=`Upright`, y=`USSPS_SleepThreshold`; children=2
