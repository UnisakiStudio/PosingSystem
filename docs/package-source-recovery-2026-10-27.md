# Unity検証中のパッケージ正本復旧

2026-10-27 15時台、正本への反映準備で6パッケージのソースフォルダが空になっていることを検出。各フォルダ更新時刻は14:26:03〜05で、最初のUnityバッチ起動に一致する。

前の検証で、外部VPM正本を参照する元Junctionと追加のhost-linkを `Temp/Run001PackageLinks` に退避していた。このフォルダとmanifestも消えている。Unity起動時のTemp削除がJunctionの参照先へ及んだことが強く疑われる。意図的に再現して追加データを削除する試験はしていない。

これは検証用の退避場所の選び方に問題があった。今後、UnityのTemp/Libraryなど自動削除される場所へ、生きたJunction・symlinkを保存しない。退避スクリプトの既定先を `Recordings/PackageLinkBackups` に変更した。バッチ起動スクリプトはTemp/Libraryをリンク先に降りずに検査し、Junction・symlink・未知のreparse tagがあれば起動を拒否する。[Microsoftの定義するCLOUDタグ](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-fscc/c8e77b37-3909-4fe6-a4ea-2b9d423b1ee4)のOneDrive属性だけを許可する。PowerShellからfsutilを毎回起動する試作は遅かったため停止し、Pythonのlstatでタグを直接調べる方式へ変更。42,810項目の検査後、再起動しても正本6パッケージが保持されていることを確認した。

## 復旧

Gitコミットと、以前ハッシュ検証して作った開発プロジェクトの実体コピーは残っていた。Unityの追加起動を止め、開発コピーの全ファイルSHA-256、正本HEAD、復旧前statusを記録。すべての復旧先が空であることを確認し、存在しないファイルだけを新規作成した。既存ファイルの上書き・削除・Git resetは行っていない。

| 正本 | 復旧・ハッシュ再照合したファイル数 |
|---|---:|
| PosingSystem-3.1.0 | 391 |
| KawaiiPosing-3.1.0 | 7,466 |
| CoolPosing-3.1.0 | 445 |
| PerfectLove（Git管理外） | 104 |
| VirtualBed | 256 |
| VirtualLove | 124 |

PosingSystemのコピーには進行中のController生成器の変更も含まれる。復旧後も通常の未コミット差分として扱い、試験完了後に最終同期する。VirtualBedの既存未コミット内容も開発コピーから復旧した。

証跡: `Recordings/PackageSourceRecovery_20261027/plan.json`、各 `*-verified.json`、`completed.json`。復旧コードは `Tools/AnimatorController/recover_package_sources.py`。二重実行、ハッシュ変化、既存ファイルへの上書きを拒否する。

退避補助スクリプトはさらに、生きたバックアップJunction自体を保存しない方式へ変更した。対象は `jp.unisakistudio.*` のJunctionのみ。実体コピーとハッシュ確認を先に行い、参照先メタデータを記録して、リンクのエントリだけを非再帰で削除する。実体コピーの配置に失敗した場合は元のリンクを作り直す。この改訂スクリプトを本プロジェクトへ再適用はしていない。現在は復旧済みの実体コピーを保持する。
