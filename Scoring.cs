using System;
using MathNet.Numerics;

namespace MMS
{
    static public class Scoring
    {
        public static int Score(double value, double control, double tolLow, double tolHigh)
        {
            double[] x = { control + tolLow, control, control + tolHigh };
            double[] y = { 1, 9, 1 };

            double[] p = Fit.Polynomial(x, y, 2);

            double min = control + tolLow;
            double max = control + tolHigh;

            int m = 10;
            double controlRange = max - min;
            double controlStep = controlRange / (m - 1);
            double[] x1 = new double[m];

            for (int i = 0; i < m - 1; i++)
            {
                x1[i] = min + (controlStep * i);
            }

            x1[m - 1] = max;
            double x2 = value;

            double[] y1 = new double[m];

            for (int i = 0; i < m; i++)
            {
                y1[i] = (p[2] * (x1[i] * x1[i])) + (p[1] * x1[i]) + p[0];
            }

            double y2 = (p[2] * (x2 * x2)) + (p[1] * x2) + p[0];

            int score = Convert.ToInt32(y2);
            if (y2 < 1)
            {
                score = 1;
            }
            return score;
        }

        static readonly string[] Columns = new[] { "I", "H", "G", "F", "E", "D", "C", "B", "A" };
        public static string NumberToLetter(int index)
        {
            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            return Columns[index - 1];
        }
    }
}
