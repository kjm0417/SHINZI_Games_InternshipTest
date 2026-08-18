using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal sealed class DataReferenceValidator
{
    private readonly Dictionary<Type, Dictionary<string, ScriptableObject>> referenceCache = new();

    public DataReferenceValidator(FieldInfo[] referenceFields)
    {
        BuildCache(referenceFields);
    }

    public bool ValidateNormal<T>(DataTable sheet, string outputFolderPath, string[] headers,
        FieldInfo[] fields) where T : ScriptableObject
    {
        bool isValid = true;

        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            string id = sheet.Rows[row][0].ToString().Trim();
            if (string.IsNullOrEmpty(id)) continue;

            string path = $"{outputFolderPath}/{id}.asset";
            T data = AssetDatabase.LoadAssetAtPath<T>(path);

            if (data == null)
            {
                Debug.LogError($"{typeof(T).Name} SO를 찾을 수 없습니다: {path}");

                isValid = false;
            }

            for (int col = 0; col < fields.Length; col++)
            {
                FieldInfo field = fields[col];

                if (field == null) continue;

                if (!typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                string refId = sheet.Rows[row][col].ToString().Trim();

                if (!ValidateReference(refId, field.FieldType, headers[col], row))
                {
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    public bool ValidateList<T>(DataTable sheet, string outputFolderPath, string[] headers,
        FieldInfo listField, FieldInfo[] elementFields)  where T : ScriptableObject
    {
        bool isValid = true;
        var checkedIds = new HashSet<string>();

        for (int row = 1; row < sheet.Rows.Count; row++)
        {
            string id = sheet.Rows[row][0].ToString().Trim();
            if (string.IsNullOrEmpty(id)) continue;

            //같은 ID가 여러 행에 있어도 루트 SO는 한 번만 검사
            if (checkedIds.Add(id))
            {
                string path = $"{outputFolderPath}/{id}.asset";
                T data = AssetDatabase.LoadAssetAtPath<T>(path);

                if (data == null)
                {
                    Debug.LogError($"{typeof(T).Name} SO를 찾을 수 없습니다: {path}");

                    isValid = false;
                }
                else
                {
                    IList list = listField.GetValue(data) as IList;

                    if (list == null)
                    {
                        Debug.LogError($"{typeof(T).Name} '{id}'의 리스트가 없습니다.");

                        isValid = false;
                    }
                }
            }

            for (int col = 0; col < elementFields.Length; col++)
            {
                FieldInfo field = elementFields[col];

                if (field == null) continue;

                if (!typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                string refId =
                    sheet.Rows[row][col].ToString().Trim();

                if (!ValidateReference(refId, field.FieldType, headers[col], row))
                {
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    public ScriptableObject GetReferenceOrNull(string refId, Type referenceType)
    {
        if (string.IsNullOrWhiteSpace(refId))
        {
            return null;
        }

        if (referenceCache.TryGetValue( referenceType, out Dictionary<string, ScriptableObject> references) &&
            references.TryGetValue(refId, out ScriptableObject reference))
        {
            return reference;
        }

        return null;
    }

    private bool ValidateReference(string refId, Type referenceType, string header, int row)
    {
        // 빈 셀은 참조를 비우겠다는 정상 데이터
        if (string.IsNullOrWhiteSpace(refId))
        {
            return true;
        }

        if (GetReferenceOrNull(refId, referenceType) != null)
        {
            return true;
        }

        Debug.LogError($"참조 오류: {row + 1}행 '{header}'의 ID '{refId}'에 해당하는 " +
            $"{referenceType.Name} 데이터를 찾을 수 없습니다.");

        return false;
    }

    private void BuildCache(FieldInfo[] fields)
    {
        foreach (FieldInfo field in fields)
        {
            if (field == null) continue;

            Type referenceType = field.FieldType;

            if (!typeof(ScriptableObject).IsAssignableFrom(referenceType))
            {
                continue;
            }

            if (referenceCache.ContainsKey(referenceType))
            {
                continue;
            }

            var references = new Dictionary<string, ScriptableObject>();

            string[] guids = AssetDatabase.FindAssets($"t:{referenceType.Name}");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                string assetId = Path.GetFileNameWithoutExtension(assetPath);

                ScriptableObject data = AssetDatabase.LoadAssetAtPath( assetPath, referenceType) as ScriptableObject;

                if (data != null)
                {
                    references[assetId] = data;
                }
            }

            referenceCache.Add(referenceType, references);
        }
    }
}