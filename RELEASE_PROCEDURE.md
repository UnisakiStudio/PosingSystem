# PosingSystem リリース手順

PosingSystemの修正をKawaiiPosingとVPM一覧へ反映するための手順です。別セッションでも、作業前にこのファイルと各リポジトリの現在状態を読み直してください。

## 対象リポジトリ

- `C:\Users\gaoth\OneDrive\ドキュメント\UnityProjects\VPM\PosingSystem`
- `C:\Users\gaoth\OneDrive\ドキュメント\UnityProjects\VPM\KawaiiPosing`
- Unity検証用プロジェクト: `C:\Users\gaoth\OneDrive\ドキュメント\UnityProjects\UnisakiStudioTools`

`UnisakiStudioTools/Packages/jp.unisakistudio.posingsystem`と`jp.unisakistudio.kawaiiposing`は上記リポジトリ内PackageへのJunctionです。

## 1. 作業前確認

1. `git status --short --branch`、`git log --oneline origin/main..HEAD`、`git stash list`を両リポジトリで確認する。
2. stash、未追跡ファイル、今回と無関係な差分は変更・stage・削除しない。
3. 特にPosingSystemの`Packages/jp.unisakistudio.posingsystem/Resources/Generated.meta`はUnity生成物なのでリリースへ含めない。
4. Gitが`dubious ownership`を報告する環境では、グローバル設定を変更せず、各コマンドへ次を付ける。

   ```powershell
   git -c safe.directory='C:/Users/gaoth/OneDrive/ドキュメント/UnityProjects/VPM/PosingSystem' -C 'C:/Users/gaoth/OneDrive/ドキュメント/UnityProjects/VPM/PosingSystem' status --short --branch
   ```

## 2. Unity検証

1. Unity 2022.3.22f1で`UnisakiStudioTools`を開き、コンパイルエラーがないことを確認する。
2. PosingSystemのEditModeテストassembly `jp.unisakistudio.posingsystemeditor.tests`を実行する。
3. 修正箇所の回帰テスト名を確認し、テスト総数・成功数・失敗数を記録する。
4. `RELEASE_CHECK_AI.md`に従い、対象アバターでプレビルドとNDMF/VRChatの安全なリリースチェックを行う。
5. Unityを開いたままCLIテストを実行しない。同一projectの`UnityLockfile`と衝突するため、Editorを閉じるか、Packages・ProjectSettings・必要Sceneを一時projectへ複製して実行する。
6. NDMFプレビューとTest Runnerが未保存の無題Sceneで衝突する場合、保存済みSceneを起動Sceneにする。一時変更した`Library/LastSceneManagerSetup.txt`は検証後に元へ戻す。
7. テスト失敗、結果XML未生成、コンパイルエラーのいずれかがある場合はリリースしない。Unityの終了コード0だけではテスト合格とみなさない。

## 3. PosingSystemの版上げ

1. パッチ版を1つ上げ、次を同じ版へ揃える。
   - `Packages/jp.unisakistudio.posingsystem/package.json`の`version`
   - 同ファイルのRelease ZIP `url`
   - `README.md`のVersion
   - `CHANGELOG.md`の版・日付・修正内容
2. `git diff --check`とJSON parseを実行する。
3. 対象ファイルだけを明示的にstageし、`Ver x.y.z`でcommitする。既存の修正commitと版上げcommitが`origin/main`より先に並んでいることを確認する。
4. `git fetch origin`後に、remoteが進んでいないことを確認する。進んでいた場合は内容を確認して安全にrebaseし、再検証する。
5. `main`をpushする。`package.json`のpushで`Build Release`が起動する。

   ```powershell
   gh run list -R UnisakiStudio/PosingSystem --workflow release.yml --limit 3
   gh run watch <run-id> -R UnisakiStudio/PosingSystem --exit-status
   gh release view <version> -R UnisakiStudio/PosingSystem
   ```

6. ReleaseにZIPとunitypackageがあり、package.jsonのZIP URLがHTTP 200で取得できることを確認する。

## 4. KawaiiPosingの追随リリース

1. PosingSystem Release成功後に、KawaiiPosingのパッチ版を1つ上げる。
2. 次を更新する。
   - `package.json`の`version`とRelease ZIP `url`
   - `dependencies.jp.unisakistudio.posingsystem`を新しい厳密版
   - `vpmDependencies.jp.unisakistudio.posingsystem`を新しい`^`版
   - `README.md`のVersion
   - `CHANGELOG.md`
3. 対象ファイルだけをstageし、`Ver x.y.z PosingSystem x.y.zに更新`でcommit・pushする。
4. `Build Release`を監視し、Release assetとHTTP 200を確認する。
5. KawaiiPosingのVPM一覧workflowは`source.json`変更時以外は自動起動しないため、Release成功後に手動実行する。

   ```powershell
   gh workflow run build-listing.yml -R UnisakiStudio/KawaiiPosing --ref main
   gh run list -R UnisakiStudio/KawaiiPosing --workflow build-listing.yml --limit 3
   gh run watch <run-id> -R UnisakiStudio/KawaiiPosing --exit-status
   ```

6. `https://UnisakiStudio.github.io/KawaiiPosing/index.json`を取得し、次を確認する。
   - PosingSystemの新バージョンが存在する。
   - KawaiiPosingの新バージョンが存在する。
   - KawaiiPosingの依存先が新しいPosingSystem版になっている。

## 5. 完了報告

次をまとめて記録・報告する。

- Unityのバージョン、テスト総数・成功数・失敗数、実施した手動チェック
- 両リポジトリのcommit SHA
- GitHub Actionsのrun URLと成否
- 両Release URLとasset名
- VPM indexの反映バージョンと依存関係
- 触らず維持したstash・未追跡ファイル

VirtualLoveなど他製品は`vpmDependencies`の範囲で新しいPosingSystemを取得できる場合、製品固有変更がなければ不要な追随リリースを増やさない。厳密依存の更新や製品側の修正が必要なときだけ、同じ検証手順で別途リリースする。
