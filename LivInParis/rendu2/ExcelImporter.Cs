using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace GraphProject
{
    /// <summary>
    /// Classe utilitaire pour charger les arcs depuis la feuille "Arcs".
    /// La feuille "Arcs" est supposée contenir les colonnes suivantes :
    /// A: Station Id (int)
    /// B: Station (string)
    /// C: Précédent (int)
    /// D: Suivant (int)
    /// E: Temps entre 2 stations (double)
    /// F: Temps de Changement (double)
    /// G: Direction (string) : "uni" pour unidirectionnel, "bi" pour bidirectionnel (valeur par défaut = "bi")
    /// 
    /// Ainsi, pour une ligne donnée, si la colonne "Précédent" n'est pas 0, on crée un arc depuis
    /// la station correspondant à cet identifiant vers la station actuelle. Idem pour "Suivant".
    /// Si la direction est "bi", on ajoute également l'arc dans l'autre sens.
    /// </summary>
    public static class ExcelImporter
    {
        public static void ChargerArcs(Graphe<string> graphe, string cheminExcel)
        {
            try
            {
                using (var workbook = new XLWorkbook(cheminExcel))
                {
                    var ws = workbook.Worksheet("Arcs");
                    int row = 2; // Supposons que la 1ère ligne est l'en-tête
                    var lignes = new List<ArcLine>();
                    var stationDict = new Dictionary<int, string>();

                    while (!ws.Row(row).IsEmpty())
                    {
                        int stationId = ws.Cell(row, 1).GetValue<int>(); // colonne A
                        string stationName = ws.Cell(row, 2).GetValue<string>(); // colonne B
                        int precedent = ws.Cell(row, 3).GetValue<int>(); // colonne C
                        int suivant = ws.Cell(row, 4).GetValue<int>();   // colonne D
                        double tempsEntre = ws.Cell(row, 5).GetValue<double>(); // colonne E
                        double tempsChangement = ws.Cell(row, 6).GetValue<double>(); // colonne F

                        // Lecture de la colonne G pour la direction (optionnelle)
                        string direction = "bi"; // valeur par défaut : bidirectionnel
                        var cellDirection = ws.Cell(row, 7);
                        if (!cellDirection.IsEmpty())
                        {
                            direction = cellDirection.GetValue<string>().Trim().ToLower();
                        }

                        // Stocker la correspondance stationId -> stationName
                        if (!stationDict.ContainsKey(stationId))
                            stationDict.Add(stationId, stationName);

                        lignes.Add(new ArcLine
                        {
                            StationId = stationId,
                            StationName = stationName,
                            Precedent = precedent,
                            Suivant = suivant,
                            TempsEntre = tempsEntre,
                            TempsChangement = tempsChangement,
                            Direction = direction
                        });

                        row++;
                    }

                    // Construction des arcs en fonction des informations lues
                    foreach (var line in lignes)
                    {
                        double poids = line.TempsEntre + line.TempsChangement;

                        // Arc depuis "Précédent" vers la station actuelle
                        if (line.Precedent != 0 && stationDict.ContainsKey(line.Precedent))
                        {
                            string source = stationDict[line.Precedent];
                            string cible = line.StationName;
                            graphe.AjouterArc(source, cible, poids);
                            // Si bidirectionnel, ajouter aussi l'arc inverse
                            if (line.Direction == "bi")
                            {
                                graphe.AjouterArc(cible, source, poids);
                            }
                        }

                        // Arc depuis la station actuelle vers "Suivant"
                        if (line.Suivant != 0 && stationDict.ContainsKey(line.Suivant))
                        {
                            string source = line.StationName;
                            string cible = stationDict[line.Suivant];
                            graphe.AjouterArc(source, cible, poids);
                            if (line.Direction == "bi")
                            {
                                graphe.AjouterArc(cible, source, poids);
                            }
                        }
                    }
                    Console.WriteLine("Les arcs ont été chargés depuis l'Excel.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la lecture de l'Excel : " + ex.Message);
            }
        }

        // Classe interne pour stocker les informations d'une ligne de la feuille "Arcs"
        public class ArcLine
        {
            public int StationId { get; set; }
            public string StationName { get; set; }
            public int Precedent { get; set; }
            public int Suivant { get; set; }
            public double TempsEntre { get; set; }
            public double TempsChangement { get; set; }
            public string Direction { get; set; }
        }
    }
}
