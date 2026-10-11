using System;
using Microsoft.EntityFrameworkCore;
using EXAM.Data;
using Amazon.SQS;
using Amazon.S3;
using Amazon.Runtime;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core SQL Server
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(conn))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(conn, sqlOptions =>
        {
            // Enable transient error retry (recommended for RDS/network issues)
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            // Increase command timeout to avoid short timeouts on slow networks
            sqlOptions.CommandTimeout(60);
        })
    );
}

// AWS SQS
var awsAccessKey = builder.Configuration["AWS:AccessKeyId"];
var awsSecretKey = builder.Configuration["AWS:SecretAccessKey"];
var awsRegion = builder.Configuration["AWS:Region"] ?? "eu-central-1";

if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
{
    // Explicit credentials from configuration (suitable for local testing only). Do NOT commit real secrets.
    var creds = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
    var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
    builder.Services.AddSingleton<IAmazonSQS>(sp => new AmazonSQSClient(creds, regionEndpoint));
}
else
{
    // Use default AWS options (profile, environment vars, or instance profile)
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonSQS>();
}

// AWS S3 client registration (same credential strategy)
if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
{
    var creds = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
    var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
    builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(creds, regionEndpoint));
}
else
{
    // AddDefaultAWSOptions() already called above when needed
    builder.Services.AddAWSService<IAmazonS3>();
}

// SQS and S3 helper services
builder.Services.AddSingleton(sp => new EXAM.Services.SqsService(sp.GetRequiredService<IAmazonSQS>(), builder.Configuration["AWS:QueueUrl"] ?? string.Empty));
builder.Services.AddSingleton(sp => new EXAM.Services.S3Service(sp.GetRequiredService<IAmazonS3>(), builder.Configuration["AWS:S3Bucket"] ?? string.Empty, awsRegion));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
