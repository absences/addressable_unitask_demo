set -e

function DEFINE()
{
    VS_PATH="/c/Program Files/Microsoft Visual Studio/2022/Professional/Common7/IDE/devenv.exe"

    UNITY_APP="/d/Program Files/Unity 2021.3.41f1/Editor/Unity.exe"

    UNITY_PROJECT_PATH=$WORKSPACE/Unity

    UNITY_AUTOBUILD_PATH=$UNITY_PROJECT_PATH/AutoBuild

    TARGET_PATH=$WORKSPACE/$(printf "%05d_%s" $BUILD_NUMBER $BuildPlatform)

    if [ ! -d "$TARGET_PATH" ] ; then
		mkdir "$TARGET_PATH"
	fi

    LOG_PATH=$TARGET_PATH/Log

    if [ ! -d "$LOG_PATH" ] ; then
		mkdir "$LOG_PATH"
	fi
}

function BuildVSProject()
{
    local logFile="$LOG_PATH/build_vs.log"

    #vs路径     sln解决方案                                  选项      项目 //Project   //log路径
    "$VS_PATH" "Unity/HybridProject/HybridProject.csproj" //build "Release"  //out "$logFile"

    echo "####vsbuild done####"

}
# git提交 地址、日志
function GitPush()
{
    if [ -n "$(git status -s "$1")" ]; then
        git add "$1"
        git commit -m "$2"
        git push git@github.com:absences/addressable_unitask_demo.git HEAD:main
    fi
}

function PreLoadUnityForWriteInfo()
{
    echo "PreLoadUnity Begine "$(date)
    local logFile="$LOG_PATH/preload_build.log"
	local executeMethodName=AutoBuilder.WriteBuildinfo	
	"$UNITY_APP" -quit -batchmode -projectPath "$UNITY_PROJECT_PATH" -executeMethod $executeMethodName -logFile "$logFile" $(GetBuildParam)
    echo 'PreLoadUnity End' $(date)
}

function BuildPlatform()
{
 	echo "BuildPlatform Begine "$(date)
    local logFile="$LOG_PATH/build_platfrom.log"
	local executeMethodName=AutoBuilder.BuildPlatformTarget	
	"$UNITY_APP" -quit -batchmode -projectPath "$UNITY_PROJECT_PATH" -executeMethod $executeMethodName -logFile "$logFile" $(GetBuildParam)
    echo 'BuildPlatform End' $(date)
}
#打包参数
function GetBuildParam()
{
echo -customerParam:BuildPlatform^"$BuildPlatform",\
ResUrl^"$ResUrl",HOT_UPDATE^"$HOT_UPDATE",CodeVersion^"$BUILD_NUMBER",\
BuildType^"$BuildType",SDKType^"$SDKType",\
MarksStatus^"$MarksStatus",EnableDebug^"$EnableDebug"
}

function Main()
{
    DEFINE

    BuildVSProject

    GitPush "Unity/Assets/Builds/HotUpdate/Assembly/*" "build hybrid project"

    PreLoadUnityForWriteInfo

    BuildPlatform

    GitPush "Unity/ServerData" "build asset $BUILD_NUMBER"
}

Main