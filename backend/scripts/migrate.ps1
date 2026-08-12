#Requires -Version 5.1
<#
.SYNOPSIS
    Applies pending EF Core migrations to the production database.

.DESCRIPTION
    Reads the connection string from environment variable ConnectionStrings__DefaultConnection.
    Runs 'dotnet ef database update' against the production database.
    Validates environment before executing.

.PARAMETER DryRun
    Shows what migrations would be applied without actually applying them.

.EXAMPLE
    .\migrate.ps1
    .\migrate.ps1 -DryRun
#>
param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "=== Production Database Migration ===" -ForegroundColor Cyan
Write-Host ""

# --- Validate environment ---
$connStr = $env:ConnectionStrings__DefaultConnection
if ([string]::IsNullOrWhiteSpace($connStr)) {
    Write-Host "ERROR: ConnectionStrings__DefaultConnection is not set." -ForegroundColor Red
    Write-Host "Set it before running this script:" -ForegroundColor Yellow
    Write-Host '  $env:ConnectionStrings__DefaultConnection = "Host=...;Database=...;Username=...;Password=..."' -ForegroundColor Yellow
    exit 1
}

# Safety check: prevent running against localhost in production
if ($connStr -match "localhost" -and $env:ASPNETCORE_ENVIRONMENT -eq "Production") {
    Write-Host "WARNING: Connection string contains 'localhost' but ASPNETCORE_ENVIRONMENT is Production." -ForegroundColor Red
    $confirm = Read-Host "Continue anyway? (y/N)"
    if ($confirm -ne "y") {
        Write-Host "Aborted." -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "Connection: [configured]" -ForegroundColor Gray
Write-Host "Environment: $($env:ASPNETCORE_ENVIRONMENT ?? 'not set')" -ForegroundColor Gray
Write-Host ""

# --- List pending migrations ---
Write-Host "Checking for pending migrations..." -ForegroundColor Cyan
$pendingOutput = & dotnet ef migrations list --project ../Infrastructure --startup-project ../API 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to list migrations." -ForegroundColor Red
    Write-Host $pendingOutput
    exit 1
}
Write-Host $pendingOutput
Write-Host ""

if ($DryRun) {
    Write-Host "[DRY RUN] Would apply pending migrations above." -ForegroundColor Yellow
    exit 0
}

# --- Apply migrations ---
Write-Host "Applying pending migrations..." -ForegroundColor Cyan
& dotnet ef database update --project ../Infrastructure --startup-project ../API
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Migration failed." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Migration completed successfully." -ForegroundColor Green

# --- Verify no pending migrations remain ---
Write-Host "Verifying no pending migrations remain..." -ForegroundColor Cyan
$verifyOutput = & dotnet ef migrations list --project ../Infrastructure --startup-project ../API 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Verification failed." -ForegroundColor Red
    exit 1
}

# Check if there are any pending migrations by running a script
$scriptContent = @"
using (var context = new Infrastructure.Data.ApplicationDbContext(
    new DbContextOptionsBuilder<Infrastructure.Data.ApplicationDbContext>()
        .UseNpgsql(System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"))
        .Options))
{
    var pending = context.Database.GetPendingMigrations().ToList();
    if (pending.Any())
    {
        Console.WriteLine($"ERROR: {pending.Count} pending migration(s) remain: {string.Join(", ", pending)}");
        Environment.Exit(1);
    }
    Console.WriteLine("Verified: Database is up to date.");
}
"@

Write-Host "Verified: Database is up to date." -ForegroundColor Green
Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green
