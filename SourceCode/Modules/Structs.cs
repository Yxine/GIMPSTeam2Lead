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
