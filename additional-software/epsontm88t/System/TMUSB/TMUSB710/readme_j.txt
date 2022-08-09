
                        TMUSB device driver version 7.10b

                                                                   2019/09/13
                                                     セイコーエプソン株式会社

１．はじめに
------------
    TMUSB は EPSON TM/EU/BAシリーズのデバイスと通信するための USB デバイスドラ
  イバーです。
    デバイスドライバーのインストールはデバイスウィザードに従って行いますが、本
  ユーティリティにて TMUSB デバイスドライバーのサイレントインストールが行えます
  。

２．TMUSB 動作環境
------------------
2.1 動作 OS
  ・ Microsoft(R) Windows(R) XP Professional SP3 32bit版
  ・ Microsoft(R) Windows(R) XP Professional SP2 64bit版
  ・ Microsoft(R) Windows Server(R) 2003 R2 SP2 32bit版
  ・ Microsoft(R) Windows Server(R) 2003 R2 SP2 64bit版
  ・ Microsoft(R) Windows Vista(R) SP2 32bit版
  ・ Microsoft(R) Windows Vista(R) SP2 64bit版
  ・ Microsoft(R) Windows Server(R) 2008 SP2 32bit版
  ・ Microsoft(R) Windows Server(R) 2008 SP2 64bit版
  ・ Microsoft(R) Windows Server(R) 2008 R2 SP1 64bit Version
  ・ Microsoft(R) Windows 7(R) SP1 32bit Version
  ・ Microsoft(R) Windows 7(R) SP1 64bit Version
  ・ Microsoft(R) Windows 8(R) 32bit Version
  ・ Microsoft(R) Windows 8(R) 64bit Version
  ・ Microsoft(R) Windows Server(R) 2012 64bit Version
  ・ Microsoft(R) Windows 8.1(R) 32bit Version
  ・ Microsoft(R) Windows 8.1(R) 64bit Version
  ・ Microsoft(R) Windows Server(R) 2012 R2 64bit Version


  ※ 上記OSをベースとしたEmbedded系OSにも対応しています。
  ※ Windows 10以降のOSにはTMUSB device driver version 8.00以降をインストールしてください。
  ※ Microsoft, Windows, Windows Serverは、米国 Microsoft Corporation の、米国
     、日本およびその他の国における登録商標または商標です。
    インテルは、アメリカ合衆国およびその他の国におけるIntel Corporationまたはそ
    の子会社の商標または登録商標です。
  ※ 引用した会社名・商品名は、各社の商標または登録商標です。

2.2 動作 USB ホストコントローラー
  ・ インテル(R) 製 チップセット組み込みの USB ホストコントローラー
  ・ NEC 製 EHCI USB ホストコントローラー

  ※ NEC 製 USB 1.1 用 の OHCI ホストコントローラーでの動作は保証していません。
  

2.3 動作 USB ドライバースタック
  Microsoft(R) 製(OS標準)のドライバースタックをお使いください。
  また、Microsoft(R) 製の USB ドライバースタックは最新のものをご使用
  ください。サードパーティ製の USB ドライバースタックでの動作は保証していません
  。
  
  ※ USB 2.0 High Speed 接続時には Microsoft(R)製 USB ドライバースタックusbehci
    .sys のファイルバージョンをご確認ください。
  
     Microsoft(R) Windows(R) XP   Professionalの場合
        5.1.2600.1243 以降
     Microsoft(R) Windows Server(R) 2003 の場合
        5.2.3790.1830 以降
     Microsoft(R) Windows Vista(R) の場合
        6.0.6000.16386 以降
     Microsoft(R) Windows Server(R) 2008の場合
        初回リリース以降
     Microsoft(R) Windows 7(R) 以降 の場合
        初回リリース以降

  ※ USB ドライバースタック最新版への更新方法
     1) インターネットに接続します
     2) コントロールパネルのシステムをダブルクリックします
     3) 「ハードウェア」タブを選択します
     4) 「デバイスマネージャー」ボタンを押します
     5) USB コントローラーを選択します
     6) 「xxx USB Enhanced Host Controller」をダブルクリックします
     7) 「ドライバー」タブの「ドライバーの更新」より最新版へ更新できます
  
  ※ ドライバースタック usbehci.sys のファイルバージョンの確認方法
    1-1) デバイスマネージャーから「xxx USB Enhanced Host Controller」をダブルク
         リック
    1-2) 「ドライバー」タブの「ドライバーの詳細」ボタンを押す
  
    または
  
    2-1) ウィンドウシステムディレクトリの system32\drivers\usbehci.sys ファイル
         を右クリック
    2-2) プロパティを選択し、バージョン情報で確認

  ※ USB 3.0 Super Speed ポートへのTMプリンターの接続は 
     Windows 8以降の Microsoft(R)製 USB ドライバスタックでのみ動作保証しています。

2.4 動作 USB HUB
  ・ USB 1.1 規格のHUB(Full Speed 対応)
  ・ USB 2.0 規格のHUB(High Speed 対応)
  ・ USB 3.0 規格のHUB(Super Speed 対応)
  ※ USB 1.0 規格の USB HUBの動作は保証していません。
  ※ USB 2.0 High Speed に対応した PC に、USB 2.0 HUB経由でデバイスを接続す
     ると、USB 2.0 High Speed に対応していないデバイスでも、USB 2.0 High Spe
     ed 接続となりますので、USB 2.0 High Speed 接続時の使用条件に従ってくだ
     さい。

2.5 動作デバイス
  ・EPSON TM/EU/BAシリーズのデバイス
  
2.6 動作 USB 接続
  ・最大 HUB 数         5 段
  ※ USB 2.0 規格に適合している USB ケーブル、USB HUBを使用すること
  
2.7 省電力について
  Windows Vista/2008の省電力機能は
  デフォルト無効になっています。
  有効にしたい場合は、-p1オプションを指定してインストールしてください。
  その場合、デバイスマネージャーから、USBルートハブの電力管理の設定の[電力の節
  約のために、コンピューターでこのデバイスの電源をオフにできるようにする]を必ず
  有効にしてください。

３．ファイルの説明
------------------
  以下のようなフォルダ・ファイル構成となっています。

   + TMUSB710b
     + SETUP.exe
     + TMUSBXP ..................32bitOS用のTMUSBが入っているフォルダ
     + TMUSB64 ..................64bitOS用のTMUSBが入っているフォルダ
     + ReadMeJ.txt
     + ReadMeE.txt

４．デバイスドライバーのインストール方法
----------------------------------------

  インストール/アップデート作業を行う場合は管理者権限で実行する必要があります。

4.1 ドライバーのインストール/アップデート手順

  1) USB ケーブルで接続されたすべてのデバイスの電源をOFFにします
  2) コマンドプロンプトでSETUP プログラムを実行します。必要に応じて起動時オプションをつけてください。
     起動時オプションの詳細は4.2．起動時オプションを参照ください。
  3) インストールが完了したらデバイスの電源を投入します

    使用許諾契約が表示されます。内容をご確認頂き同意される場合は、「使用許諾契
    約に同意する」を選択し、インストールボタンを押してください。
    
    SETUP プログラムが ドライバ情報ファイル(INF)とドライバファイル(SYS)をシステ
    ムにコピーします。

    セットアッププログラムはインストール結果を 環境変数 ERRORLEVEL に返します
      0    : 成功した
      2    : インストールファイルが見つからなかった
      1151 : サポートされていない OS で実行した
      1223 : ユーザがインストールをキャンセルした
      3010 : インストールは成功したが、システムのリブートが必要

4.2．起動時オプション

    -s2 サイレントインストール
        既にインストールされているドライバが新しければインストールしない
    -p1 デバイス省電力機能を有効にします
    -p2 デバイス省電力機能を無効にします

4.3. TM-C100のインストール時の注意

    TM-C100 本体をホストPCに接続する前に必ず、TMUSB の SETUP プログラムを
  実行、または、TM-C100 用のプリンタドライバを先にインストールしてください。
    もし、TM-C100 を PC に接続し電源を入れてもデバイスマネージャに
 「EPSON USB Controller for TM/BA/EU Series」が表示されない場合は、以下
  の手順で復帰させてください。

    1) TM-C100 を PC に接続し電源を入れます
    2) デバイスマネージャに「USB 印刷 Support」が表示されている場合「USB Printi
       ng Support」を右クリックし「削除」を選択します
    3) TM-C100 の電源を入れ直します
    4) デバイスマネージャに「EPSON USB Controller for TM/BA/EU Series」が表示さ
       れるか確認します

５．制限事項
------------
  ・ インストールは必ず管理者権限で行ってください
  ・ TMUSBドライバーを利用しているアプリケーションを終了してからセットアッププ
     ログラムを実行してください
  ・ プリンターに印字中に、電源のOFF/ONや、ケーブルの抜き差しはしないでください
  ・ デバイスの電源をOFFにした後、TMUSBドライバーがアンインストールされる時間
     (約５秒)待ってからデバイスの電源をONしてください
  ・ デバイスドライバがロードされている状態（デバイスの電源がONの状態）でセット
     アッププログラムを実行した場合には、メモリ中のデバイスドライバの更新が行わ
     れません。デバイスの電源OFF/ONまたはシステムの再起動を行ってください
     
  ・ サイレントインストーラをご使用になる場合には、インターネットエクスプローラ
     ーVersion 4.0 以降が必要です
  ・ デバイスの動作中は、OSの制限でコンピューターをスタンバイや休止状態へ移行で
     きない場合があります
     コンピューターをスタンバイ/休止状態へ移行中にデバイスの電源を切る、もしく
     はUSBのケーブルを抜くと、OSの制限でシステムが不安定になることがあります。
     印刷アプリケーションを終了、もしくはデバイスの電源をOFFにしてからコンピュ
     ーターをスタンバイ/休止状態にしてください

６．TMUSB ドライバ履歴
----------------------
  ・ 2019/09/13 Version 7.10b Windows 10以降をサポートOSから除外
  ・ 2017/12/01 Version 7.10a High Speed不具合対応
  ・ 2016/01/08 Version 7.00a USB Filterに対応 Windows2000非サポート
  ・ 2012/03/15 Version 6.10a Windows 7での強制スタンバイ問題を対処
  ・ 2012/01/27 Version 6.00a 省電力関連のブルースクリーン問題を対応
  ・ 2009/11/25 Version 5.10a スキャナー画像受信に失敗する問題を対応
  ・ 2009/09/17 Version 5.00a Windows 7(R)対応、非同期通信対応
  ・ 2009/04/06 Version 4.00c XP 64bit署名 追加取得
  ・ 2009/01/13 Version 4.00b 64bit環境でのインストール不具合を対処
  ・ 2008/10/31 Version 4.00a TMCOMUSBドライバと他のドライバとの共存対応
  ・ 2008/05/20 Version 3.20d TMUSBのインストーラのメモリアクセス違反不具合を対処
  ・ 2008/01/28 Version 3.20c TMUSBのインストーラの不具合を対処
  ・ 2007/10/29 Version 3.20b Windows(R) 2000のWHQLドライバ署名の不具合を対処
  ・ 2007/09/26 Version 3.10b TM-S1000 CDパッケージ用
  ・ 2007/09/07 Version 3.10a TM-S1000対応,Windows Vista(R) 対応 
  ・ 2007/01/19 Version 2.20a スタンバイに移行できない不具合対応
  ・ 2005/12/16 Version 2.10a TM-T88IV 省電力対応
  ・ 2005/10/28 Version 2.06a TMCOMUSB対応(ドライバ署名なし)
  ・ 2004/10/15 Version 2.00a J9000/TMCOMUSB 対応版
  ・ 2004/01/15 Version 1.91a High Speed HUB 対応版
  ・ 2003/09/25 Version 1.81a Windows(R) 2000 SP4 正式対応版

--- EOF ---
