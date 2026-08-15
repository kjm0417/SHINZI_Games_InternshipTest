using UnityEditor;
using UnityEngine;

public static class DataImporterMenu
{
    // 경로 상수로 정리 (중복 줄이고 한 곳에서 관리)
    private const string ExcelPath = "Assets/Editor/ExcelData";
    private const string DataPath = "Assets/GameData";

    //========== 1-Pass: 변환 (참조 없는 것부터, 순서 무관하지만 논리적으로 배치) ==========

    [MenuItem("Tools/Excel Import/1. Import All")]
    public static void ImportAll()
    {
        DataImporter.Import<WeaponData>(
            $"{ExcelPath}/WeaponData.xlsx", $"{DataPath}/Weapons");

        DataImporter.Import<PlayerData>(
            $"{ExcelPath}/PlayerData.xlsx", $"{DataPath}/Players");

        DataImporter.Import<AIBehaviorData>(
            $"{ExcelPath}/AIBehaviorData.xlsx", $"{DataPath}/AIBehaviors");

        DataImporter.Import<AIData>(
            $"{ExcelPath}/AIData.xlsx", $"{DataPath}/AIs");

        DataImporter.Import<MatchDropData>(
            $"{ExcelPath}/MatchDropData.xlsx", $"{DataPath}/MatchDrops");


        DataImporter.Import<MatchData>(
            $"{ExcelPath}/MatchData.xlsx", $"{DataPath}/Matches");

        DataImporter.Import<ProjectileData>(
            $"{ExcelPath}/ProjectileData.xlsx", $"{DataPath}/Projectiles");

        Debug.Log("=== 전체 변환(1-Pass) 완료 ===");
    }

    //========== 2-Pass: 참조 연결 (참조 있는 테이블만) ==========

    [MenuItem("Tools/Excel Import/2. Resolve All References")]
    public static void ResolveAll()
    {
        DataImporter.ResolveReferences<WeaponData>(
            $"{ExcelPath}/WeaponData.xlsx", $"{DataPath}/Weapons");

        DataImporter.ResolveReferences<AIData>(
            $"{ExcelPath}/AIData.xlsx", $"{DataPath}/AIs");

        DataImporter.ResolveReferences<MatchDropData>(
            $"{ExcelPath}/MatchDropData.xlsx", $"{DataPath}/MatchDrops");

        DataImporter.ResolveReferences<MatchData>(
            $"{ExcelPath}/MatchData.xlsx", $"{DataPath}/Matches");  


        Debug.Log("=== 전체 참조 연결(2-Pass) 완료 ===");
    }

    // ========== 전체 실행 (1-Pass → 2-Pass 한 번에) ==========

    [MenuItem("Tools/Excel Import/0. Import And Resolve (전체)")]
    public static void ImportAndResolveAll()
    {
        ImportAll();
        ResolveAll();
        Debug.Log("=== 변환 + 참조 연결 전체 완료 ===");
    }
}