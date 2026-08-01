<#
.SYNOPSIS
  GitHub 레포지토리에 .github/labels.json 라벨을 자동으로 등록/동기화하는 스크립트
.EXAMPLE
  .\sync-labels.ps1 -GitHubToken "ghp_xxxxxxxxxxxx"
#>

param (
    [Parameter(Mandatory=$false)]
    [string]$GitHubToken,

    [string]$RepoOwner = "ESP32-RAIDAR-SENSOR",
    [string]$RepoName = "raidar_front",
    [string]$LabelsFilePath = ".github/labels.json"
)

if (-not (Test-Path $LabelsFilePath)) {
    Write-Error "라벨 파일이 존재하지 않습니다: $LabelsFilePath"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($GitHubToken)) {
    $GitHubToken = Read-Host "GitHub Personal Access Token (PAT)을 입력하세요"
}

$headers = @{
    "Authorization" = "Bearer $GitHubToken"
    "Accept"        = "application/vnd.github+json"
    "User-Agent"    = "LabelSyncScript"
}

$labelsJson = Get-Content -Path $LabelsFilePath -Raw -Encoding UTF8 | ConvertFrom-Json

Write-Host "📡 GitHub 레포지토리 ($RepoOwner/$RepoName) 라벨 동기화 시작..." -ForegroundColor Cyan

foreach ($label in $labelsJson) {
    $name = $label.name
    $color = $label.color.Replace("#", "")
    $description = $label.description

    $body = @{
        name        = $name
        color       = $color
        description = $description
    } | ConvertTo-Json

    $url = "https://api.github.com/repos/$RepoOwner/$RepoName/labels/$([Uri]::EscapeDataString($name))"

    try {
        # 라벨이 기존에 존재하는지 업데이트 (PATCH)
        $response = Invoke-RestMethod -Uri $url -Method Patch -Headers $headers -Body $body -ContentType "application/json" -ErrorAction Stop
        Write-Host "✅ [수정완료] 라벨: $name" -ForegroundColor Green
    }
    catch {
        # 없는 경우 새로 생성 (POST)
        $createUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/labels"
        try {
            $response = Invoke-RestMethod -Uri $createUrl -Method Post -Headers $headers -Body $body -ContentType "application/json" -ErrorAction Stop
            Write-Host "✨ [생성완료] 라벨: $name" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ [실패] 라벨 '$name': $_" -ForegroundColor Red
        }
    }
}

Write-Host "🎉 모든 라벨 동기화가 완료되었습니다!" -ForegroundColor Cyan
