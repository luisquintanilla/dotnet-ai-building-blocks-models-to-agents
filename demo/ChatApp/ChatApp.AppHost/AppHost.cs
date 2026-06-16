var builder = DistributedApplication.CreateBuilder(args);

// Azure OpenAI (Foundry), keyless. Set the endpoint-only connection string as a user secret on
// this AppHost project, which tells the client integration to authenticate with managed identity
// (DefaultAzureCredential) -- no API key is stored anywhere:
//   cd this-project-directory
//   dotnet user-secrets set ConnectionStrings:openai "https://<your-resource>.openai.azure.com/"
// Sign in first with `az login`. Your identity needs the "Cognitive Services OpenAI User" role on
// the resource. See demo/ChatApp/README.md.
var openai = builder.AddConnectionString("openai");

var markitdown = builder.AddContainer("markitdown", "mcp/markitdown")
    .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001")
    .WithHttpEndpoint(targetPort: 3001, name: "http");

var webApp = builder.AddProject<Projects.ChatApp_Web>("aichatweb-app");
webApp.WithReference(openai);
webApp
    .WithEnvironment("MARKITDOWN_MCP_URL", markitdown.GetEndpoint("http"));

builder.Build().Run();
