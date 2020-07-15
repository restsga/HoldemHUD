using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HoldemHUD
{
    public class PokerStars_Analyse
    {
        //プレイヤーごとの分析結果
        public List<PlayerData> playerDatas = new List<PlayerData>();
        //分析済みのハンドID
        public List<long> analyzed = new List<long>();

        public void Analyze_Lines(List<string> lines)
        {
            //ハンドIDの探索フラグ
            bool search_id_flag = true;
            //BTNのSeat番号
            int btn=-1;
            //Seat位置と名前の対応
            List<string> seats_names = new List<string>();
            //BTNのindex
            int btn_index=-1;
            //アクションのフラグやフローの管理クラス
            PokerStars_Action stars_action = new PokerStars_Action();

            //ハンド履歴の解析
            for (int i = 0; i < lines.Count; i++)
            {
                //ハンドIDが記載されている
                if (lines[i].Contains(PokerStars_Strings.HandIdSearchTag))
                {
                    //トーナメントorキャッシュ
                    if (lines[i].Contains(PokerStars_Strings.Tournament))
                    {

                    }
                    else
                    {

                    }

                    //No Limit Holdem
                    if (lines[i].Contains(PokerStars_Strings.NoLimitHoldem))
                    {

                    }
                    else
                    {
                        return;
                    }

                    //初期化
                    search_id_flag = true;
                    btn = -1;
                    seats_names.Clear();
                    btn_index = -1;
                    stars_action.Reset();

                    //分析中のハンドID
                    long hand_id = HandId(lines[i]);
                    int index;
                    if ((search_id_flag = (index = analyzed.BinarySearch(hand_id)) >= 0) == false)
                    {
                        //分析済みでないなら分析済みリストに追加
                        analyzed.Insert(~index, hand_id);
                    }
                }
                //スキップ対象でない
                else if (search_id_flag == false)
                {
                    //BTN位置の記述がある
                    if (lines[i].Contains(PokerStars_Strings.ButtonSearchTag))
                    {
                        //9-Max Table
                        if (lines[i].Contains(PokerStars_Strings.MaxPlayer_9))
                        {
                            btn = BTN_Pos(lines[i]);
                        }
                        else
                        {
                            return;
                        }
                    }

                    //Seatと名前の記述がある
                    if (PokerStars_Strings.SeatSearchTag.IsMatch(lines[i]))
                    {
                        //SeatIDがBTN番号と一致
                        if (lines[i].Contains(PokerStars_Strings.SeatHeader + btn + PokerStars_Strings.SeatFooter))
                        {
                            //BTN位置のindexを保存
                            btn_index = seats_names.Count;
                        }

                        //名前の出現位置
                        int start = 
                            lines[i].IndexOf(PokerStars_Strings.SeatFooter)+
                            PokerStars_Strings.SeatFooter.Length;
                        //名前の文字数
                        Match match = PokerStars_Strings.NameFooter.Match(lines[i]);
                        int count = match.Index - start;
                        //名前を格納
                        seats_names.Add(lines[i].Substring(start, count));

                        //次の行にSeatの記述が無い(全員分のSeatIDが取得出来た)
                        if (i+1<lines.Count && PokerStars_Strings.SeatSearchTag.IsMatch(lines[i + 1])==false)
                        {
                            //ヘッズアップは除外
                            if (seats_names.Count <= 2)
                            {
                                search_id_flag = true;
                            }

                            //ポジションに合わせてソート
                            seats_names= SortByPos(seats_names, btn_index);

                            //ポジションと紐付けたプレイヤーデータ格納クラスをインスタンス化
                            for (int p = 1; p <= seats_names.Count; p++)
                            {
                                PlayerData player = new PlayerData(
                                    seats_names[seats_names.Count - p], PlayerData.POSITIONS[PlayerData.POSITIONS.Length - p]);
                                stars_action.tablePlayers.Add(player);
                            }
                        }
                    }

                    //ホールカードの記述
                    if (lines[i].Contains(PokerStars_Strings.HoleCards))
                    {
                        stars_action.GoPreflop();
                    }
                    //フロップの記述
                    if (lines[i].Contains(PokerStars_Strings.Flop))
                    {
                        stars_action.GoPostflop(PokerStars_Action.FLOP);
                    }
                    //ターンの記述
                    if (lines[i].Contains(PokerStars_Strings.Turn))
                    {
                        stars_action.GoPostflop(PokerStars_Action.TURN);
                    }
                    //リバーの記述
                    if (lines[i].Contains(PokerStars_Strings.River))
                    {
                        stars_action.GoPostflop(PokerStars_Action.RIVER);
                    }

                    //カードを配る段階が終わっている
                    if (stars_action.phase != PokerStars_Action.DEAL)
                    {
                        //自分に配られたハンドの記述
                        if (lines[i].Contains(PokerStars_Strings.DealtSearchTag))
                        {

                        }
                        else
                        {
                            stars_action.Action(lines[i]);
                        }
                    }

                    //ショーダウンの記述
                    if (lines[i].Contains(PokerStars_Strings.Showdown))
                    {
                        search_id_flag = true;
                    }
                }
                //サマリの記述
                if (lines[i].Contains(PokerStars_Strings.Summary))
                {
                    search_id_flag = true;

                    //プレイヤー名を検索
                    foreach (PlayerData player in stars_action.tablePlayers)
                    {
                        int index;
                        if ((index = playerDatas.BinarySearch(player)) >= 0)
                        {
                            //存在するなら値を足す
                            playerDatas[index].CombineData(player);
                        }
                        else
                        {
                            //存在しないなら新たに生成して追加
                            playerDatas.Insert(~index, player);
                        }
                    }
                }
            }
        }

        public string SearchPlayer(string player_name)
        {
            int index= playerDatas.BinarySearch(new PlayerData(player_name,-1));

            if (index >= 0)
            {
                PlayerData player = playerDatas[index];
                double deal_count = (double)BaseSystem.SumArray(player.deal_count);
                double VPIP= 
                    (double)BaseSystem.SumArray(player.actively_join) 
                    / deal_count;
                double PFR= 
                    (double)BaseSystem.SumArray(player.preflop_raise)
                    / deal_count;
                double Bet_3 =
                    (double)BaseSystem.SumArray(player.preflop_3bet)
                    / deal_count;
                double Bet_3_Fold =
                    (double)BaseSystem.SumArray(player.preflop_3bet_fold)
                    / (double)BaseSystem.SumArray(player.preflop_3bet_encount);

                double AF_F =
                    ((double)player.postflop_raise[0] +
                    (double)player.postflop_bet[0])
                    / (double)player.postflop_call[0];
                double CB_F =
                    (double)player.cb_count
                    / (double)player.cb_chance;
                double CR_F =
                    (double)player.postflop_check_raise[0]
                    /(double)player.postflop_check[0];

                return
                    BaseSystem.SumArray(player.deal_count)+"/"+
                    VPIP.ToString("P2")+"/"+PFR.ToString("P2")+"/"+
                    Bet_3.ToString("P2")+"/"+Bet_3_Fold.ToString("P2")+"/"+
                    AF_F.ToString("F2")+"/"+CB_F.ToString("P2")+"/"+
                    CR_F.ToString("P2");
            }

            return "NotFound!";
        }

        //ハンドIDを取得
        private long HandId(string line)
        {
            //IDの出現位置
            int start = line.IndexOf(PokerStars_Strings.HandIdHeader) + 1;
            //IDの文字数
            int count = line.IndexOf(PokerStars_Strings.HandIdFooter) - start;

            //格納用
            long id;
            //long型に変換
            long.TryParse(line.Substring(start, count),out id);
            
            return id;
        }

        private int BTN_Pos(string line)
        {
            //BTNの記述位置
            int start = line.IndexOf(PokerStars_Strings.ButtonHeader) + 1;
            //BTNIDの文字数
            int count = line.IndexOf(PokerStars_Strings.ButtonFooter) - start;

            //格納用
            int btn;
            //int型に変換
            int.TryParse(line.Substring(start, count), out btn);

            return btn;
        }

        //ポジションに合わせて並べ替え
        private List<string> SortByPos(List<string> seats_names,int btn_index)
        {
            //ソートの必要が無い
            if (btn_index + 2 == seats_names.Count - 1)
            {
                return seats_names;
            }

            //BBとUTGの間で分割して、入れ替えて結合することで並べ替える
            int cut_index = (btn_index + 2+1) % seats_names.Count;
            List<string> cut = seats_names.GetRange(0, cut_index);
            List<string> result = seats_names.GetRange(cut_index, seats_names.Count - cut_index);
            result.AddRange(cut);

            return result;
        }
    }

    public class PlayerData :IComparable<PlayerData>
    {
        //プレイヤー名
        public string player_name;

        //ポジション
        public const int UTG = 0, UTG1 = 1, UTG2 = 2, MP = 3, HJ = 4, CO = 5, BTN = 6, SB = 7, BB = 8;
        public static readonly int[] POSITIONS = { UTG, UTG1, UTG2, MP, HJ, CO, BTN, SB, BB };
        public int position;

        //ハンドが配られた回数
        public int[] deal_count=new int[9];
        //自発的なポット参加回数
        public int[] actively_join=new int[9];
        //プリフロップレイズ回数
        public int[] preflop_raise=new int[9];
        //プリフロップ3bet回数
        public int[] preflop_3bet = new int[9];
        //プリフロップ4bet回数
        public int[] preflop_4bet = new int[9];
        //プリフロップ3bet遭遇回数
        public int[] preflop_3bet_encount = new int[9];
        //プリフロップ4bet遭遇回数
        public int[] preflop_4bet_encount = new int[9];
        //プリフロップ3bet遭遇時フォールド回数
        public int[] preflop_3bet_fold=new int[9];
        //プリフロップ4bet遭遇時フォールド回数
        public int[] preflop_4bet_fold = new int[9];

        //ポストフロップフォールド回数
        public int[] postflop_fold = new int[3];
        //ポストフロップでのチェック回数
        public int[] postflop_check = new int[3];
        //ポストフロップでのコール回数
        public int[] postflop_call = new int[3];
        //ポストフロップでのベット回数
        public int[] postflop_bet=new int[3];
        //ポストフロップでのレイズ回数
        public int[] postflop_raise = new int[3];
        //ポストフロップでのチェックレイズ回数
        public int[] postflop_check_raise = new int[3];

        //ポストフロップ参加回数
        public int[] postflop_join = new int[3];
        //ポストフロップ勝利回数
        public int[] postflop_win = new int[3];
        //ショーダウン回数
        public int showdown_count;
        //ショーダウン勝利回数
        public int showdown_win;

        //ブラインドスチール可能な状況の遭遇回数
        public int[] blind_steal_encount = new int[2];
        //ブラインドスチールを試みた回数
        public int[] blind_steal_try = new int[2];
        //ブラインドディフェンスの選択を迫られた回数
        public int blind_defence_encount;
        //ブラインドディフェンス回数
        public int blind_defence_try;

        //コンティニュエーションベット可能な状況に遭遇した回数
        public int cb_chance;
        //コンティニュエーションベットを行った回数
        public int cb_count;

        public PlayerData(string player_name,int position)
        {
            this.player_name = player_name;
            this.position = position;

            Reset();
        }

        public void Reset()
        {
            BaseSystem.ResetArray(ref deal_count);
            BaseSystem.ResetArray(ref actively_join);
            BaseSystem.ResetArray(ref preflop_raise);
            BaseSystem.ResetArray(ref preflop_3bet);
            BaseSystem.ResetArray(ref preflop_4bet);
            BaseSystem.ResetArray(ref preflop_3bet_encount);
            BaseSystem.ResetArray(ref preflop_4bet_encount);
            BaseSystem.ResetArray(ref preflop_3bet_fold);
            BaseSystem.ResetArray(ref preflop_4bet_fold);

            BaseSystem.ResetArray(ref postflop_fold);
            BaseSystem.ResetArray(ref postflop_check);
            BaseSystem.ResetArray(ref postflop_call);
            BaseSystem.ResetArray(ref postflop_bet);
            BaseSystem.ResetArray(ref postflop_raise);
            BaseSystem.ResetArray(ref postflop_check_raise);

            BaseSystem.ResetArray(ref postflop_join);
            BaseSystem.ResetArray(ref postflop_win);
            showdown_count = 0;
            showdown_win = 0;

            BaseSystem.ResetArray(ref blind_steal_encount);
            BaseSystem.ResetArray(ref blind_steal_try);
            blind_defence_encount = 0;
            blind_defence_try = 0;

            cb_chance = 0;
            cb_count = 0;
        }

        public void CombineData(PlayerData add)
        {
            if (this.player_name.Equals(add.player_name))
            {
                position = -1;

                BaseSystem.AddArray(
                    ref deal_count,
                    add.deal_count);
                BaseSystem.AddArray(
                    ref actively_join,
                    add.actively_join);
                BaseSystem.AddArray(
                    ref preflop_raise,
                    add.preflop_raise);
                BaseSystem.AddArray(
                    ref preflop_3bet,
                    add.preflop_3bet);
                BaseSystem.AddArray(
                    ref preflop_4bet,
                    add.preflop_4bet);
                BaseSystem.AddArray(
                    ref preflop_3bet_encount,
                    add.preflop_3bet_encount);
                BaseSystem.AddArray(
                    ref preflop_4bet_encount,
                    add.preflop_4bet_encount);
                BaseSystem.AddArray(
                    ref preflop_3bet_fold,
                    add.preflop_3bet_fold);
                BaseSystem.AddArray(
                    ref preflop_4bet_fold,
                    add.preflop_4bet_fold);

                BaseSystem.AddArray(
                    ref postflop_fold,
                    add.postflop_fold);
                BaseSystem.AddArray(
                    ref postflop_check,
                    add.postflop_check);
                BaseSystem.AddArray(
                    ref postflop_call,
                    add.postflop_call);
                BaseSystem.AddArray(
                    ref postflop_bet,
                    add.postflop_bet);
                BaseSystem.AddArray(
                    ref postflop_raise,
                    add.postflop_raise);
                BaseSystem.AddArray(
                    ref postflop_check_raise,
                    add.postflop_check_raise);

                BaseSystem.AddArray(
                    ref postflop_join,
                    add.postflop_join);
                BaseSystem.AddArray(
                    ref postflop_win,
                    add.postflop_win);
                showdown_count += 
                    add.showdown_count;
                showdown_win += 
                    add.showdown_win;

                BaseSystem.AddArray(
                    ref blind_steal_encount,
                    add.blind_steal_encount);
                BaseSystem.AddArray(
                    ref blind_steal_try,
                    add.blind_steal_try);
                blind_defence_encount += 
                    add.blind_defence_encount;
                blind_defence_try += 
                    add.blind_defence_try;

                cb_chance += 
                    add.cb_chance;
                cb_count += 
                    add.cb_count;
            }
        }

        public void AddDealCount()
        {
            deal_count[position]++;
        }

        public int CompareTo(PlayerData obj)
        {
            if (obj == null)
            {
                return 1;
            }

            return this.player_name.CompareTo(obj.player_name);
        }
    }
}
