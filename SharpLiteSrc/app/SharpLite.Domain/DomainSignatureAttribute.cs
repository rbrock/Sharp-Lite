using System;

namespace SharpLite.Domain
{
    /// <summary>
    /// Facilitates indicating which property(s) describe the unique signature of an entity.  
    /// See <see cref="EntityWithTypedId{TId}.GetTypeSpecificSignatureProperties"/> for when this is leveraged.
    /// </summary>
    /// <remarks>This is intended for use with <see cref="EntityWithTypedId{TId}" />.</remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DomainSignatureAttribute : Attribute { }
}