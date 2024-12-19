using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.IO.Compression;

// Prompt the user to enter a URL
Console.WriteLine("Please enter your Dataverse URL (ex: https://your-environment.crm.dynamics.com): ");
// Read the input from the console 
string url = Console.ReadLine();
// Prompt the user to enter a solution name
Console.WriteLine("Please enter your solution name: ");
// Read the input from the console 
string solutionUniqueName = Console.ReadLine();

// Dataverse connection string
string connectionString = $"AuthType=OAuth;Url={url};RedirectUri=http://localhost;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;LoginPrompt=Once";  // Microsoft Entra ID app registration shared by all Power App samples. Replace AppId with your own

// Connect to Dataverse
using (ServiceClient serviceClient = new ServiceClient(connectionString))
{
    if (!serviceClient.IsReady)
    {
        Console.WriteLine($"Failed to connect: {serviceClient.LastError}");
        return;
    }

    Console.WriteLine("Connected to Dataverse.");

    try
    {
        // Export solution request
        var exportRequest = new ExportSolutionRequest
        {
            SolutionName = solutionUniqueName,
            Managed = false // Set to true for managed solutions
        };

        // Execute the request
        Console.WriteLine($"Exporting solution: {solutionUniqueName}");
        var exportResponse = (ExportSolutionResponse)serviceClient.Execute(exportRequest);

        string tempPath = @"c:\temp";

        // Save the ZIP file
        string zipFilePath = Path.Combine(tempPath, $"{solutionUniqueName}.zip");
        File.WriteAllBytes(zipFilePath, exportResponse.ExportSolutionFile);
        Console.WriteLine($"Solution exported to: {zipFilePath}");

        string extractPath = Path.Combine(tempPath, solutionUniqueName);

        // Delete existing msappExtractPath if it exists
        if (Directory.Exists(extractPath))
        {
            Console.WriteLine($"Cleaning up existing directory: {extractPath}");
            Directory.Delete(extractPath, true);
            Console.WriteLine("Existing directory deleted.");
        }

        // Extract the ZIP file
        ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
        Console.WriteLine($"Solution extracted to: {extractPath}");

        // Locate and extract *.msapp files
        string canvasAppsPath = Path.Combine(extractPath, "CanvasApps");
        if (Directory.Exists(canvasAppsPath))
        {
            Console.WriteLine("Processing *.msapp files in CanvasApps directory...");
            var msappFiles = Directory.GetFiles(canvasAppsPath, "*.msapp", SearchOption.TopDirectoryOnly);

            foreach (var msappFile in msappFiles)
            {
                Console.WriteLine($"Found msapp file: {msappFile}");
                string msappExtractPath = Path.Combine(extractPath, "CanvasApps", Path.GetFileNameWithoutExtension(msappFile));

                // Ensure the target directory exists
                Directory.CreateDirectory(msappExtractPath);

                // Extract the msapp file
                ZipFile.ExtractToDirectory(msappFile, msappExtractPath, true);
                Console.WriteLine($"Extracted msapp file to: {msappExtractPath}");

                // Navigate to the Src folder and list files
                string srcFolderPath = Path.Combine(msappExtractPath, "Src");
                if (Directory.Exists(srcFolderPath))
                {
                    Console.WriteLine($"Listing YAML in Src folder of {Path.GetFileNameWithoutExtension(msappFile)}:");
                    var srcFiles = Directory.GetFiles(srcFolderPath, "*", SearchOption.AllDirectories);
                    foreach (var srcFile in srcFiles)
                    {
                        Console.WriteLine($" - {srcFile}");
                    }
                }
                else
                {
                    Console.WriteLine($"Src folder not found in {msappExtractPath}.");
                }
            }
        }
        else
        {
            Console.WriteLine("No CanvasApps directory found in the extracted solution.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}