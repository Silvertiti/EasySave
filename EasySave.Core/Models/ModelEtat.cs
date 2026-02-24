using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class ModelEtat
    {
        public string Name { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;     // Fichier en cours
        public string TargetFile { get; set; } = string.Empty;     // Destination en cours
        public string State { get; set; } = string.Empty;          // "ACTIF" ou "INACTIF"
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesLeft { get; set; }         // Fichiers restants
        public long SizeLeft { get; set; }         // Taille restante
        public int Progression { get; set; }       // Pourcentage (0-100)
        public string Timestamp { get; set; } = string.Empty;
    }
}
