# Перед первым запуском

Ставим scoop и jq через PowerShell

```
Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://get.scoop.sh')
scoop install jq
```

```
git config --unset core.hooksPath
git config core.hooksPath ./git-hooks
```