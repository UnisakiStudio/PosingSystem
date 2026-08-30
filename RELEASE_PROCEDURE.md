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
5. KawaiiPosingのVPM一覧workflowは`source.json`変更時以外は自動起動しないため、Release成功後に`C:\Users\gaoth\OneDrive\ドキュメント\UnityProjects\VPM\releaseKawaiiPosing.sh`を実行する。このスクリプトが次の必須2リポジトリを両方更新する。片方だけで完了としない。

   ```bash
   gh workflow run "Build Repo Listing" -R unisaki-studio/KawaiiPosing --ref main
   gh workflow run "Build Repo Listing" -R UnisakiStudio/KawaiiPosing --ref main
   ```

6. 両方のworkflowを個別に監視し、GitHub PagesへのDeploy成功を確認する。

   ```powershell
   gh run list -R unisaki-studio/KawaiiPosing --workflow "Build Repo Listing" --limit 3
   gh run watch <run-id> -R unisaki-studio/KawaiiPosing --exit-status
   gh run list -R UnisakiStudio/KawaiiPosing --workflow build-listing.yml --limit 3
   gh run watch <run-id> -R UnisakiStudio/KawaiiPosing --exit-status
   ```

7. 次の両方の`index.json`を取得する。どちらもHTTP 200で、同じ新バージョンを配信していることを確認する。
   - `https://unisaki-studio.github.io/KawaiiPosing/index.json`
   - `https://UnisakiStudio.github.io/KawaiiPosing/index.json`

   両indexで次を確認する。
   - PosingSystemの新バージョンが存在する。
   - KawaiiPosingの新バージョンが存在する。
   - KawaiiPosingの依存先が新しいPosingSystem版になっている。

## 5. Xでの告知

KawaiiPosingのリリース告知は、過去のリリース告知ツリーを文体・構成の正本とする。

- 正本: `https://x.com/unisakistudio/status/2093393727092064647`
- 不具合調査中の投稿や引用ポストではなく、上記ツリー内の各バージョン告知を複数確認して踏襲する。
- 1ポストを改行込み140文字以内に収める。文案を提示する前に必ず文字数を数える。
- 絵文字、ハッシュタグ、リンク、独自の見出しは、正本の形式にないため勝手に追加しない。
- 商品名、`Ver`表記、「アップデートいたしました」「内容は、」「併せて」の語調を揃える。
- KawaiiPosingのバージョンを最初に告知し、主な修正内容、PosingSystemの追随バージョンの順に書く。

基本形:

```text
『可愛いポーズツール』をVer<KawaiiPosing版>にアップデートいたしました
内容は、<利用者に伝わる主な修正内容>です
併せて『ゆにさきポーズシステム』もVer<PosingSystem版>に更新しています
```

3.0.10 / 3.0.11で使用した文案（改行込み121文字）:

```text
『可愛いポーズツール』をVer3.0.10にアップデートいたしました
内容は、NDMF最新版でプレビルドに失敗する不具合と、エラー表示時に別の例外が発生する不具合の修正です
併せて『ゆにさきポーズシステム』もVer3.0.11に更新しています
```

投稿前に、文案を文字列へ入れて文字数を確認する。

```powershell
$post = @'
ここに投稿文
'@
$post.TrimEnd().Length
```

投稿後は、バージョン・修正内容・誤字を実際のポストで確認する。Codexが投稿操作まで依頼された場合は、送信直前にユーザーの確認を取る。

## 6. 完了報告

次をまとめて記録・報告する。

- Unityのバージョン、テスト総数・成功数・失敗数、実施した手動チェック
- 両リポジトリのcommit SHA
- Release workflow、および新旧両KawaiiPosingリポジトリの一覧workflowのrun URLと成否
- 両Release URLとasset名
- 新旧両URLのVPM indexに反映されたバージョンと依存関係
- 触らず維持したstash・未追跡ファイル

VirtualLoveなど他製品は`vpmDependencies`の範囲で新しいPosingSystemを取得できる場合、製品固有変更がなければ不要な追随リリースを増やさない。厳密依存の更新や製品側の修正が必要なときだけ、同じ検証手順で別途リリースする。
