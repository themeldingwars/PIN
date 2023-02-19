using System.Data;

namespace Core.Data;

public abstract class RowView : IRowView
{
    public virtual void Load(IDataRecord row)
    {
    }
}