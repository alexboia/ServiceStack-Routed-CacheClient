var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

Task("LVDRCC-Restore-Packages")
	.Does(() => 
	{
		DotNetCoreRestore("../LVD.ServiceStack.sln");
	});

Task("LVDRCC-Build")
	.IsDependentOn("LVDRCC-Restore-Packages")
	.Does(() => 
	{
		DotNetCoreBuild("./LVD.ServiceStack.RoutedCacheClient.csproj", (new DotNetCoreBuildSettings()
		{
			Configuration = configuration,
			ArgumentCustomization = args => args.Append("--no-restore"),
			NoRestore = true,
			DiagnosticOutput = true,
			Verbosity = DotNetCoreVerbosity.Minimal,
			MSBuildSettings = new DotNetCoreMSBuildSettings()
		}));
	});

Task("LVDRCC-Pack")
	.IsDependentOn("LVDRCC-Build")
	.Does(() => 
	{
		DeleteFiles("./*.nupkg");
		
		DotNetCorePack("./LVD.ServiceStack.RoutedCacheClient.csproj", new DotNetCorePackSettings() 
		{
			NoBuild = true,
			Configuration = configuration,
			OutputDirectory = "./"
		});
	});

Task("LVDRCC-Push")
	.IsDependentOn("LVDRCC-Pack")
	.Does(() => 
	{
		DotNetCoreNuGetPush("LVD.ServiceStack.RoutedCacheClient.*.nupkg", new DotNetCoreNuGetPushSettings()
		{
			Source = "https://api.nuget.org/v3/index.json",
			IgnoreSymbols = true,
			DiagnosticOutput = true
		});

		DeleteFiles("./*.nupkg");
	});

Task("Default")
	.IsDependentOn("LVDRCC-Pack");

RunTarget(target);