﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScreenBabel.Resources {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.7.0.0")]
    internal sealed partial class Setting : global::System.Configuration.ApplicationSettingsBase {
        
        private static Setting defaultInstance = ((Setting)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Setting())));
        
        public static Setting Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("YouDao")]
        public string RecognitionMode {
            get {
                return ((string)(this["RecognitionMode"]));
            }
            set {
                this["RecognitionMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string YouDaoAppKey {
            get {
                return ((string)(this["YouDaoAppKey"]));
            }
            set {
                this["YouDaoAppKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string YouDaoAppSecret {
            get {
                return ((string)(this["YouDaoAppSecret"]));
            }
            set {
                this["YouDaoAppSecret"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://openapi.youdao.com/ocrtransapi?from=auto&to=zh-CHS")]
        public string YouDaoOCRApi {
            get {
                return ((string)(this["YouDaoOCRApi"]));
            }
            set {
                this["YouDaoOCRApi"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AzureOCRKey {
            get {
                return ((string)(this["AzureOCRKey"]));
            }
            set {
                this["AzureOCRKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr?language=unk&de" +
            "tectOrientation=true")]
        public string AzureOCRApi {
            get {
                return ((string)(this["AzureOCRApi"]));
            }
            set {
                this["AzureOCRApi"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AzureTransKey {
            get {
                return ((string)(this["AzureTransKey"]));
            }
            set {
                this["AzureTransKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=zh-Han" +
            "s")]
        public string AzureTransApi {
            get {
                return ((string)(this["AzureTransApi"]));
            }
            set {
                this["AzureTransApi"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("./tessdata")]
        public string TesseractData {
            get {
                return ((string)(this["TesseractData"]));
            }
            set {
                this["TesseractData"] = value;
            }
        }
    }
}
