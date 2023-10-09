# psake
# Copyright (c) 2012 James Kovacs
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

#Requires -Version 2.0

#-- Public Module Functions --#

# .ExternalHelp  psake.psm1-help.xml
function Invoke-Task
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)] [string]$taskName
    )

    Assert $taskName ($msgs.error_invalid_task_name)

    $taskKey = $taskName.ToLower()

    if ($currentContext.aliases.Contains($taskKey)) {
        $taskName = $currentContext.aliases.$taskKey.Name
        $taskKey = $taskName.ToLower()
    }

    $currentContext = $psake.context.Peek()

    Assert ($currentContext.tasks.Contains($taskKey)) ($msgs.error_task_name_does_not_exist -f $taskName)

    if ($currentContext.executedTasks.Contains($taskKey))  { return }

    Assert (!$currentContext.callStack.Contains($taskKey)) ($msgs.error_circular_reference -f $taskName)

    $currentContext.callStack.Push($taskKey)

    $task = $currentContext.tasks.$taskKey

    $precondition_is_valid = & $task.Precondition

    if (!$precondition_is_valid) {
        WriteColoredOutput ($msgs.precondition_was_false -f $taskName) -foregroundcolor Cyan
    } else {
        if ($taskKey -ne 'default') {

            if ($task.PreAction -or $task.PostAction) {
                Assert ($task.Action -ne $null) ($msgs.error_missing_action_parameter -f $taskName)
            }

            if ($task.Action) {
                try {
                    foreach($childTask in $task.DependsOn) {
                        Invoke-Task $childTask
                    }

                    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                    $currentContext.currentTaskName = $taskName

                    & $currentContext.taskSetupScriptBlock

                    if ($task.PreAction) {
                        & $task.PreAction
                    }

                    if ($currentContext.config.taskNameFormat -is [ScriptBlock]) {
                        & $currentContext.config.taskNameFormat $taskName
                    } else {
                        WriteColoredOutput ($currentContext.config.taskNameFormat -f $taskName) -foregroundcolor Cyan
                    }

                    foreach ($variable in $task.requiredVariables) {
                        Assert ((test-path "variable:$variable") -and ((get-variable $variable).Value -ne $null)) ($msgs.required_variable_not_set -f $variable, $taskName)
                    }

                    & $task.Action

                    if ($task.PostAction) {
                        & $task.PostAction
                    }

                    & $currentContext.taskTearDownScriptBlock
                    $task.Duration = $stopwatch.Elapsed
                } catch {
                    if ($task.ContinueOnError) {
                        "-"*70
                        WriteColoredOutput ($msgs.continue_on_error -f $taskName,$_) -foregroundcolor Yellow
                        "-"*70
                        $task.Duration = $stopwatch.Elapsed
                    }  else {
                        throw $_
                    }
                }
            } else {
                # no action was specified but we still execute all the dependencies
                foreach($childTask in $task.DependsOn) {
                    Invoke-Task $childTask
                }
            }
        } else {
            foreach($childTask in $task.DependsOn) {
                Invoke-Task $childTask
            }
        }

        Assert (& $task.Postcondition) ($msgs.postcondition_failed -f $taskName)
    }

    $poppedTaskKey = $currentContext.callStack.Pop()
    Assert ($poppedTaskKey -eq $taskKey) ($msgs.error_corrupt_callstack -f $taskKey,$poppedTaskKey)

    $currentContext.executedTasks.Push($taskKey)
}

# .ExternalHelp  psake.psm1-help.xml
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd),
        [Parameter(Position=2,Mandatory=0)][int]$maxRetries = 0,
        [Parameter(Position=3,Mandatory=0)][string]$retryTriggerErrorPattern = $null
    )

    $tryCount = 1

    do {
        try {
            $global:lastexitcode = 0
            & $cmd
            if ($lastexitcode -ne 0) {
                throw ("Exec: " + $errorMessage)
            }
            break
        }
        catch [Exception]
        {
            if ($tryCount -gt $maxRetries) {
                throw $_
            }

            if ($retryTriggerErrorPattern -ne $null) {
                $isMatch = [regex]::IsMatch($_.Exception.Message, $retryTriggerErrorPattern)

                if ($isMatch -eq $false) {
                    throw $_
                }
            }

            Write-Host "Try $tryCount failed, retrying again in 1 second..."

            $tryCount++

            [System.Threading.Thread]::Sleep([System.TimeSpan]::FromSeconds(1))
        }
    }
    while ($true)
}

# .ExternalHelp  psake.psm1-help.xml
function Assert
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)]$conditionToCheck,
        [Parameter(Position=1,Mandatory=1)]$failureMessage
    )
    if (!$conditionToCheck) {
        throw ("Assert: " + $failureMessage)
    }
}

# .ExternalHelp  psake.psm1-help.xml
function Task
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$name = $null,
        [Parameter(Position=1,Mandatory=0)][scriptblock]$action = $null,
        [Parameter(Position=2,Mandatory=0)][scriptblock]$preaction = $null,
        [Parameter(Position=3,Mandatory=0)][scriptblock]$postaction = $null,
        [Parameter(Position=4,Mandatory=0)][scriptblock]$precondition = {$true},
        [Parameter(Position=5,Mandatory=0)][scriptblock]$postcondition = {$true},
        [Parameter(Position=6,Mandatory=0)][switch]$continueOnError = $false,
        [Parameter(Position=7,Mandatory=0)][string[]]$depends = @(),
        [Parameter(Position=8,Mandatory=0)][string[]]$requiredVariables = @(),
        [Parameter(Position=9,Mandatory=0)][string]$description = $null,
        [Parameter(Position=10,Mandatory=0)][string]$alias = $null,
        [Parameter(Position=11,Mandatory=0)][string]$maxRetries = 0,
        [Parameter(Position=12,Mandatory=0)][string]$retryTriggerErrorPattern = $null
    )
    if ($name -eq 'default') {
        Assert (!$action) ($msgs.error_default_task_cannot_have_action)
    }

    $newTask = @{
        Name = $name
        DependsOn = $depends
        PreAction = $preaction
        Action = $action
        PostAction = $postaction
        Precondition = $precondition
        Postcondition = $postcondition
        ContinueOnError = $continueOnError
        Description = $description
        Duration = [System.TimeSpan]::Zero
        RequiredVariables = $requiredVariables
        Alias = $alias
        MaxRetries = $maxRetries
        RetryTriggerErrorPattern = $retryTriggerErrorPattern
    }

    $taskKey = $name.ToLower()

    $currentContext = $psake.context.Peek()

    Assert (!$currentContext.tasks.ContainsKey($taskKey)) ($msgs.error_duplicate_task_name -f $name)

    $currentContext.tasks.$taskKey = $newTask

    if($alias)
    {
        $aliasKey = $alias.ToLower()

        Assert (!$currentContext.aliases.ContainsKey($aliasKey)) ($msgs.error_duplicate_alias_name -f $alias)

        $currentContext.aliases.$aliasKey = $newTask
    }
}

# .ExternalHelp  psake.psm1-help.xml
function Properties {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$properties
    )
    $psake.context.Peek().properties += $properties
}

# .ExternalHelp  psake.psm1-help.xml
function Include {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$fileNamePathToInclude
    )
    Assert (test-path $fileNamePathToInclude -pathType Leaf) ($msgs.error_invalid_include_path -f $fileNamePathToInclude)
    $psake.context.Peek().includes.Enqueue((Resolve-Path $fileNamePathToInclude));
}

# .ExternalHelp  psake.psm1-help.xml
function FormatTaskName {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)]$format
    )
    $psake.context.Peek().config.taskNameFormat = $format
}

# .ExternalHelp  psake.psm1-help.xml
function TaskSetup {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$setup
    )
    $psake.context.Peek().taskSetupScriptBlock = $setup
}

# .ExternalHelp  psake.psm1-help.xml
function TaskTearDown {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$teardown
    )
    $psake.context.Peek().taskTearDownScriptBlock = $teardown
}

# .ExternalHelp  psake.psm1-help.xml
function Framework {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$framework
    )
    $psake.context.Peek().config.framework = $framework
    ConfigureBuildEnvironment
}

# .ExternalHelp  psake.psm1-help.xml
function Invoke-psake {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = 0)][string] $buildFile,
        [Parameter(Position = 1, Mandatory = 0)][string[]] $taskList = @(),
        [Parameter(Position = 2, Mandatory = 0)][string] $framework,
        [Parameter(Position = 3, Mandatory = 0)][switch] $docs = $false,
        [Parameter(Position = 4, Mandatory = 0)][hashtable] $parameters = @{},
        [Parameter(Position = 5, Mandatory = 0)][hashtable] $properties = @{},
        [Parameter(Position = 6, Mandatory = 0)][alias("init")][scriptblock] $initialization = {},
        [Parameter(Position = 7, Mandatory = 0)][switch] $nologo = $false,
        [Parameter(Position = 8, Mandatory = 0)][switch] $detailedDocs = $false
    )
    try {
        if (-not $nologo) {
            "psake version {0}`nCopyright (c) 2010-2015 James Kovacs, Damian Hickey & Contributors`n" -f $psake.version
        }

        if (!$buildFile) {
          $buildFile = $psake.config_default.buildFileName
        }
        elseif (!(test-path $buildFile -pathType Leaf) -and (test-path $psake.config_default.buildFileName -pathType Leaf)) {
            # If the $config.buildFileName file exists and the given "buildfile" isn 't found assume that the given
            # $buildFile is actually the target Tasks to execute in the $config.buildFileName script.
            $taskList = $buildFile.Split(', ')
            $buildFile = $psake.config_default.buildFileName
        }

        # Execute the build file to set up the tasks and defaults
        Assert (test-path $buildFile -pathType Leaf) ($msgs.error_build_file_not_found -f $buildFile)

        $psake.build_script_file = get-item $buildFile
        $psake.build_script_dir = $psake.build_script_file.DirectoryName
        $psake.build_success = $false

        $psake.context.push(@{
            "taskSetupScriptBlock" = {};
            "taskTearDownScriptBlock" = {};
            "executedTasks" = new-object System.Collections.Stack;
            "callStack" = new-object System.Collections.Stack;
            "originalEnvPath" = $env:path;
            "originalDirectory" = get-location;
            "originalErrorActionPreference" = $global:ErrorActionPreference;
            "tasks" = @{};
            "aliases" = @{};
            "properties" = @();
            "includes" = new-object System.Collections.Queue;
            "config" = CreateConfigurationForNewContext $buildFile $framework
        })

        LoadConfiguration $psake.build_script_dir

        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

        set-location $psake.build_script_dir

        LoadModules

        $frameworkOldValue = $framework
        . $psake.build_script_file.FullName

        $currentContext = $psake.context.Peek()

        if ($framework -ne $frameworkOldValue) {
            writecoloredoutput $msgs.warning_deprecated_framework_variable -foregroundcolor Yellow
            $currentContext.config.framework = $framework
        }

        ConfigureBuildEnvironment

        while ($currentContext.includes.Count -gt 0) {
            $includeFilename = $currentContext.includes.Dequeue()
            . $includeFilename
        }

        if ($docs -or $detailedDocs) {
            WriteDocumentation($detailedDocs)
            CleanupEnvironment
            return
        }

        foreach ($key in $parameters.keys) {
            if (test-path "variable:\$key") {
                set-item -path "variable:\$key" -value $parameters.$key -WhatIf:$false -Confirm:$false | out-null
            } else {
                new-item -path "variable:\$key" -value $parameters.$key -WhatIf:$false -Confirm:$false | out-null
            }
        }

        # The initial dot (.) indicates that variables initialized/modified in the propertyBlock are available in the parent scope.
        foreach ($propertyBlock in $currentContext.properties) {
            . $propertyBlock
        }

        foreach ($key in $properties.keys) {
            if (test-path "variable:\$key") {
                set-item -path "variable:\$key" -value $properties.$key -WhatIf:$false -Confirm:$false | out-null
            }
        }

        # Simple dot sourcing will not work. We have to force the script block into our
        # module's scope in order to initialize variables properly.
        . $MyInvocation.MyCommand.Module $initialization

        # Execute the list of tasks or the default task
        if ($taskList) {
            foreach ($task in $taskList) {
                invoke-task $task
            }
        } elseif ($currentContext.tasks.default) {
            invoke-task default
        } else {
            throw $msgs.error_no_default_task
        }

        WriteColoredOutput ("`n" + $msgs.build_success + "`n") -foregroundcolor Green

        WriteTaskTimeSummary $stopwatch.Elapsed

        $psake.build_success = $true
    } catch {
        $currentConfig = GetCurrentConfigurationOrDefault
        if ($currentConfig.verboseError) {
            $error_message = "{0}: An Error Occurred. See Error Details Below: `n" -f (Get-Date)
            $error_message += ("-" * 70) + "`n"
            $error_message += "Error: {0}`n" -f (ResolveError $_ -Short)
            $error_message += ("-" * 70) + "`n"
            $error_message += ResolveError $_
            $error_message += ("-" * 70) + "`n"
            $error_message += "Script Variables" + "`n"
            $error_message += ("-" * 70) + "`n"
            $error_message += get-variable -scope script | format-table | out-string
        } else {
            # ($_ | Out-String) gets error messages with source information included.
            $error_message = "Error: {0}: `n{1}" -f (Get-Date), (ResolveError $_ -Short)
        }

        $psake.build_success = $false

        # if we are running in a nested scope (i.e. running a psake script from a psake script) then we need to re-throw the exception
        # so that the parent script will fail otherwise the parent script will report a successful build
        $inNestedScope = ($psake.context.count -gt 1)
        if ( $inNestedScope ) {
            throw $_
        } else {
            if (!$psake.run_by_psake_build_tester) {
                WriteColoredOutput $error_message -foregroundcolor Red
            }
        }
    } finally {
        CleanupEnvironment
    }
}

#-- Private Module Functions --#
function WriteColoredOutput {
    param(
        [string] $message,
        [System.ConsoleColor] $foregroundcolor
    )

    $currentConfig = GetCurrentConfigurationOrDefault
    if ($currentConfig.coloredOutput -eq $true) {
        if (($Host.UI -ne $null) -and ($Host.UI.RawUI -ne $null) -and ($Host.UI.RawUI.ForegroundColor -ne $null)) {
            $previousColor = $Host.UI.RawUI.ForegroundColor
            $Host.UI.RawUI.ForegroundColor = $foregroundcolor
        }
    }

    $message

    if ($previousColor -ne $null) {
        $Host.UI.RawUI.ForegroundColor = $previousColor
    }
}

function LoadModules {
    $currentConfig = $psake.context.peek().config
    if ($currentConfig.modules) {

        $scope = $currentConfig.moduleScope

        $global = [string]::Equals($scope, "global", [StringComparison]::CurrentCultureIgnoreCase)

        $currentConfig.modules | foreach {
            resolve-path $_ | foreach {
                "Loading module: $_"
                $module = import-module $_ -passthru -DisableNameChecking -global:$global
                if (!$module) {
                    throw ($msgs.error_loading_module -f $_.Name)
                }
            }
        }
        ""
    }
}

function LoadConfiguration {
    param(
        [string] $configdir = $PSScriptRoot
    )

    $psakeConfigFilePath = (join-path $configdir "psake-config.ps1")

    if (test-path $psakeConfigFilePath -pathType Leaf) {
        try {
            $config = GetCurrentConfigurationOrDefault
            . $psakeConfigFilePath
        } catch {
            throw "Error Loading Configuration from psake-config.ps1: " + $_
        }
    }
}

function GetCurrentConfigurationOrDefault() {
    if ($psake.context.count -gt 0) {
        return $psake.context.peek().config
    } else {
        return $psake.config_default
    }
}

function CreateConfigurationForNewContext {
    param(
        [string] $buildFile,
        [string] $framework
    )

    $previousConfig = GetCurrentConfigurationOrDefault

    $config = new-object psobject -property @{
        buildFileName = $previousConfig.buildFileName;
        framework = $previousConfig.framework;
        taskNameFormat = $previousConfig.taskNameFormat;
        verboseError = $previousConfig.verboseError;
        coloredOutput = $previousConfig.coloredOutput;
        modules = $previousConfig.modules;
        moduleScope =  $previousConfig.moduleScope;
    }

    if ($framework) {
        $config.framework = $framework;
    }

    if ($buildFile) {
        $config.buildFileName = $buildFile;
    }

    return $config
}

function ConfigureBuildEnvironment {
    $framework = $psake.context.peek().config.framework
    if ($framework -cmatch '^((?:\d+\.\d+)(?:\.\d+){0,1})(x86|x64){0,1}$') {
        $versionPart = $matches[1]
        $bitnessPart = $matches[2]
    } else {
        throw ($msgs.error_invalid_framework -f $framework)
    }
    $versions = $null
    $buildToolsVersions = $null
    switch ($versionPart) {
        '1.0' {
            $versions = @('v1.0.3705')
        }
        '1.1' {
            $versions = @('v1.1.4322')
        }
        '2.0' {
            $versions = @('v2.0.50727')
        }
        '3.0' {
            $versions = @('v2.0.50727')
        }
        '3.5' {
            $versions = @('v3.5', 'v2.0.50727')
        }
        '4.0' {
            $versions = @('v4.0.30319')
        }
        {($_ -eq '4.5.1') -or ($_ -eq '4.5.2')} {
            $versions = @('v4.0.30319')
            $buildToolsVersions = @('14.0', '12.0')
        }
        '4.6' {
            $versions = @('v4.0.30319')
            $buildToolsVersions = @('14.0')
        }

        default {
            throw ($msgs.error_unknown_framework -f $versionPart, $framework)
        }
    }

    $bitness = 'Framework'
    if ($versionPart -ne '1.0' -and $versionPart -ne '1.1') {
        switch ($bitnessPart) {
            'x86' {
                $bitness = 'Framework'
                $buildToolsKey = 'MSBuildToolsPath32'
            }
            'x64' {
                $bitness = 'Framework64'
                $buildToolsKey = 'MSBuildToolsPath'
            }
            { [string]::IsNullOrEmpty($_) } {
                $ptrSize = [System.IntPtr]::Size
                switch ($ptrSize) {
                    4 {
                        $bitness = 'Framework'
                        $buildToolsKey = 'MSBuildToolsPath32'
                    }
                    8 {
                        $bitness = 'Framework64'
                        $buildToolsKey = 'MSBuildToolsPath'
                    }
                    default {
                        throw ($msgs.error_unknown_pointersize -f $ptrSize)
                    }
                }
            }
            default {
                throw ($msgs.error_unknown_bitnesspart -f $bitnessPart, $framework)
            }
        }
    }
    $frameworkDirs = @()
    if ($buildToolsVersions -ne $null) {
        foreach($ver in $buildToolsVersions) {
            if (Test-Path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\$ver") {
                $frameworkDirs += (Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\$ver" -Name $buildToolsKey).$buildToolsKey
            }
        }
    }
    $frameworkDirs = $frameworkDirs + @($versions | foreach { "$env:windir\Microsoft.NET\$bitness\$_\" })

    for ($i = 0; $i -lt $frameworkDirs.Count; $i++) {
        $dir = $frameworkDirs[$i]
        if ($dir -Match "\$\(Registry:HKEY_LOCAL_MACHINE(.*?)@(.*)\)") {
            $key = "HKLM:" + $matches[1]
            $name = $matches[2]
            $dir = (Get-ItemProperty -Path $key -Name $name).$name
            $frameworkDirs[$i] = $dir
        }
    }

    $frameworkDirs | foreach { Assert (test-path $_ -pathType Container) ($msgs.error_no_framework_install_dir_found -f $_)}

    $env:path = ($frameworkDirs -join ";") + ";$env:path"
    # if any error occurs in a PS function then "stop" processing immediately
    # this does not effect any external programs that return a non-zero exit code
    $global:ErrorActionPreference = "Stop"
}

function CleanupEnvironment {
    if ($psake.context.Count -gt 0) {
        $currentContext = $psake.context.Peek()
        $env:path = $currentContext.originalEnvPath
        Set-Location $currentContext.originalDirectory
        $global:ErrorActionPreference = $currentContext.originalErrorActionPreference
        [void] $psake.context.Pop()
    }
}

function SelectObjectWithDefault
{
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)]
        [PSObject]
        $InputObject,
        [string]
        $Name,
        $Value
    )

    process {
        if ($_ -eq $null) { $Value }
        elseif ($_ | Get-Member -Name $Name) {
          $_.$Name
        }
        elseif (($_ -is [Hashtable]) -and ($_.Keys -contains $Name)) {
          $_.$Name
        }
        else { $Value }
    }
}

# borrowed from Jeffrey Snover http://blogs.msdn.com/powershell/archive/2006/12/07/resolve-error.aspx
# modified to better handle SQL errors
function ResolveError
{
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)]
        $ErrorRecord=$Error[0],
        [Switch]
        $Short
    )

    process {
        if ($_ -eq $null) { $_ = $ErrorRecord }
        $ex = $_.Exception

        if (-not $Short) {
            $error_message = "`nErrorRecord:{0}ErrorRecord.InvocationInfo:{1}Exception:`n{2}"
            $formatted_errorRecord = $_ | format-list * -force | out-string
            $formatted_invocationInfo = $_.InvocationInfo | format-list * -force | out-string
            $formatted_exception = ''

            $i = 0
            while ($ex -ne $null) {
                $i++
                $formatted_exception += ("$i" * 70) + "`n" +
                    ($ex | format-list * -force | out-string) + "`n"
                $ex = $ex | SelectObjectWithDefault -Name 'InnerException' -Value $null
            }

            return $error_message -f $formatted_errorRecord, $formatted_invocationInfo, $formatted_exception
        }

        $lastException = @()
        while ($ex -ne $null) {
            $lastMessage = $ex | SelectObjectWithDefault -Name 'Message' -Value ''
            $lastException += ($lastMessage -replace "`n", '')
            if ($ex -is [Data.SqlClient.SqlException]) {
                $lastException += "(Line [$($ex.LineNumber)] " +
                    "Procedure [$($ex.Procedure)] Class [$($ex.Class)] " +
                    " Number [$($ex.Number)] State [$($ex.State)] )"
            }
            $ex = $ex | SelectObjectWithDefault -Name 'InnerException' -Value $null
        }
        $shortException = $lastException -join ' --> '

        $header = $null
        $current = $_
        $header = (($_.InvocationInfo |
            SelectObjectWithDefault -Name 'PositionMessage' -Value '') -replace "`n", ' '),
            ($_ | SelectObjectWithDefault -Name 'Message' -Value ''),
            ($_ | SelectObjectWithDefault -Name 'Exception' -Value '') |
                ? { -not [String]::IsNullOrEmpty($_) } |
                Select -First 1

        $delimiter = ''
        if ((-not [String]::IsNullOrEmpty($header)) -and
            (-not [String]::IsNullOrEmpty($shortException)))
            { $delimiter = ' [<<==>>] ' }

        return "$($header)$($delimiter)Exception: $($shortException)"
    }
}

function WriteDocumentation($showDetailed) {
    $currentContext = $psake.context.Peek()

    if ($currentContext.tasks.default) {
        $defaultTaskDependencies = $currentContext.tasks.default.DependsOn
    } else {
        $defaultTaskDependencies = @()
    }

    $docs = $currentContext.tasks.Keys | foreach-object {
        if ($_ -eq "default") {
            return
        }

        $task = $currentContext.tasks.$_
        new-object PSObject -property @{
            Name = $task.Name;
            Alias = $task.Alias;
            Description = $task.Description;
            "Depends On" = $task.DependsOn -join ", "
            Default = if ($defaultTaskDependencies -contains $task.Name) { $true }
        }
    }
    if ($showDetailed) {
        $docs | sort 'Name' | format-list -property Name,Alias,Description,"Depends On",Default
    } else {
        $docs | sort 'Name' | format-table -autoSize -wrap -property Name,Alias,"Depends On",Default,Description
    }

}

function WriteTaskTimeSummary($invokePsakeDuration) {
    if ($psake.context.count -gt 0) {
        "-" * 70
        "Build Time Report"
        "-" * 70
        $list = @()
        $currentContext = $psake.context.Peek()
        while ($currentContext.executedTasks.Count -gt 0) {
            $taskKey = $currentContext.executedTasks.Pop()
            $task = $currentContext.tasks.$taskKey
            if ($taskKey -eq "default") {
                continue
            }
            $list += new-object PSObject -property @{
                Name = $task.Name;
                Duration = $task.Duration
            }
        }
        [Array]::Reverse($list)
        $list += new-object PSObject -property @{
            Name = "Total:";
            Duration = $invokePsakeDuration
        }
        # using "out-string | where-object" to filter out the blank line that format-table prepends
        $list | format-table -autoSize -property Name,Duration | out-string -stream | where-object { $_ }
    }
}

DATA msgs {
convertfrom-stringdata @'
    error_invalid_task_name = Task name should not be null or empty string.
    error_task_name_does_not_exist = Task {0} does not exist.
    error_circular_reference = Circular reference found for task {0}.
    error_missing_action_parameter = Action parameter must be specified when using PreAction or PostAction parameters for task {0}.
    error_corrupt_callstack = Call stack was corrupt. Expected {0}, but got {1}.
    error_invalid_framework = Invalid .NET Framework version, {0} specified.
    error_unknown_framework = Unknown .NET Framework version, {0} specified in {1}.
    error_unknown_pointersize = Unknown pointer size ({0}) returned from System.IntPtr.
    error_unknown_bitnesspart = Unknown .NET Framework bitness, {0}, specified in {1}.
    error_no_framework_install_dir_found = No .NET Framework installation directory found at {0}.
    error_bad_command = Error executing command {0}.
    error_default_task_cannot_have_action = 'default' task cannot specify an action.
    error_duplicate_task_name = Task {0} has already been defined.
    error_duplicate_alias_name = Alias {0} has already been defined.
    error_invalid_include_path = Unable to include {0}. File not found.
    error_build_file_not_found = Could not find the build file {0}.
    error_no_default_task = 'default' task required.
    error_loading_module = Error loading module {0}.
    warning_deprecated_framework_variable = Warning: Using global variable $framework to set .NET framework version used is deprecated. Instead use Framework function or configuration file psake-config.ps1.
    required_variable_not_set = Variable {0} must be set to run task {1}.
    postcondition_failed = Postcondition failed for task {0}.
    precondition_was_false = Precondition was false, not executing task {0}.
    continue_on_error = Error in task {0}. {1}
    build_success = Build Succeeded!
'@
}

import-localizeddata -bindingvariable msgs -erroraction silentlycontinue

$script:psake = @{}
$psake.version = "4.4.2" # contains the current version of psake
$psake.context = new-object system.collections.stack # holds onto the current state of all variables
$psake.run_by_psake_build_tester = $false # indicates that build is being run by psake-BuildTester
$psake.config_default = new-object psobject -property @{
    buildFileName = "default.ps1";
    framework = "4.0";
    taskNameFormat = "Executing {0}";
    verboseError = $false;
    coloredOutput = $true;
    modules = $null;
    moduleScope = "";
} # contains default configuration, can be overriden in psake-config.ps1 in directory with psake.psm1 or in directory with current build script

$psake.build_success = $false # indicates that the current build was successful
$psake.build_script_file = $null # contains a System.IO.FileInfo for the current build script
$psake.build_script_dir = "" # contains a string with fully-qualified path to current build script

LoadConfiguration

export-modulemember -function Invoke-psake, Invoke-Task, Task, Properties, Include, FormatTaskName, TaskSetup, TaskTearDown, Framework, Assert, Exec -variable psake

# SIG # Begin signature block
# MIIvGwYJKoZIhvcNAQcCoIIvDDCCLwgCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCDcKRc7cZuIvtRz
# 0+gOJXxQx793aKbAb73cwcpnKHh2BaCCE98wggVkMIIDTKADAgECAhAGzuExvm1V
# yAf3wMf7ROYgMA0GCSqGSIb3DQEBDAUAMEwxCzAJBgNVBAYTAlVTMRcwFQYDVQQK
# Ew5EaWdpQ2VydCwgSW5jLjEkMCIGA1UEAxMbRGlnaUNlcnQgQ1MgUlNBNDA5NiBS
# b290IEc1MB4XDTIxMDExNTAwMDAwMFoXDTQ2MDExNDIzNTk1OVowTDELMAkGA1UE
# BhMCVVMxFzAVBgNVBAoTDkRpZ2lDZXJ0LCBJbmMuMSQwIgYDVQQDExtEaWdpQ2Vy
# dCBDUyBSU0E0MDk2IFJvb3QgRzUwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIK
# AoICAQC2M3OA2GIDcBQsERw5XnyufIOGHf4mL0wkrYvqg1+pvD1b/AuYTAJHMOzi
# /uzoNFtmXr871yymJf+MWbPf6tp8KdlGUHIIHW7RGwrdH82ZifoPD3PE4ZwddTLN
# b5faKmqVsmzJCdDqC3t9FwZJme/W3uDIU9SuxnfxhrsjHLjA31n3jn3R74LmJota
# OLX/ddWy2U8J8zeIUNoRpIoUFNFTBAB982pEGP5QcDIHHKiaDjodxQofbgsmabc8
# oldwLIb6TG6VqVhDuawS1v8/7ddDF2tMzp7EkKv/+hBQmqOQV9bnjBCunxYazzUd
# f9d27YqcNacouKddIfwwN93eCBlPFcbnptqQR473lFNMjlMCvv2Z5eqG0K8DAtOb
# qpPxqyiOIAH/TPvMtylA9YekEhMFH0Nu11FQnzi0IO0XCRKPzLkZr5/NvmkR069V
# EG0XhnmWUsayAJ3lrziwNfSIa48OBD187q/N02oQSsbNhsoiPaFKXPsO/4jfXGKn
# wLke2axsfjg3/neTJcKFik+1NwZaBoEU8c6UnZmR6jJazmc9bgRmrQxPLaMu9571
# eJ33Cv1+j+NCilWWvPGfNy38nl+V/owYG/yO/UuQr9cDaBJjrOKTp6LLBOVPZM4D
# +sYUn9mL6MzUYoxr5AAsGZ8aBsYxgVT7UySar1WZup11rrjC3QIDAQABo0IwQDAd
# BgNVHQ4EFgQUaAGTsdJKQEJplEYsHFqIqSW0R08wDgYDVR0PAQH/BAQDAgGGMA8G
# A1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEMBQADggIBAJL87rgCeRcCUX0hxUln
# p6TxqCQ46wxo6lpCa5z0c8FpSi2zNwVQQpiSngZ5LC4Gmfbv3yugzbOSAYO1oMsn
# tTwjGphJouwtmaVZQ6zSsZPWV9ccvJPWxkDhs28ZVbcT1+VDM6S1q8vawTFkDXTW
# LO3DjW7ru68ZR2FhLcD0BblveNw690JAZVORvZkNk5JUpqk3WSuby5nGvD33BITw
# lDMdD4JaOcsuRcMoGaOym5jI/DFrYI/26YYovOA8fXRdFolbaSTHEIvES7s2T9RZ
# P8OwpJGZ+C7RSgGd9YgS779aEWpZT1lrWmfzj7QTD8DYLz0ocqoZfxF9alufledf
# t5RP8T6hWv8tzJ3fJ3ePMnMcZwp28/pcsb+8Hb0MKJuyxxdnCzMPw7023Pu6Qgur
# 7YTDYtaEFqmxB2upbu7Gz+awRCnC8LNhgCqLb9IUXCWHVGTzpEzBofina+r+6jr8
# edsOj9zG88nUbN7pg6GOHSLsyTqyAHvcO6dCGn/ci6kRPY6nwCBvXQldQ0Tmj2bM
# qVsH8e+beg6zVOGU/Q4sxpPXVf1xmDW4CUr/xikoLPZSLdsUGJIn4hZ+jMrUYb6C
# h5HrmDc/v19ddz80rBs4Q6tocpkyHjoaGaWjOEwj16PnzNUqkheQC1pLvRa9+4Zq
# 4omZ7OSgVRjJowgfE+AyCHLQMIIGkDCCBHigAwIBAgIQCt4y6VCbRKo0sdrxvA7I
# czANBgkqhkiG9w0BAQsFADBMMQswCQYDVQQGEwJVUzEXMBUGA1UEChMORGlnaUNl
# cnQsIEluYy4xJDAiBgNVBAMTG0RpZ2lDZXJ0IENTIFJTQTQwOTYgUm9vdCBHNTAe
# Fw0yMTA3MTUwMDAwMDBaFw0zMTA3MTQyMzU5NTlaMFsxCzAJBgNVBAYTAlVTMRgw
# FgYDVQQKEw8uTkVUIEZvdW5kYXRpb24xMjAwBgNVBAMTKS5ORVQgRm91bmRhdGlv
# biBQcm9qZWN0cyBDb2RlIFNpZ25pbmcgQ0EyMIICIjANBgkqhkiG9w0BAQEFAAOC
# Ag8AMIICCgKCAgEAznkpxwyD+QKdO6sSwPBJD5wtJL9ncyIlhHFM8rWvU5lCCoMp
# 9TcAiXvCIIaJCeOxjXFv0maraGhSr8SANVefC74HBfDTAl/JyoWmOfBxRY/30/0q
# ivfUtoxrw91SR3Gu3eucWxxb4b+hoIpTgbKU+//cnSvi8EmBTk7ntfFkAWw/6Lov
# +nMXU+qEzm/TuCT8qWX2IffLkdXIt4UqQS8Jqjxn7cGLhjqDA9w+5zXpSxSu/JhK
# OecY05XcdGlGnQBPc8RBzUD3ZzXMPoPBSFiH7UZs23iVmVXCJoU9IFaN3WSLD/jZ
# 3TXE8RxJxoY1DODwr4t6kTSQdDPrx3aPrtAcJFblh3JMP0SpZZpV8DHALVZkKKfF
# u2SOL9Wv57MJ6M/mhfyUot2vLVxVlWlplgwOhcHP7a40cVBczF/cAb+IBz+tuB1q
# wGGi4B3qnE2kpYju6xYz75hVcfFqXGmy3+NMZIF6oMJUSLUZmU7HUDCUyMgHt6SP
# 42r7vzRyPJEMXARiNwe5jI6oAWxyeX6dN4ZXiBDa1lVaVuK8yUd7ShbETPbTPaZ5
# BaV/yxcl1rqExPqKzIH+y/a6F33KXSYVGTSFcg/tSEd4vuXbBUuIf2UpPVkK+J2/
# 0J/o8sBSkF3nFZ/USwrvcMKEiINKokHvmivypLkhSfMIEismXSO6rke8ElECAwEA
# AaOCAV0wggFZMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFCgOTIkcmZfx
# gfCPCN5XEku8uHjPMB8GA1UdIwQYMBaAFGgBk7HSSkBCaZRGLBxaiKkltEdPMA4G
# A1UdDwEB/wQEAwIBhjATBgNVHSUEDDAKBggrBgEFBQcDAzB5BggrBgEFBQcBAQRt
# MGswJAYIKwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBDBggrBgEF
# BQcwAoY3aHR0cDovL2NhY2VydHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0Q1NSU0E0
# MDk2Um9vdEc1LmNydDBFBgNVHR8EPjA8MDqgOKA2hjRodHRwOi8vY3JsMy5kaWdp
# Y2VydC5jb20vRGlnaUNlcnRDU1JTQTQwOTZSb290RzUuY3JsMBwGA1UdIAQVMBMw
# BwYFZ4EMAQMwCAYGZ4EMAQQBMA0GCSqGSIb3DQEBCwUAA4ICAQA66iJQhuBqWcn+
# rukrkzv8/2CGpyWTqoospvt+Nr2zsgl1xX97v588lngs94uqlTB90YLR4227PZQz
# HQAFEs0K0/UNUfoKPC9DZSn4xJxjPsLaK8N+l4d6oZBAb7AXglpxfk4ocrzM0KWY
# Tnaz3+lt0uGi8VgP8Wggh24FLxzxxiC5SqwZ7wfU7w1g7YugDn6xbcH4+yd6ZpCn
# MDcxYkGKqOtjM7V3bd3Rbl/PDySZ+chy/n6Q6ZNj9Oz/WFYlYOId7CMmI5SzbMWp
# qbdwvPNkrSYwFRtnRV9rwJo/q9jctfwrG9FQBkHMXiLHRQPw4oEoROk0AYCm26zv
# dEqc1pPCIEQHtXyOW+GqX2MORRdiePFfmG8b1xlIw8iBJOorlbEJA6tvOpDTb1I5
# +ph6tM4DwrMrS0LbGPJv0ah9fh26ZPta1xF0KC4/gdyqY7Bmij+X+atdrRZ0jdqc
# SHspWYc9U6SaXWKVXFwvkc0b19dkzECS7ebrPQuC0+euLpvDMzHIafzL21XHB1+g
# ncuwbt7/RknJoDbFKsx5x0qDQ6vfJmrajyNAMd5fGQdgcUHs75G+KWvg7M4RtGRq
# 6NHrXnBg1LHQlRbLDSCXbIoXkywzzksKuxxm9sn2gdz0/v4o4vPQHrxk8Mj6i23U
# 6h97uJQWVPhLWhQtcjc8MJmU2i5xlDCCB98wggXHoAMCAQICEAzRQHpave1D1cFz
# Eh04xSkwDQYJKoZIhvcNAQELBQAwWzELMAkGA1UEBhMCVVMxGDAWBgNVBAoTDy5O
# RVQgRm91bmRhdGlvbjEyMDAGA1UEAxMpLk5FVCBGb3VuZGF0aW9uIFByb2plY3Rz
# IENvZGUgU2lnbmluZyBDQTIwHhcNMjEwODEzMDAwMDAwWhcNMjQxMDI5MjM1OTU5
# WjCB5TEdMBsGA1UEDwwUUHJpdmF0ZSBPcmdhbml6YXRpb24xEzARBgsrBgEEAYI3
# PAIBAxMCVVMxGzAZBgsrBgEEAYI3PAIBAhMKV2FzaGluZ3RvbjEUMBIGA1UEBRML
# NjAzIDM4OSAwNjgxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
# DgYDVQQHEwdSZWRtb25kMSMwIQYDVQQKExpKc29uLk5FVCAoLk5FVCBGb3VuZGF0
# aW9uKTEjMCEGA1UEAxMaSnNvbi5ORVQgKC5ORVQgRm91bmRhdGlvbikwggIiMA0G
# CSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQCnII4GuLW+CHW2l9pM19eJKgk1xmmZ
# DytuitUDpv4VV+xq2x9SSVoQOkGySsxvdd6zFCTzery00/7N3waQgV9WFFTfNbJK
# QhjwSVkWm+3h8gQFNhlCCE5+cUzsp+6vqGt0Yb6IQcnEcRYlc6LvK1tnezPVbFYW
# t7qFzjGYMx7coXtem69hQg0Wn2ERdJvuQ3Wa1bxiiPQzrO7MSvSVfsAvJgwbBcOl
# UdiZK9ZjdKc4htUA+KDyMkCywt+/vSx3MFeUOy0Ke3pu9Aj6a33aiwr/z9N4+WO6
# eiQsHg8j8YvABLBE7HBVaxh7gPqHsu7wjSBTS9jZhPP2+zvsdwjeO2xQE1r1Uw5q
# VPlR2ayNjO47mVMPGK0DaxD+a+Lma6Y/7ZpjgMDgis/P44pK7HovVZlhk18tVnGQ
# +8uPCpDeUgmiA2/oSO5JJYvvPCST6z14rcJhM+HPWMlO6HKnuCx0IX2DfIQMhYGI
# VWif29/HH4QLbVSn5TU6wA+ZqS7nQdhjb1lbJNeKQiGDXu3FH6/h7GfCh2+NanE2
# OyTZ85930BtVFrkbjhx6rwkp4KurSX8/RaGbbdPQWFUSsy5lqHVaMvpQ5avPmyxw
# 1NYcjcfmMxSKze7L31hahFsbg1k85thGDVhbicodoU3ryK68TB+A21TQV/S86INn
# T5gn6PG63ZnWxwIDAQABo4ICEjCCAg4wHwYDVR0jBBgwFoAUKA5MiRyZl/GB8I8I
# 3lcSS7y4eM8wHQYDVR0OBBYEFKHXs/GXtdQV3amc+i6u319cvT1nMDQGA1UdEQQt
# MCugKQYIKwYBBQUHCAOgHTAbDBlVUy1XQVNISU5HVE9OLTYwMyAzODkgMDY4MA4G
# A1UdDwEB/wQEAwIHgDATBgNVHSUEDDAKBggrBgEFBQcDAzCBmwYDVR0fBIGTMIGQ
# MEagRKBChkBodHRwOi8vY3JsMy5kaWdpY2VydC5jb20vTkVURm91bmRhdGlvblBy
# b2plY3RzQ29kZVNpZ25pbmdDQTIuY3JsMEagRKBChkBodHRwOi8vY3JsNC5kaWdp
# Y2VydC5jb20vTkVURm91bmRhdGlvblByb2plY3RzQ29kZVNpZ25pbmdDQTIuY3Js
# MD0GA1UdIAQ2MDQwMgYFZ4EMAQMwKTAnBggrBgEFBQcCARYbaHR0cDovL3d3dy5k
# aWdpY2VydC5jb20vQ1BTMIGFBggrBgEFBQcBAQR5MHcwJAYIKwYBBQUHMAGGGGh0
# dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBPBggrBgEFBQcwAoZDaHR0cDovL2NhY2Vy
# dHMuZGlnaWNlcnQuY29tL05FVEZvdW5kYXRpb25Qcm9qZWN0c0NvZGVTaWduaW5n
# Q0EyLmNydDAMBgNVHRMBAf8EAjAAMA0GCSqGSIb3DQEBCwUAA4ICAQCqWWdfBXJu
# 9ZlyooN7QwYhvnp7o5xZDQLmaA5pculIb16l8GKDNicrhRWgo+3LReL/3vlhiasr
# fe0CFiHjUtdfORIa7jVBxiMQhJhe91WJmdc54MpR+dYHRH9DHwZS2jN8qERpf4NR
# 7aP69BtRlUudIiOvbrCBbBPvbcn+SE5LGJlS+wAEpIzSoKqN4N56vEDd7dX2AtBi
# r6letzxgxMbpcmgAfAAuT8lu1BRGwPI+I82Lul4e7/gqByeDz0kQBbqBaGc/01dU
# NVvjmUj585JOe8Qi47ZkoUB8sKucmCXoWthp/5SKLE+B65MZWRAU5mxh/Cneciuy
# zULvMTziy+rFQkAMDmxRtzOujYtjCg2AzG2csd5zz1X5yAUTjdMOJbQqreECAEtf
# KD0+oRBPsyfCrjNd+YWlZIZ2/S616nFdf0mzHTvEJylksSaHvIhhLV85f9A5hjU+
# /f6OgSWqwbrWSk/Zpi9ca/CpuzWjr/43aAlzSsqhkwrOPR5ACNYUMxn3ch1pP0Zi
# s1uhWWXmqY9+U/Ar+gEmy5uFNeMFVh2S7llQA+2trYk+EkbKoOBlUiz8f4bsTjjF
# uCG/Ke6QxVilQALiIoXlb1e5SxteKli7lIX2ywGQ+FUTjLJVB+S6248yhlYzePbX
# smIbLukAeRUXqS1YOBvZ4F3kGh+R5lbMNjGCGpIwghqOAgEBMG8wWzELMAkGA1UE
# BhMCVVMxGDAWBgNVBAoTDy5ORVQgRm91bmRhdGlvbjEyMDAGA1UEAxMpLk5FVCBG
# b3VuZGF0aW9uIFByb2plY3RzIENvZGUgU2lnbmluZyBDQTICEAzRQHpave1D1cFz
# Eh04xSkwDQYJYIZIAWUDBAIBBQCggbQwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcC
# AQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwLwYJKoZIhvcNAQkEMSIE
# IBkaRo0NkSWPoRtNFFrFzztM16i1ei1myx1hChkgzrtvMEgGCisGAQQBgjcCAQwx
# OjA4oBKAEABKAHMAbwBuAC4ATgBFAFShIoAgaHR0cHM6Ly93d3cubmV3dG9uc29m
# dC5jb20vanNvbiAwDQYJKoZIhvcNAQEBBQAEggIAEoFnZ9NoVb5CmT001NR/FchS
# 7Hx+DQYq5xbphi+gH1+DnwNLN94wLkipJOpr8Bx0yY3iTda3Q2O49cP8YPYkzGDs
# i15YArEIRDzsYfJSaSJZsDWxB6CYS8p9ezI8A7LpmSf16GLVqF/NilOJjtKURz61
# h6AwN/5MHmMA9fS2yrTDjKF3ASLCC+s/qshI3iLUpKJ0dRGv9qefZne5ETnQ+w0J
# OdTGjTMMhu1ItUrXySFswsIiz/0XB5TS6TT2nLuSD6SjuljD7HMtZExHpPnCB8Hu
# DzuAB6OCKMK19unmd593wxA7TPOssL9U/N5w9U1f35piZOQtD+VB4LCQSxW58DTn
# tW/QQsI3sO4Tj9Jx37iIE7Urv8nBpM2mYrw5R4qko31M/bHm75rlDB3dE4Cp6+jh
# u+aR10tiNwMB+nu6zfNt3STQ9eLaBtORnJKAybngukjjHT7JHT+fSBrA6WzwKxgy
# 9Uq+Tmzx3y0roF482TK12BH4UxqBVdiuAc/2H4Lcxzr7QH3L3Z0mt1RjUQ1EK7bV
# HZiF3ab+4Ej/y6yc9fkCEdt/RdfzBkll1sI0cD38LR51X6gz98I+OFGpA3pcIA7I
# NGVJ70Cnb7+BCC51ZNAexLIajCd1edV3IPVg14CnyWWNrIQbqBIHIIvRIuVD8mND
# X/BwsU1AAUpCTH4Ed0Ohghc9MIIXOQYKKwYBBAGCNwMDATGCFykwghclBgkqhkiG
# 9w0BBwKgghcWMIIXEgIBAzEPMA0GCWCGSAFlAwQCAQUAMHcGCyqGSIb3DQEJEAEE
# oGgEZjBkAgEBBglghkgBhv1sBwEwMTANBglghkgBZQMEAgEFAAQgrC6s/CCqSeRQ
# mSRW+0UFJ2YvAjhr0k0Bf3tfPyl8MlwCEH2FqH71/pguaO2iKrmsE/cYDzIwMjMw
# MzA4MDcxMDA0WqCCEwcwggbAMIIEqKADAgECAhAMTWlyS5T6PCpKPSkHgD1aMA0G
# CSqGSIb3DQEBCwUAMGMxCzAJBgNVBAYTAlVTMRcwFQYDVQQKEw5EaWdpQ2VydCwg
# SW5jLjE7MDkGA1UEAxMyRGlnaUNlcnQgVHJ1c3RlZCBHNCBSU0E0MDk2IFNIQTI1
# NiBUaW1lU3RhbXBpbmcgQ0EwHhcNMjIwOTIxMDAwMDAwWhcNMzMxMTIxMjM1OTU5
# WjBGMQswCQYDVQQGEwJVUzERMA8GA1UEChMIRGlnaUNlcnQxJDAiBgNVBAMTG0Rp
# Z2lDZXJ0IFRpbWVzdGFtcCAyMDIyIC0gMjCCAiIwDQYJKoZIhvcNAQEBBQADggIP
# ADCCAgoCggIBAM/spSY6xqnya7uNwQ2a26HoFIV0MxomrNAcVR4eNm28klUMYfSd
# CXc9FZYIL2tkpP0GgxbXkZI4HDEClvtysZc6Va8z7GGK6aYo25BjXL2JU+A6LYyH
# Qq4mpOS7eHi5ehbhVsbAumRTuyoW51BIu4hpDIjG8b7gL307scpTjUCDHufLckko
# HkyAHoVW54Xt8mG8qjoHffarbuVm3eJc9S/tjdRNlYRo44DLannR0hCRRinrPiby
# tIzNTLlmyLuqUDgN5YyUXRlav/V7QG5vFqianJVHhoV5PgxeZowaCiS+nKrSnLb3
# T254xCg/oxwPUAY3ugjZNaa1Htp4WB056PhMkRCWfk3h3cKtpX74LRsf7CtGGKMZ
# 9jn39cFPcS6JAxGiS7uYv/pP5Hs27wZE5FX/NurlfDHn88JSxOYWe1p+pSVz28Bq
# mSEtY+VZ9U0vkB8nt9KrFOU4ZodRCGv7U0M50GT6Vs/g9ArmFG1keLuY/ZTDcyHz
# L8IuINeBrNPxB9ThvdldS24xlCmL5kGkZZTAWOXlLimQprdhZPrZIGwYUWC6poEP
# CSVT8b876asHDmoHOWIZydaFfxPZjXnPYsXs4Xu5zGcTB5rBeO3GiMiwbjJ5xwtZ
# g43G7vUsfHuOy2SJ8bHEuOdTXl9V0n0ZKVkDTvpd6kVzHIR+187i1Dp3AgMBAAGj
# ggGLMIIBhzAOBgNVHQ8BAf8EBAMCB4AwDAYDVR0TAQH/BAIwADAWBgNVHSUBAf8E
# DDAKBggrBgEFBQcDCDAgBgNVHSAEGTAXMAgGBmeBDAEEAjALBglghkgBhv1sBwEw
# HwYDVR0jBBgwFoAUuhbZbU2FL3MpdpovdYxqII+eyG8wHQYDVR0OBBYEFGKK3tBh
# /I8xFO2XC809KpQU31KcMFoGA1UdHwRTMFEwT6BNoEuGSWh0dHA6Ly9jcmwzLmRp
# Z2ljZXJ0LmNvbS9EaWdpQ2VydFRydXN0ZWRHNFJTQTQwOTZTSEEyNTZUaW1lU3Rh
# bXBpbmdDQS5jcmwwgZAGCCsGAQUFBwEBBIGDMIGAMCQGCCsGAQUFBzABhhhodHRw
# Oi8vb2NzcC5kaWdpY2VydC5jb20wWAYIKwYBBQUHMAKGTGh0dHA6Ly9jYWNlcnRz
# LmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydFRydXN0ZWRHNFJTQTQwOTZTSEEyNTZUaW1l
# U3RhbXBpbmdDQS5jcnQwDQYJKoZIhvcNAQELBQADggIBAFWqKhrzRvN4Vzcw/HXj
# T9aFI/H8+ZU5myXm93KKmMN31GT8Ffs2wklRLHiIY1UJRjkA/GnUypsp+6M/wMkA
# mxMdsJiJ3HjyzXyFzVOdr2LiYWajFCpFh0qYQitQ/Bu1nggwCfrkLdcJiXn5CeaI
# zn0buGqim8FTYAnoo7id160fHLjsmEHw9g6A++T/350Qp+sAul9Kjxo6UrTqvwlJ
# FTU2WZoPVNKyG39+XgmtdlSKdG3K0gVnK3br/5iyJpU4GYhEFOUKWaJr5yI+RCHS
# PxzAm+18SLLYkgyRTzxmlK9dAlPrnuKe5NMfhgFknADC6Vp0dQ094XmIvxwBl8kZ
# I4DXNlpflhaxYwzGRkA7zl011Fk+Q5oYrsPJy8P7mxNfarXH4PMFw1nfJ2Ir3kHJ
# U7n/NBBn9iYymHv+XEKUgZSCnawKi8ZLFUrTmJBFYDOA4CPe+AOk9kVH5c64A0JH
# 6EE2cXet/aLol3ROLtoeHYxayB6a1cLwxiKoT5u92ByaUcQvmvZfpyeXupYuhVfA
# YOd4Vn9q78KVmksRAsiCnMkaBXy6cbVOepls9Oie1FqYyJ+/jbsYXEP10Cro4mLu
# eATbvdH7WwqocH7wl4R44wgDXUcsY6glOJcB0j862uXl9uab3H4szP8XTE0AotjW
# AQ64i+7m4HJViSwnGWH2dwGMMIIGrjCCBJagAwIBAgIQBzY3tyRUfNhHrP0oZipe
# WzANBgkqhkiG9w0BAQsFADBiMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNl
# cnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMSEwHwYDVQQDExhEaWdp
# Q2VydCBUcnVzdGVkIFJvb3QgRzQwHhcNMjIwMzIzMDAwMDAwWhcNMzcwMzIyMjM1
# OTU5WjBjMQswCQYDVQQGEwJVUzEXMBUGA1UEChMORGlnaUNlcnQsIEluYy4xOzA5
# BgNVBAMTMkRpZ2lDZXJ0IFRydXN0ZWQgRzQgUlNBNDA5NiBTSEEyNTYgVGltZVN0
# YW1waW5nIENBMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAxoY1Bkmz
# wT1ySVFVxyUDxPKRN6mXUaHW0oPRnkyibaCwzIP5WvYRoUQVQl+kiPNo+n3znIkL
# f50fng8zH1ATCyZzlm34V6gCff1DtITaEfFzsbPuK4CEiiIY3+vaPcQXf6sZKz5C
# 3GeO6lE98NZW1OcoLevTsbV15x8GZY2UKdPZ7Gnf2ZCHRgB720RBidx8ald68Dd5
# n12sy+iEZLRS8nZH92GDGd1ftFQLIWhuNyG7QKxfst5Kfc71ORJn7w6lY2zkpsUd
# zTYNXNXmG6jBZHRAp8ByxbpOH7G1WE15/tePc5OsLDnipUjW8LAxE6lXKZYnLvWH
# po9OdhVVJnCYJn+gGkcgQ+NDY4B7dW4nJZCYOjgRs/b2nuY7W+yB3iIU2YIqx5K/
# oN7jPqJz+ucfWmyU8lKVEStYdEAoq3NDzt9KoRxrOMUp88qqlnNCaJ+2RrOdOqPV
# A+C/8KI8ykLcGEh/FDTP0kyr75s9/g64ZCr6dSgkQe1CvwWcZklSUPRR8zZJTYsg
# 0ixXNXkrqPNFYLwjjVj33GHek/45wPmyMKVM1+mYSlg+0wOI/rOP015LdhJRk8mM
# DDtbiiKowSYI+RQQEgN9XyO7ZONj4KbhPvbCdLI/Hgl27KtdRnXiYKNYCQEoAA6E
# VO7O6V3IXjASvUaetdN2udIOa5kM0jO0zbECAwEAAaOCAV0wggFZMBIGA1UdEwEB
# /wQIMAYBAf8CAQAwHQYDVR0OBBYEFLoW2W1NhS9zKXaaL3WMaiCPnshvMB8GA1Ud
# IwQYMBaAFOzX44LScV1kTN8uZz/nupiuHA9PMA4GA1UdDwEB/wQEAwIBhjATBgNV
# HSUEDDAKBggrBgEFBQcDCDB3BggrBgEFBQcBAQRrMGkwJAYIKwYBBQUHMAGGGGh0
# dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBBBggrBgEFBQcwAoY1aHR0cDovL2NhY2Vy
# dHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1c3RlZFJvb3RHNC5jcnQwQwYDVR0f
# BDwwOjA4oDagNIYyaHR0cDovL2NybDMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0VHJ1
# c3RlZFJvb3RHNC5jcmwwIAYDVR0gBBkwFzAIBgZngQwBBAIwCwYJYIZIAYb9bAcB
# MA0GCSqGSIb3DQEBCwUAA4ICAQB9WY7Ak7ZvmKlEIgF+ZtbYIULhsBguEE0TzzBT
# zr8Y+8dQXeJLKftwig2qKWn8acHPHQfpPmDI2AvlXFvXbYf6hCAlNDFnzbYSlm/E
# UExiHQwIgqgWvalWzxVzjQEiJc6VaT9Hd/tydBTX/6tPiix6q4XNQ1/tYLaqT5Fm
# niye4Iqs5f2MvGQmh2ySvZ180HAKfO+ovHVPulr3qRCyXen/KFSJ8NWKcXZl2szw
# cqMj+sAngkSumScbqyQeJsG33irr9p6xeZmBo1aGqwpFyd/EjaDnmPv7pp1yr8TH
# wcFqcdnGE4AJxLafzYeHJLtPo0m5d2aR8XKc6UsCUqc3fpNTrDsdCEkPlM05et3/
# JWOZJyw9P2un8WbDQc1PtkCbISFA0LcTJM3cHXg65J6t5TRxktcma+Q4c6umAU+9
# Pzt4rUyt+8SVe+0KXzM5h0F4ejjpnOHdI/0dKNPH+ejxmF/7K9h+8kaddSweJywm
# 228Vex4Ziza4k9Tm8heZWcpw8De/mADfIBZPJ/tgZxahZrrdVcA6KYawmKAr7ZVB
# tzrVFZgxtGIJDwq9gdkT/r+k0fNX2bwE+oLeMt8EifAAzV3C+dAjfwAL5HYCJtnw
# ZXZCpimHCUcr5n8apIUP/JiW9lVUKx+A+sDyDivl1vupL0QVSucTDh3bNzgaoSv2
# 7dZ8/DCCBY0wggR1oAMCAQICEA6bGI750C3n79tQ4ghAGFowDQYJKoZIhvcNAQEM
# BQAwZTELMAkGA1UEBhMCVVMxFTATBgNVBAoTDERpZ2lDZXJ0IEluYzEZMBcGA1UE
# CxMQd3d3LmRpZ2ljZXJ0LmNvbTEkMCIGA1UEAxMbRGlnaUNlcnQgQXNzdXJlZCBJ
# RCBSb290IENBMB4XDTIyMDgwMTAwMDAwMFoXDTMxMTEwOTIzNTk1OVowYjELMAkG
# A1UEBhMCVVMxFTATBgNVBAoTDERpZ2lDZXJ0IEluYzEZMBcGA1UECxMQd3d3LmRp
# Z2ljZXJ0LmNvbTEhMB8GA1UEAxMYRGlnaUNlcnQgVHJ1c3RlZCBSb290IEc0MIIC
# IjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAv+aQc2jeu+RdSjwwIjBpM+zC
# pyUuySE98orYWcLhKac9WKt2ms2uexuEDcQwH/MbpDgW61bGl20dq7J58soR0uRf
# 1gU8Ug9SH8aeFaV+vp+pVxZZVXKvaJNwwrK6dZlqczKU0RBEEC7fgvMHhOZ0O21x
# 4i0MG+4g1ckgHWMpLc7sXk7Ik/ghYZs06wXGXuxbGrzryc/NrDRAX7F6Zu53yEio
# ZldXn1RYjgwrt0+nMNlW7sp7XeOtyU9e5TXnMcvak17cjo+A2raRmECQecN4x7ax
# xLVqGDgDEI3Y1DekLgV9iPWCPhCRcKtVgkEy19sEcypukQF8IUzUvK4bA3VdeGbZ
# OjFEmjNAvwjXWkmkwuapoGfdpCe8oU85tRFYF/ckXEaPZPfBaYh2mHY9WV1CdoeJ
# l2l6SPDgohIbZpp0yt5LHucOY67m1O+SkjqePdwA5EUlibaaRBkrfsCUtNJhbesz
# 2cXfSwQAzH0clcOP9yGyshG3u3/y1YxwLEFgqrFjGESVGnZifvaAsPvoZKYz0YkH
# 4b235kOkGLimdwHhD5QMIR2yVCkliWzlDlJRR3S+Jqy2QXXeeqxfjT/JvNNBERJb
# 5RBQ6zHFynIWIgnffEx1P2PsIV/EIFFrb7GrhotPwtZFX50g/KEexcCPorF+CiaZ
# 9eRpL5gdLfXZqbId5RsCAwEAAaOCATowggE2MA8GA1UdEwEB/wQFMAMBAf8wHQYD
# VR0OBBYEFOzX44LScV1kTN8uZz/nupiuHA9PMB8GA1UdIwQYMBaAFEXroq/0ksuC
# MS1Ri6enIZ3zbcgPMA4GA1UdDwEB/wQEAwIBhjB5BggrBgEFBQcBAQRtMGswJAYI
# KwYBBQUHMAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBDBggrBgEFBQcwAoY3
# aHR0cDovL2NhY2VydHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0QXNzdXJlZElEUm9v
# dENBLmNydDBFBgNVHR8EPjA8MDqgOKA2hjRodHRwOi8vY3JsMy5kaWdpY2VydC5j
# b20vRGlnaUNlcnRBc3N1cmVkSURSb290Q0EuY3JsMBEGA1UdIAQKMAgwBgYEVR0g
# ADANBgkqhkiG9w0BAQwFAAOCAQEAcKC/Q1xV5zhfoKN0Gz22Ftf3v1cHvZqsoYcs
# 7IVeqRq7IviHGmlUIu2kiHdtvRoU9BNKei8ttzjv9P+Aufih9/Jy3iS8UgPITtAq
# 3votVs/59PesMHqai7Je1M/RQ0SbQyHrlnKhSLSZy51PpwYDE3cnRNTnf+hZqPC/
# Lwum6fI0POz3A8eHqNJMQBk1RmppVLC4oVaO7KTVPeix3P0c2PR3WlxUjG/voVA9
# /HYJaISfb8rbII01YBwCA8sgsKxYoA5AY8WYIsGyWfVVa88nq2x2zm8jLfR+cWoj
# ayL/ErhULSd+2DrZ8LaHlv1b0VysGMNNn3O3AamfV6peKOK5lDGCA3YwggNyAgEB
# MHcwYzELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDkRpZ2lDZXJ0LCBJbmMuMTswOQYD
# VQQDEzJEaWdpQ2VydCBUcnVzdGVkIEc0IFJTQTQwOTYgU0hBMjU2IFRpbWVTdGFt
# cGluZyBDQQIQDE1pckuU+jwqSj0pB4A9WjANBglghkgBZQMEAgEFAKCB0TAaBgkq
# hkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQwHAYJKoZIhvcNAQkFMQ8XDTIzMDMwODA3
# MTAwNFowKwYLKoZIhvcNAQkQAgwxHDAaMBgwFgQU84ciTYYzgpI1qZS8vY+W6f4c
# fHMwLwYJKoZIhvcNAQkEMSIEICiELIwluzMGQ9DaOH/gy5iaATk+3pCHuVBdCe+i
# w2IPMDcGCyqGSIb3DQEJEAIvMSgwJjAkMCIEIMf04b4yKIkgq+ImOr4axPxP5ngc
# LWTQTIB1V6Ajtbb6MA0GCSqGSIb3DQEBAQUABIICABPe5HinR4D4Syb5NGQVM5Ct
# Sd4UwHIGFxkmB5zKX6xKlyxtlACgd79o3Z0df98XV3Ol7yxiiy6qfGLPLg1rjXYY
# oFhyc/HGLMMK9xTzEcLmolBGzaXQvA+boXOohCDCW3gnLmDV5ZxAqlYbdYkhB8Wx
# eWHHfJN2n9Tcr34ntur503XYAK1j25d5vh6Llm98ls3o3M+g0BpUGvMaN0UyRePn
# rxOZ/2vgke6GVhhCLQ5H1ENZ5qYpG5wQzje8j469G1VOh9tcD81jGgKh8X2Z41E0
# va3i6JoSCSuKoi/n2HGiCxGJh9pQUs5Sz3jwbubT3fPVBzWvK25zLVdmyrCbw2qe
# 1GiffeppT3QmDhS6PzhAF94pb/wm2ChX6bbhISXwpyUUgFOPbdBw9ukO8jGv4avX
# na7ErEFIpwn//yXbSbnm6IDx4WNygvQJ/CoeeXpGCj41v1zYALigTKa5iYyOmSl2
# vLdXk63StSLgzLJNMWu/xSH+iDWCzfElw9hp+pBsU8fEGJZcCRsHNiXVi++hoWI7
# Hc7umARU6eledrEQ3iZjSCOoXT5XbMisiONSPXqbXziILRtmCA5vROUsQF83nQP8
# 4vMAYIE8ZipWj/IHZYxqiyGSqs0/m1XvzF7DKLWQYQIne6w+ve2jEEgZ1/6W4203
# lfejiJ5gwo7S8qiFFQpa
# SIG # End signature block
