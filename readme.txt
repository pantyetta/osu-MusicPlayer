﻿ダウンロードして頂きありがとうございます!

--新機能--
bpm違いの曲も区別して検索、再生できるようになりました。
パス設定時にキャンセルすると何もできなくなるバグ修正。
聞いている曲がDiscordからSpotifyみたいに確認出来るようになりました。
パスを変えてもプレイリストなどが再生されるようになった。(*要注意あり)

* ver.0.7.1以降を使っていた場合はプレイリストやブラックリストをメモ帳で編集し以下の部分をすべて置き換え機能などで削除してください。
 再生が出来ません。
 例：D:osu!\songs\817560 Jashin_Girls - Anoko ni Drop Kick (TV Size)\audio.mp3
					    		 ↓
	  \817560 Jashin_Girls - Anoko ni Drop Kick (TV Size)\audio.mp3

------------------------------------------------------------------------


●使用上の注意事項
ゲームOsu!(https://osu.ppy.sh/home) の曲を聞くため専用のミュージックプレイヤーです。
.NET Framework 4.7.2が必須となります。
そのため違うファイルを読み込もうとすると、エラーが出て落ちます。
×ボタンを押してもタスクセンターに収納され、バックグラウンドで再生を続けます。閉じる場合はアプリ上部のメニューよりファイル(F)→終了 又はタスクアイコンを右クリック→終了 を押して下さい。
連打など連続して押そうとすると固まることがあります。
また、このアプリはぱんちぇったがC#の練習を兼ねて作ったものですのでバグがあるかもしれません。その場合は以下のtwitterまたはgithubよりご連絡ください。


●使い方
--基本的な使い方--
1. 初回起動時にはosu!のフォルダーが聞かれますので指定してください。(曲数によって時間がかかります。)
2. playボタンを押すことによってランダムに曲が流れます。
3. Nextボタン横のバーは音量調節用です。
4. アプリ上部のメニューより再生(S)→リピート で現在の曲をリピートします。

--曲検索について--
1. アプリ上部のメニューよりファイル(F)→曲検索を押すことによって曲検索ウィンドウが出ます。
2. 検索したい曲名・アーティスト・タグ(基本ローマ字・英語、一部日本語対応)を入力して検索できます。
3. 再生したい物を押すと自動的に再生されます。

--プレイリストについて--
1. アプリ上部のメニューよりプレイリスト(P)→選択→追加 よりプレイリストを作成してください。
2. 再生してる曲を追加したい場合はアプリ上部のメニューよりプレイリスト(P)→選択→プレイリスト名 を選び音量バー横のチェックボックスにチェックを入れてください。
3. プレイリストから消したい場合はプレイリストを選択した状態でチェックボックスのチェックを外してください。
4. 上部のメニューよりプレイリスト(P)→削除→プレイリスト名 でそのプレイリストを削除できます。(確認メッセージが出ます)
※ブラックリストに入れることで再生されなくなります。間違えて入れてしまった場合は本アプリのディレクトリのblacklistからその一行を消してください。


注意！！
プレイリストに追加した後そのまま曲を聴くとプレイリスト内の曲しか流れません。そのため他の曲を聴くには検索機能を使うかメニューよりプレイリスト(P)→選択→選択しない を押して下さい。

--タスクトレイでの操作--
1. 再生、スキップ等はタスクアイコンを右クリック→コントロール より選択してください。
2. プレイリストに追加したい場合はアイコンを右クリック→プレイリストに追加 より追加したいプレイリストを選択してください。


●設定について
設定項目はosuのファイル場所変更・最大音量の変更(0-100)・曲の更新です。
osu!で新しく曲を追加したら手動で上部のメニューより設定(O)→曲の更新 をして下さい。(曲数により時間がかかります。)


------------------------------------------------------------------------

・転載、2次配布は禁止です。
・場合により配布を止めることがあります。

仕様ツール
visual Studio 2019

2020年9月11日
Osu! Music Player
ver.0.7.1
ぱんちぇった
twitter: https://twitter.com/Pantyetta1226
github: https://github.com/pantyetta