using Cyh.Net.Data.Extension;
using Cyh.Net.Data.Models;
using System.Linq.Expressions;

namespace Cyh.Net.Data
{
    public abstract class DataRepositoryBase<TData, TView> : IDTOHelper<TData, TView>
    {
        public DataRepositoryBase(IDTOHelperActivator activator) {
            activator.Activate(this);
            if (this.DataSource == null) {
                throw new Exception("Invalid data source.");
            }
        }

        public IMyDataSource<TData> DataSource { get; set; }

        public bool Any(Expression<Func<TView, bool>>? predicate, DataTransResult? result) {
            return this.HasAny(predicate);
        }

        public int Count(Expression<Func<TView, bool>>? predicate, DataTransResult? result) {
            return this.GetCount(predicate);
        }

        public TView? GetFirst(Expression<Func<TView, bool>>? predicate, DataTransResult? result) {
            return this.TryGetFirst(predicate, result);
        }

        public IEnumerable<TView> GetAll(Expression<Func<TView, bool>>? predicate, DataRange? dataRange, DataTransResult? result) {
            return this.TryGetAll(predicate, dataRange, result);
        }

        public IEnumerable<TView> GetAllByAsc<TKey>(Expression<Func<TView, bool>>? predicate, Expression<Func<TView, TKey>>? order, DataRange? dataRange, DataTransResult? result) {
            return this.TryGetAllByAsc(predicate, order, dataRange, result);
        }

        public IEnumerable<TView> GetAllByDesc<TKey>(Expression<Func<TView, bool>>? predicate, Expression<Func<TView, TKey>>? order, DataRange? dataRange, DataTransResult? result) {
            return this.TryGetAllByDesc(predicate, order, dataRange, result);
        }
        public DataTransResult Add(TView? data, DataTransResult? result, bool exec_now) {
            return this.TryAdd(data, result, exec_now);
        }
        public DataTransResult Update(TView? data, DataTransResult? result, bool exec_now) {
            return this.TryUpdate(data, result, exec_now);
        }

        public abstract Expression<Func<TView, TData>> GetExprToData(TView? x = default);

        public abstract Expression<Func<TData, bool>> GetExprToFindData(TView view);

        public abstract Expression<Func<TData, TView>> GetExprToView(TData? x = default);

        public abstract int UpdateToData(TView view, TData data);
    }
}