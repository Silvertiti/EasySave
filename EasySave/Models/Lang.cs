using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave
{
    public static class Lang
    {
        public static Dictionary<string, string> Msg = new Dictionary<string, string>();

        public static void Init(string culture)
        {
            Msg.Clear();
            Console.OutputEncoding = Encoding.UTF8;

            if (culture == "en")
            {
                Msg["MenuTitle"] = "\n--- EASY SAVE MENU ---";
                Msg["List"] = "1. List backup jobs";
                Msg["Add"] = "2. Add a backup job";
                Msg["Run"] = "3. RUN ALL BACKUPS";
                Msg["Quit"] = "4. Quit";
                Msg["Choice"] = "Your choice: ";
                Msg["Success"] = "Success for: ";
                Msg["Error"] = "Error: ";
                Msg["Copy"] = "File copied: ";
                Msg["Start"] = "\n--- Starting job: ";
                Msg["SourceMissing"] = "Error: Source directory not found.";
                Msg["MaxJobs"] = "Error: Maximum 5 jobs reached!";
                Msg["EnterName"] = "Enter name: ";
                Msg["EnterSource"] = "Source path: ";
                Msg["EnterTarget"] = "Target path: ";
                Msg["EnterType"] = "Type (1 for Full, 2 for Differential): ";
                Msg["Saved"] = "Job saved!";
                Msg["Deleted"] = "Job deleted!";
                Msg["DeletePrompt"] = "\nEnter job number to delete (or Enter to cancel): ";
                Msg["NoJob"] = "No job found.";
                Msg["PressKey"] = "\nPress a key to continue...";
            }
            else
            {
                Msg["MenuTitle"] = "\n--- MENU EASY SAVE ---";
                Msg["List"] = "1. Lister les travaux";
                Msg["Add"] = "2. Ajouter un travail";
                Msg["Run"] = "3. LANCER TOUTES LES SAUVEGARDES";
                Msg["Quit"] = "4. Quitter";
                Msg["Choice"] = "Votre choix : ";
                Msg["Success"] = "Succès pour : ";
                Msg["Error"] = "Erreur : ";
                Msg["Copy"] = "Fichier copié : ";
                Msg["Start"] = "\n--- Démarrage du travail : ";
                Msg["SourceMissing"] = "Erreur : Source introuvable.";
                Msg["MaxJobs"] = "Erreur : Maximum 5 travaux !";
                Msg["EnterName"] = "Nom : ";
                Msg["EnterSource"] = "Source : ";
                Msg["EnterTarget"] = "Cible : ";
                Msg["EnterType"] = "Type (1 pour Complet, 2 pour Différentiel) : ";
                Msg["Saved"] = "Travail enregistré !";
                Msg["Deleted"] = "Travail supprimé !";
                Msg["DeletePrompt"] = "\nEntrez le numéro du travail à supprimer (ou Entrée pour annuler) : ";
                Msg["NoJob"] = "Aucun travail trouvé.";
                Msg["PressKey"] = "\nAppuyez sur une touche pour continuer...";
            }
        }
    }
}