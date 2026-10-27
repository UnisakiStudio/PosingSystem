# ポーズシステム AnimatorController マスター仕様

更新: 2026-10-27。状態: コード生成・最適化・NDMF統合の専用試験は保存試験追加後の44件を確認。全体回帰試験は603合格・9失敗・16スキップで、失敗9件はコミット済み基準版でも再現。スキップ16件は専用の保存シーンを準備した再試験で全件合格。試用版として実装し、通常ビルドの方式は変更していない。

## 目的と基準

ユーザー依頼は、現行の要件・仕組みの分析、問題の抽出、要件に対応する厳密なテスト、負荷に配慮したソースコード生成方式の実装。動作の期待値と現行実装の偶発的な振る舞いを分ける。ローカルのC/Z入力から姿勢適用までの不要な遅延を短縮する。

既存変更は先に機能別コミット済み。正本の基準はPosingSystem release/3.1.0 `8da20d6`、KawaiiPosing release/3.1.0 `9b92028`。内訳は `run001-client-verification-2026-10-27.md`。開発プロジェクトPackagesは検証用実体コピーで、自動同期しない。生成方式の変更は正本と検証コピーの一致を確認して扱う。

旧ゆにしなののC二連打後の持続異常はユーザーの現行版実機確認を根拠に解決扱いを維持する。本作業の短い姿勢遅延とは別件。旧現象の原因が確定したことにはしない。

## 現行構成

全レイヤーのState、順序付き遷移条件、Driverの書込み、パラメーター初期値は [現行参照付録](animator-controller-reference.md) に記録している。付録は現状の機械的な記録、本文の要件表は採用判断の基準。

`Resources/PosingSystem_Locomotion.controller` が基本テンプレート。Converterが商品・設定・アバターに応じてポーズ、同期、補正、追加機能を構成する。テンプレートだけをコード化しても全製品機能の置き換えにはならない。

| 範囲 | 現行値 | 解釈 |
|---|---:|---|
| 基本レイヤー | 17 | 識別用空レイヤー1つを含む。実生成時に機能レイヤーが追加される |
| 宣言パラメーター | 37 | 商品のポーズ数・機能により増える |
| ファイル内State / StateMachine | 116 / 34 | 未参照オブジェクトを含む |
| ファイル内遷移 | 506 | StateTransition 418 + AnimatorTransition 88 |
| ファイル内BlendTree | 40 | 未参照オブジェクトを含む |
| 参照で到達するState / StateMachine | 84 / 20 | 実際の状態到達可能性とは別。オブジェクト参照の到達数 |
| 参照で到達する遷移 / BlendTree | 215 / 3 | 192 + 23遷移。外部Clipは別 |
| 参照で到達するBehaviour | 52 | ParameterDriver 36、Tracking 11、Locomotion 2、PoseSpace 3 |

ファイル内全オブジェクト749に対し、Controllerから参照で到達するものは375。未参照資産を除くことは保存容量の整理になるが、その分だけ実行CPU負荷が減るとは言えない。現行のGraph.Cloneも参照グラフを扱うため、生成済みControllerとの比較を別途行う。

### 判定と再生の流れ

1. Shallow/Deep/SleepThresholdの3レイヤーが、Uprightと可変の閾値からUSSPS_IsShallow/Deep/Sleepを算出。
2. GroupSwitchが姿勢グループを決定。グループ値は立ち0・浅い1・深い2・寝3・VirtualLove4・姿勢固定5。
3. Standing / SitShallowSwitch / SitDeepSwitch / SitSleepSwitch / VirtualLoveSwitch / TypeFixがLocomotionTypeを決定。
4. LocomotionTypeレイヤーが選択値との組合せから同期ポーズを決定。既定の立ち0、中腰1、伏せ2に商品ポーズが加わる。
5. Locomotionが同期ポーズとVRModeで再生Stateへ遷移。
6. Tracking Control、LocomotionSwitch、HeadTrackingSwitch、HandTrackingSwitchと追加機能が身体・移動・PoseSpaceを制御。

USSPS_IsDefaultは既定テンプレート識別用の空レイヤーで、生成時に除去される。レイヤー重み0でもParameterDriver等の状態処理は設計上必要。重み0を根拠に削除しない。

LocomotionTypeの値: 標準立ち0、選択立ち1、標準中腰11、選択中腰12、標準伏せ21、選択深座り22、仰向け32、うつ伏せ33。VirtualLove連携は個別定義を追加調査する。

### 状態を保持する部分

- LocomotionLockはグループ選択を保持する経路を持つ。
- TypeFixは最後に操作したメニューとその選択値を使う。
- 寝姿勢はKawaiiPosing_IsDown、接地、跳躍速度、着地待ちを使い、入力だけの単純関数ではない。
- Head/Hand/Locomotionの制御には開始待機と復帰がある。
- VRのブレンド時間と割り込み順序はユーザー体験の一部。ゼロ時間へ一括置換しない。
- リモートは受信した同期ポーズで再生する。見る側のUprightや未同期メニュー値で上書きしてはいけない。

## 要件と検証ID

「現行を保持」は、不具合の可能性がある分岐まで正しいと認定する意味ではない。疑わしい分岐は課題一覧へ分離し、意図を確認できるまで互換性を保つ。

| ID | 要件 | 必要な検証 |
|---|---|---|
| GEN-01 | 基本Controllerをソース定義から生成できる | テンプレートを読み込まず構築、現行との全プロパティ・参照・遷移順比較 |
| GEN-02 | 同一入力の生成結果が意味的に同一 | 反復生成、保存・再読込、入力順序依存の検査 |
| GEN-03 | 元アセットと別アバターを変更しない | 元資産のハッシュ、独立生成物の変更分離 |
| GEN-04 | 必須参照欠損は明示的に失敗する | GUID/local ID不明、型不一致、片付け、部分出力を採用しない |
| SEL-01 | 立ち・浅い・深い・寝の選択が成立する | 閾値前後・等値、0/有効値/無効値、商品別・併用 |
| SEL-02 | ローカルの一般的な姿勢入力への応答を短縮 | 同じ入力列・同じフレーム刻みで現行と比較、初回と復帰を区別 |
| SEL-03 | 遷移中に何度変化しても所定姿勢へ収束する | 矩形波・ランプ・固定乱数列、1/2/3フレームの反転、30/60/90/144fps |
| SEL-04 | 未選択の標準・独自モーションを保持する | Clip・入れ子BlendTree・proxy・移動・停止・設定解除 |
| MEM-01 | 姿勢固定・足固定・最後のメニューの意味を保持 | 固定前後で姿勢・選択・接地を変化、解除後の復帰 |
| MEM-02 | 寝姿勢の切替・着地待ちを保持 | 上下両選択、片側未選択、接地・速度境界・途中離脱 |
| NET-01 | リモートの再生が同期値に従う | IsLocal=falseでローカル入力を変更、送信者と受信者を分離 |
| NET-02 | 同期コストと既存値の割当を維持する | 8bit境界・複数商品の割当・ExpressionParametersの比較 |
| TRK-01 | 頭・手・足・移動・PoseSpaceの開始/解除が一致 | Behaviour適用順と最終値、姿勢と足制御のタイミング |
| TRK-02 | VRの滑らかな遷移・割り込みを保持する | duration、exit time、SourceThenDestination、自己遷移、VRMode切替 |
| OPT-01 | 撮影・床・足高さ・反転・寝返り・呼吸を保持 | 個別ON/OFF、重要な併用、全機能、NDMF最終生成後 |
| IMP-01 | 自動取り込み・手動調整・設定非変更を保持 | 既存の取り込み・生成資産管理テストを再実行 |
| BUILD-01 | Scene/Prefab/複製/再生成の管理を保持 | 既存の所有者管理と安全な整理の回帰試験 |
| PERF-01 | 不要な組合せ展開を避ける | ポーズ数別State/Transition/条件数、生成時間、ファイル量 |
| PERF-02 | 実行コストを計測して採用判断する | 同一環境でAnimator評価の複数反復、中央値・ばらつき、CPU実測と静的指標を区別 |

テストは、独立した期待値による要件試験、現行との差分試験、Unity実行試験、NDMF後の統合試験を分ける。生成した条件を同じコードで再評価するだけの自己確認にしない。全設定の直積を実施したと誤記せず、全列挙した範囲と代表組合せを記録する。正式試験の失敗・timeoutを観測ログだけで合格へ変えない。

## 現時点の課題

| ID | 観測 | 判定・扱い |
|---|---|---|
| ACO-01 | 保存済みUnityログでGroup→Type→Pose→再生が段階更新。約5フレームの例 | 短縮対象。実機で見える一瞬の標準中腰の描画原因までは未確定 |
| ACO-02 | TypeFixのDownProneFixとSitSleepDownFixへのEntry条件が同一で前者が先 | 後者の直入が先行分岐に隠れる。現行の追加遷移で後から切替えるが余分な段階が疑われる |
| ACO-03 | GroupSwitch/SitDeepに同じTypeFix離脱が二つ | 同一結果の重複。削除候補として差分試験する |
| ACO-04 | GroupSwitchの浅い・立ち・寝と深いでVirtualLoveへの離脱条件が非対称 | 意図未確定。最適化に紛れて仕様変更しない |
| ACO-05 | 判定floatの初期値-1と同期加算レイヤーでUnity内の判定値汚染を観測 | 実機では同じ不具合と確認できていない。初期化経路ごとの試験が必要 |
| ACO-06 | 判定の>0/<0境界と固定・待機を持つ | 等値は通常の不等号反転と同義でない。否定条件の展開で境界を落とさない |
| ACO-07 | テンプレートに参照されない374オブジェクト | コード定義は必要な参照グラフだけを表現。CPU改善量とは混同しない |
| ACO-08 | 大量の条件展開でポーズ数の二乗に増える可能性 | 各姿勢から全姿勢への直接遷移を安易に作らず、条件共通化と上限検査を設ける |
| ACO-09 | 全体回帰試験で、プレビルドなしの永続NDMFビルドが呼吸Controller分離エラー | 基準版でも再現。通常採用前の既存課題。新生成器の専用試験成功で覆い隠さない |

## 実装方針

1. 最初は現行と意味的に同等のコード生成器を作り、対応していないプロパティを黙って捨てない。
2. 元テンプレートは比較基準として保持する。生成定義の由来と基準ハッシュを記録する。
3. 状態を保持する処理と、その場の入力だけで決まる処理を分け、後者の段数短縮を先に評価する。
4. 今回は状態を持たないGroup判定の条件展開を実装・実測した。BlendTreeへの全面移行や全処理1フレーム化は実装していない。状態保持・可変閾値・同期・Behaviourの副作用を表現する追加設計が必要。
5. ローカル一般経路の短縮を検証し、VR・同期・固定・特殊連携の互換性を保つ。
6. 利用中のSceneやPrefab、配付ZIPを実験途中で書き換えない。生成方法の切替と元方式への復帰を用意する。

## 一次資料

- [Unity 2022.3 Animation transitions](https://docs.unity3d.com/2022.3/Documentation/Manual/class-Transition.html): 遷移条件、継続時間、割り込みの評価仕様。
- [VRChat State Behaviors](https://creators.vrchat.com/avatars/state-behaviors/): Behaviourは通常遷移開始時に動作するが、極端に短い滞在での保証はない。時間を潰す際の実機確認点とする。
- [VRChat Animator Parameters](https://creators.vrchat.com/avatars/animator-parameters/): Upright等の組込み入力、IsLocal、同期種別。組込み値を実機へ書き込んで検証成功を作らない。

## 進捗と残作業

- 完了: 既存変更の11件の機能別コミット、テンプレートの構造・参照グラフ集計、要件と課題の初版、375オブジェクトのC#定義生成。
- `Tools/AnimatorController/export_definition.py` は移行・保守用。製品のC#生成器はテンプレート/YAMLを実行時に読まず、外部Clip/MaskだけGUIDとlocal IDで解決する。未知のネイティブフィールド・Behaviourフィールドは移行時に拒否する。
- `compiler-tenth.xml`: Unity EditMode 6/6合格。元の375オブジェクトとの全SerializedProperty・参照先・順序（辞書キー順のみ正規化）、独立生成、入力非変更、欠損時の片付け、保存・再読込、完全一致した重複だけの除去。
- 移行用Pythonテスト9/9合格。生成の再現性、参照ハッシュ、未知の構造・欠落参照、未知のSDKフィールド・別Script・Prefab参照の拒否を検査。別途キャッシュのリンク検査3/3合格。
- 開発過程の失敗も保存。UnityのネイティブAnimatorオブジェクトにはJSON上書きが適用できず、型付きAPIによる構築に変更。StateMachine遷移は遷移先の初期化完了後に割り当てる。未永続オブジェクトのJSONだけでは遷移先を識別できず、重複判定も型付き比較へ変更。
- `selection-red.xml`: 最適化前の10件中5件合格/5件失敗。独立した仕様判定・復帰試験は合格し、短縮要求だけ失敗することを確認。閾値3層＋GroupSwitchのUnity実評価では立ち→中腰が30/60/90/144fpsで3フレーム。全アバター・VRChatの遅延値ではない。
- `selection-second.xml`: 16/16合格。最適化後は上記判定が2フレーム。書き下ろした期待値に対し、3判定floatの負/零/正、Lock、VirtualLove、TypeFix、TrackingTypeの組合せ432通り×8状態=3,456通りを新旧それぞれ検証。30/60/90/144fpsの1/2/3フレーム反転と固定乱数入力後の復帰も合格。
- `compiler-integration-third.xml` のCPU測定: 4層（閾値3＋Group）の手動Unity評価を交互順序で9回×3,000フレーム。中央値は従来0.010699ms、新方式0.010211ms。Editorの部分系CPU値でありVRChat/ヘッドセット負荷の保証ではない。許容基準は従来中央値×1.35＋0.005ms。
- `compiler-ndmf-early.xml`: 15/15合格。NDMFが先に複製したControllerはパラメーター配列の順序等が変わるため、名前や識別レイヤーだけで置換しない。対象アバター限定のScopeで、MA加工前にコード生成する。以後は既存のポーズ変換・追加機能・生成資産管理を通す。全機能ON/OFFと足高さ方式、メニュー、同期ビット、比較コピー、再プレビルドを検査。
- 独自Controllerの保護: 匿名コピーを扱う場合は参照グラフと全SerializedPropertyを比較。条件・Driver値・パラメーター初期値・遷移先を変えたコピーは拒否。通常ビルドではScopeがなく、この生成経路を使用しない。
- 試用入口: Unityメニュー `Tools > ゆにさきスタジオ > 実験 > 選択アバターの最適化比較コピーを作成`。元アバターを変更せず、同じSceneの比較コピーを作り、Blueprint IDを空にしてから新方式でプレビルドする。アップロードやVRChat操作は行わない。独自の共通Controllerは自動置換しない。
- 最初の最適化はGroupSwitchの空Entryを経由する2段階判定の展開。Entry自体・一致先がない時の待機・固定状態を保持する。浮動小数のゼロを別扱いとし、不等号の否定変換をしない。8状態の固定規模でありポーズ数に応じて二乗増加させない。
- `compiler-suite.xml`: 専用試験43/43合格。全体グラフで実際のSDKコールバックを介してParameterDriver、Tracking、Locomotion、PoseSpaceの適用を観測。デスクトップ30/60/90/144fps、VR30/90fps、TrackingType=3/6で固定・解除・最後のメニュー・仰向け/うつ伏せ・空中/着地・短間隔反転後の新旧の収束状態が一致。リモートでは受信ポーズをローカルのUpright等が上書きしないことを確認。
- 同じ試験で選択済み中腰ポーズに達するまでが従来6評価→最適化5評価（30/60/90/144fpsとも）。閾値＋Groupだけでは3→2評価。CPU測定の最新中央値は従来0.011226ms→最適化0.010880ms。微小な差であり、全アバターのCPU改善を立証した数値ではない。
- 検証用のSDKブリッジは実際のAnimatorControllerPlayableとSDKイベントを接続する。VRChatのクライアント実装、ネットワーク送受信、IK、描画、Behaviourのクライアント内スケジューリングまでは再現していない。実機の一瞬の標準中腰が消えたとは判定していない。
- `posing-system-regression.xml`: 同じ名前空間の既存Assetsテストも含め628件、603合格・9失敗・16スキップ、実行約19分35秒。新しい接続変更2ファイルだけを基準コミットへ一時的に戻した `existing-failures-baseline.xml` では対象12件中3合格・同じ9件失敗。新規生成器により追加された失敗ではない。比較後は退避SHA-256と一致する新実装へ復帰済み。
- `compiler-persistence-second.xml`: 18/18合格。新旧の生成Controllerの保存・再読込を独立した再生成グラフと全プロパティ/参照比較し、保存・Undo関連16件も実行。初回の追加試験ではUnityのファイル名への自動改名と未保存Runnerシーンの制約を検出し、期待名と試験用シーン準備を修正した。製品ロジックを変更して合格させたものではない。
- 専用試験は旧43ケースの保存1ケースを新旧2ケースへ拡張した44ケース。全実行の最新結果をケースごとに統合すると629件中620合格・既存9失敗。これは1回の全体実行が全件成功したという意味ではない。基準版との比較は新しい本番接続2ファイルを外して実施し、その後ハッシュ一致で復帰した。
- 新機能を通常ビルドへ自動適用したり、バージョン・配付ZIPを更新したりはしていない。正本 `VPM/PosingSystem-3.1.0` へは検証コピーとの全ファイルSHA-256一致を確認して反映する。
- 初期集計データ: `Recordings/CommitPreparation_20261027/controller-inventory.json`。以後の最適化記録は別フォルダへ保存する。
- Unityは開始時に停止しておりMCPは接続拒否。`Tools/AnimatorController/run-tests.ps1` で2022.3.22f1を非表示のバッチモードで起動。ユーザーのSceneを保存せず、試験用の空Sceneのみ準備・削除する。試験中はUnity側のソースを編集しない。

## 実装の責務と保守

| ファイル（Editor/） | 責務 |
|---|---|
| PosingSystemControllerDefinition.Generated.cs | 基本テンプレートの到達する375オブジェクトを、型付きC# APIで構築する定義 |
| PosingSystemControllerCompiler.cs | 独立した生成物の所有権、外部Clip/Mask参照の解決、正確に同じ遷移だけの除去 |
| PosingSystemControllerOptimization.cs | GroupSwitchの副作用のないEntry判定を固定規模の条件へ展開。未知の構造は拒否 |
| PosingSystemControllerEquivalence.cs | 匿名のController複製が元と同等かを、全SerializedPropertyと参照構造で検査 |
| PosingSystemControllerGeneration.cs | 対象アバター限定の試用Scope、MA処理前の生成、元を保護した比較コピー作成 |
| PosingSystemGeneratedControllers.cs / PosingSystemConverter.cs | 既存の生成ファイル管理と、既存のポーズ・追加機能生成につなぐ接続部 |

コード生成の対象は共通Locomotionテンプレート。追加機能・商品別ポーズ・取り込み・補正は既存Converterの共通処理を引き続き通す。すべての補助Controller/Clipをこの定義に置き換えたわけではない。

基本Controllerはテンプレートの複製ではなくC#で新規作成する。一方、試用入口では変更済みテンプレートを誤って置き換えないよう、参照テンプレートの未保存編集とSHA-256を確認する。基準ハッシュは `462fa9b486999c6e53727dffa657bc24c80dff31fd4242c06efec92dc8bcd6c7`。テンプレートを変更したら、移行スクリプトで定義・付録を更新して等価性試験を再実施する。製品利用時にPython/PyYAMLは不要。

`Tools/AnimatorController/export_definition.py` と `export_specification.py` は保守用。前者は未知のフィールドを無視せず停止する。自動生成ファイルを手編集して現行仕様と食い違わせず、最適化の変更は手書きのOptimization側へ置く。

## 規模と検証の限界

同じ合成Humanoidへ各カテゴリのポーズを割り当てて生成した値。元からある遷移数も記録し、最適化の追加分と区別する。

| ポーズ数 | State数 | 遷移数：従来→最適化 | 最適化後の条件数 |
|---:|---:|---:|---:|
| 1 | 87 | 238→349 | 550 |
| 16 | 132 | 583→694 | 1,180 |
| 64 | 276 | 1,687→1,798 | 3,196 |
| 128 | 468 | 3,159→3,270 | 5,884 |

追加遷移は111本で一定。Stateとパラメーターは増やさず、同期パラメーター容量も維持する。これは検査したポーズ数・構成での結果で、すべての他社ギミックとの併用保証ではない。

| 検証範囲 | 根拠 | 残る確認 |
|---|---|---|
| GEN-01〜04・重複除去 | 全参照グラフ/プロパティ比較、独立生成、欠損時片付け、保存 | Unity/SDK更新時は再実行 |
| SEL-01〜03・MEM・TRK | 独立期待値3,456通り、新旧の実Playable、連打・固定・寝姿勢 | 実機での視覚的な瞬間姿勢、全オプション直積 |
| NET-01/02 | リモートPlayableの入力隔離、NDMF後のメニューと同期bit比較 | 別クライアント間の実通信 |
| OPT-01・BUILD・IMP | NDMF後の機能ON/OFF、比較コピー、既存回帰試験 | 任意の他社Controller・全商品との併用 |
| PERF-01/02 | ポーズ数別規模、部分グラフのCPU反復 | VRChat全体・ヘッドセットのCPU/GPU/メモリ実測 |

全試験の成功は、仕様表の全組合せや実機確認が完了したことを意味しない。通常採用前には、比較コピーでC/Z、姿勢固定、寝返り、床/撮影、VRの遷移とリモート表示を確認する。元アバターの生成方式はそのままなので、試用終了は比較コピーを除くことで可能。

検証環境の復旧経緯と再発防止は [パッケージ正本の復旧記録](package-source-recovery-2026-10-27.md) を参照。Temp内の旧Junction退避は廃止し、開発コピーと正本をハッシュ照合して扱う。

## 基準版にも残る失敗

今回の最適化とは分けて残す。下記の既存テストを、合格にするためだけに削除・期待値変更していない。

| 既存テスト/件数 | 観測された不一致・次の調査 |
|---|---|
| SettingsPrefab_HasSavedSyncedBreathingToggleEnabledByDefault / 1 | 共通設定Prefabの呼吸はOFFで、旧テストの「保存済み同期呼吸トグルが既定で存在する」前提と不一致。現在の商品Prefabと初期仕様を基準にテストを整理する必要がある |
| Defaults_DoNotAllocateNewFeatureBits / 1 | 現在の共通設定Prefabは追加機能OFFで0bit。テストは11bitを期待している |
| NdmfBuild_PersistedPackageAssets…(false,false) / 1 | プレビルドなし・永続AssetSaverで呼吸用Controllerを一時コピーとして認められず失敗。プレビルド済みの2経路は合格。EditorOnlyタグの復旧と複製・同期の順序を追加調査する |
| RolloverGeneration_SettingsSavedAndInvalidatePrebuild / 1 | 本体のassert後、テストのTearDownがテスト用保存アセットをDestroyImmediateしようとしてUnityの保護エラー。テスト自身のアセット所有権・後片付けを修正する必要がある |
| PrefabOverrideFont_DoesNotChangeSettingsTypography / 1 | バッチ・GUI未初期化状態でEnsureStylesが失敗。GUIを初期化したEditor内での試験が必要 |
| LocalFeatures_KeepMotionAndPhotoMenus… / 1 | ローカル撮影・呼吸設定時、生成メニューに関する期待値と不一致。既存初期設定と同期処理の前提を調査する |
| IkKneeSafetyChanges_InvalidatePrebuiltClips / 1 | テストはautoPoseLogicVersion=2を期待するが、実装は既に4。キャッシュ無効化の仕様に沿うテストへ更新する必要がある |
| ConversionValidationTests.…OutsideAvatar_DoesNotThrow / 2 | Assets側の旧テストはアバター外でも例外なしを期待。現行はPS002診断等で明示的に拒否する仕様 |

16スキップはEditorPersistenceTestsの「保存済みの開始シーンが必要」という保護条件。バッチ時だけ、Runnerの未保存シーンにユーザーオブジェクトが存在しないことを確認して固有のテストパスへ保存する準備を追加。NDMFの既知の空Activator以外を含む場合は拒否する。対話Editorでユーザーの未保存シーンを自動保存することはない。この準備による再試験で16件すべて合格した。
