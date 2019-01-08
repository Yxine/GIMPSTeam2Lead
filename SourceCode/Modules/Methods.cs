using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using C = System.Console;

namespace GIMPSTeam2Lead
{

	/// <summary>Класс программы</summary>
	public static partial class Program
	{

		/// <summary></summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string Get(string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";
			var response = (HttpWebResponse)request.GetResponse();
			var stream = response.GetResponseStream();
			return stream == null ? null : new StreamReader(stream, Encoding.UTF8).ReadToEnd();
		}

		/// <summary></summary>
		/// <param name="teamLine"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		private static Team ParseTeam(string teamLine, int a, int b, int c)
		{
			var team = new Team
			{
				Place = 0,
				Name = string.Empty,
				GHzDays = 0
			};
			if (string.IsNullOrWhiteSpace(teamLine)) return team;
			var arr = teamLine.Trim().Split('|');
			if (!arr.Length.Equals(a)) return team;
			arr = arr[0].Trim().Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);
			team.Place = Convert.ToByte(arr[0]);
			team.GHzDays = Convert.ToDecimal(arr[arr.Length + b].Replace(".", ","));
			var l = arr.Length + c;
			for (var i = 1; i < l; i++) team.Name = string.Concat(team.Name, arr[i], " ");
			team.Name = team.Name.Replace("</a>", string.Empty);
			arr = team.Name.Split('>');
			team.Name = (arr.Length.Equals(2) ? arr[1] : arr[0]).Trim();
			return team;
		}

		/// <summary>Дополнение строки пробелами слева</summary>
		/// <param name="d">Дробное число</param>
		/// <param name="length">Необходимая длина</param>
		/// <returns>Строка дополненная пробелами</returns>
		private static string AsString(decimal d, int length)
		{
			var s = d.ToString(CultureInfo.CurrentCulture);
			while (s.Length < length) s = string.Concat(" ", s);
			return s;
		}

		private static string Repeat(char c, int length)
		{
			return string.Empty.PadRight(length, c);
		}

		/// <summary></summary>
		/// <param name="s"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private static string AsString(string s, int length)
		{
			if (string.IsNullOrWhiteSpace(s)) return string.Empty;
			while (s.Length < length) s = string.Concat(s, " ");
			return s;
		}

		/// <summary></summary>
		/// <param name="team"></param>
		/// <returns></returns>
		private static Places TotalsOverall(string team)
		{

			var places = new Places();
			var html = Get("http://www.mersenne.org/report_top_500_custom/?team_flag=1&type=0&rank_lo=1&rank_hi=50&start_date=1990-01-01&end_date=");

			#region Парсинг

			var arr = html.Split(new[] { "<pre style=\"overflow:auto;\">" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[1].Split(new[] { "</pre>" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[0].Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
			var prev = string.Empty;
			var weare = string.Empty;
			var nxt = string.Empty;
			for (var i = 0; i < arr.Length; i++) if (arr[i].Contains(team))
			{
				prev = arr[i - 1];
				weare = arr[i];
				nxt = arr[i + 1];
				break;
			}
			if (string.IsNullOrWhiteSpace(prev) || string.IsNullOrWhiteSpace(weare) || string.IsNullOrWhiteSpace(nxt)) return places;
			places.PrevTeam = ParseTeam(prev, 3, -1, -1);
			places.WeAre = ParseTeam(weare, 3, -1, -1);
			places.NextTeam = ParseTeam(nxt, 3, -1, -1);

			#endregion

			return places;

		}

		/// <summary>Генерация отчета</summary>
		/// <param name="type">Тип отчета</param>
		/// <returns></returns>
		private static Places Report(string type)
		{

			var places = new Places();
			var html = Get($"http://www.mersenne.org/report_top_500_custom/?team_flag=1&type={type}&rank_lo=1&rank_hi=100&start_date=1990-01-01&end_date=");

			#region Парсинг

			var arr = html.Split(new[] { @"<pre style=""overflow:auto;"">" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[1].Split(new[] { "</pre>" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[0].Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
			var prev = string.Empty;
			var weare = string.Empty;
			var nxt = string.Empty;
			var l = arr.Length;
			for (var i = 0; i < l; i++) if (arr[i].Contains("ComputerraRU"))
			{
				prev = arr[i - 1];
				weare = arr[i];
				nxt = arr[i + 1];
				break;
			}
			if (string.IsNullOrWhiteSpace(prev) || string.IsNullOrWhiteSpace(weare) || string.IsNullOrWhiteSpace(nxt)) return places;
			places.PrevTeam = ParseTeam(prev, 2, -3, -3);
			places.WeAre = ParseTeam(weare, 2, -3, -3);
			places.NextTeam = ParseTeam(nxt, 2, -3, -3);

			#endregion

			return places;

		}

		private static void Print(string s0, string s1, string s2, string s3, string s4, string s5, string s6)
		{
			C.WriteLine($"   | {s0} | {s1} * {s2} | {s3} | {s4} | {s5} | {s6} |");
		}

	}

}
