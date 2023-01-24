using EasyLiteDB.Models;
using LiteDB;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyLiteDB.Mappers
{
    /// <summary>
    /// Permet de gérer les données en base de données LiteDB (https://www.litedb.org/)
    /// Les éléments mise à jour son seulement ceux avec le champ dirty à true
    /// </summary>
    /// <typeparam name="T">Une entité quelquonque</typeparam>
    /// <seealso cref="IEnumerable&lt;T&gt;" />
    public class LiteDbSet<T> : IEnumerable<T>, ILiteDbSet where T : LiteDBEntity
    {
        public string ConnectionStrings { get; set; }


        private bool needRefresh = true;

        /// <summary>
        /// The entities (only entity with _id != null)
        /// </summary>
        private readonly List<T> _entities;

        /// <summary>
        /// The entities to delete (only entity with _id != null)
        /// </summary>
        private readonly List<T> _entitiesToDelete;

        /// <summary>
        /// The entities to insert (only entity with _id == null)
        /// </summary>
        private readonly List<T> _entitiesToInsert;

        /// <summary>
        /// The custom query
        /// </summary>
        private List<Action<ILiteQueryable<T>>> CustomQuery;

        public LiteDbSet()
        {
            _entities = new List<T>();
            _entitiesToDelete = new List<T>();
            _entitiesToInsert = new List<T>();
        }

        #region ADD Entity without ID
        /// <summary>
        /// Adds the specified entity. And track it
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Add(T entity)
        {
            if (IsAlreadyInDb(entity))
            {
                throw new ArgumentException("Entity already in database");
            }

            _entitiesToInsert.Add(entity);
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void AddRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }
        #endregion

        /// <summary>
        /// Tracks the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public void TrackEntity(T entity)
        {
            if (!IsAlreadyInDb(entity))
            {
                throw new ArgumentException("Entity must have an id");
            }

            _entities.Add(entity);
        }

        /// <summary>
        /// Uses the query. (Directly bind to entity)
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public LiteDbSet<T> UseQuery(Action<ILiteQueryable<T>> query)
        {
            CustomQuery ??= new List<Action<ILiteQueryable<T>>>();
            CustomQuery.Add(query);

            return this;
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Remove(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Remove", "Entity to delete is null");
            }

            if (entity._id == null)
            {
                RemovePendingInsertEntity(entity);
            }
            else
            {
                RemoveAlreadyInDbEntity(entity);
            }
        }

        /// <summary>
        /// Removes the already in database entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void RemoveAlreadyInDbEntity(T entity)
        {
            _entitiesToDelete.Add(entity);
            _entities.Remove(entity);
        }

        /// <summary>
        /// Removes the pending insert entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void RemovePendingInsertEntity(T entity)
        {
            _entitiesToInsert.Remove(entity);
        }

        #region DATABASE STEP

        /// <summary>
        /// Saves the changes. (ONLY UPDATE IF THE ENTITY HAS AN ID AND ISDIRTY)
        /// </summary>
        public void SaveChanges()
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection<T>(db);

                if (_entitiesToDelete.Any())
                {
                    DeleteEntitiesToDelete(col);
                }

                if (_entitiesToInsert.Any())
                {
                    InsertEntitiesInDb(col);
                }

                UpdateDirtyTrackedEntities(col);
            }

            needRefresh = true;
        }

        private void DeleteEntitiesToDelete(ILiteCollection<T> col)
        {
            foreach (var entity in _entitiesToDelete)
            {
                col.Delete(entity._id);
            }

            _entitiesToDelete.Clear();
        }

        private void UpdateDirtyTrackedEntities(ILiteCollection<T> col)
        {
            foreach (var item in _entities)
            {
                if (item.IsDirty)
                {
                    col.Update(item);

                    item.SetDirty(false);
                }
            }
        }

        private void InsertEntitiesInDb(ILiteCollection<T> col)
        {
            col.InsertBulk(_entitiesToInsert);

            AddMissingEntities(_entitiesToInsert);

            _entitiesToInsert.Clear();
        }

        /// <summary>
        /// Forces the refresh. (ALL UNSAVED CHANGE WILL BE CANCEL)
        /// </summary>
        public void ClearContext()
        {
            _entities.Clear();
            _entitiesToDelete.Clear();
            _entitiesToInsert.Clear();

            if (CustomQuery != null)
            {
                CustomQuery.Clear();
            }

            needRefresh = true;
        }

        /// <summary>
        /// Populates the entites from database.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<T> RefreshCache()
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection<T>(db);

                _entities.Clear();

                if (CustomQuery != null && CustomQuery.Any())
                {
                    var query = col.Query();

                    foreach (var item in CustomQuery)
                    {
                        item(query);
                    }

                    AddMissingEntities(query.ToEnumerable());

                    CustomQuery.Clear();
                }
                else
                {
                    AddMissingEntities(col.FindAll());
                }
            }

            needRefresh = false;

            return _entities;
        }

        #endregion

        /// <summary>
        /// Adds the missing entities.
        /// </summary>
        /// <param name="entitiesToInsert">The entities to insert.</param>
        private void AddMissingEntities(IEnumerable<T> entitiesToInsert)
        {
            //add entities if is in database and not in _entities
            foreach (var item in entitiesToInsert)
            {
                if (IsAlreadyInDb(item) && !IsAlreadyTrack(item))
                {
                    _entities.Add(item);
                }
            }
        }

        /// <summary>
        /// Determines whether [is already track] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if [is already track] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAlreadyTrack(T item)
        {
            return _entities.Any(x => x._id == item._id);
        }


        /// <summary>
        /// Gets the combined datasource. if need to refresh its search on Database
        /// </summary>
        /// <returns></returns>
        private IEnumerable<T> GetCombinedDatasource()
        {
            if (needRefresh)
            {
                RefreshCache();
            }

            return _entities.Concat(_entitiesToInsert);
        }

        /// <summary>
        /// Determines whether [is already in database] [the specified entity].
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if [is already in database] [the specified entity]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsAlreadyInDb(T entity)
        {
            return entity._id != null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            var results = GetCombinedDatasource().GetEnumerator();

            needRefresh = true;

            return results;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var results = ((IEnumerable)GetCombinedDatasource()).GetEnumerator();

            needRefresh = true;

            return results;
        }

        public int Count()
        {
            using (var db = new LiteDatabase(ConnectionStrings))
            {
                var col = GetCollection<T>(db);

                if (CustomQuery != null && CustomQuery.Any())
                {
                    var query = col.Query();

                    foreach (var item in CustomQuery)
                    {
                        item(query);
                    }

                    CustomQuery.Clear();
                }

                return col.Count();
            }
        }

        public static ILiteCollection<T> GetCollection<T>(LiteDatabase liteDatabase) where T : LiteDBEntity
        {
            //get attribute Table
            var table = (typeof(T)
                .GetCustomAttributes(typeof(TableAttribute), true)
                .FirstOrDefault() as TableAttribute)?.Name;

            if (string.IsNullOrEmpty(table))
            {
                throw new Exception("Table attribute is missing");
            }

            return liteDatabase.GetCollection<T>(table);
        }
    }
}