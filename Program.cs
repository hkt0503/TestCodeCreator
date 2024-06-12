using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCodeCreator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // MSBuildのインスタンスを取得
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            /*var instance = visualStudioInstances.Length == 1
                ? visualStudioInstances[0]
                : SelectVisualStudioInstance(visualStudioInstances);
            */

            var instance = visualStudioInstances[0];
            MSBuildLocator.RegisterInstance(instance);

            Console.Write("Please Enter of target solution path: ");
            var input = Console.ReadLine();

            try
            {
                // MSBuildWorkspaceを作成
                using (var workspace = MSBuildWorkspace.Create())
                {
                    var solutionPath = input;
                    var solution = workspace.OpenSolutionAsync(solutionPath).Result;
                    var projects = solution.Projects;
                    foreach (var project in projects)
                    {
                        //var compilation = await project.GetCompilationAsync();
                        //var mainMethodSymbol = compilation;
                        Console.WriteLine($"Project Name: '{project.Name}'");

                        //メモ：コンパイルしなくてもSyntaxTreeはドキュメント経由で全部とれる
                        
                        // プロジェクト内のすべてのドキュメント（ソースファイル）を取得
                        var documents = project.Documents;

                        foreach (var document in documents)
                        {

                            // ドキュメントのファイルパスを取得
                            var filePath = document.FilePath;
                            Console.WriteLine($"  Document: {filePath}");

                            // ソースコードを取得
                            var sourceCode = await document.GetTextAsync();
                            Console.WriteLine(sourceCode);

                            break;//1件目目のみ出力
                        }

                    }
                }
            }catch(NullReferenceException ex)
            {
                Console.WriteLine($"Solution Not Found: ' {ex.Message}'");
            }
            
        }


        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
