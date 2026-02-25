namespace FfAdmin.PublicApi.Function;

public static class FileNames
{
    private const string OPTIONWORTHHISTORY = "option_worth_history_{0}.json";
    private const string OPTIONCHART = "option_chart_{0}.svg";

    public static string Options()
        => "options.json";
    public static string OptionWorthHistory(string option)
        => string.Format(OPTIONWORTHHISTORY, option);

    public static string OptionChart(string option)
        => string.Format(OPTIONCHART, option);
}