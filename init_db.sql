-- =============================================================================
-- PSI 2025-2026 – Objectif 3 : Base de données
-- Script d'initialisation de la base de données TourneeFutee
--
-- Instructions :
--   1. Créez la base de données avec : CREATE DATABASE tourneefutee;
--   2. Sélectionnez-la avec      : USE tourneefutee;
--   3. Exécutez ce script complet pour créer toutes les tables.
-- =============================================================================
CREATE DATABASE IF NOT EXISTS tourneefutee_test;
USE tourneefutee_test;
DROP TABLE IF EXISTS EtapeTournee;
DROP TABLE IF EXISTS Tournee;
DROP TABLE IF EXISTS Arc;
DROP TABLE IF EXISTS Sommet;
DROP TABLE IF EXISTS Graphe;

-- =============================================================================
-- Table : Graphe
-- =============================================================================
CREATE TABLE  IF NOT EXISTS Graphe (
    id           INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    est_oriente  TINYINT(1)      NOT NULL DEFAULT 0,
    nb_sommets   INT UNSIGNED    NOT NULL DEFAULT 0,

    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- =============================================================================
-- Table : Sommet
-- =============================================================================
CREATE TABLE  IF NOT EXISTS Sommet (
    id          INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id   INT UNSIGNED    NOT NULL,
    nom         VARCHAR(50)     NOT NULL,
    valeur      FLOAT           NULL,

    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- =============================================================================
-- Table : Arc
-- Pour un graphe non orienté, un seul arc est stocké par paire (indice source < indice dest).
-- AddEdge au chargement recrée automatiquement les deux sens.
-- =============================================================================
CREATE TABLE  IF NOT EXISTS Arc (
    id              INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id       INT UNSIGNED    NOT NULL,
    sommet_source   INT UNSIGNED    NOT NULL,
    sommet_dest     INT UNSIGNED    NOT NULL,
    poids           FLOAT           NOT NULL,

    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id)     REFERENCES Graphe(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_source) REFERENCES Sommet(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_dest)   REFERENCES Sommet(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- =============================================================================
-- Table : Tournee
-- =============================================================================
CREATE TABLE  IF NOT EXISTS Tournee (
    id          INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id   INT UNSIGNED    NOT NULL,
    cout_total  FLOAT           NOT NULL,

    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- =============================================================================
-- Table : EtapeTournee
-- Chaque ligne = un sommet visité à un rang donné dans la tournée.
-- =============================================================================
CREATE TABLE EtapeTournee (
    tournee_id      INT UNSIGNED    NOT NULL,
    numero_ordre    INT UNSIGNED    NOT NULL,
    sommet_id       INT UNSIGNED    NOT NULL,

    PRIMARY KEY (tournee_id, numero_ordre),
    FOREIGN KEY (tournee_id) REFERENCES Tournee(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_id)  REFERENCES Sommet(id)  ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


SHOW TABLES;