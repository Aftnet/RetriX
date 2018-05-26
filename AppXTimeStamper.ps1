$appXFile = Get-ChildItem -Filter "*.appxbundle" | Select-Object -First 1;
$appXPath = $appXFile.FullName;
&"C:\Program Files (x86)\Windows Kits\10\bin\x86\signtool.exe" sign /fd SHA256 /a /n "Alberto Fustinoni" /tr http://tsa.startssl.com/rfc3161 "$appXPath";