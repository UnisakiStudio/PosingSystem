# 導入プログラムの多言語化

言語だけを共通の `EditorPrefs` キー `jp.unisakistudio.posingsystem.language` に保存します。明示した選択を優先し、未選択なら `Application.systemLanguage` と地域情報を使い、非対応の言語なら英語を使います。Unityの表示言語には依存しません。macOS/Linuxでも同じAPIを使用できます。

EditorPrefsは同じOSユーザーのUnityプロジェクト間で共有できます。WindowsはUnityのレジストリ領域、macOSはユーザーのplist、LinuxはユーザーのUnity設定ファイルにUnityが保存します。別PCへの同期は行いません。保存値は公開された言語コードだけです。ライセンスのレジストリ／ファイル保存、登録値、判定、削除対象は既存のままです。

- Unity API: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorPrefs.html
- OS言語: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-systemLanguage.html

## 動作

### インストール失敗の詳細表示（2026-11-12）

VPAIの最上位catchでも、概要に加えて実際の例外メッセージ、内部例外／複数例外の各メッセージ、Consoleの案内を表示します。パッケージID・要求バージョン・HTTPエラー等は原文を保ち、元の例外とスタックのConsole記録も維持します。言語ボタンで戻ったときも詳細を失わず、概要と案内を再描画します。既存の30言語の `Error installing packages`／`Please see console for more details` を使い、新規未翻訳ラベルは追加しません。

`InstallerError.cs.in` はオーバーレイ生成時にVPAIソースへ追加する純粋な整形処理です。`AssemblyChecks.cs.in` ではインストーラー本体の初期化を呼ばずに、30言語、例外のパッケージ名・バージョン、内部／集約例外、日本語への復帰を検証します。公開前の `3.2.x` を指定したインストーラーは、まだその版が公開VPMに無い場合に `package not found` で止まります。詳細表示の修正だけで公開状態は変わりません。

言語が未保存の場合、ライセンス／VPAIの最初の導入確認にだけ「Language...」を表示します。選択を適用すると、対象商品・バージョン・ファイル名を保持して元の確認画面へ戻ります。PC言語のまま確認を進めた場合もその選択を保存します。キャンセルだけでは保存しません。保存済みの場合と完了・エラー画面では通常のボタンだけを表示します。以後はInspector最上部で言語を変更します。インストールの同意は元の確認画面で行います。

同じOSユーザーのすべてのポーズツール・アバター・プロジェクトで共通の言語を参照し、ゲーム内メニューも同じ値を使います。旧アバターの保存言語は参照せず、言語変更でシーンやPrefabを書き換えません。ゲーム内メニューへの反映には再アップロードが必要です。

MenuItemの動的登録はUnity 2022.3内部APIを限定的に使用します。導入前は独立した共有ソース、導入後は本体のアセンブリが登録を担当します。元の優先順位、検証関数、コールバックを保持し、登録時にはコマンドを実行しません。内部APIがないUnityでは静的メニューを残します。

### メニュー／全文訳変更時の必須確認（2026-11-09）

レビュー済みの文章単位の訳は、全言語で組み合わせ用の `message_models` より優先します。基本語の合成で完成済みの訳を上書きしないでください。特にネイティブMenuItemの表示名へ改行を入れず、登録パスはJSON配列で保存します。旧版の改行入りライセンスメニューを識別する移行aliasも保持します。

新しいMenuItemを追加したら、通常Inspectorのcatalogだけでなく、導入前にも動くbootstrapのaliasと全対象言語を確認してください。言語切替後に新しい表示があることだけでなく、**旧言語の登録が消えること**と日本語への復帰を検証します。`PosingSystemNativeMenuTests` は30言語の連続切替、旧改行パスの除去、末尾空白、Console相談、登録時にコマンドを実行しないことを確認します。

Unity内部の `MenuItemExists` は末尾空白付きパスにfalseを返す場合があります。削除は実際のメニュー一覧のパスを基準とし、自社のサブメニューだけを検査して無関係なUnity／他社コマンドを翻訳解析しないようにします。

30言語の導入用基本41語と、それを組み合わせた確認・結果・進捗文言を同梱します。詳細な診断はConsoleへ保持します。通常のInspector全文言・全姿勢名・オンラインマニュアルの30言語翻訳とは別の範囲です。タイ語・インド系言語・RTLのフォントと字形は実画面の確認が必要です。

## ソースと生成

- `Bootstrap/` / `bootstrap_embed.py`: 初回言語選択の7言語（ar/he/hi/bn/ta/te/th）の固定caption描画。autonym・記憶先の説明・Applyの21画像を単一の生成helperへ埋め込む。独立installerへ追加の描画DLLやfontを導入しない。Popupの選択肢、OS dialog、入力値はlogical Unicodeのまま維持する。
- 固定captionの翻訳やautonymを変更した場合は、画像を再生成して描画を確認する。翻訳と画像のSHA対応が一致しなければ生成を停止する。`Bootstrap/caption-manifest.json` と3つの `.cs.in` を手動で食い違わせない。実行時にも画像を検証し、失敗したcaptionはnative表示を残す。画像生成元NotoのOFL通知は既存のthird-party通知へ追記する。
- `Bootstrap/.gitattributes` はSHA検査対象の4素材とmanifestをLFに固定する。素材のraw-byte SHAはLFのGit保存／checkout表現から計算し、CRLFの作業コピーから登録しない。`python Tools/InstallerLocalization/test_bootstrap_embed.py` は改行・checkout属性・archive相当の再生成と、翻訳／画像の改変拒否を確認する。logical captionとPNG自体のSHAは改行規則の変更で書き換えない。
- `SetupLanguage.cs.in` / `SetupMenus.cs.in`: パッケージと独立導入側に共通の言語処理。
- `bootstrap.translations.json`: 導入語彙・既存日本語からの対応表・確認文の構成。
- `generate_language.py`: 本体の翻訳表も取り込み、`Editor/Setup/SetupLanguage.cs` を生成。
- `prepare_sources.py`: 上流ソースと手元の配布元から抽出したActivatorにUI変更だけを適用。
- `verify_sources.py`: 30言語の基本語、書式引数、既存ライセンス保存処理とパスの維持を検査。
- `AssemblyChecks.cs.in`: Unity外のプロセスで完成DLLの純粋な翻訳・言語解決だけを検査。

VPAIは [anatawa12/VPMPackageAutoInstaller](https://github.com/anatawa12/VPMPackageAutoInstaller) のMITライセンスを保持します。再現用の上流・submoduleコミットは `build.py` の `PINS` に固定しています。上流のSimpleJson／semver用シンボリックリンクはWindowsで通常ファイルになる場合があるため、ビルドはsubmodule内の実ファイルを直接参照します。配布元のライセンススクリプトやバイナリはこのGitに保存しません。

## ビルド

Python 3.10以上、.NET SDK 9、Unity 2022.3 Editorの実参照アセンブリを使用します。`--inputs` には配布用unitypackageから読み出した `KawaiiPosingLicenseActivator.cs` と `CoolPosingLicenseActivator.cs` を置きます。ライセンス値をログへ出力しないでください。実行例はPowerShellです。

```powershell
git clone --recurse-submodules https://github.com/anatawa12/VPMPackageAutoInstaller <upstream>
git -C <upstream> checkout f0c1034b1e9d6863e37e3b27c7f97d1ca3c3df25
git -C <upstream> submodule update --init --recursive
python extract_inputs.py --output <inputs> <kawaii.unitypackage> <cool.unitypackage>
python build.py --upstream <upstream> --inputs <inputs> --output <build-output> --unity 'C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor' --dotnet 'C:/Program Files/dotnet/dotnet.exe'
python package.py --input <original.unitypackage> --build <build-output> --output <new-preview.unitypackage>
```

ビルドは完成DLLと3OS条件のライセンスコードをコンパイルし、Unity外で192項目を確認します。実行環境のライセンス登録、削除、パッケージ導入は呼び出しません。Windows以外は条件コンパイルと言語マッピングの検証で、実OS上の画面確認は別途必要です。

`package.py` は元の配布物を上書きせず、ActivatorとDLLを差し替え、共通ヘルパーとMIT本文を追加します。共通ヘルパーは両商品で同一のパス・GUIDを使い、本体パッケージとは異なるGUIDです。VPAIの自己削除フォルダ外に置きます。書き出した全ファイルを再読込して一致を検査し、変更箇所・ハッシュをmanifestへ記録します。

### 日本語ファイル名の展開互換性（2026-11-12）

gzipヘッダーの内部ファイル名を `archtemp.tar` に固定します。内部名を省略すると外側の日本語unitypackage名が中間TAR名に使われ、Windows Unity 2022.3.22f1のTAR展開で `Couldn't decompress package` が発生しました。同じデータは英語名なら成功します。内部名だけを修正すると日本語名でも成功することを、Unityの `PackageUtility.ExtractAndPrepareAssetList` で可愛い／カッコいい両方について確認済みです。通常のgzip/TAR読戻し成功だけではこの問題を検出できません。

既存生成物を直す `normalize_gzip_header` は、展開後のTARを1バイトも変えずに内部名を補います。GUID・スクリプト・DLL・ライセンス値・依存設定は維持します。`python -m unittest discover -s Tools/InstallerLocalization -p test_package.py -v` で、内部名、TAR保持、再現性、破損拒否と新規生成を検査します。配布前には日本語名と英語名の両方でUnity実展開も確認してください。展開一覧取得ではインストーラーをAssetsへ取り込まず、ライセンス登録や自動導入を実行しません。

`package.py` は指定がなければ入力パッケージの依存範囲をそのまま保ちます。入力が既に `3.2.x` のように固定されている場合は固定のまま残るため、manifestの「retained」は「入力のまま」という意味です。公開用の配布物は `--dependency jp.unisakistudio.kawaiiposing=3.x.x`（カッコいいは `1.x.x`）を明示し、新規購入者が常に公開済みの最新3.x／1.xを受け取るようにします。開発側の `build_minimal_store_bundles.py` はこの範囲を指定し、生成後のconfigを検査します。本体の公開前に取り込んだ場合は、公開済みの旧版本体が導入されます。可愛いポーズツールのconfigは旧アカウントのlisting URLを参照しているため、公開時は両方のlistingに新版があることを確認してください。

### ダイアログとビルドの注意（2026-11-12）

- ライセンス導入ダイアログへは**原文の見出し・ボタン**と再評価できる本文（`() => ...`）を渡します。翻訳済みの文字列を渡すと、言語ボタン後の再描画で逆引きになり、1語が2つの意味を持つ言語（ベトナム語の「Cài đặt」＝Install／Settings）で同意ボタンが別の語になります。`prepare_sources.py` の `dialog_sources` がこの形へ変換します。
- `Debug.Log` 系のConsole行は翻訳しません。`[InitializeOnLoad]` の「既に有効化されています」ログを翻訳すると、スクリプト再読込のたびに全言語表を構築していました。
- `build.py` は `DebugType=none` でビルドし、完成DLLにビルドPCの絶対パス（アカウント名やフォルダー構成）が含まれないことを検査します。配布物の生成側でも全ファイルを同じ観点で検査します。

**完成したVPAI DLLを開発中のUnityのAssetsに直接置かないでください。** 上流と同様、読み込むだけで自動導入と自己削除が動きます。Unity内で試す場合は専用プロジェクトで事前に `VPM_PACKAGE_AUTO_INSTALLER_DEV_ENV` を定義し、実行対象のconfigと導入先を確認してください。

### 三点だいしゅきツール（2026-11-13）

三点だいしゅきツール（VirtualLove）のライセンスは男女別です。`VirtualLoveBoyLicenseActivator.cs` と `VirtualLoveGirlLicenseActivator.cs` を `--inputs` に置けば、同じ `build.py` / `package.py` でビルドできます（保存先・キー・値は変更しません）。

- 導入確認・完了・状態・削除の各文と見出し（男女それぞれ7件）は、パッケージ導入前に表示するため `bootstrap.translations.json` に全文訳で登録しています。商品パッケージ側の翻訳表には、パッケージ導入後に使うメニュー名とInspectorの文言だけを置きます。
- 状態表示の行は `三点だいしゅきツール（男の子用）: 有効` のように日本語の商品名で始まるため、`商品名:` と `ゆにさきポーズシステム:` を接頭辞として登録しています。先頭行が見出し（`=== Windows レジストリ ===` など）の実際の表示では、行ごとに翻訳されます。
- `AssemblyChecks.cs.in` で、全言語の全文訳に仮名が残らないこと、日本語へ戻せること、状態表示の3行が翻訳されることを確認します。
- 共通ヘルパーの内容が変わるため、可愛い・カッコいいの配布用インストーラーも、次に作るときは必ず再ビルドしてください（古いビルドフォルダのDLLは使いません）。
