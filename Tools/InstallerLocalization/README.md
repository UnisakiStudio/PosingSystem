# 導入プログラムの多言語化

言語だけを共通の `EditorPrefs` キー `jp.unisakistudio.posingsystem.language` に保存します。明示した選択を優先し、未選択なら `Application.systemLanguage` と地域情報を使い、非対応の言語なら英語を使います。Unityの表示言語には依存しません。macOS/Linuxでも同じAPIを使用できます。

EditorPrefsは同じOSユーザーのUnityプロジェクト間で共有できます。WindowsはUnityのレジストリ領域、macOSはユーザーのplist、LinuxはユーザーのUnity設定ファイルにUnityが保存します。別PCへの同期は行いません。保存値は公開された言語コードだけです。ライセンスのレジストリ／ファイル保存、登録値、判定、削除対象は既存のままです。

- Unity API: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorPrefs.html
- OS言語: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-systemLanguage.html

## 動作

ライセンス確認とVPAIの確認・完了・エラーダイアログから「Language...」を開けます。選択を適用すると、対象商品・バージョン・ファイル名を保持して元の確認画面に戻ります。インストールの同意は元の確認画面で行います。ツール追加後はInspector最上部の言語選択を使い、ゲーム内メニューも同じ値を使います。アバターに保存済みの言語は引き続き尊重します。

MenuItemの動的登録はUnity 2022.3内部APIを限定的に使用します。導入前は独立した共有ソース、導入後は本体のアセンブリが登録を担当します。元の優先順位、検証関数、コールバックを保持し、登録時にはコマンドを実行しません。内部APIがないUnityでは静的メニューを残します。

### メニュー／全文訳変更時の必須確認（2026-11-09）

レビュー済みの文章単位の訳は、全言語で組み合わせ用の `message_models` より優先します。基本語の合成で完成済みの訳を上書きしないでください。特にネイティブMenuItemの表示名へ改行を入れず、登録パスはJSON配列で保存します。旧版の改行入りライセンスメニューを識別する移行aliasも保持します。

新しいMenuItemを追加したら、通常Inspectorのcatalogだけでなく、導入前にも動くbootstrapのaliasと全対象言語を確認してください。言語切替後に新しい表示があることだけでなく、**旧言語の登録が消えること**と日本語への復帰を検証します。`PosingSystemNativeMenuTests` は30言語の連続切替、旧改行パスの除去、末尾空白、Console相談、登録時にコマンドを実行しないことを確認します。

Unity内部の `MenuItemExists` は末尾空白付きパスにfalseを返す場合があります。削除は実際のメニュー一覧のパスを基準とし、自社のサブメニューだけを検査して無関係なUnity／他社コマンドを翻訳解析しないようにします。

30言語の導入用基本41語と、それを組み合わせた確認・結果・進捗文言を同梱します。詳細な診断はConsoleへ保持します。通常のInspector全文言・全姿勢名・オンラインマニュアルの30言語翻訳とは別の範囲です。タイ語・インド系言語・RTLのフォントと字形は実画面の確認が必要です。

## ソースと生成

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

依存範囲は元の `3.x.x` / `1.x.x` を保ちます。必要なら `--dependency jp.unisakistudio.kawaiiposing=3.2.x` のように明示指定できます。開発版本体が公開されるまで試作インストーラーだけを取り込んでも、公開済みの本体が導入されます。リリース前に依存範囲と公開先を確定してください。

**完成したVPAI DLLを開発中のUnityのAssetsに直接置かないでください。** 上流と同様、読み込むだけで自動導入と自己削除が動きます。Unity内で試す場合は専用プロジェクトで事前に `VPM_PACKAGE_AUTO_INSTALLER_DEV_ENV` を定義し、実行対象のconfigと導入先を確認してください。
