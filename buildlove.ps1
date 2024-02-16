param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [Parameter()]
    [switch] $Clean = $false
)

Write-Host -ForegroundColor green "Building LÖVE2D for $Configuration..."

$ErrorActionPreference="Stop"

$OutputPath = "$PSScriptRoot/Thirdparty/megasource/build/love/$Configuration"

Push-Location $PSScriptRoot/Thirdparty/megasource/
if ($Clean -or !(Test-Path $OutputPath)) {
    Remove-Item -Recurse -Force ./build
}
if (!(Test-Path ./build)) {
    cmake -S. -Bbuild -DSDL_CMAKE_DEBUG_POSTFIX=""
    if ($LASTEXITCODE -ne 0) { throw "Exit code is $LASTEXITCODE" }
}
cmake --build build --parallel 8 --target love/love --config $Configuration
if ($LASTEXITCODE -ne 0) { throw "Exit code is $LASTEXITCODE" }
Pop-Location

$LibPath = Join-Path "$($Env:Temp)/Love2dCS" $(New-Guid)
if (!(Test-Path $LibPath)) {
    New-Item -ItemType Directory -Path $LibPath
}
Get-ChildItem -Path $PSScriptRoot/Thirdparty/megasource/build/love/$Configuration -Recurse -Include *.dll,*.pdb | Copy-Item -Destination $LibPath

Compress-Archive -DestinationPath $PSScriptRoot/Thirdparty/Love2dCS/native_lib/native_lib_win_x64.zip -Path $LibPath/* -Force
Remove-Item -Recurse -Force $LibPath
