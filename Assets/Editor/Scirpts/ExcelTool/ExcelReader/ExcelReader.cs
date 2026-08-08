using System.IO;
using ExcelDataReader;
using UnityEngine;
using System.Data;

//1. 리펙토링 1. 디버깅만 하는 걸 이제 DataTable을 실제로 넘겨주게 완성하기



public class ExcelReader
{
    //엑셀 파일 읽어오기 
    public static DataTable Read(string filePath)
    {
        //파일 없으면 
        if (!File.Exists(filePath))
        {
            Debug.LogError($"파일 없음: {filePath}");
            return null;
        }

        //파일 읽기 전용 스트림 : using을 활용하여 열었던 파일을 자동으로 닫게 만들기
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read   ))
        {
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
            {

                DataSet result = reader.AsDataSet();

                //표 전체 가져오기
                return result.Tables[0];
            }
        }
    }
}
