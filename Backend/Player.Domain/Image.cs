using Player.Domain.Base;

namespace Player.Domain
{
    public class Image : Entity
    {
        public string Path { get; set; }
        public bool IsMain { get; set; }
        public ObjectInfo Object { get; set; }
        public Size Size { get; set; }
    }
}