using System.Data;

namespace Core.Data
{
    public interface IRowView
    {
        void Load(IDataRecord row);
    }
}