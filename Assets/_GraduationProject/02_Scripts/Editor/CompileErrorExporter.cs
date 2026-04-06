using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.IO;
using System.Text;
using System.Linq;

[InitializeOnLoad]
public class CompileErrorExporter
{
    private static string OutputPath = "compile_errors.txt";

    static CompileErrorExporter()
    {
        CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
    }

    private static void OnCompilationFinished(string assemblyPath, CompilerMessage[] messages)
    {
        // 에러만 필터링 (Warning 제외)
        var errors = messages.Where(m => m.type == CompilerMessageType.Error).ToArray();
        StringBuilder sb = new StringBuilder();

        if (errors.Length > 0)
        {
            sb.AppendLine($"[Compile Errors] Timestamp: {System.DateTime.Now}");
            foreach (var error in errors)
            {
                // 포맷: 파일경로(줄,열): error 메시지
                sb.AppendLine($"{error.file}({error.line},{error.column}): error {error.message}");
            }
        }
        else
        {
            sb.AppendLine("No Errors");
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), OutputPath);
        
        try
        {
            // 파일 쓰기 (기존 내용 덮어쓰기)
            File.WriteAllText(fullPath, sb.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[CompileErrorExporter] Failed to write error log: {e.Message}");
        }
    }
}
