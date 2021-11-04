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
            public IDRect(string id_, Rectangle rect_)
            {
                id = id_;
                rect = rect_;
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

                double dwd = (double.Parse(para[3]) * (double)imageWd);
                double dht = (double.Parse(para[4]) * (double)imageHt);
                double dsx = (double.Parse(para[1]) * (double)imageWd) - (dwd * 0.5);
                double dsy = (double.Parse(para[2]) * (double)imageHt) - (dht * 0.5);

                int sx = (int)Math.Round(dsx, MidpointRounding.AwayFromZero);
                int sy = (int)Math.Round(dsy, MidpointRounding.AwayFromZero);
                int wd = (int)Math.Round(dwd, MidpointRounding.AwayFromZero);
                int ht = (int)Math.Round(dht, MidpointRounding.AwayFromZero);
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

            int imgWd = sourceImage.Width;
            int imgHt = sourceImage.Height;

            //"C:\test\1.txt"をShift-JISコードとして開く
            System.IO.StreamReader sr = new System.IO.StreamReader(filename);

            //内容をすべて読み込む
            while (sr.Peek() > -1)
            {
                idRects.Add( new IDRect(sr.ReadLine(), imgWd, imgHt) );
            }
            
            //閉じる
            sr.Close();
        }

        private void SaveTextFile(string filename)
        {
            //Shift JISで書き込む
            //書き込むファイルが既に存在している場合は、上書きする
            System.IO.StreamWriter sw = new System.IO.StreamWriter( filename, false );

            int imgWd = sourceImage.Width;
            int imgHt = sourceImage.Height;

            foreach (IDRect rc in idRects)
            {
                sw.WriteLine(rc.GetStringData(imgWd, imgHt));
            }

            //閉じる
            sw.Close();
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

        private void DrawSelectedRect(Rectangle rect, Color rectCol)
        {
            if (null == workImage) return;

            Bitmap canvasImage = new Bitmap(workImage);
            {
                Graphics g = Graphics.FromImage(canvasImage);

                Pen pn = new Pen(new SolidBrush(rectCol), 3);
                pn.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pn, rect);
                pn.Dispose();

                g.Flush();
                g.Dispose();
            }
            pictureBox.Image = canvasImage;
        }

        private void DrawCrossLine(int x, int y)
        {
            if (null == workImage) return;

            Bitmap canvasImage = new Bitmap(workImage);
            {
                Graphics g = Graphics.FromImage(canvasImage);

                Pen pn = new Pen(new SolidBrush(Color.LightGray), 1);
                pn.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawLine(pn, x, 0, x, workImage.Height);
                g.DrawLine(pn, 0, y, workImage.Width, y);
                pn.Dispose();

                g.Flush();
                g.Dispose();
            }
            pictureBox.Image = canvasImage;
        }

        private void UpdateRectsListBox()
        {
            listBox.Items.Clear();
            foreach (IDRect rc in idRects)
            {
                listBox.Items.Add($"{rc.id} {rc.rect.X} {rc.rect.Y}  {rc.rect.Width} {rc.rect.Height}");
            }
        }

        int msx, msy;
        bool msDown = false;
        string selectedID;

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            msx = e.X;
            msy = e.Y;
            msDown = true;
            selectedID = GetSelectedID();
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (msDown)
            {
                Rectangle rect = new Rectangle(msx, msy, e.X - msx, e.Y - msy);
                Color col = idColor[selectedID];

                DrawSelectedRect(rect, col);
            }
            else
            {
                DrawCrossLine(e.X, e.Y);
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            Rectangle rect = new Rectangle(msx, msy, e.X - msx, e.Y - msy);
            if( rect.Width > 0 && rect.Height > 0)
            {
                idRects.Add(new IDRect(selectedID, rect));
            }
            UpdateRectsImage();
            UpdateRectsListBox();
            msDown = false;
        }

        private void buttonWriteTextFile_Click(object sender, EventArgs e)
        {
            string textFileName = System.IO.Path.ChangeExtension(sourceFileName, "txt");
            SaveTextFile(textFileName);
        }

        private void listBox_Click(object sender, EventArgs e)
        {
            int idx = listBox.SelectedIndex;
            if (idx >= 0 && idx < idRects.Count)
            {
                Color penCol;
                IDRect rc = idRects[idx];
                if (idColor.ContainsKey(rc.id))
                {
                    penCol = idColor[rc.id];
                }
                else
                {
                    penCol = Color.LightGray;
                }

                DrawSelectedRect(rc.rect, penCol);
            }
        }

        private void buttonListAllClear_Click(object sender, EventArgs e)
        {
            idRects.Clear();
            UpdateRectsImage();
            UpdateRectsListBox();
        }

        private void buttonListDelete_Click(object sender, EventArgs e)
        {
            int idx = listBox.SelectedIndex;
            if (idx >= 0 && idx < idRects.Count)
            {
                idRects.RemoveAt(idx);
                UpdateRectsImage();
                UpdateRectsListBox();
                listBox.SelectedIndex = idx - 1;
            }
        }

        private string GetSelectedID()
        {
            List<RadioButton> radioBtns = new List<RadioButton>()
            {
                radioButtonID0, radioButtonID1, radioButtonID2, radioButtonID3, radioButtonID4,
                radioButtonID5, radioButtonID6, radioButtonID7, radioButtonID8, radioButtonID9,
            };

            foreach( RadioButton rd in radioBtns)
            {
                if (rd.Checked) return rd.Text;
            }
            radioButtonID0.Checked = true;
            return radioButtonID0.Text;
        }
    }
}
