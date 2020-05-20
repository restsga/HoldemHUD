using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemHUD
{
    class PokerStars_Action
    {
        //ハンドを配る/プリフロップ/フロップ/ターン/リバー
        public const int DEAL = 0, PREFLOP = 1, FLOP = 2, TURN = 3, RIVER = 4;
        public int phase=DEAL;
        //現在のテーブルのプレイヤー分析結果
        public List<PlayerData> tablePlayers = new List<PlayerData>();

        //アクション回数
        private int[] action_count;
        //ベット/レイズ回数
        private int bet_raise_count;
        //フォールドしたタイミング
        private int[] fold_phase;
        //オリジナルレイザー
        private bool[] original_raise;
        //Checkしたプレイヤー
        private bool[] check_flag;

        public void Reset()
        {
            phase = DEAL;
            tablePlayers.Clear();
        }

        public void GoPreflop()
        {
            //プリフロップ開始
            phase = PREFLOP;

            //フラグ関連の初期化
            //アクション回数
            action_count = new int[tablePlayers.Count];
            BaseSystem.ResetArray(ref action_count);
            //プリフロップなのでベット/レイズ回数の初期値は1
            bet_raise_count = 1;
            //フォールドタイミングは-1(null扱い)に設定
            fold_phase = new int[tablePlayers.Count];
            BaseSystem.ResetArray(ref fold_phase, -1);
            //オリジナルレイザー
            original_raise = new bool[tablePlayers.Count];
            BaseSystem.ResetArray(ref original_raise, false);
            //Check
            check_flag = new bool[tablePlayers.Count];
            BaseSystem.ResetArray(ref check_flag, false);

            //全員に対して手札が配られた回数を+1
            foreach (PlayerData player in tablePlayers)
            {
                player.AddDealCount();
            }
        }
        public void GoPostflop(int phase)
        {
            //フロップ/ターン/リバー開始
            this.phase= phase;

            //フラグ関連の初期化
            //アクション回数
            BaseSystem.ResetArray(ref action_count);
            //ポストフロップなのでベット/レイズ回数の初期値は0
            bet_raise_count = 0;
            //Checkフラグ
            BaseSystem.ResetArray(ref check_flag, false);

            //プリフロップでフォールドしてないプレイヤーはフロップ到達回数を+1
            for (int i = 0; i < tablePlayers.Count; i++)
            {
                if (fold_phase[i] == -1)
                {
                    tablePlayers[i].postflop_join[phase-FLOP]++;
                }
            }
        }

        public void Action(string line)
        {
            //テーブルのプレイヤーの名前を検索
            for (int i = 0; i <tablePlayers.Count;i++)
            {
                //一致するプレイヤー名が存在
                if (line.Contains(tablePlayers[i].player_name + PokerStars_Strings.Name_Action))
                {
                    //プレイヤー名の部分を削除
                    line = line.Replace(tablePlayers[i].player_name + PokerStars_Strings.Name_Action, "");
                    //キャッシュ
                    PlayerData player = tablePlayers[i];

                    if (phase == PREFLOP)
                    {
                        if (line.Contains(PokerStars_Strings.Flod))
                        {
                            fold_phase[i] = PREFLOP;

                            if (bet_raise_count <= 2)
                            {
                                //何もしない
                            }
                            if (bet_raise_count == 3)
                            {
                                if (action_count[i] >= 1)
                                {
                                    player.preflop_3bet_fold[player.position]++;
                                }
                            }
                            if (bet_raise_count >= 4)
                            {
                                if (action_count[i] >= 1)
                                {
                                    player.preflop_4bet_fold[player.position]++;
                                }
                            }
                        }

                        if (line.Contains(PokerStars_Strings.Call)||
                            line.Contains(PokerStars_Strings.Raise))
                        {
                            if (action_count[i] <= 0)
                            {
                                player.actively_join[player.position]++;
                            }
                            action_count[i]++;

                            if (line.Contains(PokerStars_Strings.Raise))
                            {
                                bet_raise_count++;

                                BaseSystem.ResetArray(ref original_raise, false);
                                original_raise[i] = true;

                                if (bet_raise_count == 2)
                                {
                                    player.preflop_raise[player.position]++;
                                }
                                if (bet_raise_count == 3)
                                {
                                    player.preflop_3bet[player.position]++;

                                    for(int p = 0; p < fold_phase.Length; p++)
                                    {
                                        if (fold_phase[p] == -1)
                                        {
                                            tablePlayers[p].preflop_3bet_encount[tablePlayers[p].position]++;
                                        }
                                    }
                                }
                                if (bet_raise_count >= 4)
                                {
                                    player.preflop_4bet[player.position]++;

                                    for (int p = 0; p < fold_phase.Length; p++)
                                    {
                                        if (fold_phase[p] == -1)
                                        {
                                            tablePlayers[p].preflop_4bet_encount[tablePlayers[p].position]++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (phase == FLOP||phase==TURN||phase==RIVER)
                    {
                        if (phase == FLOP)
                        {
                            if (original_raise[i])
                            {
                                player.cb_chance++;
                            }
                        }

                        if (line.Contains(PokerStars_Strings.Flod))
                        {
                            player.postflop_fold[phase-FLOP]++;
                        }

                        if (line.Contains(PokerStars_Strings.Check))
                        {
                            check_flag[i] = true;

                            player.postflop_check[phase - FLOP]++;

                            if (original_raise[i])
                            {
                                BaseSystem.ResetArray(ref original_raise, false);
                            }
                        }

                        if (line.Contains(PokerStars_Strings.Call))
                        {
                            player.postflop_call[phase - FLOP]++;
                        }

                        if (line.Contains(PokerStars_Strings.Bet))
                        {
                            if (phase == FLOP)
                            {
                                if (original_raise[i])
                                {
                                    player.cb_count++;
                                }
                            }

                            BaseSystem.ResetArray(ref original_raise, false);
                            original_raise[i] = true;

                            player.postflop_bet[phase - FLOP]++;
                        }

                        if (line.Contains(PokerStars_Strings.Raise))
                        {
                            BaseSystem.ResetArray(ref original_raise, false);
                            original_raise[i] = true;

                            player.postflop_raise[phase - FLOP]++;

                            if (check_flag[i])
                            {
                                player.postflop_check_raise[phase - FLOP]++;
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}
