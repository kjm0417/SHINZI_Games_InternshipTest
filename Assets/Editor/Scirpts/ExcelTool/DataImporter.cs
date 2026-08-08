using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class DataImporter 
{
    //prviate 필드 접근을 위한 처리
    private const BindingFlags FieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;

    //엑셀 주소를 가져와서 그 값을 어디 파일에 저장할건지
    public static void Import<T>(string excelPath, string outputFolderPath) where T : ScriptableObject  
    {
        //엑셀 표 읽어오기
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null) return;

        //파일이 없으면 파일 생성 
        EnsureFolderExists(outputFolderPath);


        int colCount = sheet.Columns.Count;

        //헤더 개수에 맞게 배열 정의 필드 정의
        string[] headers = new string[colCount];
        FieldInfo[] fields = new FieldInfo[colCount];

        for(int col = 0; col < colCount; col++)
        {
            //헤더와 필드를 맞춰서 타입 결정하기
            headers[col] = sheet.Rows[0][col].ToString().Trim();
            fields[col] = typeof(T).GetField(headers[col], FieldFlags);
            if (fields[col] == null)
            {
                Debug.LogWarning($"필드 없음: 헤더 {headers[col]}");
            }
                
        }

        //데이터(1행부터) 순회
        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            string id = sheet.Rows[row][0].ToString().Trim();
            if (string.IsNullOrEmpty(id)) continue;

            string path = $"{outputFolderPath}/{id}.asset";

            //있으면 불러오기(덮어쓰기), 없으면 생성 (참조 끊김 방지)
            T data = AssetDatabase.LoadAssetAtPath<T>(path);
            bool isNew = data == null;
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<T>();
            }
               

            //리플렉션으로 값 채우기
            for (int col = 0; col < colCount; col++)
            {
                if (fields[col] == null) continue;

                string raw = sheet.Rows[row][col].ToString().Trim();
                object converted = ConvertValue(raw, fields[col].FieldType, headers[col], row);
                if (converted != null)
                {
                    fields[col].SetValue(data, converted);
                }
                    
            }

            if (isNew)
            {
                AssetDatabase.CreateAsset(data, path);
            }
            EditorUtility.SetDirty(data);
        }

        //마지막에 한 번만 저장 (성능: 디스크 쓰기 1회)
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();    
        Debug.Log("무기 데이터 변환 완료");
    }

    private static object ConvertValue(string raw, Type type, string header, int row)
    {
        if (string.IsNullOrEmpty(raw)) return null;

        try
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, raw);
            }  
            return Convert.ChangeType(raw, type);
        }
        catch
        {
            Debug.LogError($"변환 실패: {row + 1}행 '{header}'의 값 '{raw}'를 {type.Name}으로 바꿀 수 없음");
            return null;
        }
    }


    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        Debug.Log($"출력 폴더가 없어 생성함: {folderPath}");  // 알림 추가

        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
                
            current = next;
        }
    }

}
