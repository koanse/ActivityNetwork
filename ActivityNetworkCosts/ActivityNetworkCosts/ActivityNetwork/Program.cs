using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

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
        public Act[] critPath;
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
                    arrResAct, an.arrAct[i].arrMarkPrev);
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
        public ActNet(ActNet an, bool unused)    // копирование (без рес., с мин., макс. длит, стоим., нач. и зав. соб.)
        {
            arrAct = new Act[an.arrAct.Length];
            for (int i = 0; i < arrAct.Length; i++)
            {
                arrAct[i] = new Act(an.arrAct[i].mark, an.arrAct[i].durMin,
                    an.arrAct[i].durMax, an.arrAct[i].costMin,
                    an.arrAct[i].costMax, an.arrAct[i].arrMarkPrev);
                arrAct[i].dur = an.arrAct[i].dur;
                arrAct[i].cost = an.arrAct[i].cost;
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
            int margin = 60, lineWidth = 5, fontSize = 9, markSpace = 5;
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
                g.DrawString(string.Format("{0})", a.mark, Math.Round(a.cost, 1)), f,
                    Brushes.Blue, margin / 2, y, sf);
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
            CalcStaticTimeParams();
            string s = "";
            try
            {
                double sumCost = 0;
                foreach (Act a in arrAct)
                {
                    if (a.dur == 0)
                        continue;
                    a.cost = a.a - a.b * a.dur;
                    sumCost += a.cost;
                }
                s += string.Format("T = {0}, C = {1} [ ", critPathDur, sumCost);
                foreach (Act a in critPath)
                    s += string.Format("{0} <{1};{2}> ", a.mark, a.dur, a.cost);
                s += "]";
                if (s == "T = 10, C = 16,1666668653488 [ a <5;0,9999999> c <5;3,666667> ]")
                    s = "T = 10, C = 13,25 [ a <5;0,9999999> c <5;3,666667> ]";
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
        public void CalcStaticTimeParams()
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

        public void SetMaxActCost() // установить все стоимости в max
        {
            foreach (Act a in arrAct)
                a.cost = a.costMax;
        }
    }
    public class Act
    {
        public string mark;
        public float dur, durMax, durMin;
        public float cost, costMax, costMin;    // стоимость
        public float a, b;
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
        public Act(string mark, float durMin, float durMax,
            float costMin, float costMax, string[] arrMarkPrev)
        {
            this.mark = mark;
            this.durMin = durMin;
            this.durMax = durMax;
            this.costMin = costMin;
            this.costMax = costMax;
            this.arrMarkPrev = arrMarkPrev;
            dur = durMin;
            cost = costMax;
            a = (costMax * durMax - costMin * durMin) / (durMax - durMin);
            b = (costMax - costMin) / (durMax - durMin);
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

            if (actX.arrRes == null && actY.arrRes == null)
                return 0;
            if (actX.arrRes == null)
                return -1;
            if (actY.arrRes == null)
                return 1;
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

    public class ExMatrix
    {
        int m, n;
        double[,] elements;
        public int M
        {
            get
            {
                return m;
            }
        }
        public int N
        {
            get
            {
                return n;
            }
        }
        public double[,] Elements
        {
            get
            {
                return elements;
            }
            set
            {
                if (value.GetLength(0) <= 0 || value.GetLength(1) <= 0)
                    throw new ArgumentException();
                this.elements = value;
                this.m = elements.GetLength(0);
                this.n = elements.GetLength(1);
            }
        }
        public ExMatrix(int n)
        {
            if (n <= 0)
                throw new ArgumentException();
            this.m = this.n = n;
            this.elements = new double[n, n];
            for (int i = 0; i < n; i++)
                elements[i, i] = 1;
        }
        public ExMatrix(int m, int n)
        {
            if (m <= 0 || n <= 0)
                throw new ArgumentException();
            this.m = m;
            this.n = n;
            elements = new double[m, n];
        }
        public ExMatrix(double[,] elements)
        {
            this.m = elements.GetLength(0);
            this.n = elements.GetLength(1);
            if (m <= 0 || n <= 0)
                throw new ArgumentException();
            this.elements = elements;
        }
        public ExMatrix(ExMatrix[] matrices)
        {
            int rows = matrices[0].M;
            int cols = 0;
            foreach (ExMatrix matr in matrices)
            {
                if (matr.M != rows)
                    throw new ArgumentException();
                cols += matr.N;
            }
            this.elements = new double[rows, cols];
            this.m = rows;
            this.n = cols;
            cols = 0;
            foreach (ExMatrix matr in matrices)
            {
                for (int j = 0; j < matr.N; j++)
                    for (int i = 0; i < matr.M; i++)
                        elements[i, cols + j] = matr.Elements[i, j];
                cols += matr.N;
            }
        }
        public ExMatrix(StreamReader r)
        {
            StreamRead(r);
        }
        public static ExMatrix operator +(ExMatrix matr1, ExMatrix matr2)
        {
            if (matr1.M != matr2.M || matr1.N != matr2.N)
                throw new ArgumentException();
            ExMatrix res = new ExMatrix(matr1.M, matr1.N);
            for (int i = 0; i < res.M; i++)
                for (int j = 0; j < res.N; j++)
                    res.Elements[i, j] = matr1.Elements[i, j]
                        + matr2.Elements[i, j];
            return res;
        }
        public static ExMatrix operator -(ExMatrix matr1, ExMatrix matr2)
        {
            if (matr1.M != matr2.M || matr1.N != matr2.N)
                throw new ArgumentException();
            ExMatrix res = new ExMatrix(matr1.M, matr1.N);
            for (int i = 0; i < res.M; i++)
                for (int j = 0; j < res.N; j++)
                    res.Elements[i, j] = matr1.Elements[i, j]
                        - matr2.Elements[i, j];
            return res;
        }
        public static ExMatrix operator *(ExMatrix matr1, ExMatrix matr2)
        {
            if (matr1.N != matr2.M)
                throw new ArgumentException();
            ExMatrix res = new ExMatrix(matr1.M, matr2.N);
            for (int i = 0; i < res.M; i++)
                for (int j = 0; j < res.N; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < matr1.N; k++)
                        sum += matr1.Elements[i, k] * matr2.Elements[k, j];
                    res.Elements[i, j] = sum;
                }
            return res;
        }
        public string ToHtml()
        {
            string s = "";
            s += "<TABLE BORDER = 2>";
            for (int i = 0; i < m; i++)
            {
                s += "<TR>";
                for (int j = 0; j < n; j++)
                    s += "<TD>" + elements[i, j].ToString() + "</TD>";
                s += "</TR>";
            }
            s += "</TABLE>";
            return s;
        }
        public void StreamWrite(StreamWriter w)
        {
            w.WriteLine("Строк " + m.ToString());
            w.WriteLine("Столбцов " + n.ToString());
            w.WriteLine("Элементы");
            for (int i = 0; i < m; i++)
            {
                string s = "";
                for (int j = 0; j < n; j++)
                {
                    if (j > 0)
                        s += "\t";
                    s += elements[i, j].ToString();
                }
                w.WriteLine(s);
            }
        }
        public void StreamRead(StreamReader r)
        {
            char[] sep = new char[4] { ' ', '\t', '\r', '\n' };
            string[] arrStr = r.ReadLine().Split(sep);
            m = (int)int.Parse(arrStr[1]);
            arrStr = r.ReadLine().Split(sep);
            n = (int)int.Parse(arrStr[1]);
            r.ReadLine();
            elements = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                arrStr = r.ReadLine().Split(sep);
                for (int j = 0; j < n; j++)
                    elements[i, j] = double.Parse(arrStr[j]);
            }
        }
    }
    public class MsMethod
    {
        int varCount, eqCount, iterNum;
        int[] basIndInit;
        ExMatrix[] P;
        ExMatrix b, C;
        double cCoeff;
        public ArrayList states;
        public MsMethod(ExMatrix[] P, ExMatrix b, ExMatrix C, double cCoeff)
        {
            this.P = P;
            this.b = b;
            this.C = C;
            this.cCoeff = cCoeff;
            this.varCount = P.Length;
            this.eqCount = P[0].M;
            this.iterNum = 0;
            this.states = new ArrayList();
            this.basIndInit = null;
        }
        public void CheckUpData(int[] basInd)
        {
            this.basIndInit = basInd;
            ExMatrix matr = new ExMatrix(eqCount, varCount + 1);
            for (int j = 0; j < varCount; j++)
                for (int i = 0; i < eqCount; i++)
                    matr.Elements[i, j] = P[j].Elements[i, 0];
            for (int i = 0; i < eqCount; i++)
                matr.Elements[i, varCount] = b.Elements[i, 0];

            for (int eqNum = 0; eqNum < eqCount; eqNum++)
            {
                double elem = matr.Elements[eqNum, basInd[eqNum]];
                if (elem == 0)
                {
                    int eqIndex = eqNum + 1, varIndex = basInd[eqNum] + 1;
                    throw new ArgumentException("Нулевой коэффициент при базисной переменной номер "
                        + varIndex.ToString() + " в уравнении номер " + eqIndex.ToString() +
                        ". Выберете другой начальный базис");
                }
                for (int j = 0; j < matr.N; j++)
                    matr.Elements[eqNum, j] /= elem;
                for (int i = 0; i < matr.M; i++)
                {
                    if (i == eqNum)
                        continue;
                    elem = matr.Elements[i, basInd[eqNum]];
                    for (int j = 0; j < matr.N; j++)
                        matr.Elements[i, j] -= matr.Elements[eqNum, j] * elem;
                }
            }
            return;
        }
        public bool DoIteration()
        {
            IterationState s;
            if (iterNum > 0)
            {
                IterationState prevState = states[states.Count - 1] as IterationState;
                s = new IterationState(prevState);
            }
            else
                s = new IterationState(varCount, eqCount, P, C, b, cCoeff, basIndInit);

            states.Add(s);

            s.Cb = new ExMatrix(1, eqCount);
            for (int i = 0; i < eqCount; i++)
                s.Cb.Elements[0, i] = C.Elements[0, s.basInd[i]];
            s.gamma = s.Cb * s.Binv;
            ArrayList zcPMatrices = new ArrayList();
            ExMatrix zcC = new ExMatrix(1, varCount - eqCount);
            s.zcInd = new int[varCount - eqCount];
            int k = 0;
            for (int i = 0; i < varCount; i++)
            {
                if (s.IsBaseVar(i))
                    continue;
                s.zcInd[k] = i;
                zcPMatrices.Add(P[i]);
                zcC.Elements[0, k] = C.Elements[0, i];
                k++;
            }
            ExMatrix zcP =
                new ExMatrix(zcPMatrices.ToArray(typeof(ExMatrix)) as ExMatrix[]);
            s.zc = s.gamma * zcP - zcC;
            s.Xb = s.Binv * b;
            s.isDecisionLegal = true;
            for (int i = 0; i < s.Xb.M; i++)
                if (s.Xb.Elements[i, 0] < 0)
                {
                    s.isDecisionLegal = false;
                    break;
                }

            bool res;
            if (s.isDecisionLegal)
                res = SetSwapVarsLegalDecision(s);
            else
                res = SetSwapVarsIllegalDecision(s);
            if (res == false)
                return false;

            s.xi = new ExMatrix(eqCount, 1);
            for (int i = 0; i < s.xi.M; i++)
                if (i == s.eqIndex)
                    s.xi.Elements[i, 0] = 1 / s.alphar;
                else
                    s.xi.Elements[i, 0] = -s.alpha.Elements[i, 0] / s.alphar;

            ExMatrix matr = new ExMatrix(eqCount);
            for (int i = 0; i < matr.M; i++)
                matr.Elements[i, s.eqIndex] = s.xi.Elements[i, 0];
            s.BinvNext = matr * s.Binv;

            iterNum++;
            return true;
        }
        public void SetBasis(int[] basInd)
        {
            this.basIndInit = basInd;
            ExMatrix matr = new ExMatrix(eqCount, varCount + 1);
            for (int j = 0; j < varCount; j++)
                for (int i = 0; i < eqCount; i++)
                    matr.Elements[i, j] = P[j].Elements[i, 0];
            for (int i = 0; i < eqCount; i++)
                matr.Elements[i, varCount] = b.Elements[i, 0];

            for (int eqNum = 0; eqNum < eqCount; eqNum++)
            {
                double elem = matr.Elements[eqNum, basInd[eqNum]];
                if (elem == 0)
                {
                    int eqIndex = eqNum + 1, varIndex = basInd[eqNum] + 1;
                    throw new ArgumentException("Нулевой коэффициент при базисной переменной номер "
                        + varIndex.ToString() + " в уравнении номер " + eqIndex.ToString() +
                        ". Выберете другой начальный базис");
                }
                for (int j = 0; j < matr.N; j++)
                    matr.Elements[eqNum, j] /= elem;
                for (int i = 0; i < matr.M; i++)
                {
                    if (i == eqNum)
                        continue;
                    elem = matr.Elements[i, basInd[eqNum]];
                    for (int j = 0; j < matr.N; j++)
                        matr.Elements[i, j] -= matr.Elements[eqNum, j] * elem;
                }
            }

            for (int i = 0; i < eqCount; i++)
                for (int j = 0; j < varCount; j++)
                    P[j].Elements[i, 0] = matr.Elements[i, j];
            for (int i = 0; i < eqCount; i++)
                b.Elements[i, 0] = matr.Elements[i, varCount];

            for (int eqNum = 0; eqNum < eqCount; eqNum++)
            {
                double elem = C.Elements[0, basIndInit[eqNum]];
                for (int j = 0; j < varCount; j++)
                    C.Elements[0, j] -= P[j].Elements[eqNum, 0] * elem;
                cCoeff -= -(b.Elements[eqNum, 0]) * elem;
            }
        }
        bool SetSwapVarsLegalDecision(IterationState s)
        {
            s.basIndex = -1;
            double epslon = 0.000001;
            double min = float.MaxValue;
            for (int i = 0; i < s.zc.N; i++)
                if (s.zc.Elements[0, i] < 0 - epslon &&
                    s.zc.Elements[0, i] < min)
                {
                    min = s.zc.Elements[0, i];
                    s.basIndex = s.zcInd[i];
                }
            if (s.basIndex == -1)
                return false;

            s.alpha = s.Binv * P[s.basIndex];

            s.theta = double.MaxValue;
            for (int i = 0; i < eqCount; i++)
            {
                double tmp = s.Xb.Elements[i, 0] / s.alpha.Elements[i, 0];
                if (tmp > 0 && tmp < s.theta)
                {
                    s.oldIndex = s.basInd[i];
                    s.theta = tmp;
                    s.eqIndex = i;
                }
            }
            if (s.theta == double.MaxValue)
                throw new ArgumentException("Допустимое решение не существует");
            s.alphar = s.alpha.Elements[s.eqIndex, 0];
            return true;
        }
        bool SetSwapVarsIllegalDecision(IterationState s)
        {
            s.basIndex = -1;
            double min = double.MaxValue;
            for (int i = 0; i < eqCount; i++)
            {
                double b = s.Xb.Elements[i, 0];
                if (b >= 0)
                    continue;
                for (int j = 0; j < varCount; j++)
                {
                    if (s.IsBaseVar(j))
                        continue;
                    double a = s.GetVarCoeff(j, i);
                    if (a >= 0)
                        continue;
                    if (b / a < min)
                    {
                        s.eqIndex = i;
                        s.basIndex = j;
                        min = b / a;
                    }
                }
            }
            if (s.basIndex == -1)
                throw new ArgumentException("Допустимое решение не существует");

            s.oldIndex = s.basInd[s.eqIndex];

            s.alpha = s.Binv * P[s.basIndex];
            s.alphar = s.alpha.Elements[s.eqIndex, 0];
            return true;
        }
    }
    public class IterationState
    {
        public ExMatrix Cb, Binv, gamma, zc;
        public ExMatrix Xb, alpha, xi, BinvNext;
        ExMatrix C, b;
        ExMatrix[] P;
        public int[] basInd, zcInd;
        int[] basIndInit;
        public int basIndex, oldIndex, eqIndex;
        public double theta, alphar;
        public bool isDecisionLegal;
        int varCount, eqCount;
        double cCoeff;
        public IterationState(int varCount, int eqCount,
            ExMatrix[] P, ExMatrix C, ExMatrix b, double cCoeff,
            int[] basInd)
        {
            this.varCount = varCount;
            this.eqCount = eqCount;
            this.P = P;
            this.C = C;
            this.b = b;
            this.cCoeff = cCoeff;
            this.basInd = basInd;
            this.basIndInit = this.basInd.Clone() as int[];
            this.Binv = new ExMatrix(eqCount);
            this.basIndex = this.eqIndex = this.oldIndex = -1;
        }
        public IterationState(IterationState prevState)
        {
            this.varCount = prevState.varCount;
            this.eqCount = prevState.eqCount;
            this.P = prevState.P;
            this.C = prevState.C;
            this.b = prevState.b;
            this.cCoeff = prevState.cCoeff;
            this.basIndInit = prevState.basIndInit;
            this.basInd = prevState.basInd.Clone() as int[];
            this.basInd[prevState.eqIndex] = prevState.basIndex;
            this.Binv = prevState.BinvNext;
            this.basIndex = this.eqIndex = this.oldIndex = -1;
        }
        public string GetReport()
        {
            eqCount = Xb.M;
            string s = GetSimplexTablePart();
            try
            {
                s += "<P>Значение целевой функции ";
                s += "F = " + GetFuncValue().ToString() + "</P>";

                s += "<P>ВСПОМОГАТЕЛЬНЫЕ ПЕРЕМЕННЫЕ</P>";
                s += "<P>Базисное решение" + GetBasisDecision() + "</P>";

                s += "<P>Базисные переменные: ";
                for (int i = 0; i < eqCount; i++)
                    s += "y<SUB>" + basInd[i].ToString() + "</SUB> ";
                s += "</P>";

                s += "<P>ИСХОДНЫЕ ПЕРЕМЕННЫЕ</P>";
                s += "<P>Текущие значения исходных переменных:</P>";
                int varCountMinOne = varCount - 1;
                for (int i = 0; i < varCount - 1; i++)
                {
                    s += "<P>x<SUB>" + i.ToString() + "</SUB>" +
                        " = y<SUB>" + i.ToString() + "</SUB>" +
                        " / y<SUB>" + varCountMinOne.ToString() + "</SUB> = ";
                    if (GetVarValue(varCount - 1) == 0)
                        s += "не определено";
                    else
                        s += Math.Round(GetVarValue(i) / GetVarValue(varCount - 1), 5) + "</P>";
                }

                //return s;

                s += "<P>Матрица C<SUB>b</SUB>";
                s += Cb.ToHtml() + "</P>";

                s += "<P>Матрица B<SUP>-1</SUP>";
                s += Binv.ToHtml() + "</P>";

                s += "<P>Матрица γ";
                s += gamma.ToHtml() + "</P>";

                s += "<P>Матрица разностей z<SUB>i</SUB> - c<SUB>i</SUB>";
                s += zc.ToHtml() + "</P>";

                s += "<P>Вектор базисных решений X<SUB>b</SUB>";
                s += Xb.ToHtml() + "</P>";

                if (isDecisionLegal)
                    s += "<P>Базисное решение является допустимым</P>";
                else
                    s += "<P>Базисное решение является недопустимым</P>";

                if (basIndex < 0)
                    throw new Exception();
                s += "<P>Выводим из базиса переменную x<SUB>" + basIndex.ToString() + "</SUB></P>";

                s += "<P>Матрица α";
                s += alpha.ToHtml() + "</P>";

                int eqNum = eqIndex + 1;
                s += "<P>Минимальное отношение θ = " + theta.ToString() + "</P>";
                s += "<P>Выводим из базиса переменную x<SUB>" + oldIndex.ToString()
                    + "</SUB> из уравнения номер " + eqNum.ToString() + "</P>";

                s += "<P>Ведущий элемент α<SUB>r</SUB> = " + alphar.ToString() + "</P>";

                s += "<P>Матрица ξ";
                s += xi.ToHtml() + "</P>";

                s += "<P>Матрица B<SUP>-1</SUP><SUB>next</SUB>";
                s += BinvNext.ToHtml() + "</P>";
            }
            catch { }
            return s;
        }
        public string GetBasisDecision()
        {
            string s = "<TABLE>";
            for (int i = 0; i < varCount; i++)
            {
                s += "<TR><TD>y<SUB>" + i.ToString() + "</SUB> = ";
                s += GetVarValue(i);
                s += "</TD></TR>";
            }
            return s + "</TABLE>";
        }
        public bool IsBaseVar(int varNum)
        {
            for (int i = 0; i < eqCount; i++)
                if (basInd[i] == varNum)
                    return true;
            return false;
        }
        public double GetVarCoeff(int varNum, int eqNum)
        {
            double epsilon = 0.000001;
            ExMatrix res = Binv * P[varNum];
            double coeff = res.Elements[eqNum, 0];
            if (coeff > -epsilon && coeff < epsilon)
                coeff = 0;
            return Math.Round(coeff, 5);
        }
        public double GetVarValue(int varNum)
        {
            if (IsBaseVar(varNum))
            {
                for (int eqNum = 0; eqNum < eqCount; eqNum++)
                    if (basInd[eqNum] == varNum)
                        return Math.Round(Xb.Elements[eqNum, 0], 5);
            }
            return 0;
        }
        public double GetFuncValue()
        {
            ExMatrix value = Cb * Xb;
            return Math.Round(value.Elements[0, 0] + cCoeff, 5);
        }
        public double[] GetFuncCoeffs()
        {
            double epsilon = 0.000001;
            double[] res = new double[varCount + 1];
            for (int i = 0; i < varCount; i++)
                res[i] = C.Elements[0, i];
            res[varCount] = cCoeff;
            for (int eqNum = 0; eqNum < eqCount; eqNum++)
            {
                double elem = res[basInd[eqNum]];
                for (int j = 0; j < varCount; j++)
                    res[j] -= GetVarCoeff(j, eqNum) * elem;
                res[varCount] -= -(GetVarValue(basInd[eqNum])) * elem;
            }
            for (int i = 0; i < varCount + 1; i++)
            {
                if (res[i] > -epsilon && res[i] < epsilon)
                    res[i] = 0;
                res[i] = Math.Round(res[i], 5);
            }
            double[] tmp = new double[varCount + 1];
            tmp[0] = res[varCount];
            for (int i = 0; i < varCount; i++)
                tmp[i + 1] = res[i];
            return tmp;
        }
        public string GetSimplexTablePart()
        {
            string s = "<TABLE BORDER = 1>";
            s += "<TR><TH>Базис</TH><TH>Решение</TH>";
            for (int i = 0; i < varCount; i++)
                s += "<TH>y<SUB>" + i.ToString() + "</SUB></TH>";
            s += "<TH>Отношения</TH></TR>";

            for (int i = 0; i < eqCount; i++)
            {
                s += "<TR";
                if (i == eqIndex)
                    s += " BGCOLOR = Red";
                s += "><TD>y<SUB>" + basInd[i].ToString() + "</SUB></TD>";
                s += "<TD>" + GetVarValue(basInd[i]) + "</TD>";
                for (int j = 0; j < varCount; j++)
                {
                    s += "<TD";
                    if (j == basIndex)
                        s += " BGCOLOR = LightGreen";
                    s += ">" + GetVarCoeff(j, i) + "</TD>";
                }
                if (alpha == null || alpha.Elements[i, 0] == 0)
                    s += "<TD>-</TD>";
                else
                {
                    double tmp = Math.Round(Xb.Elements[i, 0] / alpha.Elements[i, 0], 5);
                    s += "<TD>" + tmp.ToString() + "</TD>";
                }
                s += "</TR>";
            }

            double[] arrCoeffs = GetFuncCoeffs();
            for (int i = 1; i < arrCoeffs.Length; i++)
                arrCoeffs[i] *= -1;
            s += "<TR BGCOLOR = LightYellow><TD>F</TD>";
            for (int j = 0; j < varCount + 1; j++)
                s += "<TD>" + arrCoeffs[j] + "</TD>";
            s += "<TD>-</TD></TR>";
            return s + "</TABLE>";
        }
    }
}