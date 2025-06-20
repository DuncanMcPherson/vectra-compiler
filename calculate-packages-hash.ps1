$projectFiles = Get-ChildItem -Recurse -Filter *.csproj
$packageReferences = @()

foreach ($file in $projectFiles) {
    [xml]$xml = Get-Content $file.FullName
    $refs = $xml.Project.ItemGroup.PackageReference
    foreach ($ref in $refs) {
        $line = "$($ref.Include):$($ref.Version)"
        $packageReferences += $line
    }
}

$packageReferences = $packageReferences | Sort-Object
$joined = [string]::Join("`n", $packageReferences)
$hash = [System.BitConverter]::ToString(
    (New-Object -TypeName System.Security.Cryptography.SHA256Managed).ComputeHash(
        [System.Text.Encoding]::UTF8.GetBytes($joined)
    )
) -replace "-", ""

Write-Output $hash