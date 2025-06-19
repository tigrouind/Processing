using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shared
{
	public class Parser
	{
		public string[][] Rows;
		public string[] Header;
		public int HeaderColumns;

		public void Read(string filename)
		{
			var lines = System.IO.File.ReadAllLines(filename)
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(x => x.Replace(',', '.'))
				.Select(x => x.Split('\t'))
				.ToArray();

			bool hasHeader = lines[0].Any(x => Regex.IsMatch(x, @"[^0-9..\s]"));
			HeaderColumns = lines.Skip(hasHeader ? 1 : 0).First().TakeWhile(x => Regex.IsMatch(x, @"[^0-9..\s]")).Count();
			Header = hasHeader ? lines[0] : [];
			Rows = [.. lines.Skip(hasHeader ? 1 : 0)];
		}

		public float[][] AsFloat()
		{
			return [.. Rows.Select(x => x.Skip(HeaderColumns).Select(y => (y == string.Empty || y == "#N/A") ? 0.0f : float.Parse(y)).ToArray())];
		}

		public static float[][] CreateEmptyArray(float[][] value)
		{
			return [.. new float[value.Length].Select(x => new float[value[0].Length])];
		}

		public static float[][] Transpose(float[][] input)
		{
			int columns = input.Min(row => row.Length);
			return [.. Enumerable.Range(0, columns).Select(col => input.Select(row => row[col]).ToArray())];
		}

		public void Write(string filename, float[][] output)
		{
			var firstRow = new[] { string.Join("\t", Header) };
			var cols = output.Select((x, i) => string.Join("\t", Enumerable.Range(0, HeaderColumns)
														.Select(y => Rows[i][y])
														.Concat(x.Select(y => y.ToString()))));

			System.IO.File.WriteAllText(filename, string.Join("\r\n", firstRow.Concat(cols)));
		}
	}
}
