using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EasySave.Core.Models
{
    public class Settings
    {
        public string ExtensionsToEncrypt { get; set; } = "txt,docx,xlsx";
        public string BusinessSoftware { get; set; } = "calc";
        public string LogFormat { get; set; } = "json";
        public string CryptoSoftPath { get; set; } = "";
    }
}