﻿namespace Cyh.Net.Data
{
    public interface IDataSource
    {
        IDisposable? BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        IQueryable<T> GetQueryable<T>() where T : class;
        IDataWriter<T> GetWriter<T>() where T : class;
        bool Save();
    }
}
