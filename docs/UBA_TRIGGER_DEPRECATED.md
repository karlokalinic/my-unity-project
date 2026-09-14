# Deprecated UBA trigger path

The old `my-unity-project/tooling/unity-cloud-devops` mirror and direct API trigger are deprecated. They targeted the wrong GitHub repository for the Unity Build Automation SCM connection and produced misleading CI failures when no Build Automation API key existed in the public repository.

The active SCM bridge lives in private `karlokalinic/UNITYLAPTOP` and publishes canonical `my-unity-project/main` snapshots to `UNITYLAPTOP/tooling/unity-cloud-devops`, the branch historically consumed by Unity Build Automation.