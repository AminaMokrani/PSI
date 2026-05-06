
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    public class ServicePersistance
    {
        private readonly string _connectionString;

        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";

            // Teste la connexion dès la construction ; lève une exception si elle échoue
            using var conn = OpenConnection();
        }


        // ─── Sauvegarde d'un graphe ──────────────────────────────────────────

        public uint SaveGraph(Graph g)
        {
            using var conn = OpenConnection();

            // 1. Insérer la ligne Graphe
            uint graphId;
            using (var cmd = new MySqlCommand(
                "INSERT INTO Graphe(est_oriente, nb_sommets) VALUES (@o, @n); SELECT LAST_INSERT_ID();",
                conn))
            {
                cmd.Parameters.AddWithValue("@o", g.Directed ? 1 : 0);
                cmd.Parameters.AddWithValue("@n", g.Order);
                graphId = Convert.ToUInt32(cmd.ExecuteScalar());
            }

            // 2. Insérer les sommets et mémoriser nom → id BDD
            var nameToId = new Dictionary<string, uint>();
            foreach (var name in g.VertexNames)
            {
                using var cmd = new MySqlCommand(
                    "INSERT INTO Sommet(graphe_id, nom, valeur) VALUES (@gid, @nom, @val); SELECT LAST_INSERT_ID();",
                    conn);
                cmd.Parameters.AddWithValue("@gid", graphId);
                cmd.Parameters.AddWithValue("@nom", name);
                cmd.Parameters.AddWithValue("@val", g.GetVertexValue(name));
                nameToId[name] = Convert.ToUInt32(cmd.ExecuteScalar());
            }

            // 3. Insérer les arcs
            // Pour un graphe non orienté : on stocke seulement une direction (j > i)
            // afin d'éviter les doublons. LoadGraph appellera AddEdge qui recrée les deux sens.
            var names = g.VertexNames;
            for (int i = 0; i < names.Count; i++)
            {
                foreach (var dest in g.GetNeighbors(names[i]))
                {
                    int j = names.IndexOf(dest);
                    if (!g.Directed && j < i) continue;

                    using var cmd = new MySqlCommand(
                        "INSERT INTO Arc(graphe_id, sommet_source, sommet_dest, poids) VALUES (@gid, @src, @dst, @w);",
                        conn);
                    cmd.Parameters.AddWithValue("@gid", graphId);
                    cmd.Parameters.AddWithValue("@src", nameToId[names[i]]);
                    cmd.Parameters.AddWithValue("@dst", nameToId[dest]);
                    cmd.Parameters.AddWithValue("@w", g.GetEdgeWeight(names[i], dest));
                    cmd.ExecuteNonQuery();
                }
            }

            return graphId;
        }


        // ─── Chargement d'un graphe ──────────────────────────────────────────

        public Graph LoadGraph(uint id)
        {
            using var conn = OpenConnection();

            // 1. Métadonnées du graphe
            bool directed;
            using (var cmd = new MySqlCommand(
                "SELECT est_oriente FROM Graphe WHERE id = @id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                if (result == null)
                    throw new Exception($"Graphe introuvable : id={id}");
                directed = Convert.ToBoolean(result);
            }

            var graph = new Graph(directed);
            var idToName = new Dictionary<uint, string>();

            // 2. Recréer les sommets dans leur ordre d'insertion (ORDER BY id)
            using (var cmd = new MySqlCommand(
                "SELECT id, nom, valeur FROM Sommet WHERE graphe_id = @gid ORDER BY id;", conn))
            {
                cmd.Parameters.AddWithValue("@gid", id);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    uint sid = Convert.ToUInt32(reader["id"]);
                    string nom = (string)reader["nom"];
                    float valeur = reader["valeur"] == DBNull.Value ? 0f : Convert.ToSingle(reader["valeur"]);
                    graph.AddVertex(nom, valeur);
                    idToName[sid] = nom;
                }
            }

            // 3. Recréer les arcs
            using (var cmd = new MySqlCommand(
                "SELECT sommet_source, sommet_dest, poids FROM Arc WHERE graphe_id = @gid;", conn))
            {
                cmd.Parameters.AddWithValue("@gid", id);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string src = idToName[Convert.ToUInt32(reader["sommet_source"])];
                    string dst = idToName[Convert.ToUInt32(reader["sommet_dest"])];
                    float poids = Convert.ToSingle(reader["poids"]);
                    graph.AddEdge(src, dst, poids);
                }
            }

            return graph;
        }


        // ─── Sauvegarde d'une tournée ────────────────────────────────────────

        public uint SaveTour(uint graphId, Tour t)
        {
            using var conn = OpenConnection();

            // 1. Insérer la ligne Tournee
            uint tourId;
            using (var cmd = new MySqlCommand(
                "INSERT INTO Tournee(graphe_id, cout_total) VALUES (@gid, @c); SELECT LAST_INSERT_ID();",
                conn))
            {
                cmd.Parameters.AddWithValue("@gid", graphId);
                cmd.Parameters.AddWithValue("@c", t.Cost);
                tourId = Convert.ToUInt32(cmd.ExecuteScalar());
            }

            // 2. Insérer chaque étape avec son numéro d'ordre et le sommet_id correspondant
            var vertices = t.Vertices;
            for (int i = 0; i < vertices.Count; i++)
            {
                // Recherche de l'id du sommet dans la base (nom + graphe_id)
                uint sommetId;
                using (var cmd = new MySqlCommand(
                    "SELECT id FROM Sommet WHERE graphe_id = @gid AND nom = @nom LIMIT 1;", conn))
                {
                    cmd.Parameters.AddWithValue("@gid", graphId);
                    cmd.Parameters.AddWithValue("@nom", vertices[i]);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                        throw new Exception($"Sommet '{vertices[i]}' introuvable dans le graphe {graphId}");
                    sommetId = Convert.ToUInt32(result);
                }

                using var ins = new MySqlCommand(
                    "INSERT INTO EtapeTournee(tournee_id, numero_ordre, sommet_id) VALUES (@tid, @ord, @sid);",
                    conn);
                ins.Parameters.AddWithValue("@tid", tourId);
                ins.Parameters.AddWithValue("@ord", i);
                ins.Parameters.AddWithValue("@sid", sommetId);
                ins.ExecuteNonQuery();
            }

            return tourId;
        }


        // ─── Chargement d'une tournée ────────────────────────────────────────

        public Tour LoadTour(uint id)
        {
            using var conn = OpenConnection();

            // 1. Coût total
            float cout;
            using (var cmd = new MySqlCommand(
                "SELECT cout_total FROM Tournee WHERE id = @id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                if (result == null)
                    throw new Exception($"Tournée introuvable : id={id}");
                cout = Convert.ToSingle(result);
            }

            // 2. Séquence ordonnée des sommets via jointure EtapeTournee ↔ Sommet
            var vertices = new List<string>();
            using (var cmd = new MySqlCommand(
                "SELECT s.nom FROM EtapeTournee e " +
                "JOIN Sommet s ON e.sommet_id = s.id " +
                "WHERE e.tournee_id = @tid " +
                "ORDER BY e.numero_ordre;", conn))
            {
                cmd.Parameters.AddWithValue("@tid", id);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    vertices.Add((string)reader["nom"]);
            }

            return new Tour(vertices, cout);
        }


        // ─── Helper privé ────────────────────────────────────────────────────

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}