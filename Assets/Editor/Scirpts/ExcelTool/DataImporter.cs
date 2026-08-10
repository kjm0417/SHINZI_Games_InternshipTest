using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using TextMateSharp.Grammars;
using UnityEditor;
using UnityEngine;

public static class DataImporter 
{
    //prviate 필드 접근을 위한 처리
    private const BindingFlags FieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;

    //타입별 "id -> SO" 캐시 (참조 연결용)
    private static Dictionary<Type, Dictionary<string, ScriptableObject>> _refCache;

    private static void ReadHeaders<T>(DataTable sheet, out string[] headers, out FieldInfo[] fields)
    {
        int colCount = sheet.Columns.Count;
        headers = new string[colCount];
        fields = new FieldInfo[colCount];

        for (int col = 0; col < colCount; col++)
        {
            headers[col] = sheet.Rows[0][col].ToString().Trim();
            fields[col] = typeof(T).GetField(headers[col], FieldFlags);
            if (fields[col] == null)
                Debug.LogWarning($"필드 없음: 헤더 '{headers[col]}'에 해당하는 {typeof(T).Name} 필드가 없어 건너뜀");
        }
    }

    //엑셀 주소를 가져와서 그 값을 어디 파일에 저장할건지
    public static void Import<T>(string excelPath, string outputFolderPath) where T : ScriptableObject  
    {
        //엑셀 표 읽어오기
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null) return;

        //파일이 없으면 파일 생성 
        EnsureFolderExists(outputFolderPath);

        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);
        int colCount = sheet.Columns.Count;

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

                if (typeof(ScriptableObject).IsAssignableFrom(fields[col].FieldType)) continue;

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
        Debug.Log($"{typeof(T)} 데이터 변환 완료");
    }


    //참조가 있는 데이터는 따로 비교 엑셀 주소, 내보낼 주소, SO 주소
    public static void ResolveReferences<T>(string excelPath, string outputFolderPath) where T : ScriptableObject
    {
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null) return;

        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);
        int colCount = sheet.Columns.Count;

        //이 테이블의 참조 필드들이 어떤 타입을 가리키는지 모아서, 그 타입들 캐시 구축
        BuildCacheForReferenceFields(fields);

        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            string id = sheet.Rows[row][0].ToString().Trim();
            if (string.IsNullOrEmpty(id)) continue;

            string path = $"{outputFolderPath}/{id}.asset";
            T data = AssetDatabase.LoadAssetAtPath<T>(path);
            if (data == null) continue;

            bool changed = false;

            for (int col = 0; col < colCount; col++)
            {
                if (fields[col] == null) continue;

                //참조 필드(SO 타입)만 처리
                if (!typeof(ScriptableObject).IsAssignableFrom(fields[col].FieldType))
                    continue;

                string refId = sheet.Rows[row][col].ToString().Trim();
                if (string.IsNullOrEmpty(refId)) continue;

                ScriptableObject refSO = FindFromCache(refId, fields[col].FieldType);

                if (refSO != null)
                {
                    fields[col].SetValue(data, refSO);
                    changed = true;
                }
                else
                {
                    Debug.LogError($"참조 실패: {row + 1}행 '{headers[col]}'의 '{refId}'({fields[col].FieldType.Name})를 찾을 수 없음");
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
            }    
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{typeof(T).Name} 참조 연결 완료");

    }

    //참조 필드들이 가리키는 타입만 골라, 그 타입들의 캐시를 구축
    private static void BuildCacheForReferenceFields(FieldInfo[] fields)
    {
        _refCache = new Dictionary<Type, Dictionary<string, ScriptableObject>>();

        foreach (FieldInfo field in fields)
        {
            if (field == null) continue;
            Type type = field.FieldType;

            // SO 타입이고, 아직 캐시 안 만든 타입만
            if (!typeof(ScriptableObject).IsAssignableFrom(type)) continue;
            if (_refCache.ContainsKey(type)) continue;

            // 이 타입의 모든 SO를 한 번만 검색해 딕셔너리로
            var map = new Dictionary<string, ScriptableObject>();
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string assetId = Path.GetFileNameWithoutExtension(assetPath);
                var so = AssetDatabase.LoadAssetAtPath(assetPath, type) as ScriptableObject;
                if (so != null)
                    map[assetId] = so;
            }
            _refCache[type] = map;
        }
    }

    //캐시에서 id로 SO 조회 (O(1))
    private static ScriptableObject FindFromCache(string id, Type type)
    {
        if (_refCache.TryGetValue(type, out var map) && map.TryGetValue(id, out var so))
        {
            return so;
        }
           
        return null;
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
