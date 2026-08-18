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

    #region 테이블 매핑
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


    #region Public API
    //엑셀 주소를 가져와서 그 값을 어디 파일에 저장할건지 SO로 변환
    public static bool Import<T>(string excelPath, string outputFolderPath) where T : ScriptableObject
    {
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null) return false;

        //필드에 리스트 필드가 있는지 확인
        FieldInfo listField = FindListField<T>();

        // 리스트가 없으면 일반 테이블이므로 ID 고유성이 필요
        bool requireUniqueIds = listField == null;

        //실제 SO를 만들기 전에 ID 검증
        if (!DataTableValidator.ValidateIds(sheet, requireUniqueIds, typeof(T).Name))
        {
            Debug.LogError($"{typeof(T).Name} Import를 중단합니다.");

            return false;
        }

        //파일이 없으면 파일 생성 
        EnsureFolderExists(outputFolderPath);


        if (listField != null)
        {
            if (!ImportAsList<T>(sheet, outputFolderPath, listField))
            {
                Debug.LogError($"{typeof(T).Name} 데이터 검증 실패로 Import를 중단합니다.");
                return false;
            }
        }
        else
        {
            if (!ImportNormal<T>(sheet, outputFolderPath))
            {
                Debug.LogError($"{typeof(T).Name} 데이터 검증 실패로 Import를 중단합니다.");
                return false;
            }
        }

        return true;
    }

    //참조가 있는 데이터는 따로 비교 엑셀 주소, 내보낼 주소, SO 주소
    public static bool ResolveReferences<T>(string excelPath, string outputFolderPath) where T : ScriptableObject
    {
        DataTable sheet = ExcelReader.Read(excelPath);

        if (sheet == null)
        {
            return false;
        }

        ReadHeaders<T>(sheet,out string[] headers,out FieldInfo[] fields);

        FieldInfo listField = FindListField<T>();

        bool requireUniqueIds = listField == null;

        if (!DataTableValidator.ValidateIds(sheet, requireUniqueIds,typeof(T).Name))
        {
            Debug.LogError($"{typeof(T).Name} 참조 연결을 중단합니다.");

            return false;
        }

        if (listField != null)
        {
            Type elementType =listField.FieldType.GetGenericArguments()[0];

            FieldInfo[] elementFields = new FieldInfo[headers.Length];

            for (int col = 0; col < headers.Length; col++)
            {
                elementFields[col] = elementType.GetField(headers[col], FieldFlags);
            }

            var referenceValidator = new DataReferenceValidator(elementFields);

            bool isValid = referenceValidator.ValidateList<T>(sheet, outputFolderPath,
                    headers, listField, elementFields);

            if (!isValid)
            {
                Debug.LogError($"{typeof(T).Name} 참조 검증 실패로 적용을 중단합니다.");

                return false;
            }

            ResolveListReferences<T>( sheet, outputFolderPath, headers, listField,
                elementFields, referenceValidator);
        }
        else
        {
            var referenceValidator = new DataReferenceValidator(fields);

            bool isValid = referenceValidator.ValidateNormal<T>(sheet, outputFolderPath, 
                headers, fields);

            if (!isValid)
            {
                Debug.LogError($"{typeof(T).Name} 참조 검증 실패로 적용을 중단합니다.");

                return false;
            }

            ResolveNormalReferences<T>(sheet, outputFolderPath, headers, fields, referenceValidator);
        }

        Debug.Log($"{typeof(T).Name} 참조 연결 완료");
        return true;

    }
    #endregion

    #region Import

    //모든 헤더가 본체 필드인지 판단
    private static bool ImportNormal<T>(DataTable sheet, string outputFolderPath) where T :ScriptableObject
    {
        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);

        //SO를 불러오거나 생성하기 전에 전체 테이블 검증
        if (!DataTableValidator.ValidateNormalData<T>( sheet, headers, fields))
        {
            return false;
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
                if (ExcelValueConverter.TryConvertValue(raw, fields[col].FieldType, headers[col],row, out object converted))
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
        return true;
    }

    //본체 또는 List 요소 필드인지 판단 -> 엑셀 여러 행을 하나의 리스트로 묶어서 저장
    private static bool ImportAsList<T>(DataTable sheet, string outputFolderPath, FieldInfo listField) where T : ScriptableObject
    {
        //엑셀 헤더와 루트 SO 필드 연결
        ReadHeaders<T>(sheet, out string[] headers, out FieldInfo[] fields);

        int colCount = sheet.Columns.Count;

        // List<DropEntry>에서 DropEntry 타입 추출
        Type elementType = listField.FieldType.GetGenericArguments()[0];

        // 각 엑셀 열에 대응하는 DropEntry 필드 저장
        FieldInfo[] elementFields = new FieldInfo[colCount];

        for (int col = 0; col < colCount; col++)
        {
            elementFields[col] = elementType.GetField(headers[col], FieldFlags);
        }

        // 같은 ID를 가진 행들을 그룹으로 구성
        Dictionary<string, List<int>> groups = GroupRowsById(sheet, out List<string> order);

        // SO를 생성하거나 변경하기 전에 전체 리스트 테이블 검증
        if (!DataTableValidator.ValidateListData<T>(sheet,headers,fields,listField, elementFields, groups, order))
        {
            return false;
        }


        //여기부터 기존 실제 SO 적용 코드
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
                if (ExcelValueConverter.TryConvertValue(raw, fields[col].FieldType, headers[col], firstRow, out object converted))
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
                    if (ExcelValueConverter.TryConvertValue(raw, elemField.FieldType, headers[col], row, out object converted))
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

        return true;
    }

    #endregion

    #region Resolve References

    // SO에 직접 있는 참조 필드 연결 (behaviorId, aiId 등)
    private static void ResolveNormalReferences<T>(DataTable sheet, string outputFolderPath,
        string[] headers, FieldInfo[] fields, DataReferenceValidator referenceValidator) where T : ScriptableObject
    {
        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            string id = sheet.Rows[row][0].ToString().Trim();
            if (string.IsNullOrEmpty(id)) continue;

            string path = $"{outputFolderPath}/{id}.asset";
            T data = AssetDatabase.LoadAssetAtPath<T>(path);

            if (data == null) continue;

            bool changed = false;

            for (int col = 0; col < fields.Length; col++)
            {
                FieldInfo field = fields[col];

                if (field == null) continue;

                if (!typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                string refId =sheet.Rows[row][col].ToString().Trim();

                ScriptableObject resolvedReference = referenceValidator.GetReferenceOrNull(refId, field.FieldType);

                ScriptableObject currentReference = field.GetValue(data) as ScriptableObject;

                if (currentReference == resolvedReference)
                {
                    continue;
                }

                field.SetValue(data, resolvedReference);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
            }
        }
    }

    // 리스트 요소 안의 참조 필드 연결 (DropEntry.weaponId 등)
    private static void ResolveListReferences<T>(DataTable sheet, string outputFolderPath,
        string[] headers, FieldInfo listField, 
        FieldInfo[] elementFields, DataReferenceValidator referenceValidator) where T : ScriptableObject
    {
        Dictionary<string, List<int>> groups =
        GroupRowsById(sheet, out List<string> order);

        foreach (string id in order)
        {
            string path = $"{outputFolderPath}/{id}.asset";
            T data = AssetDatabase.LoadAssetAtPath<T>(path);

            if (data == null) continue;

            IList list = listField.GetValue(data) as IList;
            if (list == null) continue;

            List<int> rowsInGroup = groups[id];
            bool changed = false;

            for (int i = 0; i < list.Count && i < rowsInGroup.Count;i++)
            {
                object element = list[i];
                int row = rowsInGroup[i];

                for (int col = 0; col < elementFields.Length; col++)
                {
                    FieldInfo elementField = elementFields[col];

                    if (elementField == null) continue;

                    if (!typeof(ScriptableObject).IsAssignableFrom(elementField.FieldType))
                    {
                        continue;
                    }

                    string refId = sheet.Rows[row][col].ToString().Trim();

                    ScriptableObject resolvedReference = referenceValidator.GetReferenceOrNull(refId,elementField.FieldType);

                    ScriptableObject currentReference = elementField.GetValue(element) as ScriptableObject;

                    if (currentReference == resolvedReference)
                    {
                        continue;
                    }

                    elementField.SetValue(element, resolvedReference);

                    changed = true;
                }

                // 리스트 요소가 struct일 수 있으므로 다시 넣어준다.
                list[i] = element;
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
            }
        }
    }

    #endregion

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


}
