using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using NUnit.Framework;

namespace GraphProject
{
    internal class Program
    {

        static void Main(string[] args)
        {
            string[] dico = File.ReadAllLines("soc-karate.mtx");
            string[] dicoFiltred = dico.Where(line => !line.StartsWith("%")).ToArray(); //filtre le dictionnaire pour enlever les lignes avec des commentaires
            Graphe grapheKarate = new Graphe(dicoFiltred); 
            List<List<int>> adjacenceKarate = grapheKarate.ListAdjacenceGraphe(dicoFiltred);
            int[,] matriceAdjacenceKarate = grapheKarate.MatriceAdjacence(adjacenceKarate);
            
            //grapheKarate.DFS(1);
            //grapheKarate.BFS(1);
            //testAfficheListAdj();
            //TestAfficherMatrice();

            grapheKarate.AfficherProprietes();
            grapheKarate.VisualiserGraphe("graph.png");
            Console.WriteLine("Le graphe a été visualisé et sauvegardé sous 'graph.png'.");

        }

        static void TestAfficherMatrice(){
            string[] dico = File.ReadAllLines("soc-karate.mtx");
            string[] dicoFiltred = dico.Where(line => !line.StartsWith("%")).ToArray();
            Graphe grapheKarate = new Graphe();
            List<List<int>> adajcenceKarate = grapheKarate.ListAdjacenceGraphe(dicoFiltred);
            int[,] matriceAdjacenceKarate = grapheKarate.MatriceAdjacence(adajcenceKarate);
            int lignes = matriceAdjacenceKarate.GetLength(0); // Nombre de lignes
            int colonnes = matriceAdjacenceKarate.GetLength(1); // Nombre de colonnes

            for (int i = 0; i < lignes; i++)
            {
                for (int j = 0; j < colonnes; j++)
                {
                    Console.Write(matriceAdjacenceKarate[i, j].ToString().PadRight(3)); // Tabulation pour l'alignement
                }
                Console.WriteLine();

            }
        }

        static void testAfficheListAdj(){
            string[] dico = File.ReadAllLines("soc-karate.mtx");
            string[] dicoFiltred = dico.Where(line => !line.StartsWith("%")).ToArray();
            Graphe grapheKarate = new Graphe();
            List<List<int>> adajcenceKarate = grapheKarate.ListAdjacenceGraphe(dicoFiltred);
            

            for (int i = 0;i < adajcenceKarate.Count; i++){
                Console.Write("noeud "+(i+1)+": ");
                //Console.WriteLine("2 "+adajcenceKarate[i].Count);


                for (int j = 0; j<adajcenceKarate[i].Count;j++){
                    
                    //Console.WriteLine(i+ " "+j);
                    Console.Write(adajcenceKarate[i][j]+1+"; ");
                }
                Console.WriteLine();
            }
        }

        
    }

    
}