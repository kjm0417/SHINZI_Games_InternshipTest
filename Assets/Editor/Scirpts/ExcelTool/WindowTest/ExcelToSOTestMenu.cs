using UnityEditor;

public class WeaponReadTest
{
    [MenuItem("Tools/Excel Test/Read Weapon Sheet")]
    public static void RunTest()
    {
        // 네 엑셀 파일의 실제 경로로 바꿔야 함
        string path = "Assets/Editor/ExcelSource/WeaponData.xlsx";
        ExcelReader.Read(path);
    }
}

//public static class WeaponImporterMenu
//{
//    [MenuItem("Tools/Excel Import/Weapon")]
//    public static void ImportWeapon()
//    {
//        string excelPath = "Assets/Editor/ExcelData/WeaponData.xlsx"; // 네 실제 경로로
//        string outputFolder = "Assets/GameData/Weapons";
//        DataImporter.Import<WeaponData>(excelPath, outputFolder);
//    }
//}


public static class DataImporterMenu
{
    [MenuItem("Tools/Excel Import/Weapon")]
    public static void ImportWeapon()
    {
        DataImporter.Import<WeaponData>(
            "Assets/Editor/ExcelData/WeaponData.xlsx",
            "Assets/GameData/Weapons"
            );
    }

    [MenuItem("Tools/Excel Import/AI")]
    public static void ImportAI()
    {
        DataImporter.Import<AIData>(
            "Assets/Editor/ExcelData/AIData.xlsx",
            "Assets/GameData/AIs"
            );
    }

    [MenuItem("Tools/Excel Import/AIBehavior")]
    public static void ImportAIBehaviorData()
    {
        DataImporter.Import<AIBehaviorData>(
            "Assets/Editor/ExcelData/AIBehaviorData.xlsx",
            "Assets/GameData/AIBehaviors"
            );
    }

    [MenuItem("Tools/Excel Import/MatchDrop")]
    public static void ImportMatchDrop()
    {
        DataImporter.Import<MatchDropData>(
            "Assets/Editor/ExcelData/MatchDropData.xlsx",
            "Assets/GameData/MatchDrops"
            );
    }

    [MenuItem("Tools/Excel Import/MatchProgression")]
    public static void ImportMatchProgression()
    {
        DataImporter.Import<MatchProgressionData>(
            "Assets/Editor/ExcelData/MatchProgressionData.xlsx",
            "Assets/GameData/MatchProgressions"
            );
    }

    [MenuItem("Tools/Excel Import/Match")]
    public static void ImportMatch()
    {
        DataImporter.Import<MatchData>(
            "Assets/Editor/ExcelData/MatchData.xlsx",
            "Assets/GameData/Matches"
            );
    }

    [MenuItem("Tools/Excel Import/Player")]
    public static void ImportPlayer()
    {
        DataImporter.Import<PlayerData>(
            "Assets/Editor/ExcelData/PlayerData.xlsx",
            "Assets/GameData/Players"
            );
    }

}