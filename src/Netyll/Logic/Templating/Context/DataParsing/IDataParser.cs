namespace Netyll.Logic.Templating.Context.DataParsing
{
    internal interface IDataParser
    {
        string Extension { get; }

        bool CanParse(string folder, string method);

        object Parse(string folder, string method);
    }
}
