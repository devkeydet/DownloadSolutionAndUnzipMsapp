using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DownloadSolutionAndUnzipMsapp
{
    internal class CreateTempSolutionAddCanvasExportDelete
    {
        //NOTE: This code isn't fully working, but the should be enough to get you started
        internal void Example()
        {
            // Step 1: Connect to Dataverse
            string connectionString = "your_connection_string_here";
            using (var serviceClient = new ServiceClient(connectionString))
            {
                if (!serviceClient.IsReady)
                {
                    Console.WriteLine("Failed to connect to Dataverse.");
                    return;
                }

                try
                {
                    // Step 2: Create a temporary solution
                    var solution = new Entity("solution")
                    {
                        ["uniquename"] = "TempSolution",
                        ["friendlyname"] = "Temporary Solution",
                        ["publisherid"] = new EntityReference("publisher", new Guid("your_publisher_guid")),
                        ["description"] = "A temporary solution for exporting purposes",
                        ["version"] = "1.0.0.0"
                    };
                    Guid solutionId = serviceClient.Create(solution);
                    Console.WriteLine($"Temporary solution created with ID: {solutionId}");

                    // Step 3: Add the canvas app to the solution
                    Guid canvasAppId = new Guid("your_canvas_app_guid"); // Replace with your Canvas App ID
                    var addComponentRequest = new AddSolutionComponentRequest
                    {
                        ComponentType = 300, // Canvas App component type
                        ComponentId = canvasAppId,
                        SolutionUniqueName = "TempSolution",
                    };
                    serviceClient.Execute(addComponentRequest);
                    Console.WriteLine("Canvas app added to the solution.");

                    // Step 4: Export the solution
                    var exportRequest = new ExportSolutionRequest
                    {
                        SolutionName = "TempSolution",
                        Managed = false
                    };
                    var exportResponse = (ExportSolutionResponse)serviceClient.Execute(exportRequest);
                    byte[] solutionZip = exportResponse.ExportSolutionFile;

                    // Save the exported solution as a ZIP file
                    string filePath = Path.Combine(Environment.CurrentDirectory, "TempSolution.zip");
                    File.WriteAllBytes(filePath, solutionZip);
                    Console.WriteLine($"Solution exported to {filePath}");

                    // Step 5: Delete the temporary solution
                    var deleteRequest = new DeleteRequest
                    {
                        Target = new EntityReference("solution", solutionId)
                    };
                    serviceClient.Execute(deleteRequest);
                    Console.WriteLine("Temporary solution deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
