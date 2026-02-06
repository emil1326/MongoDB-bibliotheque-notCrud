using System.Text.Json.Serialization;

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
            public List<string> MenuNames { get; set; } = [];
            public List<char> Chars { get; set; } = [];
            public List<Action> Actions { get; set; } = [];
            public Action? OnError { get; set; } = null;
        }

        /// <summary>
        /// Paramètres de l'application (sauvegardés en JSON)
        /// Ajoutez vos propres paramètres ici
        /// </summary>
        public class Settings
        {
            public string? MongoDbPassword { get; set; } = null;
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
            public List<string> Exemplaires { get; set; } = [];
            public int? Annee { get; set; }
            public string MaisonEdition { get; set; } = string.Empty;
            public string Auteur { get; set; } = string.Empty;
        }

        public class BandeDessinee : Livre
        {
            public string Dessinateur { get; set; } = string.Empty;
        }

        public class RepositoryOuvrages
        {
            public List<Ouvrage> Ouvrages { get; set; } = [];

            [JsonIgnore]
            public int NextId
            {
                get
                {
                    if (Ouvrages.Count == 0)
                        return 1;
                    return Ouvrages.Max(o => o.Id) + 1;
                }
            }

            public RepositoryOuvrages()
            {
            }

            public void AddOuvrage(Ouvrage ouvrage)
            {
                if (ouvrage.Id == 0)
                {
                    ouvrage.Id = NextId;
                }
                Ouvrages.Add(ouvrage);
            }

            public void RemoveOuvrage(Ouvrage ouvrage)
            {
                Ouvrages.Remove(ouvrage);
            }

            public void RemoveOuvrageById(int id)
            {
                var ouvrage = GetOuvrageById(id);
                if (ouvrage != null)
                {
                    RemoveOuvrage(ouvrage);
                }
            }

            public void UpdateOuvrage(Ouvrage updatedOuvrage)
            {
                var existing = GetOuvrageById(updatedOuvrage.Id);
                if (existing != null)
                {
                    int index = Ouvrages.IndexOf(existing);
                    Ouvrages[index] = updatedOuvrage;
                }
                else
                {
                    throw new ArgumentException($"Ouvrage with ID {updatedOuvrage.Id} not found.");
                }
            }

            public Ouvrage? GetOuvrageById(int id)
            {
                return Ouvrages.FirstOrDefault(o => o.Id == id);
            }

            public List<Ouvrage> GetAllOuvrages()
            {
                return Ouvrages;
            }

            public List<Ouvrage> GetOuvragesByQuery(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return GetAllOuvrages();

                var fieldFilters = ParseQuery(query.Trim());
                return [.. Ouvrages.Where(o => MatchesAllFilters(o, fieldFilters))];
            }

            #region regex back

            private Dictionary<string, List<string>> ParseQuery(string query)
            {
                var filters = new Dictionary<string, List<string>>();
                
                if (!query.Contains(':'))
                {
                    // Recherche simple globale
                    filters["global"] = [query];
                    return filters;
                }

                // Découper la requête en filtres individuels
                var parts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var part in parts)
                {
                    if (!part.Contains(':'))
                    {
                        AddFilter(filters, "global", part);
                        continue;
                    }

                    var keyValue = part.Split(':', 2);
                    if (keyValue.Length == 2)
                    {
                        string field = keyValue[0].ToLower();
                        string value = keyValue[1].Trim();
                        AddFilter(filters, field, value);
                    }
                }
                
                return filters;
            }

            private void AddFilter(Dictionary<string, List<string>> filters, string field, string value)
            {
                if (!filters.ContainsKey(field))
                    filters[field] = new List<string>();
                
                filters[field].Add(value);
            }

            private bool MatchesAllFilters(Ouvrage ouvrage, Dictionary<string, List<string>> filters)
            {
                foreach (var filter in filters)
                {
                    if (!AllValuesMatch(ouvrage, filter.Key, filter.Value))
                        return false;
                }
                return true;
            }

            private bool AllValuesMatch(Ouvrage ouvrage, string field, List<string> values)
            {
                foreach (var value in values)
                {
                    if (!MatchesSingleFilter(ouvrage, field, value))
                        return false;
                }
                return true;
            }

            private bool Match(string fieldValue, string pattern)
            {
                if (string.IsNullOrEmpty(fieldValue))
                    return false;
                
                var regex = new System.Text.RegularExpressions.Regex(
                    ToRegexPattern(pattern), 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                return regex.IsMatch(fieldValue);
            }

            private bool MatchGlobal(Ouvrage ouvrage, string pattern)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(ouvrage, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                
                var regex = new System.Text.RegularExpressions.Regex(
                    ToRegexPattern(pattern),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                return regex.IsMatch(json);
            }

            private string ToRegexPattern(string query)
            {
                // Convertir SQL LIKE vers regex: % → .* et _ → .
                string pattern = query.Replace("%", ".*").Replace("_", ".");
                
                // Si pas de wildcards, ajouter .* au début et fin
                if (!query.Contains('%') && !query.Contains('_'))
                {
                    pattern = ".*" + System.Text.RegularExpressions.Regex.Escape(pattern) + ".*";
                }
                else
                {
                    // Échapper les caractères spéciaux sauf .* déjà convertis
                    var parts = pattern.Split(new[] { ".*" }, StringSplitOptions.None);
                    for (int i = 0; i < parts.Length; i++)
                        parts[i] = System.Text.RegularExpressions.Regex.Escape(parts[i]);
                    
                    pattern = string.Join(".*", parts);
                }
                
                return pattern;
            }

            #endregion regex back

            private bool MatchesSingleFilter(Ouvrage ouvrage, string field, string value)
            {
                return field switch
                {
                    // Champs communs
                    "id" => Match(ouvrage.Id.ToString(), value),
                    "titre" => Match(ouvrage.Titre, value),
                    "dispo" or "disponibilite" => Match(ouvrage.Dispo.ToString(), value),
                    "prix" => Match(ouvrage.Prix.ToString(), value),

                    // Champs Livre/BD
                    "auteur" => ouvrage is Livre l && Match(l.Auteur, value),
                    "annee" or "année" => ouvrage is Livre lv && lv.Annee.HasValue && Match(lv.Annee.Value.ToString(), value),
                    "maison" or "maisonedition" or "edition" => ouvrage is Livre liv && Match(liv.MaisonEdition, value),
                    "exemplaires" or "exemplaire" => ouvrage is Livre livre && livre.Exemplaires.Any(ex => Match(ex, value)),

                    // Champs BD uniquement
                    "dessinateur" => ouvrage is BandeDessinee bd && Match(bd.Dessinateur, value),

                    // Champs Périodique
                    "date" => ouvrage is Periodique p && p.Date.HasValue && Match(p.Date.Value.ToString("yyyy-MM-dd"), value),
                    "periodicite" or "périodicité" => ouvrage is Periodique per && Match(per.Periodicite, value),

                    // Recherche globale
                    "global" => MatchGlobal(ouvrage, value),

                    _ => false
                };
            }


            public List<T> GetOuvragesByType<T>() where T : Ouvrage
            {
                return [.. Ouvrages.OfType<T>()];
            }

            public List<Ouvrage> GetOuvragesByTypeAndQuery<T>(string query) where T : Ouvrage
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return [.. GetOuvragesByType<T>().Cast<Ouvrage>()];
                }

                // Utiliser le même système de filtres typés
                return [.. GetOuvragesByQuery(query).OfType<T>().Cast<Ouvrage>()];
            }
        }

        public static bool TryAskUsers<T>(string Question, out T response, bool UseChar = false, bool Erase = false, bool inline = true)
        {
            try
            {
                response = AskUsers<T>(Question, UseChar, Erase, inline);
                return true;
            }
            catch
            {
                response = default;
                return false;
            }
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
