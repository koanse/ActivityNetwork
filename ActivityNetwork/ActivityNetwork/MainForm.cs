using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ActivityNetwork
{
    public partial class MainForm : Form
    {
        ActNet an;
        bool isBmpSizing = false;
        Point prevLocation;
        int xDist = 50, yAmplitude = 110, fontSize = 9, nodeDiam = 20;
        float mult = 1.7f;
        
        public MainForm()
        {
            InitializeComponent();
            nudRes_ValueChanged(null, null);
            dgvRes.Columns["Количество"].ValueType = typeof(float);
            dgvRes.Columns["Шаг"].ValueType = typeof(float);
        }
        void nudRes_ValueChanged(object sender, EventArgs e)
        {
            dgvAct.Rows.Clear();
            dgvAct.Columns.Clear();
            DataGridViewColumn c = new DataGridViewTextBoxColumn();
            c.Name = c.HeaderText = "Название";
            dgvAct.Columns.Add(c);
            c = new DataGridViewTextBoxColumn();
            c.Name = c.HeaderText = "Длительность";
            c.ValueType = typeof(float);
            dgvAct.Columns.Add(c);
            c = new DataGridViewTextBoxColumn();
            c.Name = c.HeaderText = "Предшествующие работы";
            dgvAct.Columns.Add(c);
            dgvRes.Rows.Clear();
            for (int i = 0; i < nudRes.Value; i++)
            {
                c = new DataGridViewTextBoxColumn();
                string name = string.Format("Ресурс{0}", i + 1);
                c.Name = c.HeaderText = name;
                c.ValueType = typeof(float);
                dgvAct.Columns.Add(c);
                dgvRes.Rows.Add(new object[] { name, 100f, 10f });
            }
        }
        Act[] ReadActs()
        {
            ArrayList res = new ArrayList();
            for (int i = 0; i < dgvAct.Rows.Count - 1; i++)
            {
                DataGridViewRow r = dgvAct.Rows[i];
                float[] arrRes = new float[(int)nudRes.Value];
                for (int j = 0; j < arrRes.Length; j++)
                    arrRes[j] = (float)(r.Cells[3 + j] as DataGridViewCell).Value;
                string mark = (string)(r.Cells["Название"] as DataGridViewCell).Value;
                float dur = (float)(r.Cells["Длительность"] as DataGridViewCell).Value;
                string prev = (string)(r.Cells["Предшествующие работы"] as DataGridViewCell).Value;
                string[] arrPrev;
                if (prev != null)
                    arrPrev = prev.Split(new char[] { ' ', ',' });
                else
                    arrPrev = new string[] { };
                res.Add(new Act(mark, dur, arrRes, arrPrev));
            }
            return (Act[])res.ToArray(typeof(Act));
        }
        float[] ReadRes()
        {
            float[] arrRes = new float[(int)nudRes.Value];
            for (int i = 0; i < dgvRes.Rows.Count; i++)
                arrRes[i] = (float)dgvRes.Rows[i].Cells["Количество"].Value;
            return arrRes;
        }
        void Calculate()
        {
            try
            {
                Act[] arrAct = ReadActs();
                an = new ActNet(arrAct);
                an.BuildActNet();
                float[] arrRes = ReadRes();
                an.OptimizeTime(arrRes);
            }
            catch { }
        }
        void CalculateOptRes()
        {
            ArrayList listAn = new ArrayList();
            try
            {                
                float Treal, Tdir = float.Parse(tbTime.Text);
                ActNet anCur = an;
                float[] arrRes = ReadRes();
                Treal = an.OptimizeTime(arrRes);
                while (Treal <= Tdir)
                {
                    Treal = float.MaxValue;
                    listAn.Add(anCur);
                    float[] arrCrit = anCur.GetCriterions();
                    ArrayList listCrit = new ArrayList(arrCrit);
                    listCrit.Sort();
                    listCrit.Reverse();
                    foreach (float crit in listCrit)
                    {
                        int i;
                        for (i = 0; i < arrCrit.Length; i++)
                            if (crit == arrCrit[i])
                                break;
                        float step = (float)dgvRes.Rows[i].Cells["Шаг"].Value;
                        anCur = new ActNet(anCur);
                        arrRes[i] -= step;
                        try
                        {
                            Treal = anCur.OptimizeTime(arrRes);
                            if (Treal > Tdir)
                                throw new Exception();
                            break;
                        }
                        catch
                        {
                            Treal = float.MaxValue;
                            arrRes[i] += step;
                        }
                    }
                }
            }
            catch { }
            ActNet[] arrAn = (ActNet[])listAn.ToArray(typeof(ActNet));
            lbResult.Items.Clear();
            lbResult.Items.AddRange(arrAn);
        }
        void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Calculate();
            if (tc.SelectedTab == tc.TabPages["tpOptRes"])
                CalculateOptRes();
            SetBitmaps();
        }
        void tc_TabIndexChanged(object sender, EventArgs e)
        {
            if (tc.SelectedTab == tc.TabPages["tpOptRes"])
                CalculateOptRes();
            SetBitmaps();
        }
        void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                isBmpSizing = false;
                return;
            }
            if (isBmpSizing == false)
            {
                prevLocation = e.Location;
                isBmpSizing = true;
                return;
            }
            int xDelta = e.Location.X - prevLocation.X;
            int yDelta = e.Location.Y - prevLocation.Y;
            prevLocation = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                xDist += xDelta;
                yAmplitude += yDelta;
                if (xDist > 100)
                    xDist = 100;
                if (xDist < 10)
                    xDist = 10;
                if (yAmplitude > 400)
                    yAmplitude = 400;
                if (yAmplitude < 10)
                    yAmplitude = 10;
            }
            else if (e.Button == MouseButtons.Right)
            {
                mult += xDelta * 0.02f;
                fontSize += yDelta;
                if (mult < 0.5)
                    mult = 0.5f;
                if (mult > 5)
                    mult = 5;
                if (fontSize < 4)
                    fontSize = 4;
                if (fontSize > 25)
                    fontSize = 25;
                nodeDiam = 20 * fontSize / 9;
            }
            SetBitmaps();
        }
        void pbMain_SizeChanged(object sender, EventArgs e)
        {
            SetBitmaps();
        }
        void SetBitmaps()
        {
            try
            {
                if (tc.SelectedTab == tc.TabPages["tpData"])
                    pbMain.Image = an.GetGraph(yAmplitude, xDist,
                                mult, nodeDiam, fontSize);
                else if (tc.SelectedTab == tc.TabPages["tpOptTime"])
                {
                    pbGraphTime.Image = an.GetGraph(yAmplitude, xDist,
                                mult, nodeDiam, fontSize);
                    pbMain.Image = an.GetHuntRes(pbMain.Width, pbMain.Height);
                }
                else if (tc.SelectedTab == tc.TabPages["tpOptRes"])
                    lbResult_SelectedValueChanged(null, null);
            }
            catch { }
        }
        void lbResult_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                ActNet anSel = (ActNet)lbResult.SelectedItem;
                pbMain.Image = anSel.GetHuntRes(pbMain.Width, pbMain.Height);
            }
            catch { }
        }
        void tbTime_TextChanged(object sender, EventArgs e)
        {
            CalculateOptRes();
        }

        void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "Файлы отчетов|*.htm";
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
                string s = "<P>Задача сетевого планирования проекта с учетом " +
                    "нескладируемых ресурсов при минимизации максимальных " +
                    "значений потребления ресурсов и ограничениях на сроки " +
                    "выполнения проекта и используемые ресурсы.</P>";
                s += "<P><IMG SRC = \"graph.bmp\"></IMG></P>";
                s += "<P>Количество ресурсов: " + nudRes.Value.ToString() + "</P>";
                s += "<P>Минимальное время завершения проекта: " +
                    an.OptimizeTime(ReadRes()).ToString() + "</P>";
                s += "<P><IMG SRC = \"huntrestime.bmp\"</IMG></P>";
                for (int i = 0; i < lbResult.Items.Count; i++)
                {
                    ActNet anCur = (ActNet)lbResult.Items[i];
                    s += "<P>" + anCur.ToString() + "</P>";
                    s += "<P><IMG SRC = \"iter" + i.ToString() + ".bmp\"></IMG></P>";
                }
                sw.Write(s);
                sw.Close();

                Bitmap bmp;
                bmp = an.GetGraph(yAmplitude, xDist, mult, nodeDiam, fontSize);
                bmp.Save("graph.bmp");
                bmp = an.GetHuntRes(600, 600);
                bmp.Save("huntrestime.bmp");
                for (int i = 0; i < lbResult.Items.Count; i++)
                {
                    ActNet anCur = (ActNet)lbResult.Items[i];
                    anCur.GetHuntRes(600, 600).Save("iter" + i.ToString() + ".bmp");
                }
            }
            catch { }
        }        
        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Автор: Кондауров А. С., группа АС-05-1");
        }
    }
}