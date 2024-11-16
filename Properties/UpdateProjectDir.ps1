param(
[string]$ProjectDir
)
$path = $ProjectDir + '/Properties/Resources.resx'
$xml = [xml](Get-Content $path)
$node = $xml.root.data | where {$_.name -eq 'ProjectDir'}
$node.value = $ProjectDir
$xml.Save($path)