﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Signum.Entities;
using Signum.Entities.Reflection;
using Signum.Utilities;

namespace Signum.Engine.Maps
{
    public class EntityEvents<T> : IEntityEvents
            where T : Entity
    {
        public event PreSavingEventHandler<T> PreSaving;
        public event SavingEventHandler<T> Saving;
        public event SavedEventHandler<T> Saved;

        public event AlternativeRetriveEventHandler<T> AlternativeRetrive;
        public event RetrievedEventHandler<T> Retrieved;

        public CacheControllerBase<T> CacheController { get; set; }

        public event FilterQueryEventHandler<T> FilterQuery;

        public event PreUnsafeDeleteHandler<T> PreUnsafeDelete;
        public event PreUnsafeMListDeleteHandler<T> PreUnsafeMListDelete;

        public event PreUnsafeUpdateHandler<T> PreUnsafeUpdate;

        public event PreUnsafeInsertHandler<T> PreUnsafeInsert;
        public event BulkInsetHandler<T> PreBulkInsert;

        public Dictionary<PropertyRoute, Func<LambdaExpression>> AdditionalQueryBindings { get; private set; }

        public void RegisterBinding<M>(Expression<Func<T, M>> field, Func<Expression<Func<T, M>>> getValueExpression)
        {
            if (AdditionalQueryBindings == null)
                AdditionalQueryBindings = new Dictionary<PropertyRoute, Func<LambdaExpression>>();

            var ma = (MemberExpression)field.Body;

            var pr = PropertyRoute.Construct(field);
            
            AdditionalQueryBindings.Add(pr, getValueExpression);
        }

        internal IEnumerable<FilterQueryResult<T>> OnFilterQuery()
        {
            if (FilterQuery == null)
                return Enumerable.Empty<FilterQueryResult<T>>();

            return FilterQuery.GetInvocationListTyped().Select(f => f()).ToList();
        }

        internal IDisposable OnPreUnsafeDelete(IQueryable<T> entityQuery)
        {
            IDisposable result = null;
            if (PreUnsafeDelete != null)
                foreach (var action in PreUnsafeDelete.GetInvocationListTyped().Reverse())
                    result = Disposable.Combine(result, action(entityQuery));

            return result;
        }

        internal IDisposable OnPreUnsafeMListDelete(IQueryable mlistQuery, IQueryable<T> entityQuery)
        {
            IDisposable result = null;
            if (PreUnsafeMListDelete != null)
                foreach (var action in PreUnsafeMListDelete.GetInvocationListTyped().Reverse())
                    result = Disposable.Combine(result, action(mlistQuery, entityQuery));

            return result;
        }

        IDisposable IEntityEvents.OnPreUnsafeUpdate(IUpdateable update)
        {
            IDisposable result = null;
            if (PreUnsafeUpdate != null)
            {
                var query = update.EntityQuery<T>();
                foreach (var action in PreUnsafeUpdate.GetInvocationListTyped().Reverse())
                    result = Disposable.Combine(result, action(update, query));
            }

            return result;
        }

        LambdaExpression IEntityEvents.OnPreUnsafeInsert(IQueryable query, LambdaExpression constructor, IQueryable entityQuery)
        {
            if (PreUnsafeInsert != null)
                foreach (var action in PreUnsafeInsert.GetInvocationListTyped().Reverse())
                    constructor = action(query, constructor, (IQueryable<T>)entityQuery);

            return constructor;
        }

        void IEntityEvents.OnPreBulkInsert(bool inMListTable)
        {
            if (PreBulkInsert != null)
                foreach (var action in PreBulkInsert.GetInvocationListTyped().Reverse())
                    action(inMListTable);
        }

        void IEntityEvents.OnPreSaving(Entity entity, ref bool graphModified)
        {
            PreSaving?.Invoke((T)entity, ref graphModified);
        }

        void IEntityEvents.OnSaving(Entity entity)
        {
            Saving?.Invoke((T)entity);

        }

        void IEntityEvents.OnSaved(Entity entity, SavedEventArgs args)
        {
            Saved?.Invoke((T)entity, args);

        }

        void IEntityEvents.OnRetrieved(Entity entity)
        {
            Retrieved?.Invoke((T)entity);
        }

        public Entity OnAlternativeRetriving(PrimaryKey id)
        {
            if (AlternativeRetrive == null)
                return null;

            var args = new AlternativeRetrieveArgs<T>();

            AlternativeRetrive(id, args);


            if (args.Entity == null)
                throw new EntityNotFoundException(typeof(T), id);

            if (!args.AvoidAccesVerify)
            {
                var verifyAcces = Database.Query<T>().Where(a => a.Id == id).Any();
                if (!verifyAcces)
                    throw new EntityNotFoundException(typeof(T), id);
            }

            return (Entity)args.Entity;


        }

        ICacheController IEntityEvents.CacheController
        {
            get { return CacheController; }
        }
    }

    public delegate void PreSavingEventHandler<T>(T ident, ref bool graphModified) where T : Entity;
    public delegate void RetrievedEventHandler<T>(T ident) where T : Entity;
    public delegate void SavingEventHandler<T>(T ident) where T : Entity;
    public delegate void SavedEventHandler<T>(T ident, SavedEventArgs args) where T : Entity;
    public delegate FilterQueryResult<T> FilterQueryEventHandler<T>() where T : Entity;
    public delegate void AlternativeRetriveEventHandler<T>(PrimaryKey id, AlternativeRetrieveArgs<T> args) where T : Entity;

    public delegate IDisposable PreUnsafeDeleteHandler<T>(IQueryable<T> entityQuery);
    public delegate IDisposable PreUnsafeMListDeleteHandler<T>(IQueryable mlistQuery, IQueryable<T> entityQuery);
    public delegate IDisposable PreUnsafeUpdateHandler<T>(IUpdateable update, IQueryable<T> entityQuery);
    public delegate LambdaExpression PreUnsafeInsertHandler<T>(IQueryable query, LambdaExpression constructor, IQueryable<T> entityQuery);
    public delegate void BulkInsetHandler<T>(bool inMListTable);


    public class AlternativeRetrieveArgs<T> where T : Entity
    {
        public bool AvoidAccesVerify { get; set; }
        public T Entity { get; set; }
    }

    public class SavedEventArgs
    {
        public bool IsRoot { get; set; }
        public bool WasNew { get; set; }
        public bool WasSelfModified { get; set; }
    }

    public interface IFilterQueryResult
    {
        LambdaExpression InDatabaseExpression { get; }
    }

    public class FilterQueryResult<T> : IFilterQueryResult where T : Entity
    {
        public FilterQueryResult(Expression<Func<T, bool>> inDatabaseExpression, Func<T, bool> inMemoryFunction)
        {
            this.InDatabaseExpresson = inDatabaseExpression;
            this.InMemoryFunction = inMemoryFunction;
        }

        public readonly Expression<Func<T, bool>> InDatabaseExpresson;
        public readonly Func<T, bool> InMemoryFunction;

        LambdaExpression IFilterQueryResult.InDatabaseExpression { get { return this.InDatabaseExpresson; } }
    }

    internal interface IEntityEvents
    {
        Entity OnAlternativeRetriving(PrimaryKey id);
        void OnPreSaving(Entity entity, ref bool graphModified);
        void OnSaving(Entity entity);
        void OnSaved(Entity entity, SavedEventArgs args);

        void OnRetrieved(Entity entity);

        IDisposable OnPreUnsafeUpdate(IUpdateable update);
        LambdaExpression OnPreUnsafeInsert(IQueryable query, LambdaExpression constructor, IQueryable entityQuery);
        void OnPreBulkInsert(bool inMListTable);

        ICacheController CacheController { get; }

        Dictionary<PropertyRoute, Func<LambdaExpression>> AdditionalQueryBindings { get; }
    }
}
