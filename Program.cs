using System;
using System.Globalization;
using System.IO;
using System.Net;
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

			/// <summary></summary>
			public Team PrevTeam;

			/// <summary></summary>
			public Team WeAre;

			/// <summary></summary>
			public Team NextTeam;

		}

		/// <summary></summary>
		/// <param name="teamLine"></param>
		/// <returns></returns>
		private static Team ParseTeam(string teamLine)
		{
			var team = new Team
			{
				Place = 0,
				Name = string.Empty,
				GHzDays = 0
			};
			if (string.IsNullOrWhiteSpace(teamLine)) return team;
			var arr = teamLine.Trim().Split('|');
			if (!arr.Length.Equals(3)) return team;
			arr = arr[0].Trim().Split(' ');
			team.Place = Convert.ToByte(arr[0]);
			team.GHzDays = Convert.ToDecimal(arr[arr.Length - 1].Replace(".", ","));
			for (var i = 1; i < arr.Length - 2; i++) team.Name = string.Concat(team.Name, arr[i], " ");
			team.Name = team.Name.Replace("</a>", string.Empty);
			arr = team.Name.Split('>');
			team.Name = (arr.Length.Equals(2) ? arr[1] : arr[0]).Trim();
			return team;
		}

		/// <summary></summary>
		/// <param name="teamLine"></param>
		/// <returns></returns>
		private static Team ParseTeam2(string teamLine)
		{
			var team = new Team
			{
				Place = 0,
				Name = string.Empty,
				GHzDays = 0
			};
			if (string.IsNullOrWhiteSpace(teamLine)) return team;
			var arr = teamLine.Trim().Split('|');
			if (!arr.Length.Equals(2)) return team;
			arr = arr[0].Trim().Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);
			team.Place = Convert.ToByte(arr[0]);
			team.GHzDays = Convert.ToDecimal(arr[arr.Length - 3].Replace(".", ","));
			for (var i = 1; i < arr.Length - 3; i++) team.Name = string.Concat(team.Name, arr[i], " ");
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
			places.PrevTeam = ParseTeam(prev);
			places.WeAre = ParseTeam(weare);
			places.NextTeam = ParseTeam(nxt);

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
					.Replace("p.ghzdays", places.PrevTeam.GHzDays.ToString(CultureInfo.CurrentCulture))
					.Replace("c.ghzdays", places.WeAre.GHzDays.ToString(CultureInfo.CurrentCulture))
					.Replace("n.ghzdays", places.NextTeam.GHzDays.ToString(CultureInfo.CurrentCulture))
					.ToString()
			);
			return places;

		}

		/// <summary></summary>
		/// <param name="type"></param>
		/// <param name="filename"></param>
		/// <returns></returns>
		private static Places Report(string type, string filename)
		{

			var places = new Places();

			// Получаем HTML-контент
			var request = (HttpWebRequest)WebRequest.Create(string.Concat("http://www.mersenne.org/report_top_500_custom/?team_flag=1&type=", type, "&rank_lo=1&rank_hi=50&start_date=1990-01-01&end_date="));
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
			var l = arr.Length;
			for (var i = 0; i < l; i++) if (arr[i].Contains(_teamName))
			{
				prev = arr[i - 1];
				weare = arr[i];
				nxt = arr[i + 1];
				break;
			}
			if (prev == null || weare == null || nxt == null) return places;
			places.PrevTeam = ParseTeam2(prev);
			places.WeAre = ParseTeam2(weare);
			places.NextTeam = ParseTeam2(nxt);

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
					.Replace("p.ghzdays", places.PrevTeam.GHzDays.ToString(CultureInfo.CurrentCulture))
					.Replace("c.ghzdays", places.WeAre.GHzDays.ToString(CultureInfo.CurrentCulture))
					.Replace("n.ghzdays", places.NextTeam.GHzDays.ToString(CultureInfo.CurrentCulture))
					.ToString()
			);
			return places;

		}

		/// <summary>Точка входа</summary>
		/// <param name="args">Аргумента командной строки</param>
		static void Main(string[] args)
		{

			Console.Title = RU.Title;

			if (args.Length > 0) _teamName = args[0].Trim();

			Console.WriteLine(string.Concat("Команда ", _teamName));
			Console.WriteLine();

			var totals = TotalsOverall();
			var tf = Report("1001", "tf.json");
			var llt = Report("1003", "llt.json");
			var doublellt = Report("1004", "doublellt.json");
			var p1 = Report("1002", "p1.json");
			var ecm = Report("1005", "ecm.json");
			var ecmf = Report("1006", "ecmf.json");

			Console.WriteLine(RU.TH);
			Console.WriteLine(RU.TBottom);
			Console.WriteLine($"Totals     | {AsString(totals.WeAre.Place, 2)}  | {AsString(totals.PrevTeam.GHzDays - totals.WeAre.GHzDays, 9)} | {AsString(totals.WeAre.GHzDays - totals.NextTeam.GHzDays, 9)}");
			Console.WriteLine($"TF         | {AsString(tf.WeAre.Place, 2)}  | {AsString(tf.PrevTeam.GHzDays - tf.WeAre.GHzDays, 9)} | {AsString(tf.WeAre.GHzDays - tf.NextTeam.GHzDays, 9)}");
			Console.WriteLine($"LLT        | {AsString(llt.WeAre.Place, 2)}  | {AsString(llt.PrevTeam.GHzDays - llt.WeAre.GHzDays, 9)} | {AsString(llt.WeAre.GHzDays - llt.NextTeam.GHzDays, 9)}");
			Console.WriteLine($"Double LLT | {AsString(doublellt.WeAre.Place, 2)}  | {AsString(doublellt.PrevTeam.GHzDays - doublellt.WeAre.GHzDays, 9)} | {AsString(doublellt.WeAre.GHzDays - doublellt.NextTeam.GHzDays, 9)}");
			Console.WriteLine($"P-1        | {AsString(p1.WeAre.Place, 2)}  | {AsString(p1.PrevTeam.GHzDays - p1.WeAre.GHzDays, 9)} | {AsString(p1.WeAre.GHzDays - p1.NextTeam.GHzDays, 9)}");
			Console.WriteLine($"ECM        | {AsString(ecm.WeAre.Place, 2)}  | {AsString(ecm.PrevTeam.GHzDays - ecm.WeAre.GHzDays, 9)} | {AsString(ecm.WeAre.GHzDays - ecm.NextTeam.GHzDays, 9)}");
			Console.WriteLine($"ECMF       | {AsString(ecmf.WeAre.Place, 2)}  | {AsString(ecmf.PrevTeam.GHzDays - ecmf.WeAre.GHzDays, 9)} | {AsString(ecmf.WeAre.GHzDays - ecmf.NextTeam.GHzDays, 9)}");
			Console.WriteLine(RU.TBottom);
			Console.WriteLine();
			Console.WriteLine(RU.Ads);
			Console.WriteLine(RU.Link);

			Console.ReadKey();

		}

	}

}
