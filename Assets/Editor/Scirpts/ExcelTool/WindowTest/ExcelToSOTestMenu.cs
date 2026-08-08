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


}