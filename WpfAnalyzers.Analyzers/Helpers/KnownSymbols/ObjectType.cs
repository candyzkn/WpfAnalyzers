namespace WpfAnalyzers
{
    internal class ObjectType : QualifiedType
    {
        internal new readonly QualifiedMethod Equals;
        internal new readonly QualifiedMethod ReferenceEquals;

        internal ObjectType()
            : base("System.Object")
        {
            this.Equals = new QualifiedMethod(this, nameof(this.Equals));
            this.ReferenceEquals = new QualifiedMethod(this, nameof(this.ReferenceEquals));
        }
    }
}