using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoloAnotation
{
    public partial class FormMain : Form
    {
        class IDRect
        {
            public Rectangle rect;
            public string id;

            public IDRect(string id_, int sx, int sy, int wd, int ht)
            {
                id = id_;
                rect = new Rectangle(sx, sy, wd, ht);
            }

            public IDRect(string lineStr, int imageWd, int imageHt)
            {
                SetStringData(lineStr, imageWd, imageHt);
            }

            public string GetStringData( int imageWd, int imageHt)
            {
                double dwd = (double)rect.Width / (double)imageWd;
                double dht = (double)rect.Height / (double)imageHt;

                double dsx = ((double)rect.X / (double)imageWd) + (dwd*0.5);
                double dsy = ((double)rect.Y / (double)imageHt) + (dht*0.5);

                return $"{id} {dsx} {dsy} {dwd} {dht}";
            }

            public void SetStringData(string lineStr, int imageWd, int imageHt)
            {
                string[] para = lineStr.Split(' ');

                id = para[0];

                int wd = (int)(double.Parse(para[3]) * (double)imageWd);
                int ht = (int)(double.Parse(para[4]) * (double)imageHt);
                int sx = (int)(double.Parse(para[1]) * (double)imageWd) - (wd/2);
                int sy = (int)(double.Parse(para[2]) * (double)imageHt) - (ht/2);
                rect = new Rectangle(sx,sy,wd,ht);
            }
        }


        // ------------------------------

        Bitmap sourceImage;
        Bitmap workImage;
        string sourceFileName;

        List<IDRect> idRects;
        Dictionary<string,Color> idColor;

        public FormMain()
        {
            InitializeComponent();

            pictureBox.AllowDrop = true;

            idRects = new List<IDRect>();
            idColor = new Dictionary<string, Color>();
            idColor.Add(radioButtonID0.Text, pictureBoxID0.BackColor);
            idColor.Add(radioButtonID1.Text, pictureBoxID1.BackColor);
            idColor.Add(radioButtonID2.Text, pictureBoxID2.BackColor);
            idColor.Add(radioButtonID3.Text, pictureBoxID3.BackColor);
            idColor.Add(radioButtonID4.Text, pictureBoxID4.BackColor);
            idColor.Add(radioButtonID5.Text, pictureBoxID5.BackColor);
            idColor.Add(radioButtonID6.Text, pictureBoxID6.BackColor);
            idColor.Add(radioButtonID7.Text, pictureBoxID7.BackColor);
            idColor.Add(radioButtonID8.Text, pictureBoxID8.BackColor);
            idColor.Add(radioButtonID9.Text, pictureBoxID9.BackColor);
        }

        private void pictureBox_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        private void pictureBox_DragDrop(object sender, DragEventArgs e)
        {
            //コントロール内にドロップされたとき実行される
            //ドロップされたすべてのファイル名を取得する
            string[] fileName =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);

            LoadImage(fileName[0]);
        }


        private bool LoadImage(string filename)
        {
            idRects.Clear();

            sourceFileName = filename;

            {
                if (null != sourceImage)
                {
                    sourceImage.Dispose();
                }
                sourceImage = new Bitmap(filename);
            }

            {
                string textFileName = System.IO.Path.ChangeExtension(sourceFileName, "txt");
                if (System.IO.File.Exists(textFileName))
                {
                    LoadTextFile(textFileName);
                }
                else
                {
                    idRects.Clear();
                }
            }


            UpdateRectsImage();
            UpdateRectsListBox();

            return true;
        }

        private void LoadTextFile(string filename)
        {
            idRects.Clear();

            //"C:\test\1.txt"をShift-JISコードとして開く
            System.IO.StreamReader sr = new System.IO.StreamReader(
                filename,
                System.Text.Encoding.GetEncoding("UTF-8"));

            //内容をすべて読み込む
            while (sr.Peek() > -1)
            {
                idRects.Add( new IDRect(sr.ReadLine(), sourceImage.Width, sourceImage.Height) );
            }
            
            //閉じる
            sr.Close();
        }

        private void UpdateRectsImage()
        {
            if(null != workImage)
            {
                workImage.Dispose();
            }

            workImage = new Bitmap(sourceImage);
            {
                Graphics g = Graphics.FromImage(workImage);

                foreach (IDRect rc in idRects)
                {
                    Color penCol;
                    if(idColor.ContainsKey(rc.id))
                    {
                        penCol = idColor[rc.id];
                    }
                    else
                    {
                        penCol = Color.LightGray;
                    }
                    
                    Pen pn = new Pen(new SolidBrush(penCol), 2);
                    g.DrawRectangle(pn, rc.rect);
                    pn.Dispose();
                }
                g.Flush();
                g.Dispose();
            }
            pictureBox.Image = workImage;
        }


        private void UpdateRectsListBox()
        {
            listBox.Items.Clear();
            foreach (IDRect rc in idRects)
            {
                listBox.Items.Add($"{rc.id} {rc.rect.X} {rc.rect.Y}  {rc.rect.Width} {rc.rect.Height}");
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {

        }
    }
}
