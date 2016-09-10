using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace GIMPSTeam2Lead
{

	/// <summary>Класс программы</summary>
	class Program
	{

		private static readonly string Json = RU.JSON.Trim();

		private static string _teamName = RU.TeamName;

		/// <summary>Структура команды</summary>
		private struct Team
		{

			/// <summary>Место</summary>
			public byte Place;

			/// <summary>Название</summary>
			public string Name;

			/// <summary>Наработки в GHz-Days</summary>
			public decimal GHzDays;

		}

		/// <summary></summary>
		private struct Places
		{

			/// <summary>Предидущая команда</summary>
			public Team PrevTeam;

			/// <summary>Наша команда</summary>
			public Team WeAre;

			/// <summary>Следующая команда</summary>
			public Team NextTeam;

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

		/// <summary></summary>
		/// <param name="s"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private static string AsString(string s, int length)
		{
			while (s.Length < length) s = string.Concat(s, " ");
			return s;
		}

		/// <summary></summary>
		private static Places TotalsOverall()
		{

			var places = new Places();

			// Получаем HTML-контент
			var request = (HttpWebRequest)WebRequest.Create("http://www.mersenne.org/report_top_500_custom/?team_flag=1&type=0&rank_lo=1&rank_hi=50&start_date=1990-01-01&end_date=");
			request.Method = "GET";
			var response = (HttpWebResponse)request.GetResponse();
			var stream = response.GetResponseStream();
			if (stream == null) return places;
			var html = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

			// Парсинг
			var arr = html.Split(new[] { "<pre style=\"overflow:auto;\">" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[1].Split(new[] { "</pre>" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[0].Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
			string
				prev = null,
				weare = null,
				nxt = null;
			for (var i = 0; i < arr.Length; i++) if (arr[i].Contains(_teamName))
			{
				prev = arr[i - 1];
				weare = arr[i];
				nxt = arr[i + 1];
				break;
			}
			if (prev == null || weare == null || nxt == null) return places;
			places.PrevTeam = ParseTeam(prev, 3, -1, -1);
			places.WeAre = ParseTeam(weare, 3, -1, -1);
			places.NextTeam = ParseTeam(nxt, 3, -1, -1);

			// Вывод
			File.WriteAllText(
				"totals.json",
				new StringBuilder(Json)
					.Replace("p.place", places.PrevTeam.Place.ToString())
					.Replace("c.place", places.WeAre.Place.ToString())
					.Replace("n.place", places.NextTeam.Place.ToString())
					.Replace("p.name", places.PrevTeam.Name)
					.Replace("c.name", places.WeAre.Name)
					.Replace("n.name", places.NextTeam.Name)
					.Replace("p.ghzdays", places.PrevTeam.GHzDays.ToString(CultureInfo.CurrentCulture).Replace(",", "."))
					.Replace("c.ghzdays", places.WeAre.GHzDays.ToString(CultureInfo.CurrentCulture).Replace(",", "."))
					.Replace("n.ghzdays", places.NextTeam.GHzDays.ToString(CultureInfo.CurrentCulture).Replace(",", "."))
					.ToString()
			);
			return places;

		}

		/// <summary>Генерация отчета</summary>
		/// <param name="type">Тип отчета</param>
		/// <param name="filename">Имя для JSON-файла</param>
		/// <returns></returns>
		private static Places Report(string type, string filename)
		{

			var places = new Places();

			// Получаем HTML-контент
			var request = (HttpWebRequest)WebRequest.Create($"http://www.mersenne.org/report_top_500_custom/?team_flag=1&type={type}&rank_lo=1&rank_hi=50&start_date=1990-01-01&end_date=");
			request.Method = "GET";
			var response = (HttpWebResponse)request.GetResponse();
			var stream = response.GetResponseStream();
			if (stream == null) return places;
			var html = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

			// Парсинг
			var arr = html.Split(new[] { @"<pre style=""overflow:auto;"">" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[1].Split(new[] { "</pre>" }, StringSplitOptions.RemoveEmptyEntries);
			if (arr.Length != 2) return places;
			arr = arr[0].Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
			string
				prev = null,
				weare = null,
				nxt = null;
			var l = arr.Length;
			for (var i = 0; i < l; i++) if (arr[i].Contains(_teamName))
			{
				prev = arr[i - 1];
				weare = arr[i];
				nxt = arr[i + 1];
				break;
			}
			if (prev == null || weare == null || nxt == null) return places;
			places.PrevTeam = ParseTeam(prev, 2, -3, -3);
			places.WeAre = ParseTeam(weare, 2, -3, -3);
			places.NextTeam = ParseTeam(nxt, 2, -3, -3);

			// Вывод
			File.WriteAllText(
				filename,
				new StringBuilder(Json)
					.Replace("p.place", places.PrevTeam.Place.ToString())
					.Replace("c.place", places.WeAre.Place.ToString())
					.Replace("n.place", places.NextTeam.Place.ToString())
					.Replace("p.name", places.PrevTeam.Name)
					.Replace("c.name", places.WeAre.Name)
					.Replace("n.name", places.NextTeam.Name)
					.Replace("p.ghzdays", places.PrevTeam.GHzDays.ToString(CultureInfo.CurrentCulture).Replace(",", "."))
					.Replace("c.ghzdays", places.WeAre.GHzDays.ToString(CultureInfo.CurrentCulture).Replace(",", "."))
					.Replace("n.ghzdays", places.NextTeam.GHzDays.ToString(CultureInfo.CurrentCulture).Replace(",", "."))
					.ToString()
			);
			return places;

		}

		/// <summary>Точка входа</summary>
		/// <param name="args">Аргумента командной строки</param>
		static void Main(string[] args)
		{
			Console.Clear();
			Console.CursorVisible = false;
			Console.Title = RU.Title;
			if (args.Length > 0) _teamName = args[0].Trim();
			Console.WriteLine();
			Console.WriteLine(RU.Mult);
			Console.WriteLine($" * GIMPS Team 2 Lead version {Assembly.GetEntryAssembly().GetName().Version}");
			Console.WriteLine(RU.URL);
			Console.WriteLine(RU.Author);
			Console.WriteLine(RU.Mult);
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"   Team: {_teamName}");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine();
			Console.WriteLine(RU.TBottom);
			Console.WriteLine(RU.TH);
			Console.WriteLine(RU.TBottom);
			var r = TotalsOverall();
			Console.WriteLine($"   | Totals | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			r = Report("1001", "tf.json");
			Console.WriteLine($"   | TF     | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			r = Report("1003", "llt.json");
			Console.WriteLine($"   | LLT    | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			r = Report("1004", "dllt.json");
			Console.WriteLine($"   | DLLT   | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			r = Report("1002", "p-1.json");
			Console.WriteLine($"   | P-1    | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			r = Report("1005", "ecm.json");
			Console.WriteLine($"   | ECM    | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			r = Report("1006", "ecmf.json");
			Console.WriteLine($"   | ECMF   | {AsString(r.WeAre.Place, 2)} * {AsString(r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10)} | {AsString(r.PrevTeam.Name, 25)} | {AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10)} | {AsString(r.NextTeam.Name, 26)} |");
			Console.WriteLine(RU.TBottom);
			Console.WriteLine();
			Console.Write(RU.PressAKey2Exit);
			Console.ReadKey();
		}

	}

}
