namespace TourneeFutee
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║             TourneeFutée             ║");
            Console.WriteLine("║  Planificateur de tournée optimale   ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

            // Graphe des distances entre 6 grandes villes françaises (km à vol d'oiseau)
            var villes = new List<string> { "Nantes", "Paris", "Toulouse", "Lille", "Marseille", "Strasbourg" };
            var graph = BuildGraphVillesFrance(villes);

            // Affichage des villes disponibles
            Console.WriteLine("Villes disponibles :");
            for (int i = 0; i < villes.Count; i++)
                Console.WriteLine($"  {i + 1}. {villes[i]}");

            // Choix de la ville de départ
            Console.WriteLine();
            Console.Write("Choisissez votre ville de départ (numéro) : ");
            int choix = LireChoix(1, villes.Count);
            string villeDepart = villes[choix - 1];

            Console.WriteLine();
            Console.WriteLine($" Calcul de la tournée optimale au départ de {villeDepart}...");
            Console.WriteLine();

            // Calcul de la tournée optimale
            var little = new Little(graph);
            Tour tour = little.ComputeOptimalTour();

            // Réorganiser la tournée pour commencer par la ville choisie
            var sequence = ReorganiserDepuis(tour.Vertices.ToList(), villeDepart);

            // Affichage du résultat
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║           Tournée optimale           ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

            float distanceTotale = 0;
            for (int i = 0; i < sequence.Count - 1; i++)
            {
                string src = sequence[i];
                string dst = sequence[i + 1];
                float dist = graph.GetEdgeWeight(src, dst);
                distanceTotale += dist;
                Console.WriteLine($"{src,-12} ==  {dst,-12}  ({dist} km)");
            }

            Console.WriteLine();
            Console.WriteLine($"Distance totale : {distanceTotale} km");
            Console.WriteLine();

            // Sauvegarde optionnelle en base
            Console.Write("Voulez-vous sauvegarder cette tournée en base de données? (o/n) : ");
            string? reponse = Console.ReadLine();
            if (reponse?.ToLower() == "o") SauvegarderEnBase(graph, tour);

            Console.WriteLine();
            Console.WriteLine("Appuyez sur une touche pour quitter !! au revoir");
            Console.ReadKey();
        }


        static Graph BuildGraphVillesFrance(List<string> villes)
        {
            float[,] distances = {
                { 0,343,465, 507, 697,710 },  
                { 343, 0,588,210, 661,397 }, 
                { 465,588,0,791,407,737 },  
                { 507,210,791,0,834,397 }, 
                { 697,661,407,834,0,615 }, 
                { 710,397,737,397,615,0 }, 
            };

            var g = new Graph(directed: false);
            foreach (var v in villes) g.AddVertex(v);

            for (int i = 0; i < villes.Count; i++)
                for (int j = i + 1; j < villes.Count; j++)
                    g.AddEdge(villes[i], villes[j], distances[i, j]);

            return g;
        }


        static List<string> ReorganiserDepuis(List<string> vertices, string depart)
        {
            // Trouver l'index de la ville de départ dans la séquence 
            var boucle = vertices.Take(vertices.Count - 1).ToList();
            int idx = boucle.IndexOf(depart);
            if (idx == -1) return vertices;

            // Rotation de la liste et fermeture du cycle
            var reordonne = boucle.Skip(idx).Concat(boucle.Take(idx)).ToList();
            reordonne.Add(reordonne[0]);
            return reordonne;
        }

        static int LireChoix(int min, int max)
        {
            while (true)
            {
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int val) && val >= min && val <= max) return val;
                Console.Write($"Entrée invalide. Entrez un nombre entre {min}et{max} : ");
            }
        }



        static void SauvegarderEnBase(Graph graph, Tour tour)
        {
            Console.Write("Adresse du serveur MySQL (défaut: 127.0.0.1) : ");
            string server = Console.ReadLine() is { Length: > 0 } s ? s : "127.0.0.1";
            Console.Write("Nom de la base (défaut: tourneefutee) : ");
            string db = Console.ReadLine() is { Length: > 0 } d ? d : "tourneefutee";
            Console.Write("Utilisateur (défaut: root) : ");
            string user = Console.ReadLine() is { Length: > 0 } u ? u : "root";
            Console.Write("Mot de passe : ");
            string pwd = Console.ReadLine() ?? "";

            try
            {
                var service = new ServicePersistance(server, db, user, pwd);
                uint graphId = service.SaveGraph(graph);
                uint tourId = service.SaveTour(graphId, tour);
                Console.WriteLine($"Tournée sauvegardée (graphe id={graphId}, tournée id={tourId})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
    }
}