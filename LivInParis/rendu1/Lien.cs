using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using NUnit.Framework;

namespace GraphProject
{public class Lien
    {
        public Noeud Noeud1 { get; }
        public Noeud Noeud2 { get; }

        public Lien(Noeud noeud1, Noeud noeud2)
        {
            Noeud1 = noeud1;
            Noeud2 = noeud2;
        }


        
    }
}
