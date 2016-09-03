namespace OasCommonLib.VinParser
{
    public interface IVinParser
    {
        VinInfo Parse(string json);
        string LastError { get; }
    }
}
