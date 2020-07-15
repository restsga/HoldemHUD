using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HoldemHUD
{
    static class PokerStars_Strings
    {
        //HandID
        public const string HandIdSearchTag = "PokerStars";
        public const char HandIdHeader = '#';
        public const char HandIdFooter = ':';
        //トーナメント
        public const string Tournament = "Tournament";
        //ノーリミットホールデム
        public const string NoLimitHoldem = "Hold'em No Limit";
        //ディーラーボタン
        public const string ButtonSearchTag = "is the button";
        public const char ButtonHeader = '#';
        public const string ButtonFooter = " is the button";
        //プレイヤー数
        public const string MaxPlayer_9 = "9-max";
        //座席番号
        public static readonly Regex SeatSearchTag =new Regex("Seat [1-9]: ",RegexOptions.Compiled);
        public const string SeatHeader = "Seat ";
        public const string SeatFooter = ": ";
        //プレイヤー名
        public static readonly Regex NameFooter = new Regex(@" \(\$?[0-9]+(.[0-9]*)? in chips\)",RegexOptions.Compiled);

        //ホールカード
        public const string HoleCards = "*** HOLE CARDS ***";
        //フロップ
        public const string Flop = "*** FLOP ***";
        //ターン
        public const string Turn = "*** TURN ***";
        //リバー
        public const string River = "*** RIVER ***";
        //ショーダウン
        public const string Showdown = "*** SHOW DOWN ***";
        //サマリ
        public const string Summary = "*** SUMMARY ***";

        //配られたカード
        public const string DealtSearchTag = "Dealt to ";
        //プレイヤー名とアクションの間の記号
        public const string Name_Action = ": ";

        //アクション
        public const string Flod = "folds", Call = "calls", Check = "checks", Bet = "bets", Raise = "raises";
    }
}
