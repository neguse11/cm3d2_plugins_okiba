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
