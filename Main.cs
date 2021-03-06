﻿using AppKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace MMS
{
    class EntryPoint
    {

        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.Main(args);


            Console.WriteLine("All values must be in millimetres, ascending order and comma separated.");
            Console.WriteLine("Enter reference diameter measurements:");
            string inputControl = Console.ReadLine();
            Console.WriteLine("Enter specimen diameter measurements:");
            string inputTarget = Console.ReadLine();
            Console.WriteLine("Enter measurement positions:");
            string inputMmPosition = Console.ReadLine();


            double[] control = inputControl.Split(',').Select(double.Parse).ToArray();
            double[] target = inputTarget.Split(',').Select(double.Parse).ToArray();
            double[] mmPosition = inputMmPosition.Split(',').Select(double.Parse).ToArray();

            int interpResolution = 10;
            int numberOfMeasurements = mmPosition.Length;

            Tuple<double[],double> mmPositionFigures = Process.Discretise(mmPosition, interpResolution, numberOfMeasurements);
            double[] xx = mmPositionFigures.Item1;
            double mmPositionStep = mmPositionFigures.Item2;

            // code relies on abscissa values to be sorted
            // there is a check for this condition, but no fix
            // f(x) = 1/(1+x^2)*sin(x)
			// this is a test change

            BasicInterpolation.LinearInterpolation LIControl = new BasicInterpolation.LinearInterpolation(mmPosition, control);
            BasicInterpolation.LinearInterpolation LITarget = new BasicInterpolation.LinearInterpolation(mmPosition, target);
            BasicInterpolation.CubicSplineInterpolation CSControl = new BasicInterpolation.CubicSplineInterpolation(mmPosition, control);
            BasicInterpolation.CubicSplineInterpolation CSTarget = new BasicInterpolation.CubicSplineInterpolation(mmPosition, target);

            double[] splineControl = new double[xx.Length];
            double[] splineTarget = new double[xx.Length];
            double[] gradControl = new double[xx.Length - 1];
            double[] gradTarget = new double[xx.Length - 1];

            for (int i = 0; i < interpResolution; i++)
            {
                splineControl[i] = CSControl.Interpolate(xx[i]).Value;
                splineTarget[i] = CSTarget.Interpolate(xx[i]).Value;
            }

            for (int i = 0; i < interpResolution - 1; i++)
            {
                gradControl[i] = (splineControl[i + 1] - splineControl[i]) / mmPositionStep;
                gradTarget[i] = (splineTarget[i + 1] - splineTarget[i]) / mmPositionStep;
            }

            double meanGradientControl = gradControl.Sum() / gradControl.Length;
            double meanGradientTarget = gradTarget.Sum() / gradTarget.Length;

            double[] trimmedMmPosition = new double[mmPosition.Length - 2];
            double[] trimmedControl = new double[control.Length - 2];
            double[] trimmedTarget = new double[target.Length - 2];

            for (int i = 1; i < numberOfMeasurements - 1; i++)
            {
                trimmedMmPosition[i - 1] = mmPosition[i];
                trimmedControl[i - 1] = control[i];
                trimmedTarget[i - 1] = target[i];
            }

            Tuple<double, double> linearControlFit = Fit.Line(trimmedMmPosition, trimmedControl);
            double linearControlIntercept = linearControlFit.Item1;
            double linearControlSlope = linearControlFit.Item2;

            Tuple<double, double> linearTargetFit = Fit.Line(trimmedMmPosition, trimmedTarget);
            double linearTargetIntercept = linearTargetFit.Item1;
            double linearTargetSlope = linearTargetFit.Item2;

            double[] linearControl = new double[xx.Length];
            double[] linearTarget = new double[xx.Length];

            for (int i = 0; i < interpResolution; i++)
            {
                linearControl[i] = (linearControlSlope * xx[i]) + linearControlIntercept;
                linearTarget[i] = (linearTargetSlope * xx[i]) + linearTargetIntercept;
            }

            double[] diameterShift = new double[control.Length - 2];

            for (int i = 1; i < numberOfMeasurements - 1; i++)
            {
                diameterShift[i - 1] = trimmedTarget[i - 1] - trimmedControl[i - 1];
            }

            double mmShift = (diameterShift.Sum() / diameterShift.Length) / meanGradientControl;

            double[] controlPoly = Fit.Polynomial(mmPosition, control, 2);
            double[] targetPoly = Fit.Polynomial(mmPosition, target, 2);

            double controlPoly2Coeff = controlPoly[2];
            double targetPoly2Coeff = targetPoly[2];

            int shiftMetric = Scoring.Score(mmShift, 0, -1.5, 1.5);
            int taperMetric = Scoring.Score(meanGradientTarget, meanGradientControl, -0.0002, 0.0002);
            int linearityMetricN = Scoring.Score(targetPoly2Coeff, 0, -5E-6, 5E-6);

            var linearityMetric = Scoring.NumberToLetter(linearityMetricN);

            string MandrelMetric = String.Concat(shiftMetric, taperMetric, linearityMetric);
            Console.WriteLine(MandrelMetric);

        }
    }
}