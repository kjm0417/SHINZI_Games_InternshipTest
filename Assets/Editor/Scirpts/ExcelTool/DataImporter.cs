using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class DataImporter 
{
    //prviate 필드 접근을 위한 처리
    private const BindingFlags FieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;

    //타입별 "id -> SO" 캐시 (참조 연결용)
    private static Dictionary<Type, Dictionary<string, ScriptableObject>> _refCache;

    //해더 읽기 :  헤더 읽기와 본체 필드 검색만 담당
    private static void ReadHeaders<T>(DataTable sheet, out string[] headers, out FieldInfo[] fields)
    {
        int colCount = sheet.Columns.Count; //11
        headers = new string[colCount]; //해더를 11개 만들고
        fields = new FieldInfo[colCount]; //필드 정보도 11개 만들고

        for (int col = 0; col < colCount; col++)
        {
            headers[col] = sheet.Rows[0][col].ToString().Trim(); //해더 이름 담기
            fields[col] = typeof(T).GetField(headers[col], FieldFlags); //타입에 맞는 해더와 똑같은 필드 가져와서 넣기             
        }
    }

    #region Import
    //엑셀 주소를 가져와서 그 값을 어디 파일에 저장할건지 SO로 변환
    public static void Import<T>(string excelPath, string outputFolderPath) where T : ScriptableObject  
    {
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null) return;

        //파일이 없으면 파일 생성 
        EnsureFolderExists(outputFolderPath);

        //필드에 리스트 필드가 있는지 확인
        FieldInfo listField = FindListField<T>();
        if (listField != null)
        {
            ImportAsList<T>(sheet, outputFolderPath, listField);
        }
        else
        {
            ImportNormal<T>(sheet, outputFolderPath);
        }

        //마지막에 한 번만 저장 (성능: 디스크 쓰기 1회)
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();    
        Debug.Log($"{typeof(T)} 데이터 변환 완료");
    }

    //모든 헤더가 본체 필드인지 판단
    private static void ImportNormal<T>(DataTable sheet, string outputFolderPath) where T :ScriptableObject
    {
        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);

        for (int col = 0; col < fields.Length; col++)
        {
            if (fields[col] != null)
            {
                continue;
            }

            Debug.LogError( $"헤더 오류: '{headers[col]}'에 해당하는 " + $"{typeof(T).Name} 필드가 없습니다.");
        }


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
                if (TryConvertValue(raw,fields[col].FieldType, headers[col], row, out object converted))
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
    }

    //본체 또는 List 요소 필드인지 판단 -> 엑셀 여러 행을 하나의 리스트로 묶어서 저장
    private static void ImportAsList<T>(DataTable sheet, string outputFolderPath, FieldInfo listField) where T : ScriptableObject
    {
        //해더를 읽어옴
        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);
        int colCount = sheet.Columns.Count;

        //SO필드에 List 가져오기
        Type elementType = listField.FieldType.GetGenericArguments()[0];

        //리스트 요소(DropEntry)의 필드를 열별로 저장
        FieldInfo[] elementFields = new FieldInfo[colCount];

        // ID 기준으로 행 번호 묶기 -> GroupRowsById로 딕셔너리에 바로 저장
        Dictionary<string, List<int>> groups = GroupRowsById(sheet, out List<string> order);


        for (int col = 0; col < colCount; col++)
        {
            elementFields[col] = elementType.GetField(headers[col], FieldFlags);

            bool hasRootField = fields[col] != null && fields[col] != listField;

            bool hasElementField = elementFields[col] != null;

            // 루트에도 없고 리스트 요소에도 없는 헤더
            if (!hasRootField && !hasElementField)
            {
                Debug.LogError(
                    $"헤더 오류: '{headers[col]}'에 대응하는 " +
                    $"{typeof(T).Name} 또는 {elementType.Name} 필드가 없습니다.");
            }

            // 같은 이름의 필드가 루트와 리스트 요소 양쪽에 존재
            if (hasRootField && hasElementField)
            {
                Debug.LogError(
                    $"헤더 중복 오류: '{headers[col]}'가 " +
                    $"{typeof(T).Name}과 {elementType.Name} 양쪽에 존재합니다.");
            }
        }



        foreach (string id in order)
        {
            string path = $"{outputFolderPath}/{id}.asset";
            T data = AssetDatabase.LoadAssetAtPath<T>(path);
            bool isNew = data == null; //data가 null일 떄 isNew True
            if (isNew) //없으면 새로 생성
            {
                data = ScriptableObject.CreateInstance<T>();
            }

            //기존에 있으면 
            List<int> rowsInGroup = groups[id]; //id의 행 번호 가져오기
            int firstRow = rowsInGroup[0]; //첫번 째 행 번호

            //필드에 있는 내용만 여기서 관리하고 필드에 없고 리스트안에 따로 저장되어 있는값은 다음에
            for (int col = 0; col < colCount; col++)
            {
                if (fields[col] == null) continue;
                if (fields[col] == listField) continue;

                if (typeof(ScriptableObject).IsAssignableFrom(fields[col].FieldType)) continue;

                string raw = sheet.Rows[firstRow][col].ToString().Trim();
                if (TryConvertValue(raw, fields[col].FieldType, headers[col], firstRow, out object converted))
                {
                    fields[col].SetValue(data, converted);
                }

            }


            // List 필드: 그룹의 각 행을 요소로 (참조는 2-Pass에서)
            var newList = (IList)Activator.CreateInstance(listField.FieldType);
            foreach (int row in rowsInGroup)
            {
                object element = Activator.CreateInstance(elementType);

                for (int col = 0; col < colCount; col++)
                {
                    FieldInfo elemField = elementFields[col];
                    if (elemField == null) continue;

                    if (typeof(ScriptableObject).IsAssignableFrom(elemField.FieldType)) continue; // 참조 스킵

                    string raw = sheet.Rows[row][col].ToString().Trim();
                    if (TryConvertValue(raw, elemField.FieldType, headers[col], row, out object converted))
                    {
                        elemField.SetValue(element, converted);
                    }
                }
                newList.Add(element);
            }
            listField.SetValue(data, newList);

            if (isNew)
            {
                AssetDatabase.CreateAsset(data, path);
            }
            EditorUtility.SetDirty(data);
        }
   
       
    }

    // 같은 id(첫 열)로 행 그룹핑 (등장 순서 유지)
    private static Dictionary<string, List<int>> GroupRowsById(DataTable sheet, out List<string> order)
    {
        var groups = new Dictionary<string, List<int>>();
        order = new List<string>();
        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            string id = sheet.Rows[row][0].ToString().Trim();
            if (string.IsNullOrEmpty(id)) continue;

            if (!groups.ContainsKey(id))
            {
                groups[id] = new List<int>();
                order.Add(id);
            }
            groups[id].Add(row);
        }
        return groups;
    }

    #endregion

    #region Resolve References
    //참조가 있는 데이터는 따로 비교 엑셀 주소, 내보낼 주소, SO 주소
    public static void ResolveReferences<T>(string excelPath, string outputFolderPath) where T : ScriptableObject
    {
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null) return;

        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);
        FieldInfo listField = FindListField<T>();
        if (listField != null)
        {
            ResolveListReferences<T>(sheet, outputFolderPath, headers, listField);
        }   
        else
        {
            ResolveNormalReferences<T>(sheet, outputFolderPath, headers, fields);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{typeof(T).Name} 참조 연결 완료");

    }

    // SO에 직접 있는 참조 필드 연결 (behaviorId, aiId 등)
    private static void ResolveNormalReferences<T>(DataTable sheet, string outputFolderPath,
        string[] headers, FieldInfo[] fields) where T : ScriptableObject
    {
        int colCount = sheet.Columns.Count;
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
                if (!typeof(ScriptableObject).IsAssignableFrom(fields[col].FieldType)) continue;

                string refId = sheet.Rows[row][col].ToString().Trim();

                ScriptableObject resolvedReference = ResolveReferenceOrNull(
                        refId,
                        fields[col].FieldType,
                        headers[col],
                        row);

                //SO에 기본 참조 가져오기
                ScriptableObject currentReference = fields[col].GetValue(data) as ScriptableObject;

                if (currentReference == resolvedReference)
                {
                    continue;
                }

                fields[col].SetValue(data, resolvedReference);
                changed = true;
            }
            if (changed) EditorUtility.SetDirty(data);
        }
    }

    // 리스트 요소 안의 참조 필드 연결 (DropEntry.weaponId 등)
    private static void ResolveListReferences<T>(DataTable sheet, string outputFolderPath,
        string[] headers, FieldInfo listField) where T : ScriptableObject
    {
        int colCount = sheet.Columns.Count;
        Type elementType = listField.FieldType.GetGenericArguments()[0];

        // 요소 타입(DropEntry)의 필드들로 캐시 구축
        FieldInfo[] elemFields = elementType.GetFields(FieldFlags);
        BuildCacheForReferenceFields(elemFields);

        Dictionary<string, List<int>> groups = GroupRowsById(sheet, out List<string> order);

        foreach (string id in order)
        {
            string path = $"{outputFolderPath}/{id}.asset";
            T data = AssetDatabase.LoadAssetAtPath<T>(path);
            if (data == null) continue;

            // SO의 리스트 값 꺼내기 (1-Pass에서 만든 것)
            IList list = listField.GetValue(data) as IList;
            if (list == null) continue;

            List<int> rowsInGroup = groups[id];
            bool changed = false;

            // 리스트 요소와 엑셀 행을 순서대로 매칭
            for (int i = 0; i < list.Count && i < rowsInGroup.Count; i++)
            {
                object element = list[i];
                int row = rowsInGroup[i];

                for (int col = 0; col < colCount; col++)
                {
                    FieldInfo elemField = elementType.GetField(headers[col], FieldFlags);
                    if (elemField == null) continue;
                    if (!typeof(ScriptableObject).IsAssignableFrom(elemField.FieldType)) continue; // 참조만

                    string refId = sheet.Rows[row][col].ToString().Trim();

                    ScriptableObject resolvedReference = ResolveReferenceOrNull(
                            refId,
                            elemField.FieldType,
                            headers[col],
                            row);

                    ScriptableObject currentReference = elemField.GetValue(element) as ScriptableObject;

                    if (currentReference == resolvedReference)
                    {
                        continue;
                    }

                    elemField.SetValue(element, resolvedReference);
                    changed = true;
                }
                list[i] = element; // 값 타입 대비 재할당
            }

            if (changed) EditorUtility.SetDirty(data);
        }
    }


    //SO 변경 시 빈 셀 일 경우 초기화 안되고 기존값 유지 현상 막기 
    //문자열 ID를 해당 SO 참조로 바꿀 수 있나?
    private static ScriptableObject ResolveReferenceOrNull(string refId, Type referenceType, string header, int row)
    { //refId : Excel 참조 ID, refId : SO 타

        if (string.IsNullOrWhiteSpace(refId))
        {
            return null;
        }

        ScriptableObject reference = FindFromCache(refId, referenceType);

        if (reference == null)
        {
            Debug.LogError( $"참조 실패: {row + 1}행 " + $"'{header}'의 ID '{refId}'에 해당하는 " +
                $"{referenceType.Name} 에셋을 찾을 수 없습니다.");
        }

        return reference;
    }

    #endregion
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

    //엑셀 데이터가 있다가 사라질 때 처리를 변경함, 타입별로 처리 방식 및 오류 상태 제공
    private static bool TryConvertValue(string raw, Type type, string header, int row, out object converted)
    {
        converted = null;

        // string은 이미 원하는 타입이므로 그대로 적용
        if (type == typeof(string))
        {
            converted = raw ?? string.Empty;
            return true;
        }

        //숫자와 enum의 빈 셀은 현재 데이터 규칙에서 허용하지 않음
        if (string.IsNullOrWhiteSpace(raw))
        {
            Debug.LogError($"빈 값 오류: {row + 1}행 " +$"'{header}'에는 값이 필요합니다.");

            return false;
        }

        try
        {
            if (type.IsEnum)
            {
                //enum처리
                converted = Enum.Parse(type, raw);
            }
            else
            {
                //기본적인 처리
                converted = Convert.ChangeType(raw, type);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"변환 실패: {row + 1}행 " + $"'{header}'의 값 '{raw}'을 " +
                $"{type.Name}으로 바꿀 수 없습니다.\n" +exception.Message);

            converted = null;
            return false;
        }

    }

    //파일 없으면 파일 생성 해주기
    private static void EnsureFolderExists(string folderPath)
    {
        //폴더가 존재하는지 확인 있으면 true 없으면 false
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        Debug.Log($"출력 폴더가 없어 생성함: {folderPath}");

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

    #region List 판단 메서드

    // List 필드를 찾아 반환 (없으면 null)
    private static FieldInfo FindListField<T>()
    {
        foreach (FieldInfo field in typeof(T).GetFields(FieldFlags))
        {
            //필드가 제네릭 타입이고, 전달 되는 타입이 List인지 
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                return field;
        }
        return null;
    }

    #endregion
}
