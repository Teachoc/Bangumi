using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bangumi.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 检查属性值是否相同。
        /// 仅在不同时设置属性值。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="storage">可读写的属性。</param>
        /// <param name="value">属性值。</param>
        /// <param name="propertyName">属性名。可被支持 CallerMemberName 的编译器自动提供。</param>
        /// <returns>值改变则返回 true，未改变返回 false。</returns>
        protected bool Set<T>(ref T storage, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
