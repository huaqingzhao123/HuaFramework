using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nireus
{
    public interface IUITileListItemRenderer<T>
    {
        void UpdateTileListItemData(T data, int data_index);
    }
}
