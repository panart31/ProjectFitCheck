pipeline {
    agent any
    
    tools {
        dotnet 'dotnet-6.0'
        msbuild 'MSBuild-2022'
    }
    
    environment {
        SOLUTION = 'FitCheck.sln'
        CONFIGURATION = 'Release'
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo 'Код получен из репозитория'
            }
        }
        
        stage('Clean') {
            steps {
                bat 'dotnet clean'
                echo 'Проект очищен'
            }
        }
        
        stage('Restore') {
            steps {
                bat 'dotnet restore'
                echo 'NuGet пакеты восстановлены'
            }
        }
        
        stage('Build') {
            steps {
                bat "dotnet build ${SOLUTION} --configuration ${CONFIGURATION}"
                echo 'Проект собран'
            }
            post {
                success {
                    archiveArtifacts artifacts: '**/bin/Release/**', fingerprint: true
                }
            }
        }
        
        stage('Test') {
            steps {
                bat "dotnet test ${SOLUTION} --configuration ${CONFIGURATION} --logger:trx"
                echo 'Тесты выполнены'
            }
            post {
                always {
                    junit '**/TestResults/*.trx'
                }
            }
        }
        
        stage('Publish') {
            when {
                branch 'master'
            }
            steps {
                bat 'dotnet publish WpfAppFitCheck/WpfAppFitCheck.csproj -c Release -o ./publish'
                echo 'Артефакты подготовлены'
            }
        }
    }
    
    post {
        success {
            echo '🎉 Pipeline выполнен успешно!'
        }
        failure {
            echo '💥 Pipeline упал! Проверьте логи.'
        }
    }
}