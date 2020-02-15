using Bangumi.Api.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bangumi.Helper
{
    public static class CollectionHelper
    {
        /// <summary>
        /// 以新列表为准，将老列表改为与新列表相同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin">显示的列表</param>
        /// <param name="dest">新的列表</param>
        public static void Differ<T>(ObservableCollection<T> origin, IList<T> dest) where T : class
        {
            if (origin is null)
            {
                throw new System.ArgumentNullException(nameof(origin));
            }

            if (dest is null)
            {
                throw new System.ArgumentNullException(nameof(dest));
            }

            if (!origin.SequenceEqualExT(dest))
            {
                int compareCount = 0;
                for (int i = 0; i < dest.Count; i++)
                {
                    bool insert = true;
                    for (int j = i; j < origin.Count; j++)
                    {
                        compareCount++;
                        if (dest[i].EqualsExT(origin[j]))
                        {
                            if (j != i)
                            {
                                origin.RemoveAt(j);
                                origin.Insert(i, dest[i]);
                            }
                            insert = false;
                            break;
                        }
                        else
                        {
                            bool removed = true;
                            for (int k = j; k < dest.Count; k++)
                            {
                                compareCount++;
                                if (origin[j].EqualsExT(dest[k]))
                                {
                                    removed = false;
                                    break;
                                }
                            }
                            if (removed)
                            {
                                origin.RemoveAt(j--);
                            }
                        }
                    }
                    if (insert)
                    {
                        origin.Insert(i, dest[i]);
                    }
                }
                NotificationHelper.Notify($"{nameof(compareCount)}: {compareCount}", NotificationHelper.NotifyType.Debug);
                // 若通过以上步骤无法排好序，则重置列表
                if (!origin.SequenceEqualExT(dest))
                {
                    NotificationHelper.Notify($"{nameof(compareCount)}: {compareCount}\n比较失败，重置列表！", NotificationHelper.NotifyType.Debug);
                    origin.Clear();
                    foreach (var item in dest)
                    {
                        origin.Add(item);
                    }
                }
            }
        }
    }
}
