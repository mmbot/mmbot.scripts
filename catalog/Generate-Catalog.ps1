#verify version
if ($PSVersionTable.psversion.Major -lt 3) {
    Write-Host "Powershell must be version 3 or higher" -ForegroundColor Red
    exit
}

# find roslyn libs
$roslynPath = $env:ProgramFiles
$roslyn = "Reference Assemblies\Microsoft\Roslyn\v1.2\Roslyn.Compilers.CSharp.dll"

$dll = Join-Path $roslynPath $roslyn

if(-not (Test-Path $dll)) {
    $roslynPath = ${env:ProgramFiles(x86)}
}

$roslynCompilerDLL = join-path $roslynPath "Reference Assemblies\Microsoft\Roslyn\v1.2\Roslyn.Compilers.dll"
$roslynCSHarpDLL   = join-path $roslynPath "Reference Assemblies\Microsoft\Roslyn\v1.2\Roslyn.Compilers.CSharp.dll"

if (-not (Test-Path $roslynCompilerDLL) -or -not (Test-Path $roslynCSHarpDLL)) {
    Write-Host "Roslyn 1.2 must be installed" -ForegroundColor Red
    exit
}

Add-Type -Path $roslynCompilerDLL
Add-Type -Path $roslynCSHarpDLL

$scriptMetadata = new-object system.collections.arraylist

$scripts = ls ..\scripts -Recurse |? {-not $_.PSIsContainer -and $_.Extension -eq ".csx"} |% {$_.FullName}

if ($scripts.Count -eq 0) {
    Write-Host "No scripts found, execute this script from the catalog directory and ensure the scripts folder exists" -ForegroundColor Red
    exit
}

$scripts |% {
    $scriptFile = $_
    try {
        #parse comments using roslyn
        $parseOptions = ([Roslyn.Compilers.CSharp.ParseOptions]::Default).WithParseDocumentationComments($true)
        $cancellationToken = [System.Threading.CancellationToken]::None
        $tree = [Roslyn.Compilers.CSharp.SyntaxTree]::ParseFile($scriptFile, $parseOptions, $cancellationToken)
        $trees = New-Object 'system.collections.generic.list[Roslyn.Compilers.CSharp.SyntaxTree]'
        $trees.Add($tree)
        [system.collections.generic.ienumerable[Roslyn.Compilers.CSharp.SyntaxTree]]$trees = $trees
        $compilation = [Roslyn.Compilers.CSharp.Compilation]::Create("comments", $null, $trees, $null, $null, $null)
        $classSymbol = $compilation.GlobalNamespace.GetMembers()
        $doc = $classSymbol[0].GetDocumentationComment($null, $cancellationToken)
        $comments = [xml]"<root>$($doc.FullXmlFragmentOpt)</root>"
        

        $metadata = "" | Select "name", "description", "configuration", "commands", "notes", "author"
        $metadata.name = [System.IO.Path]::GetFileNameWithoutExtension($scriptFile)
        $metadata.description = ($comments.root.description -replace "&lt;", "<" -replace "&gt;", ">" -split ";" |% {$_.Trim()} | Out-String) -join "`n"
        $metadata.configuration = ($comments.root.configuration -replace "&lt;", "<" -replace "&gt;", ">" -split ";" |% {$_.Trim()} | Out-String) -join "`n"
        $metadata.commands = ($comments.root.commands -replace "&lt;", "<" -replace "&gt;", ">" -split ";" |% {$_.Trim()} | Out-String) -join "`n"
        $metadata.notes = ($comments.root.notes -replace "&lt;", "<" -replace "&gt;", ">" -split ";" |% {$_.Trim()} | Out-String) -join "`n"
        $metadata.author = ($comments.root.author -replace "&lt;", "<" -replace "&gt;", ">" -split ";" |% {$_.Trim()} | Out-String) -join "`n"
        [void]$scriptMetadata.add($metadata)

    } catch {
        write-host "Failed to parse comments for $scriptFile - $($_.Exception.Message)" -for Red
    }
}

$scriptMetadata | ConvertTo-Json | out-file Catalog.json

$sb = New-Object 'System.Text.StringBuilder'

[void]$sb.AppendLine("# MMBot Scripts`n")
$scriptMetadata |% {
    [void]$sb.AppendLine("## $($_.name)")
    [void]$sb.AppendLine("`n### Description")
    [void]$sb.AppendLine("$($_.description)")
    [void]$sb.AppendLine("`n### Configuration")
    [void]$sb.AppendLine("$($_.configuration)")
    [void]$sb.AppendLine("`n### Commands")
    [void]$sb.AppendLine("$($_.commands)")
    [void]$sb.AppendLine("`n### Notes")
    [void]$sb.AppendLine("$($_.notes)")
    [void]$sb.AppendLine("`n### Author")
    [void]$sb.AppendLine("$($_.author)`n`n")
}

$sb.ToString() | out-file CATALOG.md