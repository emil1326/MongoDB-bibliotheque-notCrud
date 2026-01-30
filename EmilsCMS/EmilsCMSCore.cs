using Newtonsoft.Json;
using static EmilsCMS.classes.CMSClasses;
using Formatting = Newtonsoft.Json.Formatting;

namespace EmilsCMS.core
{
    internal class EmilsCMSCore
    {
        // =================================================================
        // VARIABLES GLOBALES
        // =================================================================
        // Ces variables sont accessibles par toutes les fonctions locales
        // Ajoutez vos propres listes/données ici pour les rendre persistantes
        // =================================================================

        const string AppVersion = "1.0";        // Version de l'application
        const string AppName = "Template App";  // Nom de l'application
        const string SettingsFile = "settings.json";  // Fichier de configuration
        const string Createur = "Emilien Devauchelle et Jonathan Basque"; // Information de companie
        const string Companie = "Emil's works"; // Information de comapnie
        const string AppHeader = @"
 /$$      /$$                                         /$$ /$$$$$$$  /$$$$$$$ 
| $$$    /$$$                                        | $$| $$__  $$| $$__  $$
| $$$$  /$$$$  /$$$$$$  /$$$$$$$   /$$$$$$   /$$$$$$ | $$| $$  \ $$| $$  \ $$
| $$ $$/$$ $$ /$$__  $$| $$__  $$ /$$__  $$ /$$__  $$| $$| $$  | $$| $$$$$$$ 
| $$  $$$| $$| $$  \ $$| $$  \ $$| $$  \ $$| $$  \ $$| $$| $$  | $$| $$__  $$
| $$\  $ | $$| $$  | $$| $$  | $$| $$  | $$| $$  | $$| $$| $$  | $$| $$  \ $$
| $$ \/  | $$|  $$$$$$/| $$  | $$|  $$$$$$$|  $$$$$$/| $$| $$$$$$$/| $$$$$$$/
|__/     |__/ \______/ |__/  |__/ \____  $$ \______/ |__/|_______/ |_______/ 
                                  /$$  \ $$                                  
                                 |  $$$$$$/                                  
                                  \______/                                   
                  /$$                                                                
                 /$$/                                                                
         /$$    /$$/                                                                 
        |__/   /$$/                                                                  
              /$$/                                                                   
         /$$ /$$/                                                                    
        |__//$$/                                                                     
           |__/                                                                      
";

        MenuChar currentMenu = new();           // Menu actuellement affiché
        Settings settings = new();              // Paramètres de l'application
        bool needLoad = true;                   // Flag pour charger les données au démarrage

        // Stockage des ouvrages en mémoire
        List<Ouvrage> ouvrages = new();
        int nextId = 1;

        // =================================================================
        // POINT D'ENTRÉE - Gestion des erreurs fatales
        // =================================================================
        // Le try/catch global permet de redémarrer l'app en cas de crash
        // =================================================================


        public void Run()
        {
            //try
            {
                MainMenu();
            }
            //catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("=== ERREUR FATALE ===");
                //Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Appuyez sur une touche pour redémarrer...");
                Console.ReadKey();
                MainMenu();
            }

            ExitApp(); // Sécurité : ne devrait jamais être atteint
        }

        // =================================================================
        // MENU PRINCIPAL
        // =================================================================
        // Point d'entrée de l'application après le chargement
        // Modifiez MenuNames, Chars et Actions pour personnaliser le menu
        // =================================================================

        void MainMenu(bool showError = false)
        {
            Console.Clear();

            // Affiche un message d'erreur si la commande précédente était invalide
            if (showError)
            {
                Console.WriteLine("[!] Commande non reconnue");
                Console.WriteLine();
            }

            // --- En-tête de l'application ---
            Console.WriteLine(AppHeader);
            Console.WriteLine($"=== {AppName} v{AppVersion} ===");
            Console.WriteLine();

            // --- Chargement des données au premier affichage ---
            if (needLoad)
            {
                LoadSettings();

                // Si le chargement automatique est activé, charger les données
                if (settings.LoadOnStartup)
                {
                    LoadUserData();
                }

                needLoad = false;
                Console.WriteLine();
            }

            // --- Configuration du menu principal ---
            // MenuNames : texte affiché pour chaque option
            // Chars : touche associée à chaque option (en minuscule)
            // Actions : fonction à exécuter pour chaque option
            // IMPORTANT : le nombre d'éléments doit correspondre entre les trois listes

            currentMenu = new MenuChar
            {
                MenuNames =
                [
                    "=== MENU PRINCIPAL ===",
                    "",
                    "1. Lister tous les ouvrages",
                    "2. Ajouter un ouvrage",
                    "",
                    "",
                    "S. Paramètres",
                    "I. Informations",
                    "",
                    "Q. Quitter"
                ],
                Chars = ['1', '2', '3', 's', 'i', 'q'],
                Actions =
                [
                    () => ListAll(),
                    () => CreateOuvrage(),
                    () => HiddenMenu(),
                    () => SettingsMenu(),
                    () => ShowInfo(),
                    () => ExitApp()
                ]
            };

            // Affiche le menu et attend une entrée utilisateur
            ProcessMenuInput(currentMenu);
        }

        // =================================================================
        // MODULES - Ajoutez votre logique métier ici
        // =================================================================
        // Chaque module est une fonction séparée pour garder le code organisé
        // Terminez toujours par un appel au menu (MainMenu() ou autre)
        // =================================================================

        void ListAll()
        {
            Console.Clear();
            Console.WriteLine("=== Rechercher des ouvrages ===");
            Console.WriteLine();
            Console.WriteLine("Rechercher à travers les ouvrages de la librairie.");
            Console.WriteLine();

            // --- Exemple de sous-menu ---
            currentMenu = new MenuChar
            {
                MenuNames =
                [
                    "1. Voir tous les items",
                    "2. Rechercher par type",
                    "3. Rechercher par dessinateur (Bandes dessinées uniquement)",
                    "",
                    "Q. Retour"
                ],
                Chars = ['1', '2', '3', 'q'],
                Actions =
                [
                    () => { ShowAllItems(); },
                    () => { ShowAllItems(); },
                    () => { ShowAllItems(); },
                    () => MainMenu()
                ],
                OnError = () => { ListAll(); }
            };

            ProcessMenuInput(currentMenu);
        }

        void ShowAllItems(string searchQuery = "")
        {
            Console.Clear();
            Console.WriteLine("=== LISTE DES OUVRAGES ===");
            Console.WriteLine();

            if (ouvrages.Count == 0)
            {
                Console.WriteLine("Aucun ouvrage enregistré.");
                Console.WriteLine("Appuyez sur Entrée pour revenir...");
                Console.ReadLine();
            }
            else
            {
                foreach (var o in ouvrages)
                {
                    Console.WriteLine($"#{o.Id} - {o.Titre} ({o.Dispo} dispo) - {o.Prix:C}");
                    switch (o)
                    {
                        case BandeDessinee bd:
                            Console.WriteLine($"  Type : Bande Dessinée | Auteur : {bd.Auteur} | Dessinateur : {bd.Dessinateur}");
                            if (bd.Exemplaires?.Count > 0) Console.WriteLine($"  Exemplaires: {string.Join(", ", bd.Exemplaires)}");
                            if (bd.Annee.HasValue) Console.WriteLine($"  Année : {bd.Annee}");
                            break;
                        case Livre livre:
                            Console.WriteLine($"  Type : Livre | Auteur : {livre.Auteur}");
                            if (livre.Exemplaires?.Count > 0) Console.WriteLine($"  Exemplaires: {string.Join(", ", livre.Exemplaires)}");
                            if (livre.Annee.HasValue) Console.WriteLine($"  Année : {livre.Annee}");
                            break;
                        case Periodique p:
                            Console.WriteLine($"  Type : Périodique | Périodicité : {p.Periodicite} | Date : {p.Date?.ToShortDateString()}");
                            break;
                        default:
                            Console.WriteLine("  Type : Inconnu");
                            break;
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("Appuyez sur Entrée pour revenir...");
                Console.ReadLine();
            }

            ListAll();
        }

        void CreateOuvrage()
        {
            Console.Clear();
            Console.WriteLine("=== Ajouter un ouvrage ===");
            Console.WriteLine("Enregistrer dans notre base de donner votre livre maintenant!");
            Console.WriteLine();

            string titre = AskUsers<string>("Titre de l'ouvrage : ");
            int dispo = AskUsers<int>("Nombre d'exemplaires disponibles : ");
            decimal prix = AskUsers<decimal>("Prix de l'ouvrage (en $) : ");
            Console.WriteLine();
            Console.WriteLine("Quel est le type d'ouvrage ?");

            currentMenu = new MenuChar
            {
                MenuNames =
                [
                    "1. Ajouter un livre",
                    "2. Ajouter une bande dessinée",
                    "3. Enregistrer un périodique",
                ],
                Chars = ['1', '2', '3'],
                Actions =
                [
                    () => { AddSub(new Livre(){Titre = titre, Dispo = dispo, Prix = prix}); },
                    () => { AddSub(new BandeDessinee(){Titre = titre, Dispo = dispo, Prix = prix}); },
                    () => { AddSub(new Periodique(){Titre = titre, Dispo = dispo, Prix = prix}); },
                ],
                OnError = () => { CreateOuvrage(); }
            };

            ProcessMenuInput(currentMenu);
        }

        void AddSub(Ouvrage BaseOuvrage)
        {
            // Collect specific fields based on concrete type
            switch (BaseOuvrage)
            {
                case BandeDessinee bd:
                    bd.Auteur = AskUsers<string>("Auteur de la BD : ");
                    bd.Dessinateur = AskUsers<string>("Dessinateur de la BD : ");
                    var exBd = AskUsers<string>("Exemplaires (séparés par des virgules, vide si aucun) : ");
                    if (!string.IsNullOrWhiteSpace(exBd)) bd.Exemplaires = exBd.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                    bd.Annee = AskUsers<int?>("Année (laisser vide si inconnue) : ");
                    break;

                case Livre livre:
                    livre.Auteur = AskUsers<string>("Auteur du livre : ");
                    var ex = AskUsers<string>("Exemplaires (séparés par des virgules, vide si aucun) : ");
                    if (!string.IsNullOrWhiteSpace(ex)) livre.Exemplaires = ex.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                    livre.Annee = AskUsers<int>("Année (laisser vide si inconnue) : ");
                    livre.MaisonEdition = AskUsers<string>("Maison d'édition : ");
                    break;

                case Periodique periodique:
                    periodique.Periodicite = AskUsers<string>("Périodicité du périodique : ");
                    periodique.Date = AskUsers<DateTime?>("Date de publication (yyyy-mm-dd, vide si inconnue) : ");
                    break;
            }

            // Assign Id and store
            if (BaseOuvrage.Id <= 0) BaseOuvrage.Id = nextId++;
            ouvrages.Add(BaseOuvrage);
            SaveUserData();

            Console.WriteLine("Ouvrage ajouté avec succès.");
            Console.WriteLine("Appuyez sur Entrée pour revenir au menu principal...");
            Console.ReadLine();
            MainMenu();
        }

        void HiddenMenu()
        {
            Console.Clear();
            Console.WriteLine("=== MODULE caché :p ===");
            Console.WriteLine();
            Console.WriteLine("Bonjour!");
            Console.WriteLine();
            Console.WriteLine("Appuyez sur Entrée pour revenir...");
            Console.ReadLine();
            MainMenu();
        }

        void ShowInfo()
        {
            Console.Clear();
            Console.WriteLine("=== INFORMATIONS ===");
            Console.WriteLine();
            Console.WriteLine($"Application : {AppName}");
            Console.WriteLine($"Version : {AppVersion}");
            Console.WriteLine();
            Console.WriteLine("Ce template fournit :");
            Console.WriteLine("- Système de menu navigable");
            Console.WriteLine("- Sauvegarde/chargement JSON automatique");
            Console.WriteLine("- Structure modulaire extensible");
            Console.WriteLine();
            Console.WriteLine($"Application faite par {Createur} dans le but de faire une preuve de concept MongoDB");
            Console.WriteLine();
            Console.WriteLine("Appuyez sur Entrée pour revenir...");
            Console.ReadLine();
            MainMenu();
        }

        // =================================================================
        // PARAMÈTRES
        // =================================================================
        // Menu de configuration de l'application
        // Les paramètres sont sauvegardés dans settings.json
        // =================================================================

        void SettingsMenu(bool showError = false)
        {
            Console.Clear();

            if (showError)
            {
                Console.WriteLine("[!] Commande non reconnue");
                Console.WriteLine();
            }

            Console.WriteLine("=== PARAMÈTRES ===");
            Console.WriteLine();

            currentMenu = new MenuChar
            {
                MenuNames =
                [
                    $"1. Sauvegarder à la fermeture : {(settings.SaveOnExit ? "OUI" : "NON")}",
                    $"2. Charger au démarrage : {(settings.LoadOnStartup ? "OUI" : "NON")}",
                    "",
                    "S. Sauvegarder maintenant",
                    "",
                    "Q. Retour au menu principal"
                ],
                Chars = ['1', '2', 's', 'q'],
                Actions =
                [
                    () => { settings.SaveOnExit = !settings.SaveOnExit; SettingsMenu(); },
                    () => { settings.LoadOnStartup = !settings.LoadOnStartup; SettingsMenu(); },
                    () => { SaveSettings(); SettingsMenu(); },
                    () => MainMenu()
                ],
                OnError = () => { SettingsMenu(true); }
            };

            ProcessMenuInput(currentMenu);
        }

        // =================================================================
        // CHARGEMENT / SAUVEGARDE
        // =================================================================
        // Fonctions pour persister les données entre les sessions
        // Modifiez ces fonctions pour ajouter vos propres données
        // =================================================================

        void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    Settings? loaded = JsonConvert.DeserializeObject<Settings>(json);
                    if (loaded != null)
                    {
                        settings = loaded;
                        Console.WriteLine($"[OK] Paramètres chargés ({SettingsFile})");
                    }
                }
                else
                {
                    Console.WriteLine("[INFO] Aucun fichier de paramètres trouvé, utilisation des valeurs par défaut");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] Impossible de charger les paramètres : {ex.Message}");
            }
        }

        void SaveSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
                Console.WriteLine($"[OK] Paramètres sauvegardés ({SettingsFile})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] Impossible de sauvegarder : {ex.Message}");
            }
        }

        /// <summary>
        /// Charge les données utilisateur depuis les fichiers JSON
        /// Ajoutez ici le chargement de vos propres données
        /// </summary>
        void LoadUserData()
        {
            Console.WriteLine("[INFO] Chargement des données utilisateur...");
            try
            {
                if (File.Exists("data.json"))
                {
                    string json = File.ReadAllText("data.json");
                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                    var data = JsonConvert.DeserializeObject<List<Ouvrage>>(json, settings);
                    if (data != null)
                    {
                        ouvrages = data;
                        nextId = ouvrages.Any() ? ouvrages.Max(o => o.Id) + 1 : 1;
                        Console.WriteLine("[OK] Données chargées");
                    }
                }
                else
                {
                    Console.WriteLine("[INFO] Aucun fichier data.json trouvé, démarrage avec une liste vide");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] Impossible de charger les données : {ex.Message}");
            }
            Console.WriteLine("[OK] Chargement terminé");
        }

        /// <summary>
        /// Sauvegarde les données utilisateur dans les fichiers JSON
        /// Ajoutez ici la sauvegarde de vos propres données
        /// </summary>
        void SaveUserData()
        {
            Console.WriteLine("[INFO] Sauvegarde des données utilisateur...");
            try
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
                string json = JsonConvert.SerializeObject(ouvrages, settings);
                File.WriteAllText("data.json", json);
                Console.WriteLine("[OK] Données sauvegardées");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] {ex.Message}");
            }
            Console.WriteLine("[OK] Sauvegarde terminée");
        }

        // =================================================================
        // FERMETURE DE L'APPLICATION
        // =================================================================

        void ExitApp()
        {
            Console.Clear();
            Console.WriteLine("=== FERMETURE ===");
            Console.WriteLine();

            // Sauvegarde automatique si activée dans les paramètres
            if (settings.SaveOnExit)
            {
                SaveSettings();
                SaveUserData();
            }

            Console.WriteLine();
            Console.WriteLine("Au revoir !");
            Environment.Exit(0);
        }

        // =================================================================
        // SYSTÈME DE MENU
        // =================================================================
        // Ne pas modifier sauf si vous comprenez le fonctionnement
        // =================================================================

        /// <summary>
        /// Affiche le menu et traite l'entrée utilisateur
        /// </summary>
        /// <param name="menu">Configuration du menu à afficher</param>
        void ProcessMenuInput(MenuChar menu)
        {
            // Affiche toutes les lignes du menu
            foreach (string line in menu.MenuNames)
            {
                Console.WriteLine(line);
            }

            // Attend une touche et la convertit en minuscule
            char input = char.ToLower(Console.ReadKey(true).KeyChar);
            Console.WriteLine();

            // Cherche l'action correspondante
            for (int i = 0; i < menu.Chars.Count; i++)
            {
                if (menu.Chars[i] == input)
                {
                    menu.Actions[i]();
                    return;
                }
            }

            if (menu.OnError != null)
            {
                menu.OnError();
                return;
            }
            else
                // Si aucune touche valide, retour au menu principal avec erreur
                MainMenu(true);
        }

    }
}

