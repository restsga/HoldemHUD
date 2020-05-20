using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace HoldemHUD
{
    public partial class Form1 : Form
    {
        PokerStars_Analyse analyse = new PokerStars_Analyse();
        AutoCompleteStringCollection stringCollection=new AutoCompleteStringCollection();

        public Form1()
        {
            InitializeComponent();

            textBox2.AutoCompleteCustomSource = stringCollection;
            textBox3.AutoCompleteCustomSource = stringCollection;
            textBox4.AutoCompleteCustomSource = stringCollection;
            textBox5.AutoCompleteCustomSource = stringCollection;
            textBox6.AutoCompleteCustomSource = stringCollection;
            textBox7.AutoCompleteCustomSource = stringCollection;
            textBox8.AutoCompleteCustomSource = stringCollection;
            textBox9.AutoCompleteCustomSource = stringCollection;
            textBox10.AutoCompleteCustomSource = stringCollection;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartAnalyse();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SearchDatas();
        }

        int time = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                time++;

                if (time >= 100)
                {
                    time = 0;

                    StartAnalyse();
                    SearchDatas();
                }
            }
            else
            {
                time = 0;
            }

            label14.Text = "ReloadTimer : " + (time / 10);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Text = "AutoReload:ON";
            }
            else
            {
                checkBox1.Text = "AutoReload:OFF";
            }
        }

        private void StartAnalyse()
        {
            //解析開始
            label13.Text = "Analysing...";

            //ディレクトリ内の拡張子txtの全てのファイル名を取得
            string[] files = Directory.GetFiles(textBox1.Text, "*.txt");

            foreach (string name in files)
            {

                //読み込んだ文字列の格納用
                string line = "";
                List<string> lineList = new List<string>();

                //UTF-8でファイル読み込み
                using (
                    StreamReader streamReader = new StreamReader(
                    name, Encoding.GetEncoding("UTF-8")))
                {
                    //1行ずつ読み込んでListに追加
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineList.Add(line);
                    }
                }

                analyse.Analyze_Lines(lineList);
            }

            stringCollection.Clear();
            stringCollection.AddRange(
                analyse.playerDatas.Select(p => p.player_name).ToArray());

            label13.Text = "Complete! HandID:" + analyse.analyzed.Last();
        }

        private void SearchDatas()
        {
            string search = "Searching";
            label4.Text = search;
            label5.Text = search;
            label6.Text = search;
            label7.Text = search;
            label8.Text = search;
            label9.Text = search;
            label10.Text = search;
            label11.Text = search;
            label12.Text = search;

            label4.Text = analyse.SearchPlayer(textBox2.Text);
            label5.Text = analyse.SearchPlayer(textBox3.Text);
            label6.Text = analyse.SearchPlayer(textBox4.Text);
            label7.Text = analyse.SearchPlayer(textBox5.Text);
            label8.Text = analyse.SearchPlayer(textBox6.Text);
            label9.Text = analyse.SearchPlayer(textBox7.Text);
            label10.Text = analyse.SearchPlayer(textBox8.Text);
            label11.Text = analyse.SearchPlayer(textBox9.Text);
            label12.Text = analyse.SearchPlayer(textBox10.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
