param(
    [Parameter(Position = 0)]
    [string]$Command
)

# =========================
# CONFIG
# =========================
$basePath = (Get-Location).Path
$migrationProjectName = "Whitebird.Migrations"
$migrationProject = "$basePath\src\$migrationProjectName"
$webProjectPath = "$basePath\src\Whitebird"
$processor = "SqlServer2016"

# =========================
# CONNECTION STRING
# =========================
function GetConnectionString {
    $appSettingsPath = "$webProjectPath\appsettings.json"
    
    if (-not (Test-Path $appSettingsPath)) {
        throw "❌ appsettings.json tidak ditemukan di $appSettingsPath"
    }
    
    $json = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $conn = $json.ConnectionStrings.DefaultConnection
    
    if (-not $conn) {
        throw "❌ Tidak ditemukan ConnectionStrings:DefaultConnection di appsettings.json"
    }
    
    Write-Host "🔗 Connection string loaded from appsettings.json" -ForegroundColor Green
    return $conn
}

# =========================
# MIGRATION ASSEMBLY
# =========================
function GetMigrationAssembly {
    $asm = Join-Path $migrationProject "bin\Debug\net8.0\$migrationProjectName.dll"
    
    if (-not (Test-Path $asm)) {
        throw "❌ Migration assembly tidak ditemukan: $asm. Build project $migrationProjectName terlebih dahulu."
    }
    
    return (Resolve-Path $asm)
}

# =========================
# BUILD MIGRATION DLL
# =========================
function Build-MigrationDLL {
    Write-Host "🛠️ Building migration project..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "$migrationProject\bin" -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force "$migrationProject\obj" -ErrorAction SilentlyContinue

    dotnet build "$migrationProject\$migrationProjectName.csproj" -c Debug | Out-Null

    $dllPath = GetMigrationAssembly
    Write-Host "✅ Build completed: $dllPath" -ForegroundColor Green
    return $dllPath
}

# =========================
# SHOW MIGRATIONS
# =========================
function Show-Migration {
    Write-Host "📜 Listing available migrations..." -ForegroundColor Cyan
    $dllPath = Build-MigrationDLL
    $conn = GetConnectionString

    $cmd = "dotnet-fm list migrations -p $processor -c `"$conn`" -a `"$dllPath`""
    Write-Host "Executing: $cmd" -ForegroundColor Gray
    Invoke-Expression $cmd
}

# =========================
# ADD MIGRATION
# =========================
function Add-Migration {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Features,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $false)]
        [string]$Template
    )

    $timestamp = Get-Date -Format "yyyyMMddHHmmss"
    $featureDir = "$migrationProject\Features\$Features"
    
    if (-not (Test-Path $featureDir)) {
        New-Item -ItemType Directory -Path $featureDir -Force | Out-Null
        Write-Host "📁 Created folder: $featureDir" -ForegroundColor Green
    }

    $migrationFile = "$featureDir\${timestamp}_${Name}.cs"

    if ($Template -eq "CRUD") {
        $templateContent = @"
using FluentMigrator;

namespace $migrationProjectName.Features.$Features
{
    [Migration($timestamp)]
    public class $Name : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='${Features}' AND xtype='U')
                CREATE TABLE [dbo].[${Features}] (
                    [Id] INT IDENTITY(1,1) PRIMARY KEY,
                    [Name] NVARCHAR(255) NOT NULL,
                    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
                    [ModifiedDate] DATETIME NULL,
                    [CreatedBy] NVARCHAR(100) NULL,
                    [ModifiedBy] NVARCHAR(100) NULL
                );
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='${Features}' AND xtype='U')
                DROP TABLE [dbo].[${Features}];
            ");
        }
    }
}
"@
    } else {
        $templateContent = @"
using FluentMigrator;

namespace $migrationProjectName.Features.$Features
{
    [Migration($timestamp)]
    public class $Name : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- TULIS SQL MIGRASI DI SINI
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                -- TULIS SQL ROLLBACK DI SINI
            ");
        }
    }
}
"@
    }

    Set-Content -Path $migrationFile -Value $templateContent -Encoding UTF8
    Write-Host "✅ Migration created: $migrationFile" -ForegroundColor Green
    if ($Template) { Write-Host "🧩 Template used: $Template" -ForegroundColor Cyan }
}

# =========================
# DEPLOY MIGRATION
# =========================
function Deploy-Migration {
    Write-Host "🚀 Deploying migrations..." -ForegroundColor Cyan
    $dllPath = Build-MigrationDLL
    $conn = GetConnectionString

    $cmd = "dotnet-fm migrate -p $processor -c `"$conn`" -a `"$dllPath`" up"
    Write-Host "Executing: $cmd" -ForegroundColor Gray
    Invoke-Expression $cmd
}

# =========================
# UNDO MIGRATION
# =========================
function Undo-Migration {
    param (
        [int] $Step = 1,
        [string] $Target,
        [switch] $All,
        [string[]] $Tags
    )

    $dllPath = Build-MigrationDLL
    $conn = GetConnectionString

    $tagArgs = ""
    foreach ($tag in $Tags) {
        $tagArgs += " --tag $tag"
    }

    if ($All) {
        Write-Host "⚠️  WARNING: You are about to rollback ALL migrations!" -ForegroundColor Red
        $confirm = Read-Host "Are you sure? Type 'YES' to confirm"
        if ($confirm -ne "YES") {
            Write-Host "❌ Rollback cancelled." -ForegroundColor Red
            return
        }
        
        Write-Host "🔄 Rolling back ALL migrations..." -ForegroundColor Yellow
        $cmd = "dotnet-fm rollback -p $processor -c `"$conn`" -a `"$dllPath`" $tagArgs all"
        Write-Host "Executing: $cmd" -ForegroundColor Gray
        Invoke-Expression $cmd
    }
    elseif ($Target) {
        Write-Host "↩️ Rolling back to version $Target..." -ForegroundColor Yellow
        $cmd = "dotnet-fm rollback -p $processor -c `"$conn`" -a `"$dllPath`" $tagArgs to $Target"
        Write-Host "Executing: $cmd" -ForegroundColor Gray
        Invoke-Expression $cmd
    }
    else {
        Write-Host "↩️ Rolling back $Step step(s)..." -ForegroundColor Yellow
        $cmd = "dotnet-fm rollback -p $processor -c `"$conn`" -a `"$dllPath`" $tagArgs by $Step"
        Write-Host "Executing: $cmd" -ForegroundColor Gray
        Invoke-Expression $cmd
    }
}

# =========================
# ROUTING COMMAND
# =========================
switch ($Command) {
    "Show-Migration" { Show-Migration }
    "Add-Migration" { 
        $features = $null
        $name = $null
        $template = $null
        
        for ($i = 0; $i -lt $args.Count; $i++) {
            switch ($args[$i]) {
                "-Features" { $features = $args[++$i] }
                "-Name" { $name = $args[++$i] }
                "-Template" { $template = $args[++$i] }
            }
        }
        
        if (-not $features -or -not $name) {
            Write-Host "❌ Error: -Features and -Name are required" -ForegroundColor Red
            Write-Host "Usage: Add-Migration -Features <Folder> -Name <Name> [-Template CRUD]" -ForegroundColor Yellow
        } else {
            Add-Migration -Features $features -Name $name -Template $template
        }
    }
    "Deploy-Migration" { Deploy-Migration }
    "Undo-Migration" { 
        $stepValue = 1
        $allFlag = $false
        $targetValue = $null
        $tagsValue = @()
        
        for ($i = 0; $i -lt $args.Count; $i++) {
            switch ($args[$i]) {
                "-Step" { 
                    $i++
                    $stepValue = [int]$args[$i]
                }
                "-All" { $allFlag = $true }
                "-Target" {
                    $i++
                    $targetValue = $args[$i]
                }
                "-Tags" {
                    $i++
                    while ($i -lt $args.Count -and $args[$i] -notlike "-*") {
                        $tagsValue += $args[$i]
                        $i++
                    }
                    $i--
                }
            }
        }
        
        if ($allFlag) {
            Undo-Migration -All
        }
        elseif ($targetValue) {
            Undo-Migration -Target $targetValue -Tags $tagsValue
        }
        else {
            Undo-Migration -Step $stepValue -Tags $tagsValue
        }
    }
    default {
        Write-Host "🧩 FluentMigrator Helper Script" -ForegroundColor Cyan
        Write-Host "=================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Usage:" -ForegroundColor Yellow
        Write-Host "  .\fm.ps1 Show-Migration"
        Write-Host "  .\fm.ps1 Add-Migration -Features <Folder> -Name <Name> [-Template CRUD]"
        Write-Host "  .\fm.ps1 Deploy-Migration"
        Write-Host "  .\fm.ps1 Undo-Migration [-Step <n>]"
        Write-Host "  .\fm.ps1 Undo-Migration -All"
        Write-Host "  .\fm.ps1 Undo-Migration -Target <version>"
        Write-Host "  .\fm.ps1 Undo-Migration -Step <n> -Tags <tag1> <tag2>"
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Green
        Write-Host "  .\fm.ps1 Add-Migration -Features Assets -Name CreateAssetsTable -Template CRUD"
        Write-Host "  .\fm.ps1 Undo-Migration -Step 3"
        Write-Host "  .\fm.ps1 Undo-Migration -All"
    }
}