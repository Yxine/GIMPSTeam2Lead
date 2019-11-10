using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using I = System.Globalization.CultureInfo;
using C = System.Console;

namespace GIMPSTeam2Lead
{

	/// <summary>Структура команды</summary>
	public struct Team
	{

		/// <summary>Место в рейтинге</summary>
		public byte Place;

		/// <summary>Название команды</summary>
		public string Name;

		/// <summary>Наработки команды в GHz-Days</summary>
		public decimal GHzDays;

	}

	/// <summary></summary>
	public struct Places
	{

		/// <summary>Предыдущая команда</summary>
		public Team PrevTeam;

		/// <summary>Наша команда</summary>
		public Team WeAre;

		/// <summary>Следующая команда</summary>
		public Team NextTeam;

	}

	/// <summary>Класс программы</summary>
	public static class Program
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
			arr = arr[0].Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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
			var s = d.ToString(I.CurrentCulture);
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
			if (string.IsNullOrWhiteSpace(s)) return Repeat(' ', length);
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
			C.WriteLine($"  {s0} {s1} {s2} {s3} {s4} {s5} {s6}");
		}

		/// <summary>Точка входа</summary>
		/// <param name="args">Аргументы командной строки</param>
		private static void Main(string[] args)
		{

			var team = args.Length > 0 ? args[0].Trim() : "ComputerraRU";

			C.Clear();
			C.CursorVisible = false;
			C.Title = "GIMPS Team 2 Lead";
			C.WindowWidth = 150;

			C.ForegroundColor = ConsoleColor.DarkGreen;
			C.WriteLine();
			C.WriteLine(" ****************************************************************************************************************************************************");
			C.WriteLine($" *{string.Empty.PadLeft(146)}*");
			C.WriteLine($" * GIMPS Team 2 Lead version {Assembly.GetEntryAssembly()?.GetName().Version}{string.Empty.PadLeft(103)}*");
			C.WriteLine($" * Larin Alexsandr, alexsandr@larin.name{string.Empty.PadLeft(108)}*");
			C.WriteLine($" *{string.Empty.PadLeft(146)}*");
			C.WriteLine(" ****************************************************************************************************************************************************");
			C.WriteLine();
			C.ForegroundColor = ConsoleColor.White;
			C.WriteLine($"   Team: {team}");
			C.WriteLine();
			C.ForegroundColor = ConsoleColor.DarkYellow;
			C.WriteLine("   Type   We    GHz-Days  To Leader Lead Team                                To Us Follower Team");
			C.WriteLine("   ------ -- -----------  --------- ----------------------------------- ---------- ------------------------------------------------------------------");
			C.WriteLine();
			C.ForegroundColor = ConsoleColor.Gray;

			var r1  = TotalsOverall(team);
			var r2  = Report("1001");
			var r3  = Report("1002");
			var r4  = Report("2000");
			var r5  = Report("2001");
			var r6  = Report("1005");
			var r7  = Report("1009");
			var r8  = Report("1010");
			var r9  = Report("1006");
			var r10 = Report("1003");
			var r11 = Report("1004");
			var r12 = Report("1007");
			var r13 = Report("1008");

			/*
			<option value="1001" selected="selected">Trial Factoring</option>
			<option value="1002">P-1 Factoring</option>
			<option value="2000">First primality tests</option>
			<option value="2001">Double-check primality tests</option>
			<option value="1005">ECM on Mersenne numbers</option>
			<option value="1009">PRP cofactor tests</option>
			<option value="1010">PRP cofactor double-checks</option>
			<option value="1006">ECM on Fermat numbers</option>
			<option value="1003">LL first tests</option>
			<option value="1004">LL double-checks</option>
			<option value="1007">PRP tests</option>
			<option value="1008">PRP double-checks</option>
			*/

			C.ForegroundColor = ConsoleColor.White;
			Print(" Total ", AsString( r1.WeAre.Place, 2), AsString( r1.WeAre.GHzDays, 11), AsString( r1.PrevTeam.GHzDays -  r1.WeAre.GHzDays, 10), AsString( r1.PrevTeam.Name, 35), AsString(r1.WeAre.GHzDays - r1.NextTeam.GHzDays, 10), AsString(r1.NextTeam.Name, 27));
			C.WriteLine();
			C.ForegroundColor = ConsoleColor.Gray;
			Print(" TF    ", AsString( r2.WeAre.Place, 2), AsString( r2.WeAre.GHzDays, 11), AsString( r2.PrevTeam.GHzDays -  r2.WeAre.GHzDays, 10), AsString( r2.PrevTeam.Name, 35), AsString(r2.WeAre.GHzDays - r2.NextTeam.GHzDays, 10), AsString(r2.NextTeam.Name, 27));
			Print(" P-1   ", AsString(r3.WeAre.Place, 2), AsString(r3.WeAre.GHzDays, 11), AsString(r3.PrevTeam.GHzDays - r3.WeAre.GHzDays, 10), AsString(r3.PrevTeam.Name, 35), AsString(r3.WeAre.GHzDays - r3.NextTeam.GHzDays, 10), AsString(r3.NextTeam.Name, 27));
			C.WriteLine();

			Print(" LLFT  ", AsString(r10.WeAre.Place, 2), AsString(r10.WeAre.GHzDays, 11), AsString(r10.PrevTeam.GHzDays - r10.WeAre.GHzDays, 10), AsString(r10.PrevTeam.Name, 35), AsString(r10.WeAre.GHzDays - r10.NextTeam.GHzDays, 10), AsString(r10.NextTeam.Name, 27));
			Print(" LLDC  ", AsString(r11.WeAre.Place, 2), AsString(r11.WeAre.GHzDays, 11), AsString(r11.PrevTeam.GHzDays - r11.WeAre.GHzDays, 10), AsString(r11.PrevTeam.Name, 35), AsString(r11.WeAre.GHzDays - r11.NextTeam.GHzDays, 10), AsString(r11.NextTeam.Name, 27));
			C.WriteLine();

			Print(" ECMM  ", AsString(r6.WeAre.Place, 2), AsString(r6.WeAre.GHzDays, 11), AsString(r6.PrevTeam.GHzDays - r6.WeAre.GHzDays, 10), AsString(r6.PrevTeam.Name, 35), AsString(r6.WeAre.GHzDays - r6.NextTeam.GHzDays, 10), AsString(r6.NextTeam.Name, 27));
			Print(" ECMF  ", AsString(r9.WeAre.Place, 2), AsString(r9.WeAre.GHzDays, 11), AsString(r9.PrevTeam.GHzDays - r9.WeAre.GHzDays, 10), AsString(r9.PrevTeam.Name, 35), AsString(r9.WeAre.GHzDays - r9.NextTeam.GHzDays, 10), AsString(r9.NextTeam.Name, 27));
			C.WriteLine();
			Print(" FPT   ", AsString( r4.WeAre.Place, 2), AsString( r4.WeAre.GHzDays, 11), AsString( r4.PrevTeam.GHzDays -  r4.WeAre.GHzDays, 10), AsString( r4.PrevTeam.Name, 35), AsString(r4.WeAre.GHzDays - r4.NextTeam.GHzDays, 10), AsString(r4.NextTeam.Name, 27));
			Print(" DCPT  ", AsString( r5.WeAre.Place, 2), AsString( r5.WeAre.GHzDays, 11), AsString( r5.PrevTeam.GHzDays -  r5.WeAre.GHzDays, 10), AsString( r5.PrevTeam.Name, 35), AsString(r5.WeAre.GHzDays - r5.NextTeam.GHzDays, 10), AsString(r5.NextTeam.Name, 27));
			Print(" PRPCDC", AsString( r8.WeAre.Place, 2), AsString( r8.WeAre.GHzDays, 11), AsString( r8.PrevTeam.GHzDays -  r8.WeAre.GHzDays, 10), AsString( r8.PrevTeam.Name, 35), AsString(r8.WeAre.GHzDays - r8.NextTeam.GHzDays, 10), AsString(r8.NextTeam.Name, 27));
			Print(" PRPT  ", AsString(r12.WeAre.Place, 2), AsString(r12.WeAre.GHzDays, 11), AsString(r12.PrevTeam.GHzDays - r12.WeAre.GHzDays, 10), AsString(r12.PrevTeam.Name, 35), AsString(r12.WeAre.GHzDays - r12.NextTeam.GHzDays, 10), AsString(r12.NextTeam.Name, 27));
			C.WriteLine();
			Print(" PRPCT ", AsString(r7.WeAre.Place, 2), AsString(r7.WeAre.GHzDays, 11), AsString(r7.PrevTeam.GHzDays - r7.WeAre.GHzDays, 10), AsString(r7.PrevTeam.Name, 35), AsString(r7.WeAre.GHzDays - r7.NextTeam.GHzDays, 10), AsString(r7.NextTeam.Name, 27));
			Print(" PRPDC ", AsString(r13.WeAre.Place, 2), AsString(r13.WeAre.GHzDays, 11), AsString(r13.PrevTeam.GHzDays - r13.WeAre.GHzDays, 10), AsString(r13.PrevTeam.Name, 35), AsString(r13.WeAre.GHzDays - r13.NextTeam.GHzDays, 10), AsString(r13.NextTeam.Name, 27));
			C.ForegroundColor = ConsoleColor.DarkGray;
			C.WriteLine();
			C.WriteLine("   Press any key to exit...");
			C.ReadKey();

		}

	}

}
