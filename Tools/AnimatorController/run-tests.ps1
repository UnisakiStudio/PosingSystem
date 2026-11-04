param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-zA-Z0-9_-]+$')][string]$Run,
    [string]$Filter = 'PosingSystemControllerCompilerTests'
)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$testOutput = Join-Path $taskRoot 'Recordings/AnimatorCompiler_20261027'
$resultPath = Join-Path $testOutput ($Run + '.xml')
$logPath = Join-Path $testOutput ($Run + '.log')
if ((Test-Path -LiteralPath $resultPath) -or (Test-Path -LiteralPath $logPath)) { throw 'Run output already exists.' }
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Unity is already running; inspect its project before starting tests.' }
# Do not let Unity's startup cleanup traverse a saved junction into a source repository.
# Enumerate explicitly without descending into any reparse point.
& 'C:/Users/gaoth/AppData/Local/Programs/Python/Python310/python.exe' (Join-Path $PSScriptRoot 'check_unity_cache_links.py')
if ($LASTEXITCODE -ne 0) { throw 'Unity cache link verification failed.' }
New-Item -ItemType Directory -Path $testOutput -Force | Out-Null
$unityArgs = @('-batchmode','-nographics','-projectPath',('"' + $taskRoot + '"'),
    '-runTests','-testPlatform','EditMode','-testFilter',$Filter,
    '-testResults',('"' + $resultPath + '"'),'-logFile',('"' + $logPath + '"'))
$unityTask = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor/Unity.exe' `
    -ArgumentList $unityArgs -WindowStyle Hidden -PassThru
$unityTask.Id | Set-Content -LiteralPath (Join-Path $testOutput ($Run + '-pid.txt'))
[pscustomobject]@{pid=$unityTask.Id;testResults=$resultPath;log=$logPath} | ConvertTo-Json -Compress
