﻿/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Builder]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameCanvas.Editor
{
    /// <summary>
    /// モバイルビルドターゲット
    /// </summary>
    public enum MobilePlatform
    {
        Android = BuildTarget.Android,
        iOS = BuildTarget.iOS
    }

    /// <summary>
    /// ビルドオプション
    /// </summary>
    public class BuildOption
    {
        /// <summary>
        /// ビルドオプション
        /// </summary>
        /// <param name="target">ビルドターゲット</param>
        /// <param name="isDevelop">開発モードでビルドするかどうか</param>
        public BuildOption(MobilePlatform target, bool isDevelop = false)
        {
            bundleIdentifier = "jp.ac.keio.sfc.sdp" + System.DateTime.Now.ToString("MMddHHmmss");
            bundleVersion = "1.0.0";
            productName = PlayerSettings.productName;
            companyName = "Keio University";
            outFolderPath = Path.GetFullPath(Application.dataPath + "/../Build/");
            this.target = target;
            isDevelopment = isDevelop;
            connectProfiler = isDevelop;
            allowDebugging = false;
            il2cpp = target == MobilePlatform.iOS;
            minAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevel10;
            iOSSdkVersion = iOSSdkVersion.DeviceSDK;
        }

        public BuildOption()
        {
            bundleIdentifier = PlayerSettings.bundleIdentifier;
            bundleVersion = PlayerSettings.bundleVersion;
            productName = PlayerSettings.productName;
            companyName = "Keio University";
            outFolderPath = Path.GetFullPath(Application.dataPath + "/../Build/");
            target = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? MobilePlatform.iOS : MobilePlatform.Android;
            isDevelopment = false;
            connectProfiler = false;
            allowDebugging = false;
            il2cpp = target == MobilePlatform.iOS;
            minAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevel10;
            iOSSdkVersion = iOSSdkVersion.DeviceSDK;
        }

        /// <summary>アプリごとに固有の識別子</summary>
        public string bundleIdentifier;
        /// <summary>バージョン情報。`0.10.2`のように3つの数字をドットで繋げた文字列で表現してください</summary>
        public string bundleVersion;
        /// <summary>英数字のみで表したアプリケーション名</summary>
        public string productName;
        /// <summary>アプリケーションを開発した組織の名称</summary>
        public string companyName;
        /// <summary>アプリケーションを書き出す先のフォルダーパス</summary>
        public string outFolderPath;
        /// <summary>ビルドターゲット</summary>
        public MobilePlatform target;
        /// <summary>開発モードでビルドするかどうか</summary>
        public bool isDevelopment;
        /// <summary>Unityプロファイラーを使用するかどうか</summary>
        public bool connectProfiler;
        /// <summary>スクリプトデバッギングを許容するかどうか</summary>
        public bool allowDebugging;
        /// <summary>IL2CPPビルドを行うかどうか</summary>
        public bool il2cpp;
        /// <summary>最小Android SDKバージョン (Android限定)</summary>
        public AndroidSdkVersions minAndroidSdkVersion;
        /// <summary>実機向けかシミュレーター向けか (iOS限定)</summary>
        public iOSSdkVersion iOSSdkVersion;
    }

    /// <summary>
    /// アプリケーションビルダー
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// アプリケーションをAndroid向けに開発モードでビルドします
        /// </summary>
        public static void DebugBuildApk()
        {
            var option = new BuildOption(MobilePlatform.Android, true);
            OverrideOptionByCommandLine(ref option);
            Run(option);
        }

        /// <summary>
        /// アプリケーションをAndroid向けにビルドします
        /// </summary>
        public static void BuildApk()
        {
            var option = new BuildOption(MobilePlatform.Android);
            OverrideOptionByCommandLine(ref option);
            Run(option);
        }

        /// <summary>
        /// アプリケーションをiOS向けにビルドします
        /// </summary>
        public static void DebugBuildXcodeProj()
        {
            var option = new BuildOption(MobilePlatform.iOS, true);
            OverrideOptionByCommandLine(ref option);
            Run(option);
        }

        /// <summary>
        /// アプリケーションをiOS向けにビルドします
        /// </summary>
        public static void BuildXcodeProj()
        {
            var option = new BuildOption(MobilePlatform.iOS);
            OverrideOptionByCommandLine(ref option);
            Run(option);
        }

        /// <summary>
        /// ビルドを実行する
        /// </summary>
        /// <param name="option">ビルドオプション</param>
        internal static void Run(BuildOption option)
        {
            // 現在のビルド設定を控えておく
            var prevTarget          = EditorUserBuildSettings.activeBuildTarget;
            var prevAllowDebuggin   = EditorUserBuildSettings.allowDebugging;
            var prevConnectProfiler = EditorUserBuildSettings.connectProfiler;
            var prevDevelopment     = EditorUserBuildSettings.development;

            // ビルド設定を上書きする
            PlayerSettings.bundleIdentifier         = option.bundleIdentifier;
            PlayerSettings.bundleVersion            = option.bundleVersion;
            PlayerSettings.productName              = option.productName;
            PlayerSettings.companyName              = option.companyName;
            EditorUserBuildSettings.allowDebugging  = option.allowDebugging;
            EditorUserBuildSettings.connectProfiler = option.connectProfiler;
            EditorUserBuildSettings.development     = option.isDevelopment;
            var buildTarget = (BuildTarget)option.target;
            if (prevTarget != buildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
            }

            var outFilePath = option.outFolderPath + option.productName;

            switch (buildTarget)
            {
#if UNITY_ANDROID
                case BuildTarget.Android:
                    if (Path.GetExtension(option.outFolderPath) != "apk")
                    {
                        outFilePath += ".apk";
                    }

                    PlayerSettings.Android.minSdkVersion = option.minAndroidSdkVersion;
                    var prevVersionCode = PlayerSettings.Android.bundleVersionCode;
                    PlayerSettings.Android.bundleVersionCode = prevVersionCode + 1;

                    // 既に出力ファイルがあれば退避させておく
                    if (File.Exists(outFilePath))
                    {
                        File.Move(outFilePath, outFilePath.Remove(outFilePath.Length - 4) + "." + File.GetLastWriteTime(outFilePath).ToString("MMddHHmmss") + ".apk");
                    }
                    break;
#endif
#if UNITY_IOS
                case BuildTarget.iOS:
#if UNITY_5_4_2 || UNITY_5_4_3
                    PlayerSettings.iOS.cameraUsageDescription = "For education";
                    PlayerSettings.iOS.locationUsageDescription = "For education";
                    PlayerSettings.iOS.microphoneUsageDescription = "For education";
                    PlayerSettings.iOS.allowHTTPDownload = true;
#endif
                    PlayerSettings.iOS.sdkVersion = option.iOSSdkVersion;
                    PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
                    PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.Unknown;
                    break;
#endif

                default:
                    return;
            }

            // ビルドを実行する
            var errorMessage = BuildPipeline.BuildPlayer(
                GetEnabledScenePaths(),
                outFilePath,
                buildTarget,
                option.il2cpp ? BuildOptions.Il2CPP : BuildOptions.None
            );

            // ビルド前の設定に戻す
            EditorUserBuildSettings.SwitchActiveBuildTarget(prevTarget);
            EditorUserBuildSettings.allowDebugging  = prevAllowDebuggin;
            EditorUserBuildSettings.connectProfiler = prevConnectProfiler;
            EditorUserBuildSettings.development     = prevDevelopment;

            // エラー出力
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError(errorMessage);
            }
        }

        /// <summary>
        /// コマンドライン引数を読み取り、ビルドオプションを上書きします
        /// </summary>
        /// <param name="option"></param>
        static void OverrideOptionByCommandLine(ref BuildOption option)
        {
            var argv = System.Environment.GetCommandLineArgs();
            var argc = argv.Length;

            for (var i = 0; i < argc; ++i)
            {
                switch (argv[i])
                {
                    case "/name":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        option.productName = argv[i + 1];
                        option.outFolderPath = "Build/" + argv[i + 1];
                        ++i;
                        break;

                    case "/id":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        option.bundleIdentifier = "jp.ac.keio.sfc." + argv[i + 1];
                        ++i;
                        break;

                    case "/bundleIdentifier":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        option.bundleIdentifier = argv[i + 1];
                        ++i;
                        break;

                    case "/version":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        option.bundleVersion = argv[i + 1];
                        ++i;
                        break;

                    case "/company":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        option.companyName = argv[i + 1];
                        ++i;
                        break;

                    case "/out":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        option.outFolderPath = Path.GetFileNameWithoutExtension(argv[i + 1]);
                        ++i;
                        break;

                    case "/minAndroidSDK":
                        if (i + 1 >= argc || argv[i + 1][0] == '/') break;
                        int version;
                        if (int.TryParse(argv[i + 1], out version))
                        {
                            option.minAndroidSdkVersion = (AndroidSdkVersions)version;
                        }
                        ++i;
                        break;

                    case "/develop":
                        option.isDevelopment = true;
                        break;

                    case "/useProfiler":
                        option.isDevelopment = true;
                        option.connectProfiler = true;
                        break;

                    case "/allowDebugging":
                        option.allowDebugging = true;
                        break;

                    case "/release":
                        option.isDevelopment = false;
                        option.connectProfiler = false;
                        break;

                    case "/il2cpp":
                        option.il2cpp = true;
                        break;

                    case "/mono2x":
                        option.il2cpp = false;
                        break;

                    case "/simulatorSDK":
                        option.iOSSdkVersion = iOSSdkVersion.SimulatorSDK;
                        break;

                    case "/deviceSDK":
                        option.iOSSdkVersion = iOSSdkVersion.DeviceSDK;
                        break;
                }
            }
        }
        
        /// <summary>
        /// 現在有効なシーンの一覧を取得します
        /// </summary>
        /// <returns></returns>
        internal static string[] GetEnabledScenePaths()
        {
            var scenePathList = new List<string>();
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) scenePathList.Add(scene.path);
            }
            
            return scenePathList.ToArray();
        }
    }
}
