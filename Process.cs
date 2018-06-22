using System;
namespace MMS
{
    static public class Process
    {
        static public Tuple<double[], double> Discretise(double[] mmPosition, int interpResolution, int numberOfMeasurements)
        {
            double min = mmPosition[0];
            double max = mmPosition[0];

            for (int i = 0; i < numberOfMeasurements; i++)
            {
                if (min > mmPosition[i]) min = mmPosition[i];
                if (max<mmPosition[i]) max = mmPosition[i];
            }

            double mmPositionRange = max - min;
            double mmPositionStep = mmPositionRange / (interpResolution - 1);
            double[] xx = new double[interpResolution];

            for (int i = 0; i<interpResolution - 1; i++)
            {
                xx[i] = min + (mmPositionStep* i);
            }

            xx[interpResolution - 1] = max;

            return Tuple.Create(xx, mmPositionStep);
        }
    }
}
