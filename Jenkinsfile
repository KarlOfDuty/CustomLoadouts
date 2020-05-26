pipeline {
  agent any
  stages {
    stage('Dependencies') {
      steps {
        sh 'nuget restore CustomLoadouts.sln'
      }
    }
    stage('Build') {
      steps {
        sh 'msbuild CustomLoadouts/CustomLoadouts.csproj -restore -p:PostBuildEvent='
      }
    }
    stage('Setup Output Dir') {
      steps {
        sh 'mkdir Plugin'
        sh 'mkdir Plugin/dependencies'
      }
    }
    stage('Package') {
      steps {
        sh 'mv CustomLoadouts/bin/CustomLoadouts.dll Plugin/'
        sh 'mv CustomLoadouts/bin/YamlDotNet.dll Plugin/dependencies'
        sh 'mv CustomLoadouts/bin/Newtonsoft.Json.dll Plugin/dependencies'
      }
    }
    stage('Archive') {
      steps {
        sh 'zip -r CustomLoadouts.zip Plugin'
        archiveArtifacts(artifacts: 'CustomLoadouts.zip', onlyIfSuccessful: true)
      }
    }
  }
}