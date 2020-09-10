using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Core.Data {
	public abstract class RowView : IRowView {
		public virtual void Load( IDataRecord row ) {
		}
	}
}
