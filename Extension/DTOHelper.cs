using Cyh.Net.Data.Internal;
using Cyh.Net.Data.Logs;
using Cyh.Net.Data.Models;
using System.Linq.Expressions;

namespace Cyh.Net.Data.Extension
{
    public static class DTOHelper
    {
        public static bool Activate<T>(this IDTOHelperActivator activator, IDTOHelper<T>? dataManager) {
            if (dataManager == null) { return false; }
            if (dataManager.DataSource != null) { return true; }
            try {
                dataManager.DataSource = activator.GetDataSource<T>();
                return true;
            } catch {
                return false;
            }
        }
        public static V? TryGetFirst<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, DataTransResult? result) {
            return dto.DataSource.GetSingle(dto.GetExprToView(), filter, result);
        }
        public static IEnumerable<V> TryGetAll<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, DataRange? dataRange, DataTransResult? result) {
            return dto.DataSource.GetMultiple(dto.GetExprToView(), filter, dataRange, result);
        }
        public static IEnumerable<V> TryGetAllByAsc<T, V, VKey>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, Expression<Func<V, VKey>>? order, DataRange? dataRange, DataTransResult? result) {
            return dto.DataSource.GetMultipleAsc(dto.GetExprToView(), filter, order, dataRange, result);
        }
        public static IEnumerable<V> TryGetAllByDesc<T, V, VKey>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, Expression<Func<V, VKey>>? order, DataRange? dataRange, DataTransResult? result) {
            return dto.DataSource.GetMultipleDesc(dto.GetExprToView(), filter, order, dataRange, result);
        }

        public static DataTransResult TryAdd<T, V>(this IDTOHelper<T, V> dto, V? view, DataTransResult? result, bool exec_now) {
            result ??= new DataTransResult();
            if (view == null) {
                result.OnTransact(FAILURE_REASON.INV_DATA);
                return result;
            }
            dto.DataSource.AddSingle(dto.GetExprToData().Compile(), view, result, exec_now);
            return result;
        }
        public static DataTransResult TryUpdate<T, V>(this IDTOHelper<T, V> dto, V? view, DataTransResult? result, bool exec_now) {
            result ??= new DataTransResult();
            if (view == null) {
                result.OnTransact(FAILURE_REASON.INV_DATA);
                return result;
            }
            dto.DataSource.UpdateFromSingle(dto.UpdateToData, view, dto.GetExprToFindData(view), result, exec_now);
            return result;
        }

        public static bool HasAny<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter) {
            return dto.DataSource.HasAny(dto.GetExprToView(), filter);
        }
        public static int GetCount<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter) {
            return dto.DataSource.GetCount(dto.GetExprToView(), filter);
        }
    }
}