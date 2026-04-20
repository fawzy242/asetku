# generate-migrations-fixed.ps1
# Script yang sudah diperbaiki untuk deteksi success/failure

param(
    [int]$DelaySeconds = 2
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  WHITEBIRD - GENERATE ALL MIGRATIONS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Load fm.ps1
$fmPath = "scripts\fm.ps1"
if (-not (Test-Path $fmPath)) {
    Write-Host "❌ ERROR: fm.ps1 not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Loading fm.ps1..." -ForegroundColor Cyan
. $fmPath
Write-Host "✅ fm.ps1 loaded successfully" -ForegroundColor Green
Write-Host ""

# Function to create migration (fixed)
function Create-Migration {
    param($Feature, $Name, $UseTemplate = $false)
    
    Write-Host "  → Creating $Name..." -ForegroundColor Gray
    
    # Clear any previous errors
    $Error.Clear()
    
    # Run the migration command
    if ($UseTemplate) {
        Add-Migration -Features $Feature -Name $Name -Template CRUD
    } else {
        Add-Migration -Features $Feature -Name $Name
    }
    
    # Check if migration file was created
    $migrationPath = "E:\Product\Whitebird\src\Whitebird.Migrations\Features\$Feature"
    $files = Get-ChildItem -Path $migrationPath -Filter "*${Name}.cs" -ErrorAction SilentlyContinue
    
    if ($files.Count -gt 0) {
        Write-Host "  ✅ $Name created successfully: $($files[0].Name)" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  ❌ Failed to create $Name" -ForegroundColor Red
        return $false
    }
}

# Pause function
function Pause-Migration {
    if ($DelaySeconds -gt 0) {
        Write-Host "  ⏱️  Waiting $DelaySeconds seconds..." -ForegroundColor DarkGray
        Start-Sleep -Seconds $DelaySeconds
    }
}

# Define all migrations
$migrations = @()

# Sequences
$sequences = @(
    @{Feature="Asset"; Name="CreateAssetSeq"}
    @{Feature="AssetTransaction"; Name="CreateAssetTransactionSeq"}
    @{Feature="Employee"; Name="CreateEmployeeSeq"}
    @{Feature="Supplier"; Name="CreateSupplierSeq"}
    @{Feature="Category"; Name="CreateCategorySeq"}
    @{Feature="Users"; Name="CreateUsersSeq"}
    @{Feature="Location"; Name="CreateLocationSeq"}
    @{Feature="ActivityLog"; Name="CreateActivityLogSeq"}
)

foreach ($seq in $sequences) {
    $migrations += @{
        Feature = $seq.Feature
        Name = $seq.Name
        UseTemplate = $false
    }
}

# Tables (with Template CRUD)
$tables = @(
    "Category", "Supplier", "Employee", "Location", 
    "Asset", "AssetTransaction", "Users", "ActivityLog"
)

foreach ($table in $tables) {
    $migrations += @{
        Feature = $table
        Name = "Create${table}Table"
        UseTemplate = $true
    }
}

# Triggers
$triggers = @(
    @{Feature="Asset"; Name="CreateAssetTrig"}
    @{Feature="AssetTransaction"; Name="CreateAssetTransactionTrig"}
    @{Feature="Employee"; Name="CreateEmployeeTrig"}
    @{Feature="Supplier"; Name="CreateSupplierTrig"}
    @{Feature="Category"; Name="CreateCategoryTrig"}
    @{Feature="Users"; Name="CreateUsersTrig"}
    @{Feature="Location"; Name="CreateLocationTrig"}
    @{Feature="ActivityLog"; Name="CreateActivityLogTrig"}
)

foreach ($trig in $triggers) {
    $migrations += @{
        Feature = $trig.Feature
        Name = $trig.Name
        UseTemplate = $false
    }
}

# Seed Users only
$migrations += @{
    Feature = "Users"
    Name = "SeedUsers"
    UseTemplate = $false
}

# Execute migrations
$counter = 1
$total = $migrations.Count
$failed = @()

foreach ($m in $migrations) {
    Write-Host "[$counter/$total] Creating $($m.Name)..." -ForegroundColor Yellow
    
    $success = Create-Migration -Feature $m.Feature -Name $m.Name -UseTemplate $m.UseTemplate
    
    if (-not $success) {
        $failed += $m.Name
    }
    
    if ($counter -lt $total) {
        Pause-Migration
    }
    
    $counter++
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MIGRATION SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total migrations: $total" -ForegroundColor White

if ($failed.Count -eq 0) {
    Write-Host "✅ All migrations created successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Failed migrations: $($failed.Count)" -ForegroundColor Yellow
    foreach ($f in $failed) {
        Write-Host "  ❌ $f" -ForegroundColor Red
    }
}

Write-Host "========================================" -ForegroundColor Cyan