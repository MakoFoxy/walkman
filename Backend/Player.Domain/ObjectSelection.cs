using System;
using Player.Domain.Base;

namespace Player.Domain
{
    public class ObjectSelection : Entity
    {
        public ObjectInfo Object { get; set; }
        public Guid ObjectId { get; set; }
        public Selection Selection { get; set; }
        public Guid SelectionId { get; set; }
    }
}
