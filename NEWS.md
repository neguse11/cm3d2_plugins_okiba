### 2015/09/14 (2)

 - [ConsoleCodePage](https://gist.github.com/asm256/9bfb88336a1433e2328a) 2015.9.3.3 を追加

### 2015/09/14

 - MaidVoicePitch
    - AddModsSlider用のスライダー更新コールバック名を変更
 - [AddModsSlider](https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin)
    - 新しいコールバック名を用いるバージョン(Version 0.1.1.12)に更新

### 2015/09/13

 - バッチファイルのエラー処理を変更
 - MaidVoicePitch
    - AddModsSlider用のスライダー更新コールバックを追加
 - [AddModsSlider](https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin)
    - NGUI版へ更新
 - ReiPatcher 0.9.0.8, UnityInjector 1.0.1.3 対応

### 2015/09/11

 - LogWindowを追加 (その３>>8)
 - UnityInjector_1.0.1.2 の URL を変更
 - megadl
    - base64内の '_' への対応を追加
 - MaidVoicePitch
    - 顔がモーションに追従している場合はオフセット角度をつけないように修正 (その２>>41)

### 2015/09/10

 - developブランチの導入方法を修正 (その２>>25, >>28)
 - VoiceNormalizer
    - テーブル生成コードTableBuilderを追加 (VC++2015用)
 - [AddModsSlider](https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin)
    - githubから Version.0.1.0.7 r1 のコードを取るように変更 ([その２>>17](https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin/commit/c26e907c9b9d0f0aa606e721586be8a90689a005))

### 2015/09/09 (2)

 - 音声の音量補正を行う「VoiceNormalizer」を追加


### 2015/09/09

 - 全体のコード構成を変更
 - csc.exe の認識をレジストリベースで行うように変更 ([ArchiveReplacer](https://gist.github.com/asm256/8f5472657c1675bdc77a)のコードをパクらせてもらいました)
 - AddModsSlider
    - MaidVoicePitch の更新に合わせて ModsParam.xml のサンプルを更新
       -  HEAD_TRACK (顔の追従) と EYE_TRACK (目の追従) を追加
       - 「リップシンク強度」が動作していなかったので修正
       - 「FACE_ANIME_SPEED」を削除
 - ExternalSaveData
    - セーブデータに余分なメイドを保存しないように修正
 - MaidVoicePitch
    - HEAD_TRACK (顔の追従) と EYE_TRACK (目の追従) を追加
    - ModsParam.xml の変更例は「履歴」項を参照
 - PersonalizedEditSceneSettings
   - リリースビルド時のログ出力を抑制
