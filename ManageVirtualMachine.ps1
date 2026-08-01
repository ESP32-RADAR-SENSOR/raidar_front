# =========================================================
# Script: ManageVirtualMachine.ps1
# Description: Hyper-V 가상 머신(VM) 생성 및 관리 스크립트
# =========================================================

param(
    [string]$VMName = "Test-VM-01",
    [int64]$RAMMB = 2048,
    [int64]$VHDSizeGB = 20,
    [string]$SwitchName = "Default Switch"
)

# 관리자 권한 체크
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "이 스크립트는 PowerShell 관리자 권한으로 실행해야 합니다."
    exit
}

$VHDPath = "C:\Hyper-V\Virtual Hard Disks\$VMName.vhdx"

# 1. 가상 하드 디스크(VHDX) 및 VM 생성
if (-not (Get-VM -Name $VMName -ErrorAction SilentlyContinue)) {
    Write-Host "[+] 가상 머신 '$VMName' 생성 중..." -ForegroundColor Green
    
    # VM 생성
    New-VM -Name $VMName -MemoryStartupBytes ($RAMMB * 1MB) -NewVHDPath $VHDPath -NewVHDSizeBytes ($VHDSizeGB * 1GB) -SwitchName $SwitchName
    
    # Dynamic Memory 설정
    Set-VMMemory -VMName $VMName -DynamicMemoryEnabled $true -MinimumBytes (1024MB) -MaximumBytes ($RAMMB * 1MB)
    
    Write-Host "[+] VM '$VMName' 생성 완료." -ForegroundColor Green
} else {
    Write-Host "[!] 이미 존재하는 VM입니다: $VMName" -ForegroundColor Yellow
}

# 2. VM 상태 확인 및 시작
$vm = Get-VM -Name $VMName
Write-Host "[*] 현재 VM 상태: $($vm.State)"

if ($vm.State -eq "Off") {
    Write-Host "[+] VM 구동 시작..." -ForegroundColor Cyan
    Start-VM -Name $VMName
}
