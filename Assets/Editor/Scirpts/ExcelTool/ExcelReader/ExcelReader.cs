using System.IO;
using ExcelDataReader;
using UnityEngine;
using System.Data;

public class ExcelReader
{
    //엑셀 파일 읽어오기 
    public static void Read(string filePath)
    {
        //파일 읽기 전용 스트림 : using을 활용하여 열었던 파일을 자동으로 닫게 만들기
        using(var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
            {
                //리더 생성 성공
                Debug.Log($"리더 생성 성공. 시트 개수 : {reader.ResultsCount}");

                //전체 표 받기
                DataSet result = reader.AsDataSet();

                //첫 번째 시트를 꺼냄
                DataTable sheet = result.Tables[0];

                //표 크기르 확인
                int rowCount = sheet.Rows.Count;
                int colCount = sheet.Columns.Count;
                Debug.Log($"시트 이름 : {sheet.TableName}, 행 개수: {rowCount}, 열 개수 : {colCount}");

                //표 한 칸씩 순회하면 값 찍기
                for(int row =0;row < rowCount; row++)
                {
                    string line = "";
                    for(int col =0; col < colCount; col ++)
                    {
                        object cell = sheet.Rows[row][col];
                        line += cell + "\t";

                    }
                    Debug.Log($"[{row}행] {line}");
                }
            }
        }
    }
}
