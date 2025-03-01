using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
//using NUnit.Framework;

namespace GraphProject
{
    public class Noeud
    {
        public int Id { get; private set; }
        public List<int> Adjacence { get; private set;}

        public Noeud(int id, string[] fichierAdj)
        {
            Id = id;
            Adjacence = AdjacenceNoeud(fichierAdj);
        }


        public  List<int> AdjacenceNoeud(string[] fichierAdj){
            List<int> adjacences = new List<int>();

            for (int i = 1; i < fichierAdj.Length;i++){
                List<string> words = new List<string>(fichierAdj[i].Split(' ', StringSplitOptions.RemoveEmptyEntries));
                int noeud1 = Convert.ToInt32(words[0]);
                int noeud2 = Convert.ToInt32(words[1]);
                if (noeud1== this.Id ){
                    adjacences.Add(noeud2);
                }
                else if (noeud2== this.Id ){
                    adjacences.Add(noeud1);
                }
            }
            adjacences.Distinct().OrderBy(n => n).ToList();

            return adjacences;
        }
    }
}