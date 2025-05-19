namespace GameServer.Data.SDB.Records.dbconfig;
public record class Keybinding
{
    public string Category { get; set; }
    public string Action { get; set; }
    public int Id { get; set; }
    public int EnglishBinding1 { get; set; }
    public int ChineseBinding1 { get; set; }
    public int EnglishBinding2 { get; set; }
    public int GermanBinding1 { get; set; }
    public int ChineseBinding2 { get; set; }
    public int GermanBinding2 { get; set; }
    public int FrenchBinding1 { get; set; }
    public int KoreanBinding2 { get; set; }
    public int Binding3 { get; set; }
    public int Binding4 { get; set; }
    public int FrenchBinding2 { get; set; }
    public int KoreanBinding1 { get; set; }
    public int B4UseMod { get; set; }
    public int ChineseB1UseMod { get; set; }
    public int B3UseMod { get; set; }
    public int GermanB1UseMod { get; set; }
    public int GermanB2UseMod { get; set; }
    public int KoreanB2UseMod { get; set; }
    public int EnglishB2UseMod { get; set; }
    public int ChineseB2UseMod { get; set; }
    public int KoreanB1UseMod { get; set; }
    public int EnglishB1UseMod { get; set; }
    public int FrenchB2UseMod { get; set; }
    public int FrenchB1UseMod { get; set; }
}