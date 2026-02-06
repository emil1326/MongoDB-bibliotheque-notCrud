using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using static EmilsCMS.classes.CMSClasses;

namespace EmilsCMS.services
{
    /// <summary>
    /// Service MongoDB simplifié - Remplace le JSON local
    /// </summary>
    internal class MongoDBService
    {
        private readonly IMongoCollection<OuvrageDocument> _collection;

        public MongoDBService(string password)
        {
            ConfigureBsonMaps();

            var connectionString = $"mongodb+srv://emiliendevauchelle_db_user:{password}@nocrudwa.tyhfbfz.mongodb.net/?appName=noCrudWA";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BibliothequeDB");
            _collection = database.GetCollection<OuvrageDocument>("Ouvrages");
        }

        private void ConfigureBsonMaps()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(OuvrageDocument)))
            {
                BsonClassMap.RegisterClassMap<OuvrageDocument>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIsRootClass(true);
                });
                BsonClassMap.RegisterClassMap<LivreDocument>();
                BsonClassMap.RegisterClassMap<BandeDessineeDocument>();
                BsonClassMap.RegisterClassMap<PeriodiqueDocument>();
            }
        }

        /// <summary>
        /// Test de connexion
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                _collection.Database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Sauvegarde tous les ouvrages (remplace tout)
        /// </summary>
        public async Task SaveAllAsync(RepositoryOuvrages repo)
        {
            try
            {
                // Préparer les documents AVANT de supprimer
                var docs = repo.Ouvrages.Select(ToDocument).ToList();
                
                // Supprimer les anciennes données
                await _collection.DeleteManyAsync(Builders<OuvrageDocument>.Filter.Empty);
                
                // Insérer les nouvelles données (seulement si on a des ouvrages)
                if (docs.Count > 0)
                {
                    await _collection.InsertManyAsync(docs);
                }
            }
            catch (Exception ex)
            {
                // Propager l'exception avec plus de détails
                throw new Exception($"Erreur lors de la sauvegarde MongoDB : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Charge tous les ouvrages
        /// </summary>
        public async Task<RepositoryOuvrages> LoadAllAsync()
        {
            var repo = new RepositoryOuvrages();
            var docs = await _collection.Find(_ => true).ToListAsync();
            repo.Ouvrages = docs.Select(FromDocument).ToList();
            return repo;
        }

        // Conversion Ouvrage ? Document
        private OuvrageDocument ToDocument(Ouvrage o) => o switch
        {
            BandeDessinee bd => new BandeDessineeDocument
            {
                Id = bd.Id,
                Titre = bd.Titre,
                Dispo = bd.Dispo,
                Prix = bd.Prix,
                Auteur = bd.Auteur,
                Dessinateur = bd.Dessinateur,
                Annee = bd.Annee,
                MaisonEdition = bd.MaisonEdition,
                Exemplaires = bd.Exemplaires
            },
            Livre l => new LivreDocument
            {
                Id = l.Id,
                Titre = l.Titre,
                Dispo = l.Dispo,
                Prix = l.Prix,
                Auteur = l.Auteur,
                Annee = l.Annee,
                MaisonEdition = l.MaisonEdition,
                Exemplaires = l.Exemplaires
            },
            Periodique p => new PeriodiqueDocument
            {
                Id = p.Id,
                Titre = p.Titre,
                Dispo = p.Dispo,
                Prix = p.Prix,
                Date = p.Date,
                Periodicite = p.Periodicite
            },
            _ => new OuvrageDocument { Id = o.Id, Titre = o.Titre, Dispo = o.Dispo, Prix = o.Prix }
        };

        // Conversion Document ? Ouvrage
        private Ouvrage FromDocument(OuvrageDocument d) => d switch
        {
            BandeDessineeDocument bd => new BandeDessinee
            {
                Id = bd.Id,
                Titre = bd.Titre,
                Dispo = bd.Dispo,
                Prix = bd.Prix,
                Auteur = bd.Auteur,
                Dessinateur = bd.Dessinateur,
                Annee = bd.Annee,
                MaisonEdition = bd.MaisonEdition,
                Exemplaires = bd.Exemplaires ?? []
            },
            LivreDocument l => new Livre
            {
                Id = l.Id,
                Titre = l.Titre,
                Dispo = l.Dispo,
                Prix = l.Prix,
                Auteur = l.Auteur,
                Annee = l.Annee,
                MaisonEdition = l.MaisonEdition,
                Exemplaires = l.Exemplaires ?? []
            },
            PeriodiqueDocument p => new Periodique
            {
                Id = p.Id,
                Titre = p.Titre,
                Dispo = p.Dispo,
                Prix = p.Prix,
                Date = p.Date,
                Periodicite = p.Periodicite
            },
            _ => new Ouvrage { Id = d.Id, Titre = d.Titre, Dispo = d.Dispo, Prix = d.Prix }
        };
    }

    #region Documents MongoDB

    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(LivreDocument), typeof(BandeDessineeDocument), typeof(PeriodiqueDocument))]
    internal class OuvrageDocument
    {
        [BsonId]
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public int Dispo { get; set; }
        public decimal Prix { get; set; }
    }

    internal class LivreDocument : OuvrageDocument
    {
        public List<string>? Exemplaires { get; set; }
        public int? Annee { get; set; }
        public string MaisonEdition { get; set; } = string.Empty;
        public string Auteur { get; set; } = string.Empty;
    }

    internal class BandeDessineeDocument : LivreDocument
    {
        public string Dessinateur { get; set; } = string.Empty;
    }

    internal class PeriodiqueDocument : OuvrageDocument
    {
        public DateTime? Date { get; set; }
        public string Periodicite { get; set; } = string.Empty;
    }

    #endregion
}
