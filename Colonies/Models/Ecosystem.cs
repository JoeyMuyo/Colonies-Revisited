﻿namespace Colonies.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Colonies.Annotations;
    using Colonies.ViewModels;

    public sealed class Ecosystem
    {
        private List<List<Habitat>> habitats;
        public List<List<Habitat>> Habitats
        {
            get
            {
                return this.habitats;
            }
            set
            {
                this.habitats = value;
            }
        }

        // TODO: are height and width the right way round?
        public int Height
        {
            get
            {
                return this.Habitats.Count;
            }
        }

        public int Width
        {
            get
            {
                return this.Habitats.First().Count;
            }
        }

        public Ecosystem(List<List<Habitat>> habitats)
        {
            this.Habitats = habitats;
        }

        public void Update()
        {
            // currently, updating the ecosystem simply means
            // randomly moving all organisms to a different habitat
            var random = new Random();

            var occupiedHabitats = this.GetOccupiedHabitats();
            foreach (var occupiedHabitat in occupiedHabitats)
            {
                var destinationX = random.Next(this.Width);
                var destinationY = random.Next(this.Height);
                var destinationHabitat = this.Habitats[destinationX][destinationY];

                if (!destinationHabitat.Equals(occupiedHabitat) && !destinationHabitat.ContainsOrganism())
                {
                    this.MoveOrganism(occupiedHabitat, destinationHabitat);
                }
            }
        }

        private void MoveOrganism(Habitat sourceHabitat, Habitat destinationHabitat)
        {
            if (!sourceHabitat.ContainsOrganism())
            {
                throw new ArgumentException(String.Format("Source habitat {0} does not contain an organism", sourceHabitat), "sourceHabitat");
            }

            if (destinationHabitat.ContainsOrganism())
            {
                throw new ArgumentException(String.Format("Destination habitat {0} already contains an organism", destinationHabitat), "destinationHabitat");
            }

            var organismToMove = sourceHabitat.Organism;
            this.RemoveOrganism(sourceHabitat);
            this.AddOrganism(destinationHabitat, organismToMove);
        }

        private bool ContainsOrganism(int x, int y)
        {
            return this.Habitats[x][y].ContainsOrganism();
        }

        private void AddOrganism(Habitat habitat, Organism organism)
        {
            if (habitat.ContainsOrganism())
            {
                throw new ArgumentException(String.Format("Organism already exists at habitat {0}", habitat), "habitat");
            }

            habitat.Organism = organism;
        }

        private void RemoveOrganism(Habitat habitat)
        {
            if (!habitat.ContainsOrganism())
            {
                throw new ArgumentException(String.Format("No organism exists at habitat {0}", habitat), "habitat");
            }

            habitat.Organism = null;
        }

        private IEnumerable<Habitat> GetOccupiedHabitats()
        {
            var occupiedHabitats = new List<Habitat>();
            foreach (var habitatList in this.Habitats)
            {
                foreach (var habitat in habitatList)
                {
                    if (habitat.ContainsOrganism())
                    {
                        occupiedHabitats.Add(habitat);
                    }
                }
            }

            return occupiedHabitats;
        }

        public override String ToString()
        {
            return this.Width + " x " + this.Height;
        }
    }
}