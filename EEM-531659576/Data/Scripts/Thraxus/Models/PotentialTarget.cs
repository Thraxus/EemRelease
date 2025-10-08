using System.Collections.Generic;
using Sandbox.Game.Entities;

namespace Eem.Thraxus.Models
{
    internal class PotentialTarget
    {
        public HashSet<MyCubeBlock> Controllers;
        public HashSet<MyCubeBlock> PowerProducers;
        public HashSet<MyCubeBlock> Weapons;
    }
}