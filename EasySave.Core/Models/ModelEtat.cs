using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    public class ModelEtat
    {
        public string Name { get; set; }
        public string SourceFile { get; set; }     // Fichier en cours
        public string TargetFile { get; set; }     // Destination en cours
        public string State { get; set; }          // "ACTIF" ou "INACTIF"
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesLeft { get; set; }         // Fichiers restants
        public long SizeLeft { get; set; }         // Taille restante
        public int Progression { get; set; }       // Pourcentage (0-100)
        public string Timestamp { get; set; }
    }
}
