using System;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class Morpheme
    {
        public static readonly Morpheme UNKNOWN = Instance("Unknown", "Unknown");

        private readonly string name;
        private readonly string id;
        private readonly PrimaryPos pos;
        private readonly bool derivational1;
        private readonly bool informal;
        private readonly Morpheme mappedMorpheme;

        public bool Informal => informal;

        public Morpheme MappedMorpheme => mappedMorpheme;

        public bool Derivational1 => derivational1;

        public PrimaryPos Pos => pos;

        public string Id => id;

        public string Name => name;

        public Morpheme(MorphemeBuilder builder)
        {
            this.name = builder.Name;
            this.id = builder.Id;
            this.informal = builder.IsInformal;
            this.mappedMorpheme = builder.MappedMorpheme;
            this.derivational1 = builder.Derivational1;
            this.pos = builder.Pos1;
        }

        public static Morpheme Instance(string name, string id)
        {
            return Builder(name, id).Build();
        }

        public static Morpheme Instance(string name, string id, PrimaryPos pos)
        {
            return Builder(name, id).Pos(pos).Build();
        }

        public static Morpheme Derivational(string name, string id)
        {
            return Builder(name, id).Derivational().Build();
        }

        public static MorphemeBuilder Builder(string name, string id)
        {
            return new MorphemeBuilder(name, id);
        }

        public class MorphemeBuilder
        {
            private string name;
            private string id;
            private PrimaryPos pos;
            private bool derivational = false;
            private bool informal = false;
            private Morpheme mappedMorpheme;

            public string Name { get => name; set => name = value; }
            public string Id { get => id; set => id = value; }
            public PrimaryPos Pos1 { get => pos; set => pos = value; }
            public bool Derivational1 { get => derivational; set => derivational = value; }
            public bool IsInformal { get => informal; set => informal = value; }
            public Morpheme MappedMorpheme { get => mappedMorpheme; set => mappedMorpheme = value; }

            public MorphemeBuilder(string name, string id)
            {
                this.Name = name;
                this.Id = id;
            }

            public MorphemeBuilder Pos(PrimaryPos pos)
            {
                this.Pos1 = pos;
                return this;
            }

            public MorphemeBuilder Derivational()
            {
                this.Derivational1 = true;
                return this;
            }

            public MorphemeBuilder Informal()
            {
                this.IsInformal = true;
                return this;
            }

            public MorphemeBuilder SetMappedMorpheme(Morpheme morpheme)
            {
                this.MappedMorpheme = morpheme;
                return this;
            }

            public Morpheme Build()
            {
                return new Morpheme(this);
            }

        }

        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }
            Morpheme morpheme = (Morpheme)o;
            return Id.Equals(morpheme.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Name + ':' + Id;
        }
    }
}
