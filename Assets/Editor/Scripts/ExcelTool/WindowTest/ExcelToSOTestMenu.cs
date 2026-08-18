using UnityEditor;
using UnityEngine;

public static class DataImporterMenu
{
    // 경로 상수로 정리 (중복 줄이고 한 곳에서 관리)
    private const string ExcelPath = "Assets/Editor/ExcelData";
    private const string DataPath = "Assets/8.GameData";

    #region Individual Import

    [MenuItem("Tools/Excel Import/Individual/WeaponData")]
    private static void ImportWeaponData()
    {
        ImportWithReferences<WeaponData>("WeaponData", "Weapons");
    }

    [MenuItem("Tools/Excel Import/Individual/PlayerData")]
    private static void ImportPlayerData()
    {
        ImportOnly<PlayerData>("PlayerData", "Players");
    }

    [MenuItem("Tools/Excel Import/Individual/AIBehaviorData")]
    private static void ImportAIBehaviorData()
    {
        ImportOnly<AIBehaviorData>("AIBehaviorData", "AIBehaviors");
    }

    [MenuItem("Tools/Excel Import/Individual/AIData")]
    private static void ImportAIData()
    {
        ImportWithReferences<AIData>("AIData", "AIs");
    }

    [MenuItem("Tools/Excel Import/Individual/MatchDropData")]
    private static void ImportMatchDropData()
    {
        ImportWithReferences<MatchDropData>("MatchDropData", "MatchDrops");
    }

    [MenuItem("Tools/Excel Import/Individual/MatchData")]
    private static void ImportMatchData()
    {
        ImportWithReferences<MatchData>("MatchData", "Matches");
    }

    [MenuItem("Tools/Excel Import/Individual/ProjectileData")]
    private static void ImportProjectileData()
    {
        ImportOnly<ProjectileData>("ProjectileData", "Projectiles");
    }

    #endregion

    [MenuItem("Tools/Excel Import/1. Import All")]
    public static void ImportAll()
    {
        bool succeeded = ExecuteImportAll();

        SaveChanges();

        if (succeeded)
        {
            Debug.Log("=== 전체 변환(1-Pass) 완료 ===");
        }
        else
        {
            Debug.LogError("=== 전체 변환(1-Pass) 실패 ===");
        }
    }

    [MenuItem("Tools/Excel Import/2. Resolve All References")]
    public static void ResolveAll()
    {
        bool succeeded = ExecuteResolveAll();

        SaveChanges();

        if (succeeded)
        {
            Debug.Log("=== 전체 참조 연결(2-Pass) 완료 ===");
        }
        else
        {
            Debug.LogError("=== 전체 참조 연결(2-Pass) 실패 ===");
        }
    }

    [MenuItem("Tools/Excel Import/0. Import And Resolve (전체)")]
    public static void ImportAndResolveAll()
    {
        bool imported = ExecuteImportAll();

        //Import가 실패하면 Resolve는 실행하지 않음
        bool resolved = imported && ExecuteResolveAll();

        //전체 작업이 끝난 후 딱 한 번 저장
        SaveChanges();

        if (!imported)
        {
            Debug.LogError("=== Import 실패로 참조 연결을 실행하지 않았습니다 ===");

            return;
        }

        if (!resolved)
        {
            Debug.LogError("=== 참조 연결 실패 ===");
            return;
        }

        Debug.Log("=== 변환 + 참조 연결 전체 완료 ===");
    }

    private static bool ExecuteImportAll()
    {
        bool succeeded = true;

        succeeded &= DataImporter.Import<WeaponData>(
            $"{ExcelPath}/WeaponData.xlsx", $"{DataPath}/Weapons");

        succeeded &= DataImporter.Import<PlayerData>(
            $"{ExcelPath}/PlayerData.xlsx", $"{DataPath}/Players");

        succeeded &= DataImporter.Import<AIBehaviorData>(
            $"{ExcelPath}/AIBehaviorData.xlsx", $"{DataPath}/AIBehaviors");

        succeeded &= DataImporter.Import<AIData>(
            $"{ExcelPath}/AIData.xlsx", $"{DataPath}/AIs");

        succeeded &= DataImporter.Import<MatchDropData>(
            $"{ExcelPath}/MatchDropData.xlsx", $"{DataPath}/MatchDrops");

        succeeded &= DataImporter.Import<MatchData>(
            $"{ExcelPath}/MatchData.xlsx", $"{DataPath}/Matches");

        succeeded &= DataImporter.Import<ProjectileData>(
            $"{ExcelPath}/ProjectileData.xlsx", $"{DataPath}/Projectiles");

        return succeeded;
    }

    private static bool ExecuteResolveAll()
    {
        bool succeeded = true;

        succeeded &= DataImporter.ResolveReferences<WeaponData>(
            $"{ExcelPath}/WeaponData.xlsx", $"{DataPath}/Weapons");

        succeeded &= DataImporter.ResolveReferences<AIData>(
            $"{ExcelPath}/AIData.xlsx", $"{DataPath}/AIs");

        succeeded &= DataImporter.ResolveReferences<MatchDropData>(
            $"{ExcelPath}/MatchDropData.xlsx", $"{DataPath}/MatchDrops");

        succeeded &= DataImporter.ResolveReferences<MatchData>(
            $"{ExcelPath}/MatchData.xlsx", $"{DataPath}/Matches");

        return succeeded;
    }

    private static void ImportOnly<T>( string fileName, string outputFolder) where T : ScriptableObject
    {
        bool succeeded = DataImporter.Import<T>(
            $"{ExcelPath}/{fileName}.xlsx",
            $"{DataPath}/{outputFolder}");

        SaveChanges();

        if (!succeeded)
        {
            Debug.LogError($"{fileName} 개별 Import 실패");
        }
    }

    private static void ImportWithReferences<T>(string fileName, string outputFolder) where T : ScriptableObject
    {
        bool imported = DataImporter.Import<T>(
        $"{ExcelPath}/{fileName}.xlsx",
        $"{DataPath}/{outputFolder}");

        bool resolved = imported &&
            DataImporter.ResolveReferences<T>(
                $"{ExcelPath}/{fileName}.xlsx",
                $"{DataPath}/{outputFolder}");

        SaveChanges();

        if (!imported)
        {
            Debug.LogError($"{fileName} 개별 Import 실패");
            return;
        }

        if (!resolved)
        {
            Debug.LogError($"{fileName} 개별 참조 연결 실패");
        }
    }

    //저장
    private static void SaveChanges()
    {
        AssetDatabase.SaveAssets();
    }

}