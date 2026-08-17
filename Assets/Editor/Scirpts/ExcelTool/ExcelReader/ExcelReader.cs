using ExcelDataReader;
using System;
using System.Data;
using System.IO;
using UnityEngine;

//역할 : ExcelDataReader 라이브러리를 활용하여 엑셀파일 읽고 표로 넘겨주는 역할
//정적 클래스로 결정 이유 : 도구 클래스라는 설계 의도
public static class ExcelReader
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

        try
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet result = reader.AsDataSet();

                    if (result.Tables.Count == 0)
                    {
                        Debug.LogError($"엑셀 시트가 없습니다: {filePath}");
                        return null;
                    }

                    return result.Tables[0];
                }
            }
        }
        catch (UnauthorizedAccessException exception) //운영 체제가 액세스를 거부할 때
        {
            Debug.LogError($"엑셀 파일 접근 권한이 없음: {filePath}\n{exception.Message}");
        }
        catch (IOException exception) // I/O 오류가 발생하는 경우
        {
            Debug.LogError($"엑셀 파일을 열거나 읽을 수 없음: {filePath}\n{exception.Message}");
        }
        catch (Exception exception) //애플리케이션을 실행할 때 나타나는 오류
        {
            Debug.LogError($"엑셀 파일 변환 중 오류가 발생: {filePath}\n{exception.Message}");
        }

        return null;
    }
}
