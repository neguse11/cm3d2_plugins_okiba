## 拡張セーブデータに追加されるデータ

拡張セーブデータ内の `/savedata/maids/maid/plugins/plugin[@name = "CM3D2.MaidVoicePitch"]/prop` に保存されます。
タグの構成は以下のようになります

```
<savedata target="...">
  <maids>
    <maid guid="..." lastname="..." firstname="..." createtime="...">
      <plugins>
        <plugin name="CM3D2.MaidVoicePitch">
          <prop name="名前その１" value="値その１" />
          <prop name="名前その２" value="値その２" />
          ...
        </plugin>
```

「名前」と「値」の一覧は以下の通りです

 - 「XMLファイル名」は、"test.xml" を指定した場合 "C:\KISS\CM3D2_KAIZOU\test.xml" を読み込みます

| 名前                    | 値            | 既定値 | 概要 |
| ------                  | ------        | ------ | ---- |
| PITCH                   | -1.0～1.0     | 0      | 声のピッチを変更。+0.05なら5%速く、-0.16なら16%遅く再生 |
| SLIDER_TEMPLATE         | XMLファイル名 |（なし）| エディット画面のスライダーの限界値設定ファイルを指定 |
| FACE_SCRIPT_TEMPLATE    | XMLファイル名 |（なし）| スクリプト内の@Faceと@FaceBlendによる表情指定を置き換えるファイルを指定 |
| PROPSET_OFF             | True/False    | False  | Trueなら、スクリプト内の@PropSetによる首や肩スライダーの補正を抑制 |
| FACE_OFF                | True/False    | False  | Trueなら、スクリプト内の@Faceによる表情変化を抑制 |
| FACEBLEND_OFF           | True/False    | False  | Trueなら、スクリプト内の@FaceBlendによる頬と涙の表情変化を抑制 |
| EYETOCAMERA_OFF         | True/False    | False  | Trueなら、スクリプト内の@EyeToCameraによる視線指定を抑制 |
| MUHYOU                  | True/False    | False  | Trueなら、全ての表情変化を抑制 |
| HYOUJOU_OFF             | True/False    | False  | Trueなら、目と口の表情変化を抑制 |
| LIPSYNC_OFF             | True/False    | False  | Trueなら、リップシンク(口パク)を抑制 |
| LIPSYNC_INTENISTY       | True/False    | False  | Trueなら、リップシンク(口パク)強度設定を行う |
| LIPSYNC_INTENISTY.value | 0.0～1.0      | 1      | リップシンク(口パク)強度。1.0で最大、0.0で最小 |
| EYETOCAM                | -1,0,1        | 0      | 視線方向制御。1:常にカメラを向く、-1:常にカメラを向かない、0:スクリプトに従う |
| HEADTOCAM               | -1,0,1        | 0      | 顔方向制御。1:常にカメラを向く、-1:常にカメラを向かない、0:スクリプトに従う |
| MABATAKI                | 0.0～1.0      | 1      | 目の開度。1.0で最大(目を開く)、0.0で最小(目を閉じる) |
| EYEBALL.width           | 0.0～5.0      | 1      | 瞳の幅。1.0で通常、0.0で最小 |
| EYEBALL.height          | 0.0～5.0      | 1      | 瞳の高さ。1.0で通常、0.0で最小 |
| FARMFIX                 | True/False    | False  | Trueなら、前腕補正処理を行い、前腕が細くなるのを抑制する |
| WIDESLIDER              | True/False    | False  | Trueなら、拡張スライダー処理を行い、スライダーの限界値を大きくする |
| EYE_ANG.angle           | -60.0～+60.0  | 0      | 目の角度 |
| EYE_ANG.x               | -999～+999    | 0      | 目の回転中心位置の補正値(1) |
| EYE_ANG.y               | -999～+999    | 0      | 〃 (2) |
| THISCL.depth            | 0.1～5.0      | 1      | 足全体のスケーリング(奥行き) |
| THISCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| THIPOS.x                | -999～+999    | 0      | 足の位置 (左右) |
| THIPOS.z                | -999～+999    | 0      | 〃  (前後) |
| PELSCL.height           | 0.1～5.0      | 1      | 骨盤スケーリング (高さ) |
| PELSCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| PELSCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| SKTSCL.height           | 0.1～5.0      | 1      | スカート周辺スケーリング (高さ) |
| SKTSCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| SKTSCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| SPISCL.height           | 0.1～5.0      | 1      | 胴(下腹部)スケーリング (高さ) |
| SPISCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| SPISCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| S0ASCL.height           | 0.1～5.0      | 1      | 胴(腹部)スケーリング (高さ) |
| S0ASCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| S0ASCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| S1_SCL.height           | 0.1～5.0      | 1      | 胴(みぞおち)スケーリング (高さ) |
| S1_SCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| S1_SCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| S1ASCL.height           | 0.1～5.0      | 1      | 胴(肋骨)スケーリング (高さ) |
| S1ASCL.depth            | 0.1～5.0      | 1      | 〃 (奥行き) |
| S1ASCL.width            | 0.1～5.0      | 1      | 〃 (幅) |
| HEAD_TRACK              | True/False    | False  | 顔と目の追従動作の改善 |
| HEAD_TRACK.lateral      |  0.0～90.0    | 30     | 顔の横方向可動角度 |
| HEAD_TRACK.above        |-90.0～90.0    | 30     | 〃上方向〃 |
| HEAD_TRACK.below        |-90.0～90.0    | 15     | 〃下方向〃 |
| HEAD_TRACK.behind       |  0.0～180.0   | 90     | 顔が追従をあきらめる角度 |
| HEAD_TRACK.speed        |  0.0～1.0     |  0.04  | 顔の回転速度 |
| HEAD_TRACK.ofsx         |-90.0～90.0    |  0     | 横回転オフセット (負で右、正で左を向く) |
| HEAD_TRACK.ofsy         |-90.0～90.0    |  0     | 縦回転オフセット (負で下、正で上を向く) |
| HEAD_TRACK.ofsz         |-90.0～90.0    |  0     | 回転オフセット (負で右、正で左にかしげる) |
| EYE_TRACK.inside        |  0.0～90.0    | 35     | 目の内側可動角度 |
| EYE_TRACK.outside       |  0.0～90.0    | 65     | 〃外側〃 |
| EYE_TRACK.above         |-90.0～90.0    | 10     | 〃上方向〃 |
| EYE_TRACK.below         |-90.0～90.0    | 20     | 〃下方向〃 |
| EYE_TRACK.behind        |  0.0～180.0   | 90     | 目が追従をあきらめる角度 |
| EYE_TRACK.speed         |  0.0～1.0     |  0.07  | 目の回転速度 |
| EYE_TRACK.ofsx          |-90.0～90.0    |  0     | 横オフセット (負で寄り目、正で離れ目) |
| EYE_TRACK.ofsy          |-90.0～90.0    |  0     | 縦オフセット (負で下、正で上) |
