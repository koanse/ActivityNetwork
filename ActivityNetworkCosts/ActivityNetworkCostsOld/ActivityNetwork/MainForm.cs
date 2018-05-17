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
        List<IterationState[]> listArrState;
        bool isBmpSizing = false;
        Point prevLocation;
        int xDist = 50, yAmplitude = 110, fontSize = 9, nodeDiam = 20;
        float mult = 1.7f;
        
        public MainForm()
        {
            InitializeComponent();
            dgvAct.Columns["МинДлительность"].ValueType = typeof(float);
            dgvAct.Columns["МаксДлительность"].ValueType = typeof(float);
            dgvAct.Columns["МинСтоимость"].ValueType = typeof(float);
            dgvAct.Columns["МаксСтоимость"].ValueType = typeof(float);            
        }
        Act[] ReadActs()
        {
            List<Act> listAct = new List<Act>();
            for (int i = 0; i < dgvAct.Rows.Count - 1; i++)
            {
                DataGridViewRow r = dgvAct.Rows[i];
                string mark = (string)(r.Cells["Название"] as DataGridViewCell).Value;
                float durMin = 0, durMax = 0, costMin = 0, costMax = 0;
                object val = (r.Cells["МинДлительность"] as DataGridViewCell).Value;
                if (val != null)
                    durMin = (float)val;
                val = (r.Cells["МаксДлительность"] as DataGridViewCell).Value;
                if (val != null)
                    durMax = (float)val;
                val = (r.Cells["МинСтоимость"] as DataGridViewCell).Value;
                if (val != null)
                    costMin = (float)val;
                    val = (r.Cells["МаксСтоимость"] as DataGridViewCell).Value;
                if (val != null)
                    costMax = (float)val;
                string prev = (string)(r.Cells["ПредРаботы"] as DataGridViewCell).Value;
                string[] arrPrev;
                if (prev != null)
                    arrPrev = prev.Split(new char[] { ' ', ',' });
                else
                    arrPrev = new string[] { };
                listAct.Add(new Act(mark, durMin, durMax, costMin, costMax, arrPrev));
            }
            return listAct.ToArray();
        }
        void Calculate()
        {
            try
            {
                lbResult.Items.Clear();
                listArrState = new List<IterationState[]>();
                Act[] arrAct = ReadActs(), arrActAll = arrAct;
                an = new ActNet(arrAct);
                an.BuildActNet();
                an.SetMaxActCost();
                string strAn = an.ToString(), strAnOld = "";
                //while (strAn != strAnOld)
                {
                    an.CalcStaticTimeParams();
                    lbResult.Items.Add(an);
                    arrAct = an.critPath;
                    int n = arrAct.Length + 1, m = arrAct.Length + n;   // кол. ур. и пер.
                    ExMatrix mP = new ExMatrix(n, m);
                    ExMatrix mB = new ExMatrix(n, 1);
                    ExMatrix mC = new ExMatrix(1, m);
                    double cCoeff = 0, sumDurMin = 0, sumDurMinB = 0;
                    for (int i = 0; i < arrAct.Length; i++)
                    {
                        mP.Elements[i, i] = 1;
                        mP.Elements[i, i + arrAct.Length] = 1;
                        mB.Elements[i, 0] = arrAct[i].durMax - arrAct[i].durMin;
                        mP.Elements[arrAct.Length, i] = 1; // строка для Sum(dur)
                        mC.Elements[0, i] = -arrAct[i].b;
                        sumDurMin += arrAct[i].durMin;
                        sumDurMinB += arrAct[i].durMin * arrAct[i].b;
                        cCoeff += arrAct[i].a;
                    }
                    mB.Elements[arrAct.Length, 0] = float.Parse(tbTime.Text) - sumDurMin;
                    mP.Elements[arrAct.Length, m - 1] = 1;
                    cCoeff -= sumDurMinB;
                    ExMatrix[] arrMP = new ExMatrix[m];
                    for (int i = 0; i < arrMP.Length; i++)
                        arrMP[i] = new ExMatrix(n, 1);
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < m; j++)
                            arrMP[j].Elements[i, 0] = mP.Elements[i, j];
                    for (int i = 0; i < mC.N; i++)
                        mC.Elements[0, i] *= -1;
                    MsMethod method = new MsMethod(arrMP, mB, mC, 0);
                    int[] arrIndex = new int[n];
                    for (int i = 0; i < n; i++)
                        arrIndex[i] = arrAct.Length + i;
                    method.SetBasis(arrIndex);
                    while (method.DoIteration()) ;
                    listArrState.Add((IterationState[])method.states.ToArray(typeof(IterationState)));
                    IterationState stateLast = (IterationState)method.states[method.states.Count - 1];
                    for (int i = 0; i < arrAct.Length; i++)
                        arrAct[i].dur = (float)stateLast.GetVarValue(i) + arrAct[i].durMin;
                    an.CalcStaticTimeParams();
                    foreach (Act a in arrActAll)
                        if (a.dur + a.TReserv <= a.durMax)
                            a.dur += a.TReserv;
                        else
                            a.dur = a.durMax;
                    strAnOld = strAn;
                    strAn = an.ToString();
                    an = new ActNet(an, true);
                    lbResult.Items.Add(an);
                }
            }
            catch { }
        }
        void CalculateOptRes()
        {
            ArrayList listAn = new ArrayList();
            try
            {                
                /*float Treal, Tdir = float.Parse(tbTime.Text);
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
                }*/
            }
            catch { }
            //ActNet[] arrAn = (ActNet[])listAn.ToArray(typeof(ActNet));
            //lbResult.Items.Clear();
            //lbResult.Items.AddRange(arrAn);
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
                anSel.OptimizeTime(new float[] { });
                pbMain.Image = anSel.GetHuntDiagram(pbMain.Width, pbMain.Height);
                lbIter.Items.Clear();
                foreach (IterationState s in listArrState[lbResult.SelectedIndex - 1])
                    lbIter.Items.Add(string.Format("F = {0}", s.GetFuncValue()));
            }
            catch { }
        }
        void lbIter_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                wb.DocumentText =
                    listArrState[lbResult.SelectedIndex - 1][lbIter.SelectedIndex].GetSimplexTablePart();
            }
            catch { }
        } 
        void tbTime_TextChanged(object sender, EventArgs e)
        {
            Calculate();
        }

        void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                /*saveFileDialog1.Filter = "Файлы отчетов|*.htm";
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
                }*/
            }
            catch { }
        }        
        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Автор: Васильев Д., группа АС-05-1");
        }
        void exampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvAct.Rows.Clear();
            dgvAct.Rows.Add(new object[] { "a", null, 1.0f, 2.0f, 3.0f, 4.0f });
            dgvAct.Rows.Add(new object[] { "b", null, 2.0f, 3.0f, 4.0f, 5.0f });
            dgvAct.Rows.Add(new object[] { "c", null, 1.0f, 2.0f, 3.0f, 4.0f });
            dgvAct.Rows.Add(new object[] { "d", "a", 5.0f, 6.0f, 7.0f, 8.0f });
            dgvAct.Rows.Add(new object[] { "e", "a", 3.0f, 4.0f, 5.0f, 6.0f });
            dgvAct.Rows.Add(new object[] { "f", "b", 2.0f, 3.0f, 4.0f, 5.0f });
            dgvAct.Rows.Add(new object[] { "g", "c", 4.0f, 5.0f, 6.0f, 7.0f });
            dgvAct.Rows.Add(new object[] { "h", "d", 2.0f, 3.0f, 4.0f, 5.0f });
            dgvAct.Rows.Add(new object[] { "i", "e", 1.0f, 2.0f, 3.0f, 4.0f });
            Calculate();
            SetBitmaps();
        }               
    }
}