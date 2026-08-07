using UnityEditor;

public class ExcelReaderTestMenu
{
    [MenuItem("Tools/Excel Test/Read Weapon Sheet")]
    public static void RunTest()
    {
        // 네 엑셀 파일의 실제 경로로 바꿔야 함
        string path = "Assets/Editor/ExcelSource/WeaponData.xlsx";
        ExcelReader.Read(path);
    }
}