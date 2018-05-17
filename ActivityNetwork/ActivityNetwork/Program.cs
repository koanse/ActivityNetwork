using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace ActivityNetwork
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Act[] arrAct = new Act[11];
            arrAct[0] = new Act("A", 1, new float[] { 1, 2 }, new string[] { });
            arrAct[1] = new Act("B", 2, new float[] { 1, 2 }, new string[] { });
            arrAct[2] = new Act("C", 3, new float[] { 1, 2 }, new string[] { });
            arrAct[3] = new Act("D", 2, new float[] { 1, 2 }, new string[] { "B" });
            arrAct[4] = new Act("E", 5, new float[] { 1, 2 }, new string[] { "A" });
            arrAct[5] = new Act("F", 3, new float[] { 1, 2 }, new string[] { "A" });
            arrAct[6] = new Act("G", 2, new float[] { 1, 2 }, new string[] { "C" });
            arrAct[7] = new Act("H", 1, new float[] { 1, 2 }, new string[] { "B", "D", "E", "K" });
            arrAct[8] = new Act("I", 4, new float[] { 1, 2 }, new string[] { "C", "G" });
            arrAct[9] = new Act("J", 200, new float[] { 1, 2 }, new string[] { "F", "G", "H" });
            arrAct[10] = new Act("K", 3, new float[] { 1, 2 }, new string[] { "C" });
            ActNet an = new ActNet(arrAct);

            /*Act[] arrAct = new Act[5];
            arrAct[0] = new Act("A", 1, new float[] { 1, 2 }, new string[] { });
            arrAct[1] = new Act("B", 2, new float[] { 1, 2 }, new string[] { });
            arrAct[2] = new Act("C", 3, new float[] { 1, 2 }, new string[] { });
            arrAct[3] = new Act("D", 2, new float[] { 1, 2 }, new string[] { });
            arrAct[4] = new Act("E", 5, new float[] { 1, 2 }, new string[] { });

            ActNet an = new ActNet(arrAct);*/
            an.BuildActNet();
            float t = an.OptimizeTime(new float[] { 100, 200});

            Application.Run(new MainForm());
        }
    }
    public class ActNet
    {
        Act[] arrAct;
        Node[] arrNode;
        float[] arrRes;
        float[] arrCrit; // массив критериев по ресурсам

        Node nRoot, nEnd;
        Act[] critPath;
        float critPathDur;
        bool[,] mP;
        int[] arrPi, arrRo;
        int piCount, roCount;

        public ActNet(Act[] arrAct)
        {
            this.arrAct = arrAct;
            foreach (Act a in arrAct)
            {
                a.arrPrev = new Act[a.arrMarkPrev.Length];
                for (int i = 0; i < a.arrMarkPrev.Length; i++)
                    a.arrPrev[i] = GetActByMark(a.arrMarkPrev[i]);
            }
        }
        public ActNet(ActNet an)    // копирование (включая ресурсы, нач. и зав. соб.)
        {
            arrAct = new Act[an.arrAct.Length];
            for (int i = 0; i < arrAct.Length; i++)
            {
                float[] arrResAct = null;
                if (an.arrAct[i].arrRes != null)
                    arrResAct = (float[])an.arrAct[i].arrRes.Clone();
                string[] arrMarkPrevAct = null;
                if (an.arrAct[i].arrMarkPrev != null)
                    arrMarkPrevAct = (string[])an.arrAct[i].arrMarkPrev.Clone();
                arrAct[i] = new Act(an.arrAct[i].mark, an.arrAct[i].dur,
                    arrResAct, arrMarkPrevAct);
            }
            arrNode = new Node[an.arrNode.Length];
            for (int i = 0; i < arrNode.Length; i++)
                arrNode[i] = new Node(an.arrNode[i].mark);
            for (int i = 0; i < arrAct.Length; i++)
            {
                arrAct[i].Start = GetNodeByMark(an.arrAct[i].Start.mark);
                arrAct[i].End = GetNodeByMark(an.arrAct[i].End.mark);
            }
            arrRes = null;
            if (an.arrRes != null)
                arrRes = (float[])an.arrRes.Clone();

            nRoot = GetNodeByMark(an.nRoot.mark);
            nEnd = GetNodeByMark(an.nEnd.mark);
        }
        public void BuildActNet()
        {
            // матрица предшествования
            FillMP();
            ArrayList list = new ArrayList(arrAct);
            ActComparerByPrevActs comparer = new ActComparerByPrevActs(arrAct, mP);
            list.Sort(comparer);
            arrAct = (Act[])list.ToArray(typeof(Act));
            FillMP();

            SetPi();
            SetRo();

            // создание вершин
            ArrayList listNode = new ArrayList();
            Node[] arrNodePi = new Node[piCount];
            for (int i = 0; i < piCount; i++)
            {
                arrNodePi[i] = new Node(string.Format("π{0}", i));
                listNode.Add(arrNodePi[i]);
            }

            Node[] arrNodeRo = new Node[roCount];
            for (int i = 0; i < roCount; i++)
            {
                arrNodeRo[i] = new Node(string.Format("ρ{0}", i + 1));
                listNode.Add(arrNodeRo[i]);
            }
            arrNode = (Node[])listNode.ToArray(typeof(Node));

            // связывание событий и работ
            for (int i = 0; i < arrAct.Length; i++)
            {
                arrAct[i].Start = arrNodePi[arrPi[i]];
                arrAct[i].End = arrNodeRo[arrRo[i]];
            }

            // фиктивные работы
            ArrayList listFa = new ArrayList();
            for (int pi = 0; pi < piCount; pi++)
            {
                int i;
                for (i = 0; i < arrPi.Length; i++)
                    if (arrPi[i] == pi)
                        break;
                for (int j = 0; j < arrAct.Length; j++)
                    if (mP[i, j] == true)
                    {
                        Act fa =
                            new Act("fict" + listFa.Count.ToString(), 0, null, null);
                        fa.Start = arrNodeRo[arrRo[j]];
                        fa.End = arrNodePi[pi];
                        listFa.Add(fa);
                    }
            }
            Act[] res = new Act[arrAct.Length + listFa.Count];
            arrAct.CopyTo(res, 0);
            listFa.CopyTo(res, arrAct.Length);
            arrAct = res;
            ArrayList listAct = new ArrayList(res);
            
            
            // удаление ненужных фикт. работ
            foreach (Act fa in listFa)
                if (GetPaths(fa.Start, fa.End).Length > 1)
                {
                    fa.Start.DelOutAct(fa);
                    fa.End.DelInAct(fa);
                    listAct.Remove(fa);
                }
            arrAct = (Act[])listAct.ToArray(typeof(Act));
            
            // "склеивание" событий (из вершины выходит только одна фикт. работа)
            foreach (Node n in arrNode)
            {
                if (n.arrOut.Length != 1 || n.arrOut[0].dur != 0)
                    continue;
                Act[] arrOut = n.arrOut;
                n.arrOut = new Act[0];
                Act fa = arrOut[0];
                foreach (Act a in fa.End.arrOut)
                    a.Start = n;
                foreach (Act a in fa.End.arrIn)
                {
                    if (a == fa)
                        continue;
                    a.End = n;
                }
                listAct.Remove(fa);
                listNode.Remove(fa.End);
                fa.End.arrIn = new Act[0];
                fa.End.arrOut = new Act[0];
            }
            arrNode = (Node[])listNode.ToArray(typeof(Node));
            arrAct = (Act[])listAct.ToArray(typeof(Act));

            // "склеивание" событий (в вершину входит только одна фикт. работа)
            foreach (Node n in arrNode)
            {
                if (n.arrIn.Length != 1 || n.arrIn[0].dur != 0)
                    continue;
                Act[] arrIn = n.arrIn;
                n.arrIn = new Act[0];
                Act fa = arrIn[0];
                foreach (Act a in fa.Start.arrIn)
                    a.End = n;
                foreach (Act a in fa.Start.arrOut)
                {
                    if (a == fa)
                        continue;
                    a.Start = n;
                }
                listAct.Remove(fa);
                listNode.Remove(fa.Start);
                fa.Start.arrIn = new Act[0];
                fa.Start.arrOut = new Act[0];
            }
            arrNode = (Node[])listNode.ToArray(typeof(Node));
            arrAct = (Act[])listAct.ToArray(typeof(Act));
            
            // ввод фиктивных событий для параллельных работ
            foreach (Act a1 in arrAct)
                foreach (Act a2 in arrAct)
                    if (a1.Start == a2.Start && a1.End == a2.End && a1 != a2)
                    {
                        Node n = new Node("fict");
                        Act fa = new Act("fict" + listAct.Count.ToString(), 0, null, null);
                        a2.End.DelInAct(a2);
                        a2.End = n;
                        fa.Start = n;
                        fa.End = a1.End;
                        listNode.Add(n);
                        listAct.Add(fa);
                    }
            arrAct = (Act[])listAct.ToArray(typeof(Act));
            arrNode = (Node[])listNode.ToArray(typeof(Node));
            
            // расчет временных параметров
            CalcStaticTimeParams();

            // именование и сортировка событий по Te
            NodeComparerByTe ncomparer = new NodeComparerByTe();
            listNode.Sort(ncomparer);
            arrNode = (Node[])listNode.ToArray(typeof(Node));

            for (int i = 0; i < arrNode.Length; i++)
                arrNode[i].mark = i.ToString();
        }
        public float OptimizeTime(float[] arrRes)
        {
            this.arrRes = (float[])arrRes.Clone();
            float tau = 0, tauNext;
            int resCount = arrRes.Length;
            foreach (Act a in arrAct)
                if (a.arrRes != null)
                    for (int i = 0; i < resCount; i++)
                        if (a.arrRes[i] > arrRes[i])
                            throw new Exception(string.Format("Недостаточно " +
                                "ресурсов вида {0}", i + 1));
            CalcStaticTimeParams();
            arrCrit = new float[resCount];
            float[] arrResUsed = new float[resCount];
            float[] arrResUsedPrev = new float[resCount];
            foreach (Act a in arrAct)
                a.t = a.Tbe;
            while (true)
            {
                tauNext = float.MaxValue;
                foreach (Act a in arrAct)
                    if (a.t > tau && a.t < tauNext)
                        tauNext = a.t;
                    else if (a.t + a.dur > tau && a.t + a.dur < tauNext)
                        tauNext = a.t + a.dur;
                if (tauNext == float.MaxValue)
                    break;

                ArrayList listActFront = new ArrayList();
                foreach (Act a in arrAct)
                    if (!(a.t <= tau && a.t + a.dur <= tau ||
                        a.t >= tauNext && a.t + a.dur >= tauNext) &&
                        a.dur != 0)
                        listActFront.Add(a);
                ActComparerByRes comparer = new ActComparerByRes(this, tau);
                listActFront.Sort(comparer);

                Act[] arrActFront = (Act[])listActFront.ToArray(typeof(Act));
                float[] arrResRest = (float[])arrRes.Clone();
                foreach (Act a in arrActFront)
                {
                    int i;
                    for (i = 0; i < resCount; i++)
                        if (arrResRest[i] - a.arrRes[i] < 0)
                            break;
                    if (i < resCount)
                        continue;
                    for (i = 0; i < resCount; i++)
                        arrResRest[i] -= a.arrRes[i];
                    listActFront.Remove(a);
                }
                foreach (Act a in listActFront)
                    ShiftActTime(a.mark, tauNext - a.t);

                for (int i = 0; i < resCount; i++)
                    arrResUsed[i] = arrRes[i] - arrResRest[i];
                if (tau > 0)
                    for (int i = 0; i < resCount; i++)
                        arrCrit[i] += (arrResUsed[i] - arrResUsedPrev[i]) *
                            (arrResUsed[i] - arrResUsedPrev[i]) / (arrRes[i] * arrRes[i]);
                arrResUsedPrev = (float[])arrResUsed.Clone();

                tau = tauNext;
            }
            return GetDuration();
        }
        public Bitmap GetGraph(int yAmplitude, int xDist, float mult,
            int nodeDiam, int fontSize)
        {
            if (yAmplitude == -1)
                yAmplitude = 110;
            if (xDist == -1)
                xDist = 50;
            if (mult == -1)
                mult = 1.7f;
            if (nodeDiam == -1)
                nodeDiam = 20;
            if (fontSize == -1)
                fontSize = 9;
            int width;
            int height;
            width = height = xDist * (arrNode.Length - 1) + nodeDiam + 2;
            if (width < 2 * (yAmplitude + nodeDiam + 5))
                width = height = 2 * (yAmplitude + nodeDiam + 5);
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            Point[] arrPoint = new Point[arrNode.Length];
            int yInit = height / 2;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Font f = new Font("Arial", fontSize);
            g.Clear(Color.White);
            for (int i = 0; i < arrNode.Length; i++)
            {
                int y = yInit + (int)(yAmplitude * (float)Math.Sin(mult * i));
                int x = i * xDist;
                arrPoint[i] = new Point(x, y);
                g.DrawEllipse(Pens.Black, x, y, nodeDiam, nodeDiam);
            }

            foreach (Act a in arrAct)
            {
                int i;
                for (i = 0; i < arrNode.Length; i++)
                    if (a.Start == arrNode[i])
                        break;
                int j;
                for (j = 0; j < arrNode.Length; j++)
                    if (a.End == arrNode[j])
                        break;
                Pen pen = Pens.Green;
                if (a.dur == 0)
                    pen = Pens.Red;
                Point p1 = arrPoint[i], p2 = arrPoint[j];
                int x1 = p1.X + nodeDiam / 2, x2 = p2.X + nodeDiam / 2; ;
                int y1 = p1.Y + nodeDiam / 2, y2 = p2.Y + nodeDiam / 2; ;
                g.DrawLine(pen, x1, y1, x2, y2);
                int x = (x1 + x2) / 2;
                int y = (y1 + y2) / 2;
                g.DrawString(a.mark, f, Brushes.Black, x, y, sf);

                int lenOfArrow = 5;
                float lenOfAct =
                    (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
                x = (int)(x2 - (x2 - x1) * lenOfArrow / lenOfAct);
                y = (int)(y2 - (y2 - y1) * lenOfArrow / lenOfAct);
                g.DrawLine(pen, x, y - 5, x2, y2);
                g.DrawLine(pen, x, y + 5, x2, y2);
            }

            for (int i = 0; i < arrNode.Length; i++)
            {
                int x = arrPoint[i].X;
                int y = arrPoint[i].Y;
                g.DrawString(arrNode[i].mark, f, Brushes.Blue,
                    x + nodeDiam / 2, y + nodeDiam / 2, sf);
            }
            return bmp;
        }
        public Bitmap GetHuntDiagram(int width, int height)
        {
            if (width == -1)
                width = 300;
            if (height == -1)
                height = 300;
            int margin = 40, lineWidth = 5, fontSize = 9, markSpace = 5;
            ArrayList listAct = new ArrayList(arrAct);
            ActComparerByTime comparer = new ActComparerByTime();
            listAct.Sort(comparer);
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            float yDelta = (height - 2 * margin) / listAct.Count;
            float tMax = GetDuration();
            float xDelta = (width - 2 * margin) / tMax;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Font f = new Font("Arial", fontSize);            
            Pen p = new Pen(Color.Green, lineWidth);
            g.Clear(Color.White);
            ArrayList listXMark = new ArrayList();
            for (int i = 0; i < listAct.Count; i++)
            {
                Act a = (Act)listAct[i];
                int x1 = (int)(a.t * xDelta + margin);
                int x2 = (int)((a.t + a.dur) * xDelta + margin);
                int y = (int)(height - ((i + 1) * yDelta + margin));
                g.DrawLine(p, x1, y, x2, y);
                g.DrawString(a.Start.mark.ToString(), f, Brushes.Blue, x1 - markSpace, y, sf);
                g.DrawString(a.End.mark.ToString(), f, Brushes.Blue, x2 + markSpace, y, sf);
                if (listXMark.Contains(a.t) == false)
                    listXMark.Add(a.t);
                if (listXMark.Contains(a.t + a.dur) == false)
                    listXMark.Add(a.t + a.dur);
                g.DrawString(a.mark, f, Brushes.Blue, margin / 2, y, sf);
                g.DrawLine(Pens.Black, x1, height - margin - 2, x1, height - margin + 2);
                g.DrawLine(Pens.Black, x2, height - margin - 2, x2, height - margin + 2);
            }
            
            listXMark.Sort();
            foreach (float t in listXMark)
            {
                int x = (int)(t * xDelta + margin);
                int y = height - margin / 2;
                g.DrawString(t.ToString(), f, Brushes.Blue, x, y, sf);
            }

            g.DrawLine(Pens.Black, margin, height - margin, width - margin, height - margin);
            g.DrawLine(Pens.Black, margin, height - margin, margin, margin);
            g.DrawString("Линейная диаграмма", f, Brushes.Black, width / 2, margin / 2, sf);
            return bmp;
        }
        public Bitmap GetResGraph(int width, int height, int resIndex)
        {
            if (width == -1)
                width = 300;
            if (height == -1)
                height = 300;
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            int margin = 40, fontSize = 9;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Font f = new Font("Arial", fontSize);
            float tMax = GetDuration(), resMax = arrRes[resIndex];
            float yDelta = (height - 2 * margin) / resMax;
            float xDelta = (width - 2 * margin) / tMax;
            float tau = 0, tauNext;
            ArrayList listPoint = new ArrayList();
            while (true)
            {
                tauNext = float.MaxValue;
                foreach (Act a in arrAct)
                    if (a.t > tau && a.t < tauNext)
                        tauNext = a.t;
                    else if (a.t + a.dur > tau && a.t + a.dur < tauNext)
                        tauNext = a.t + a.dur;
                if (tauNext == float.MaxValue)
                    break;
                
                ArrayList listActFront = new ArrayList();
                foreach (Act a in arrAct)
                    if (!(a.t <= tau && a.t + a.dur <= tau ||
                        a.t >= tauNext && a.t + a.dur >= tauNext) &&
                        a.dur != 0)
                        listActFront.Add(a);

                float resUsed = 0;
                foreach (Act a in listActFront)
                    if (a.arrRes != null)
                        resUsed += a.arrRes[resIndex];

                int x1 = (int)(margin + xDelta * tau);
                int x2 = (int)(margin + xDelta * tauNext);
                int y = (int)(height - (margin + yDelta * resUsed));
                listPoint.Add(new Point(x1, y));
                listPoint.Add(new Point(x2, y));
                tau = tauNext;
            }
            g.Clear(Color.White);
            g.DrawLines(Pens.Green, (Point[])listPoint.ToArray(typeof(Point)));
            int yMax = (int)(height - (margin + yDelta * resMax));
            g.DrawLine(Pens.Red, margin, yMax, width - margin, yMax);

            g.DrawLine(Pens.Black, margin, height - margin, width - margin, height - margin);
            g.DrawLine(Pens.Black, margin, height - margin, margin, margin);
            
            g.DrawString("0", f, Brushes.Blue, margin / 2, height - margin, sf);
            g.DrawString(resMax.ToString(), f, Brushes.Blue, margin / 2, margin, sf);

            string s =
                string.Format("Ресурс{0}", resIndex + 1);
            g.DrawString(s, f, Brushes.Black, width / 2, margin / 2, sf);
            return bmp;
        }
        public Bitmap GetHuntRes(int width, int height)
        {
            if (width == -1)
                width = 300;
            if (height == -1)
                height = 600;
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            int resCount = arrRes.Length;
            int h = height / (resCount + 2);
            g.Clear(Color.White);
            Bitmap b = GetHuntDiagram(width, 2 * h);
            g.DrawImage(b, 0, 0);
            for (int i = 0; i < resCount; i++)
            {
                g.DrawLine(Pens.Black, 0, (i + 2) * h - 1, width, (i + 2) * h - 1);
                b = GetResGraph(width, h, i);
                g.DrawImage(b, 0, (i + 2) * h);                
            }
            return bmp;
        }
        public void ShiftActTime(string actMark, float delta)
        {
            Act act = GetActByMark(actMark);
            act.t += delta;
            bool changed;
            do
            {
                changed = false;
                foreach (Node n in arrNode)
                {
                    float nTime = GetNodeTime(n);
                    foreach (Act a in n.arrOut)
                        if (a.t < nTime)
                        {
                            changed = true;
                            a.t = nTime;
                        }
                }
            }
            while (changed);
        }
        public float GetDuration()  // реальная продолж-ть проекта
        {
            float t = 0;
            foreach (Act a in nEnd.arrIn)
                if (a.t + a.dur > t)
                    t = a.t + a.dur;
            return t;
        }
        public float GetResource(int index) // запасы ресурсов
        {
            return arrRes[index];
        }
        public float[] GetCriterions()    // знач. крит. по ресурсам
        {
            return (float[])arrCrit.Clone();
        }
        public override string ToString()
        {
            string s = "";
            try
            {
                float dur = (float)Math.Round(GetDuration(), 3);
                s += "T = " + dur.ToString() + "; ";
                for (int i = 0; i < arrRes.Length; i++)
                    s += string.Format("Р{0}: {1} [{2}]; ", i + 1, arrRes[i], arrCrit[i]);
                return s;
            }
            catch
            {
                return "Error";
            }
            
        }

        void FillMP()
        {
            mP = new bool[arrAct.Length, arrAct.Length];
            for (int i = 0; i < arrAct.Length; i++)
                for (int j = 0; j < arrAct.Length; j++)
                {
                    mP[i, j] = false;
                    foreach (Act a in arrAct[i].arrPrev)
                        if (arrAct[j] == a)
                        {
                            mP[i, j] = true;
                            break;
                        }
                }

            bool hasChanges;
            do
            {
                hasChanges = false;
                for (int i = 0; i < arrAct.Length; i++)
                    for (int j = 0; j < arrAct.Length; j++)
                    {
                        if (mP[i, j] == true)
                            for (int k = 0; k < arrAct.Length; k++)
                                if (mP[k, i] == true && mP[k, j] == false)
                                {
                                    hasChanges = true;
                                    mP[k, j] = true;
                                }
                    }
            }
            while (hasChanges);
        }
        void SetPi()
        {
            arrPi = new int[arrAct.Length];
            piCount = 0;
            for (int i = 0; i < arrAct.Length; i++)
            {
                int k;
                for (k = 0; k < i; k++)
                {
                    int j;
                    for (j = 0; j < arrAct.Length; j++)
                        if (mP[i, j] != mP[k, j])
                            break;
                    if (j == arrAct.Length)
                        break;
                }
                if (k < i)
                    arrPi[i] = arrPi[k];
                else
                {
                    arrPi[i] = piCount;
                    piCount++;
                }
            }
        }
        void SetRo()
        {
            arrRo = new int[arrAct.Length];
            roCount = 0;
            for (int j = 0; j < arrAct.Length; j++)
            {
                int k;
                for (k = 0; k < j; k++)
                {
                    int i;
                    for (i = 0; i < arrAct.Length; i++)
                        if (mP[i, j] != mP[i, k])
                            break;
                    if (i == arrAct.Length)
                        break;
                }
                if (k < j)
                    arrRo[j] = arrRo[k];
                else
                {
                    arrRo[j] = roCount;
                    roCount++;
                }
            }
        }
        void CalcStaticTimeParams()
        {
            // нач. и зав. соб.
            foreach (Node n in arrNode)
                if (n.arrOut.Length == 0)
                    nEnd = n;
                else if (n.arrIn.Length == 0)
                    nRoot = n;

            // ранние сроки событий
            nRoot = arrNode[0];
            ArrayList[] paths;
            foreach (Node n in arrNode)
            {
                if (n == nRoot)
                {
                    n.Te = 0;
                    continue;
                }
                paths = GetPaths(nRoot, n);
                float durMax = 0;
                foreach (ArrayList p in paths)
                {
                    float dur = 0;
                    foreach (Act a in p)
                        dur += a.dur;
                    if (dur > durMax)
                        durMax = dur;
                }
                n.Te = durMax;
            }

            // критический путь
            paths = GetPaths(nRoot, nEnd);
            critPathDur = 0;
            foreach (ArrayList p in paths)
            {
                float dur = 0;
                foreach (Act a in p)
                    dur += a.dur;
                if (dur > critPathDur)
                {
                    critPathDur = dur;
                    critPath = (Act[])p.ToArray(typeof(Act));
                }
            }

            // поздние сроки событий
            foreach (Node n in arrNode)
            {
                if (n == nEnd)
                {
                    n.Tl = critPathDur;
                    continue;
                }
                paths = GetPaths(n, nEnd);
                float durMax = 0;
                foreach (ArrayList p in paths)
                {
                    float dur = 0;
                    foreach (Act a in p)
                        dur += a.dur;
                    if (dur > durMax)
                        durMax = dur;
                }
                n.Tl = critPathDur - durMax;
            }

            // работы
            foreach (Act a in arrAct)
            {
                a.Tbe = a.Start.Te;
                a.Tee = a.Start.Te + a.dur;
                a.Tel = a.End.Tl;
                a.Tbl = a.End.Tl - a.dur;
                a.TReserv = a.End.Tl - a.Start.Te - a.dur;
            }
        }
        ArrayList[] GetPaths(Node nStart, Node nEnd)
        {
            if (nStart == nEnd)
                return null;
            ArrayList res = new ArrayList();
            foreach (Act a in nStart.arrOut)
            {
                if (a.End == nEnd)
                {
                    ArrayList al = new ArrayList();
                    al.Add(a);
                    res.Add(al);
                    continue;
                }
                ArrayList[] paths = GetPaths(a.End, nEnd);
                if (paths != null)
                    foreach (ArrayList al in paths)
                    {
                        al.Insert(0, a);
                        res.Add(al);
                    }
            }
            if (res.Count == 0)
                return null;
            return (ArrayList[])res.ToArray(typeof(ArrayList));
        }
        Act GetActByMark(string mark)
        {
            foreach (Act a in arrAct)
                if (a.mark == mark)
                    return a;
            throw new Exception("Работа не найдена");
        }
        Node GetNodeByMark(string mark)
        {
            foreach (Node n in arrNode)
                if (n.mark == mark)
                    return n;
            throw new Exception("Событие не найдено");
        }
        float GetNodeTime(Node n)   // время наступления события
        {
            float t = 0;
            foreach (Act a in n.arrIn)
                if (a.t + a.dur > t)
                    t = a.t + a.dur;
            return t;
        }
    }
    public class Act
    {
        public string mark;
        public float dur;
        public float t; // время начала
        public float Tbe, Tee, Tbl, Tel, TReserv; // b - нач., e - оконч., e - ран., l - позд.
        public float[] arrRes;
        public string[] arrMarkPrev;
        public Act[] arrPrev;
        Node nStart, nEnd;

        public Node Start
        {
            get { return nStart; }
            set
            {
                Node n = (Node)value;
                n.AddOutAct(this);
                nStart = n;
            }
        }
        public Node End
        {
            get { return nEnd; }
            set
            {
                Node n = (Node)value;
                n.AddInAct(this);
                nEnd = n;
            }
        }
        public Act(string mark, float dur, float[] arrRes, string[] arrMarkPrev)
        {
            this.mark = mark;
            this.dur = dur;
            this.arrRes = arrRes;
            this.arrMarkPrev = arrMarkPrev;
        }
    }
    public class ActComparerByPrevActs : IComparer
    {
        int[] arrRank;
        Act[] arrAct;
        public ActComparerByPrevActs(Act[] arrAct, bool[,] mP)
        {
            this.arrAct = arrAct;
            arrRank = new int[arrAct.Length];
            for (int i = 0; i < arrAct.Length; i++)
            {
                int sum = 0;
                for (int j = 0; j < arrAct.Length; j++)
                    if (mP[i, j] == true)
                        sum++;
                arrRank[i] = sum;
            }
        }
        public int Compare(object x, object y)
        {
            Act actX = (Act)x, actY = (Act)y;
            int rankX = 0, rankY = 0;
            for (int i = 0; i < arrAct.Length; i++)
            {
                if (arrAct[i] == actX)
                    rankX = arrRank[i];
                if (arrAct[i] == actY)
                    rankY = arrRank[i];
            }
            if (rankX != rankY)
                return rankX.CompareTo(rankY);
            return actX.mark.CompareTo(actY.mark);
        }
    }
    public class ActComparerByRes : IComparer
    {
        ActNet actNet;
        float tau, dur;
        public ActComparerByRes(ActNet actNet, float tau)
        {
            this.actNet = new ActNet(actNet);
            this.tau = tau;
            dur = actNet.GetDuration();
        }
        public int Compare(object x, object y)
        {
            Act actX = (Act)x, actY = (Act)y;
            if (actX.t != actY.t)
                return actX.t.CompareTo(actY.t);

            float reservX, reservY, delta = 10000;
            ActNet an = new ActNet(actNet);
            an.ShiftActTime(actX.mark, delta);
            reservX = dur + delta - an.GetDuration() - (tau - actX.t);
            an = new ActNet(actNet);
            an.ShiftActTime(actY.mark, delta);
            reservY = dur + delta - an.GetDuration() - (tau - actY.t);
            if (reservX != reservY)
                return reservX.CompareTo(reservY);

            float resX = 0, resY = 0;
            for (int i = 0; i < actX.arrRes.Length; i++)
                resX += actX.arrRes[i] / an.GetResource(i);
            for (int i = 0; i < actY.arrRes.Length; i++)
                resY += actY.arrRes[i] / an.GetResource(i);
            if (resX != resY)
                return -resX.CompareTo(resY);
            return actX.mark.CompareTo(actY.mark);
        }
    }
    public class ActComparerByTime : IComparer
    {
        public int Compare(object x, object y)
        {
            Act actX = (Act)x, actY = (Act)y;
            if (actX.t != actY.t)
                return actX.t.CompareTo(actY.t);            
            float endTimeX = actX.t + actX.dur;
            float endTimeY = actY.t + actY.dur;
            if (endTimeX != endTimeY)
                return endTimeX.CompareTo(endTimeY);
            return actX.mark.CompareTo(actY.mark);
        }
    }
    public class NodeComparerByTe : IComparer
    {
        public int Compare(object x, object y)
        {
            Node nX = (Node)x, nY = (Node)y;
            if (nX.Te != nY.Te)
                return nX.Te.CompareTo(nY.Te);
            return nX.mark.CompareTo(nY.mark);
        }
    }
    public class Node
    {
        public string mark;
        public float Te, Tl;    // ранний и поздний сроки
        public Act[] arrIn, arrOut;

        public Node(string mark)
        {
            this.mark = mark;
            arrIn = new Act[0];
            arrOut = new Act[0];
        }
        public void AddInAct(Act a)
        {
            Act[] res = new Act[arrIn.Length + 1];
            arrIn.CopyTo(res, 0);
            res[arrIn.Length] = a;
            arrIn = res;
        }
        public void AddOutAct(Act a)
        {
            Act[] res = new Act[arrOut.Length + 1];
            arrOut.CopyTo(res, 0);
            res[arrOut.Length] = a;
            arrOut = res;
        }
        public void DelInAct(Act a)
        {
            Act[] res = new Act[arrIn.Length - 1];
            for (int i = 0, j = 0; i < arrIn.Length; i++, j++)
            {
                if (arrIn[i] == a)
                {
                    j--;
                    continue;
                }
                res[j] = arrIn[i];
            }
            arrIn = res;
        }
        public void DelOutAct(Act a)
        {
            Act[] res = new Act[arrOut.Length - 1];
            for (int i = 0, j = 0; i < arrOut.Length; i++, j++)
            {
                if (arrOut[i] == a)
                {
                    j--;
                    continue;
                }
                res[j] = arrOut[i];
            }
            arrOut = res;
        }
    }
}