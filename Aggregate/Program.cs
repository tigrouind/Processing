using System;
using System.Linq;
using Shared;

namespace Aggregate
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.Write("Time (default=5 min): ");
			string line = Console.ReadLine();

			TimeSpan average = TimeSpan.FromMinutes(string.IsNullOrEmpty(line) ? 5 : int.Parse(line));
			TimeSpan round(TimeSpan x) => new(x.Ticks / average.Ticks * average.Ticks);

			var parser = new Parser();
			parser.Read(args.Length > 0 ? args[0] : "input.txt");

			var result = parser.Rows
							.Select(x => new Tuple<TimeSpan, float[]>(round(TimeSpan.Parse(x[0].Split(' ')[1])), [.. x.Skip(1).Select(y => float.Parse(y))]))
							.GroupBy(x => x.Item1, x => x.Item2)
							.Select(x => (new[] { (float)x.Key.TotalMinutes }).Concat(Parser.Transpose([.. x]).Select(y => y.Max())).ToArray())
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

			input = [.. input.Select(x => x.Select(y => y >= 1000 * 1000 * 1000 ? 0.0f : y).ToArray())];

			return [.. Enumerable.Range(0, input.Length).Select(row => largeValue[row] ? [.. input[row].Select(x => x / (1024 * 1024))] : input[row])];
		}
	}
}