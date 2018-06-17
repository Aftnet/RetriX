function ReplaceInFile([string]$Path) {
    [xml]$xmlContent = Get-Content -Path "$Path"
    $ns = new-object Xml.XmlNamespaceManager $xmlContent.NameTable
    $ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")
    $filter = "LibRetriX*"
    $nodes = $xmlContent.SelectNodes("//msb:PackageReference", $ns) | Where-Object {$_.Include -like $filter }
    $nodes += $xmlContent.SelectNodes("//PackageReference") | Where-Object {$_.Include -like $filter }
    foreach($i in $nodes) {
        $i.Version = "0.1.0"
    }

    $xmlContent.Save($Path)
}

ReplaceInFile -Path ".\RetriX.Shared\RetriX.Shared.csproj"
ReplaceInFile -Path ".\RetriX.UWP\RetriX.UWP.csproj"