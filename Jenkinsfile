pipeline {
  agent any
  stages {
    stage('Dependencies') {
      steps {
        sh 'nuget restore CustomLoadouts/CustomLoadouts.sln'
      }
    }
    stage('Use upstream Smod') {
        when { triggeredBy 'BuildUpstreamCause' }
        steps {
            sh ('rm CustomLoadouts/lib/Assembly-CSharp.dll')
            sh ('rm CustomLoadouts/lib/Smod2.dll')
            sh ('ln -s $SCPSL_LIBS/Assembly-CSharp.dll CustomLoadouts/lib/Assembly-CSharp.dll')
            sh ('ln -s $SCPSL_LIBS/Smod2.dll CustomLoadouts/lib/Smod2.dll')
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
        when { not { triggeredBy 'BuildUpstreamCause' } }
        steps {
            sh 'zip -r CustomLoadouts.zip Plugin/*'
            archiveArtifacts(artifacts: 'CustomLoadouts.zip', onlyIfSuccessful: true)
        }
    }
    stage('Send upstream') {
        when { triggeredBy 'BuildUpstreamCause' }
        steps {
            sh 'zip -r CustomLoadouts.zip Plugin/*'
            sh 'cp CustomLoadouts.zip $PLUGIN_BUILDER_ARTIFACT_DIR'
        }
    }
  }
}
