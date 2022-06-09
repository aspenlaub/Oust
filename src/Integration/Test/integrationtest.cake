var className = Argument("className", "OustWindowTest");

Task("RunTestsOnDebugArtifacts")
  .Description("Run unit tests on Debug artifacts")
  .Does(() => {
      var projectFiles = GetFiles("*Test.csproj");
      foreach(var projectFile in projectFiles) {
        Information("Running tests in " + projectFile.FullPath);
        var logFileName = @"./TestResults-Oust.trx";
        var dotNetTestSettings = new DotNetTestSettings {
          Configuration = "Debug", NoRestore = true, NoBuild = true,
          ArgumentCustomization = args => args.Append("--logger \"trx;LogFileName=" + logFileName + "\""),
          Filter = "ClassName=Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test." + className
        };
        DotNetTest(projectFile.FullPath, dotNetTestSettings);
    }
  });

RunTarget("RunTestsOnDebugArtifacts");