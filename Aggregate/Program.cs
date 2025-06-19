using System;
using System.Linq;
using Shared;

namespace Aggregate
{
	class Program
	{
		public static void Main()
		{
			TimeSpan average = TimeSpan.FromMinutes(30);
			Func<DateTime, DateTime> round = x => new DateTime((x.Ticks / average.Ticks) * average.Ticks);

			var parser = new Parser();
			parser.Read("input.txt");

			var result = parser.Rows
							.Select(x => new Tuple<DateTime, float[]>(round(DateTime.Parse(x[0])), x.Skip(1).Select(y => float.Parse(y)).ToArray()))
							.GroupBy(x => x.Item1, x => x.Item2)
							.Select(x => (new[] { (float)x.Key.TimeOfDay.TotalMinutes }).Concat(Parser.Transpose(x.ToArray()).Select(y => y.Max())).ToArray())
							.ToArray();

			result = Parser.Transpose(Process(Parser.Transpose(result)));

			System.IO.File.WriteAllLines(@"output.txt",
                             (new [] { string.Join("\t", parser.Header) })
                             .Concat(result.Select(x => new DateTime((long)x[0] * TimeSpan.TicksPerMinute).ToString("HH:mm") + "\t" + string.Join("\t", x.Skip(1))))
                            );
		}

		static float[][] Process(float[][] input)
		{
			var largeValue = input.Select(row => row.Any(x => x >= 10000)).ToArray();

			input = input.Select(x => x.Select(y => y >= 1000 * 1000 * 1000 ? 0.0f : y).ToArray()).ToArray();

			return Enumerable.Range(0, input.Length)
				.Select(row => largeValue[row] ? input[row].Select(x => x / (1024 * 1024)).ToArray() : input[row])
				.ToArray();
		}
	}
}