using System;
using System.Collections.Generic;
using System.Linq;
using Shared;

namespace FIR
{
	class Program
	{
		public static void Main()
		{
			var weight = GetWeights(64 + 1).ToArray();
			//var weight = System.IO.File.ReadAllLines("weight.txt").Select(x => float.Parse(x)).ToArray();
			var parser = new Parser();
			parser.Read("input.txt");

			var input = parser.AsFloat();
			float[][] output = Parser.CreateEmptyArray(input);

			Process(input, output, weight);
			//MaxSmooth(input, output);
			//Normalize(output, 0.95f);

			parser.Write("output.txt", output);
		}

		static void Process(float[][] input, float[][] output, float[] weight)
		{
			for (int j = 0; j < input.Length; j++)
			{
				for (int i = 0; i < input[0].Length; i++)
				{
					float result = 0.0f;
					for (int n = 0; n < weight.Length; n++)
					{
						int index = j + n - (weight.Length - 1) / 2;
						float value = input[Math.Max(0, Math.Min(index, input.Length - 1))][i];
						result += value * weight[n];
					}

					output[j][i] = result;
				}
			}
		}

		static void MaxSmooth(float[][] input, float[][] output)
		{
			List<float> max = [];
			const int time = 20;
			for (int i = 0; i < input[0].Length; i++)
			{
				max.Clear();
				for (int j = 0; j < input.Length; j += time)
				{
					float result = float.MinValue;
					for (int n = 0; n < time; n++)
					{
						int index = j + n - (time - 1) / 2;
						float value = input[Math.Max(0, Math.Min(index, input.Length - 1))][i];
						result = Math.Max(value, result);
					}

					max.Add(result);
				}

				for (int j = 0; j < input.Length; j++)
				{
					int index = j / time;
					float mu = (j % time) / (float)time;
					output[j][i] = CubicInterpolate(
						max[Math.Max(0, Math.Min(index-1, max.Count - 1))],
						max[Math.Max(0, Math.Min(index+0, max.Count - 1))],
						max[Math.Max(0, Math.Min(index+1, max.Count - 1))],
						max[Math.Max(0, Math.Min(index+2, max.Count - 1))], mu);
				}
			}
		}

		static float CubicInterpolate(float y0,float y1, float y2,float y3, float mu)
		{
			float a0,a1,a2,a3,mu2;

			mu2 = mu*mu;
			a0 = y3 - y2 - y0 + y1;
			a1 = y0 - y1 - a0;
			a2 = y2 - y0;
			a3 = y1;

			return a0*mu*mu2+a1*mu2+a2*mu+a3;
		}

//		static void Normalize(float[][] values, float cap)
//		{
//			for (int i = 0; i < values[0].Length; i++)
//			{
//				float min = float.MaxValue, max = float.MinValue;
//				for (int j = 0; j < values.Length; j++)
//				{
//					float value = values[j][i];
//					min = Math.Min(min, value);
//					max = Math.Max(max, value);
//				}
//
//				if ((max - min) > 0.0f)
//				{
//					for (int j = 0; j < values.Length; j++)
//					{
//						values[j][i] = (values[j][i] - min) / (max - min) * cap;
//					}
//				}
//			}
//		}

		static IEnumerable<float> GetWeights(int taps)
		{
			var weights = Gaussian(taps).ToArray();
			var sum = weights.Sum();
			return [.. weights.Select(x => x / sum)];
		}

		static IEnumerable<float> Gaussian(int taps)
		{
			for (int t = 0; t < taps; t++)
			{
				var x = (t - (taps - 1) / 2.0f) / ((taps - 1) / 5.0f);
				var weight = (float)(Math.Exp(-0.5f * x * x) / taps);
				yield return weight;
			}
		}
	}
}