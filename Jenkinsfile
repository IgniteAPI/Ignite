pipeline {
    agent any

    parameters {
        // Steam beta branch name. Leave empty for the default/stable release.
        // Example: 'beta' for a public beta, or 'some_private_branch accesscode' for password-protected branches.
        string(name: 'SE_BETA_BRANCH', defaultValue: '', description: 'Steam beta branch for SE Dedicated Server (leave empty for stable)')
    }

    environment {
        APP_NAME = 'Ignite App'
        BUILD_VERSION = '1.0.0'
        // Space Engineers Dedicated Server Steam App ID
        SE_DS_APP_ID = '298740'
        // Derive a cache key from the branch so each game version gets its own cache
        GAME_BRANCH_KEY = "${params.SE_BETA_BRANCH ? params.SE_BETA_BRANCH.split(' ')[0] : 'public'}"
        // Persistent cache directory (survives workspace cleans)
        CACHE_DIR = "${JENKINS_HOME}\\caches\\ignite-se"
        // SteamCMD is shared across all branches
        STEAMCMD_DIR = "${JENKINS_HOME}\\caches\\ignite-se\\SteamCMD"
        // Game server cache is per-branch so stable and beta don't collide
        GAME_CACHE_DIR = "${JENKINS_HOME}\\caches\\ignite-se\\GameServer-${GAME_BRANCH_KEY}"
        // Path where the build expects game references
        SE_DS_PATH = "${WORKSPACE}\\IgniteSE1\\bin\\Debug\\Game"
        STEAMCMD_ZIP_URL = 'https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out code from repository...'
                checkout scm
            }
        }

        stage('Install SteamCMD') {
            steps {
                echo 'Checking SteamCMD cache...'
                bat """
                    if not exist "%STEAMCMD_DIR%\\steamcmd.exe" (
                        echo SteamCMD not found in cache, downloading...
                        mkdir "%STEAMCMD_DIR%" 2>nul
                        powershell -Command "Invoke-WebRequest -Uri '%STEAMCMD_ZIP_URL%' -OutFile '%CACHE_DIR%\\steamcmd.zip'"
                        powershell -Command "Expand-Archive -Path '%CACHE_DIR%\\steamcmd.zip' -DestinationPath '%STEAMCMD_DIR%' -Force"
                        del "%CACHE_DIR%\\steamcmd.zip"
                    ) else (
                        echo SteamCMD already cached, skipping download.
                    )
                """
            }
        }

        stage('Update SE Dedicated Server') {
            steps {
                echo "Updating SE Dedicated Server cache (branch: ${env.GAME_BRANCH_KEY})..."
                script {
                    // Build the -beta argument only if a branch was specified
                    def betaArg = params.SE_BETA_BRANCH?.trim() ? "-beta ${params.SE_BETA_BRANCH}" : ''
                    bat """
                        cmd /c ""%STEAMCMD_DIR%\\steamcmd.exe" +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir "%GAME_CACHE_DIR%" +login anonymous +app_update %SE_DS_APP_ID% ${betaArg} +quit"
                    """
                }
            }
        }

        stage('Link Game References') {
            steps {
                echo 'Copying cached game files to build directory...'
                bat """
                    if not exist "%SE_DS_PATH%" mkdir "%SE_DS_PATH%"
                    robocopy "%GAME_CACHE_DIR%" "%SE_DS_PATH%" /MIR /NFL /NDL /NJH /NJS /nc /ns /np
                    if %ERRORLEVEL% LEQ 7 exit /b 0
                """
            }
        }

        stage('Restore NuGet Packages') {
            steps {
                echo 'Restoring NuGet packages...'
                // Solution-level restore handles both packages.config (IgniteSE1)
                // and SDK-style projects (Torch2API, Torch2WebUI) via the .NET SDK
                bat 'nuget restore IgniteSE1.slnx'
            }
        }

        stage('Build') {
            steps {
                echo "Building ${env.APP_NAME}..."
                // Build the entire solution, excluding the docker-compose project from Release builds
                bat 'msbuild IgniteSE1.slnx /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild /m /p:BuildDockerCompose=false'
            }
        }

        stage('Project Tests') {
            parallel {
                stage('Test Torch2API') {
                    steps {
                        echo 'Running Torch2API tests...'
                        bat 'dotnet test Tests\\Torch2API.Tests\\Torch2API.Tests.csproj --configuration Release --verbosity minimal'
                    }
                }
                stage('Test Torch2WebUI') {
                    steps {
                        echo 'Running Torch2WebUI tests...'
                        bat 'dotnet test Tests\\Torch2WebUI.Tests\\Torch2WebUI.Tests.csproj --configuration Release --verbosity minimal'
                    }
                }
                stage('Test IgniteSE1') {
                    steps {
                        echo 'Running IgniteSE1 tests...'
                        bat 'dotnet test Tests\\IgniteSE1.Tests\\IgniteSE1.Tests.csproj --configuration Release --verbosity minimal'
                    }
                }
            }
        }
    }

    post {
        always {
            echo 'Pipeline execution finished.'
        }
        success {
            echo 'Build was successful!'
        }
        failure {
            echo 'Build failed. Sending alerts...'
        }
    }
}
