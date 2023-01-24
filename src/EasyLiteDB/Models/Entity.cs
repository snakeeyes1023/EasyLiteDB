using LiteDB;

namespace EasyLiteDB.Models
{
    public abstract class LiteDBEntity
    {
        public ObjectId _id { get; set; }

        [BsonIgnore]
        public bool IsDirty { get; private set; }

        public void SetDirty(bool isDirty = true)
        {
            IsDirty = isDirty;
        }
    }
}
