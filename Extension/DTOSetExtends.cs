using Cyh.Net.Data.Internal;
using Cyh.Net.Data.Logs;
using Cyh.Net.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace Cyh.Net.Data.Extension
{
    public static class DTOSetExtends
    {
        private static bool ShouldReturn<T, V>(this IDTOHelper<T, V> dto, [NotNullWhen(false)] V? view, [NotNull] ref DataTransResult? result, bool exec_now) {
            if (result != null) {
                if (result.IsFinished) { return true; }
            } else {
                result = new DataTransResult();
            }

            if (view != null) { return false; }

            if (exec_now) {
                result.OnTransact(FAILURE_REASON.INV_DATA);
                bool is_succeed = dto.DataSource.ApplyChanges(result);
                result.OnFinish(is_succeed);
                return true;
            } else {
                return false;
            }
        }

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

        public static DataTransResult TryAdd<T, V>(this IDTOHelper<T, V> dto, V? view, DataTransResult? result, bool exec_now) {
            if (ShouldReturn(dto, view, ref result, exec_now)) { return result; }
            dto.DataSource.AddSingle(dto.GetExprToData().Compile(), view, result, exec_now);
            return result;
        }
        public static DataTransResult TryUpdate<T, V>(this IDTOHelper<T, V> dto, V? view, DataTransResult? result, bool exec_now) {
            if (ShouldReturn(dto, view, ref result, exec_now)) { return result; }
            dto.DataSource.UpdateFromSingle(dto.UpdateToData, view, dto.GetExprToFindData(view), result, exec_now);
            return result;
        }

    }
}
