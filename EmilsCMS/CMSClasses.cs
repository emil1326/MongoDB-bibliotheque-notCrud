namespace EmilsCMS.classes
{
    internal class CMSClasses
    {
        // =============================================================================
        // CLASSES DE SUPPORT
        // =============================================================================
        // Classes utilisées par le template
        // Ajoutez vos propres classes en dessous
        // =============================================================================

        /// <summary>
        /// Structure d'un menu : noms affichés, touches et actions associées
        /// </summary>
        public class MenuChar
        {
            public List<string> MenuNames { get; set; } = new();
            public List<char> Chars { get; set; } = new();
            public List<Action> Actions { get; set; } = new();
            public Action OnError { get; set; }
        }

        /// <summary>
        /// Paramètres de l'application (sauvegardés en JSON)
        /// Ajoutez vos propres paramètres ici
        /// </summary>
        public class Settings
        {
            public bool SaveOnExit { get; set; } = true;
            public bool LoadOnStartup { get; set; } = true;
        }

        // =============================================================================
        // VOS CLASSES PERSONNALISÉES
        // =============================================================================
        // Ouvrage représente tous les types (périodique, livre, BD) avec champs
        // communs et un objet générique `Details` pour les propriétés spécifiques.
        // =============================================================================

        public class Ouvrage
        {
            public int Id { get; set; }
            public string Titre { get; set; } = string.Empty;
            public int Dispo { get; set; }
            public decimal Prix { get; set; }
        }

        public class Periodique : Ouvrage
        {
            public DateTime? Date { get; set; }
            public string Periodicite { get; set; } = string.Empty;
        }

        public class Livre : Ouvrage
        {
            public List<string> Exemplaires { get; set; } = new();
            public int? Annee { get; set; }
            public string MaisonEdition { get; set; } = string.Empty;
            public string Auteur { get; set; } = string.Empty;
        }

        public class BandeDessinee : Livre
        {
            public string Dessinateur { get; set; } = string.Empty;
        }

        public static T AskUsers<T>(string Question, bool UseChar = false, bool Erase = false, bool inline = true)
        {
            if (inline)
                Console.Write(Question);
            else
                Console.WriteLine(Question);

            string? text = UseChar ? Console.ReadKey(Erase).KeyChar.ToString() : Console.ReadLine();

            if (text == null)
                return default;

            try
            {
                if (typeof(T) == typeof(float) || typeof(T) == typeof(double) || typeof(T) == typeof(decimal))
                {
                    text = text.Replace('.', ',');
                }

                return (T)Convert.ChangeType(text, typeof(T));
            }
            catch (Exception e)
            {
                // Wrap and preserve original exception (keeps stack trace in InnerException)
                throw new InvalidCastException("Unsupported return type or invalid input in AskUsers.", e);
            }
        }
    }
}
