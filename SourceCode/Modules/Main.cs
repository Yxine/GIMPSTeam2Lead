using System;
using System.Reflection;
using C = System.Console;

namespace GIMPSTeam2Lead
{

	/// <summary>Класс программы</summary>
	public static partial class Program
	{

		/// <summary>Точка входа</summary>
		/// <param name="args">Аргументы командной строки</param>
		private static void Main(string[] args)
		{

			var team = args.Length > 0 ? args[0].Trim() : "ComputerraRU";

			C.Clear();
			C.CursorVisible = false;
			C.Title = "GIMPS Team 2 Lead";
			
			C.WriteLine("{0} *{0} * GIMPS Team 2 Lead version {1}{0} * http://computerraru.ru/software/gimpsteam2lead{0} * Larin Alexsandr{0} *{0}", Environment.NewLine, Assembly.GetEntryAssembly().GetName().Version);
			C.ForegroundColor = ConsoleColor.White;
			C.WriteLine($"   Team: {team}{Environment.NewLine}");
			C.ForegroundColor = ConsoleColor.Gray;
			C.WriteLine("   {0}{1}   | Type   | We * GHz-Days    |  To Leader | Lead Team    |      To Us | Follower Team               |{1}   {0}", Repeat('-', 100), Environment.NewLine);

			var r = TotalsOverall(team);
			Print("Totals", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			r = Report("1001");
			Print("TF    ", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			r = Report("1003");
			Print("LLT   ", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			r = Report("1004");
			Print("DLLT  ", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			r = Report("1002");
			Print("P-1   ", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			r = Report("1005");
			Print("ECM   ", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			r = Report("1006");
			Print("ECMF  ", AsString(r.WeAre.Place, 2), AsString(r.WeAre.GHzDays, 11), AsString(r.PrevTeam.GHzDays - r.WeAre.GHzDays, 10), AsString(r.PrevTeam.Name, 12), AsString(r.WeAre.GHzDays - r.NextTeam.GHzDays, 10), AsString(r.NextTeam.Name, 27));

			C.WriteLine("   {0}{1}{1}   Press any key to exit...", Repeat('-', 100), Environment.NewLine);
			C.ReadKey();

		}

	}

}
