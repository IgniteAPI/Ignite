pipeline {
    agent any

    environment {
        APP_NAME = 'Ignite App'
        BUILD_VERSION = '1.0.0'
        // Space Engineers Dedicated Server Steam App ID
        SE_DS_APP_ID = '298740'
        // Path where SteamCMD will install the dedicated server
        SE_DS_PATH = "${WORKSPACE}\\IgniteSE1\\bin\\Debug\\Game"
        // SteamCMD working directory
        STEAMCMD_DIR = "${WORKSPACE}\\SteamCMD"
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
                echo 'Downloading and extracting SteamCMD...'
                bat """
                    if not exist "${env.STEAMCMD_DIR}\\steamcmd.exe" (
                        mkdir "${env.STEAMCMD_DIR}" 2>nul
                        powershell -Command "Invoke-WebRequest -Uri '${env.STEAMCMD_ZIP_URL}' -OutFile '%WORKSPACE%\\steamcmd.zip'"
                        powershell -Command "Expand-Archive -Path '%WORKSPACE%\\steamcmd.zip' -DestinationPath '${env.STEAMCMD_DIR}' -Force"
                        del "%WORKSPACE%\\steamcmd.zip"
                    ) else (
                        echo SteamCMD already installed, skipping download.
                    )
                """
            }
        }

        stage('Download SE Dedicated Server') {
            steps {
                echo 'Downloading Space Engineers Dedicated Server via SteamCMD...'
                bat """
                    "${env.STEAMCMD_DIR}\\steamcmd.exe" +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir "${env.SE_DS_PATH}" +login anonymous +app_update ${env.SE_DS_APP_ID} validate +quit
                """
            }
        }

        stage('Restore NuGet Packages') {
            steps {
                echo 'Restoring NuGet packages...'
                // Restore packages.config for IgniteSE1 (.NET Framework 4.8.1)
                bat 'nuget restore'
                // Restore SDK-style projects (Torch2API, Torch2WebUI)
                bat 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                echo "Building ${env.APP_NAME}..."
                // Build the entire solution using MSBuild for .NET Framework compatibility
                bat 'msbuild /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild /m'
            }
        }

        stage('Test') {
            steps {
                echo 'Running tests...'
                bat 'dotnet test --configuration Release --no-build --verbosity normal'
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
