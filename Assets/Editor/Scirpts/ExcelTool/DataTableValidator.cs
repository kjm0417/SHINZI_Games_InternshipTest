using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UnityEngine;

internal static class DataTableValidator 
{
    //데이터마다 ID는 고유의 값이므로 일반 데이터는 중복x , 리스트를 포함하고있으면 o
    public static bool ValidateIds(DataTable sheet, bool requireUniqueIds, string tableName)
    {
        bool isValid = true;

        var firstRowsById = new Dictionary<string, int>();

        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            DataRow dataRow = sheet.Rows[row];

            // 완전히 비어 있는 행은 데이터로 판단하지 않음
            if (IsRowEmpty(dataRow))
            {
                continue;
            }

            string id = dataRow[0].ToString().Trim();

            // 다른 열에는 값이 있지만 첫 번째 열의 ID가 없는 경우
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"ID 오류: {tableName}의 {row + 1}행에 ID가 없습니다.");

                isValid = false;
                continue;
            }

            // 리스트 테이블은 같은 ID 반복을 허용
            if (!requireUniqueIds)
            {
                continue;
            }

            // 처음 등장한 ID라면 ID와 행 번호를 저장
            if (!firstRowsById.TryGetValue(id, out int firstRow))
            {
                firstRowsById.Add(id, row);
                continue;
            }

            // 이미 저장된 ID라면 일반 테이블의 ID 중복
            Debug.LogError($"ID 중복 오류: {tableName}의 ID '{id}'가 " +
                $"{firstRow + 1}행과 {row + 1}행에 중복되어 있습니다.");

            isValid = false;
        }

        return isValid;
    }

    //전체 중에 틀린 내용이있으면 SO에 적용을 막는 검증
    public static bool ValidateNormalData<T>(DataTable sheet, string[] headers,
    FieldInfo[] fields) where T : ScriptableObject
    {
        bool isValid = true;

        //먼저 모든 헤더가 SO 필드와 연결되는지 확인
        for (int col = 0; col < fields.Length; col++)
        {
            if (fields[col] != null)
            {
                continue;
            }

            Debug.LogError($"헤더 오류: '{headers[col]}'에 대응하는 " + $"{typeof(T).Name} 필드가 없습니다.");

            isValid = false;
        }

        //모든 행의 일반 값을 실제 적용 전에 검사
        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            DataRow dataRow = sheet.Rows[row];

            if (IsRowEmpty(dataRow))
            {
                continue;
            }

            for (int col = 0; col < fields.Length; col++)
            {
                FieldInfo field = fields[col];

                //연결되지 않은 헤더는 위에서 이미 오류 출력
                if (field == null)
                {
                    continue;
                }

                //SO 참조는 ResolveReferences 단계에서 처리
                if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                string raw = dataRow[col].ToString().Trim();

                if (!ExcelValueConverter.TryConvertValue(raw, fields[col].FieldType, headers[col], row, out _))
                {
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    public static bool ValidateListData<T>(DataTable sheet, string[] headers, FieldInfo[] rootFields,
      FieldInfo listField, FieldInfo[] elementFields, Dictionary<string, List<int>> groups, List<string> order) where T : ScriptableObject
    {
        bool isValid = true;

        Type elementType =
            listField.FieldType.GetGenericArguments()[0];

        // 각 헤더가 루트 또는 리스트 요소 중 어디에 속하는지 검사
        for (int col = 0; col < headers.Length; col++)
        {
            bool hasRootField =
                rootFields[col] != null &&
                rootFields[col] != listField;

            bool hasElementField =
                elementFields[col] != null;

            // 양쪽 어디에도 없는 헤더
            if (!hasRootField && !hasElementField)
            {
                Debug.LogError(
                    $"헤더 오류: '{headers[col]}'에 대응하는 " +
                    $"{typeof(T).Name} 또는 {elementType.Name} 필드가 없습니다.");

                isValid = false;
            }

            // 양쪽 모두에 존재해서 적용 대상을 결정할 수 없는 헤더
            if (hasRootField && hasElementField)
            {
                Debug.LogError(
                    $"헤더 중복 오류: '{headers[col]}'가 " +
                    $"{typeof(T).Name}과 {elementType.Name} 양쪽에 존재합니다.");

                isValid = false;
            }
        }

        // 루트 SO 값 검증
        // 같은 ID 그룹의 루트 값은 첫 번째 행을 사용하므로 첫 행만 검사
        foreach (string id in order)
        {
            List<int> rowsInGroup = groups[id];
            int firstRow = rowsInGroup[0];

            for (int col = 0; col < rootFields.Length; col++)
            {
                FieldInfo rootField = rootFields[col];

                if (rootField == null || rootField == listField)
                {
                    continue;
                }

                // SO 참조는 ResolveReferences 단계에서 검사
                if (typeof(ScriptableObject).IsAssignableFrom(rootField.FieldType))
                {
                    continue;
                }

                string raw = sheet.Rows[firstRow][col].ToString().Trim();

                if (!ExcelValueConverter.TryConvertValue(raw, rootField.FieldType, headers[col], firstRow, out _))
                {
                    isValid = false;
                }
            }
        }

        // 리스트 요소 값 검증
        // DropEntry는 행마다 하나씩 생성되므로 모든 행을 검사
        foreach (string id in order)
        {
            List<int> rowsInGroup = groups[id];

            foreach (int row in rowsInGroup)
            {
                for (int col = 0; col < elementFields.Length; col++)
                {
                    FieldInfo elementField = elementFields[col];

                    if (elementField == null)
                    {
                        continue;
                    }

                    // WeaponData 같은 참조 필드는 ResolveReferences에서 검사
                    if (typeof(ScriptableObject)
                        .IsAssignableFrom(elementField.FieldType))
                    {
                        continue;
                    }

                    string raw =
                        sheet.Rows[row][col].ToString().Trim();

                    if (!ExcelValueConverter.TryConvertValue(raw, elementFields[col].FieldType, headers[col], row, out _))
                    {
                        isValid = false;
                    }
                }
            }
        }

        return isValid;
    }

    //해당 행의 모든 칸이 비어 있는지 확인
    private static bool IsRowEmpty(DataRow row)
    {
        //행에 모든 값을 받아오는데 값이 하나라도 있으면 false 
        for (int col = 0; col < row.Table.Columns.Count; col++)
        {
            string value = row[col].ToString().Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
        }

        return true;
    }
}
