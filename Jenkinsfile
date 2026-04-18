pipeline {
    // 🏗️ Define where the pipeline runs (e.g., 'any' available agent)
    agent any

    // ⚙️ Environment variables for use across all stages
    environment {
        APP_NAME = 'Ignite App'
        BUILD_VERSION = '1.0.0'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out code from repository...'
                // Code is automatically checked out if using 'Pipeline from SCM'
            }
        }

        stage('Build') {
            steps {
                echo "Building ${env.APP_NAME}..."
                // Example: sh 'mvn clean install' or 'npm run build'
                echo 'echo "Build complete"'
            }
        }

        stage('Test') {
            steps {
                echo 'Running unit tests...'
                // Example: sh 'mvn test'
                echo 'echo "Tests passed"'
            }
        }

        stage('Deploy') {
            steps {
                echo 'Deploying to Staging environment...'
                echo 'echo "Deployment successful"'
            }
        }
    }

    // 📝 Post-execution actions (cleanup, notifications)
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
