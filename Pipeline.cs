using Cyh.Net.Data.Logs;
using Cyh.Net.Data.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cyh.Net.Data
{
    public static class Pipeline
    {
        private static NEXT_ACTION GetNextAction<T, U, V>(T? source, U? data, V? saver, DataTransResult? result) {
            NEXT_ACTION retVal = NEXT_ACTION.NORM;
            if (data == null) {
                result?.OnTransact(FAILURE_REASON.INV_DATA);
                retVal = NEXT_ACTION.PASS;
            }
            if (source == null || saver == null) {
                result?.OnTransact(FAILURE_REASON.INV_SRCS);
                retVal = NEXT_ACTION.HALT;
            }
            return retVal;
        }

        public static unsafe bool Execute<T, U, V>(
            delegate*<T, U, void> exec, delegate*<V, void> save,
            T? source, U? data, V? saver,
            DataTransResult? result, bool exec_now) where T : class {

            NEXT_ACTION act = GetNextAction(source, data, saver, result);

            switch (act) {
                case NEXT_ACTION.NORM: {
#pragma warning disable CS8604
                    exec(source, data);
#pragma warning restore
                    break;
                }
                case NEXT_ACTION.PASS: {
                    break;
                }
                case NEXT_ACTION.HALT: {
                    result?.OnFinish(false);
                    return false;
                }
            }

            if (exec_now) {
                try {
#pragma warning disable CS8604
                    save(saver);
#pragma warning restore
                    result?.OnFinish(true);
                    return true;
                } catch (Exception ex) {
                    if (result != null) {
                        result.Message = $"Exception: {ex.Message}," +
#if DEBUG
                            $" InnerException: {ex.InnerException?.Message}";
#endif 
                    }
                    result?.OnFinish(false);
                    return false;
                }
            }
            return true;
        }
        public static unsafe bool Execute<T, U, V>(
            delegate*<T, U, void> exec, delegate*<V, void> save,
            T? source, IEnumerable<U> datas, V? saver,
            DataTransResult? result, bool exec_now) where T : class {

            NEXT_ACTION act = GetNextAction(source, datas, saver, result);

            switch (act) {
                case NEXT_ACTION.NORM: {
                    foreach (U? data in datas) {
#pragma warning disable CS8604
                        exec(source, data);
#pragma warning restore
                    }
                    break;
                }
                case NEXT_ACTION.PASS: {
                    break;
                }
                case NEXT_ACTION.HALT: {
                    result?.OnFinish(false);
                    return false;
                }
            }

            if (exec_now) {
                try {
#pragma warning disable CS8604
                    save(saver);
#pragma warning restore
                    result?.OnFinish(true);
                    return true;
                } catch (Exception ex) {
                    if (result != null) {
                        result.Message = $"Exception: {ex.Message}," +
#if DEBUG
                            $" InnerException: {ex.InnerException?.Message}";
#endif 
                    }
                    result?.OnFinish(false);
                    return false;
                }
            }
            return true;
        }
    }
}
