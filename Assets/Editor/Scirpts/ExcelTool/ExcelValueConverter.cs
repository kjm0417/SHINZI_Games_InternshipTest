using System;
using UnityEngine;

internal static class ExcelValueConverter
{
    //엑셀 데이터가 있다가 사라질 때 처리를 변경함, 타입별로 처리 방식 및 오류 상태 제공
    public static bool TryConvertValue(string raw,Type type,string header,int row, out object converted)
    {
        converted = null;

        if (type == typeof(string))
        {
            converted = raw ?? string.Empty;
            return true;
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            Debug.LogError($"빈 값 오류: {row + 1}행 " +
                $"'{header}'에는 값이 필요합니다.");

            return false;
        }

        try
        {
            if (type.IsEnum)
            {
                converted = Enum.Parse(type, raw);
            }
            else
            {
                converted = Convert.ChangeType(raw, type);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError( $"변환 오류: {row + 1}행 " + $"'{header}'의 값 '{raw}'을 " +
                $"{type.Name}으로 바꿀 수 없습니다.\n" + exception.Message);

            return false;
        }
    }
}